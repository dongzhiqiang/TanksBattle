package com.engine.common.ramcache.service;

import java.io.Serializable;
import java.lang.ref.WeakReference;
import java.util.Collection;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.WeakHashMap;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock.ReadLock;
import java.util.concurrent.locks.ReentrantReadWriteLock.WriteLock;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.CachedEntityConfig;
import com.engine.common.ramcache.anno.InitialConfig;
import com.engine.common.ramcache.enhance.Enhancer;
import com.engine.common.ramcache.enhance.JavassistRegionEnhancer;
import com.engine.common.ramcache.exception.ConfigurationException;
import com.engine.common.ramcache.exception.InvaildEntityException;
import com.engine.common.ramcache.exception.StateException;
import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.ramcache.orm.Querier;
import com.engine.common.ramcache.persist.AbstractListener;
import com.engine.common.ramcache.persist.Element;
import com.engine.common.ramcache.persist.Persister;
import com.engine.common.utils.collection.ConcurrentHashSet;
import com.googlecode.concurrentlinkedhashmap.ConcurrentLinkedHashMap.Builder;

@SuppressWarnings("unchecked")
public class RegionCacheServiceImpl<PK extends Comparable<PK> & Serializable, T extends IEntity<PK>> implements
		RegionCacheService<PK, T>, RegionEnhanceService<PK, T> {

	private static final Logger logger = LoggerFactory.getLogger(RegionCacheServiceImpl.class);
	
	/**
	 * 弱引用的实体持有者
	 * 
	 */
	@SuppressWarnings("rawtypes")
	private static class WeakEntityHolder<T extends IEntity> {
		
		/** 增强器 */
		private final Enhancer enhancer;
		/** 实体缓存 */
		private final WeakHashMap<T, WeakReference<T>> entities = new WeakHashMap<T, WeakReference<T>>();
		/** 实体缓存操作锁 */
		private final ReentrantReadWriteLock entitiesLock = new ReentrantReadWriteLock();

		public WeakEntityHolder(Enhancer enhancer) {
			this.enhancer = enhancer;
		}
		
		/**
		 * 获取之前的缓存实体实例(读锁保护)
		 * @param entity 当前的实体实例
		 * @return
		 */
		private T getPrevEntity(T entity) {
			// 没有获得写锁时的处理
			Lock lock = entitiesLock.readLock();
			lock.lock();
			try {
				WeakReference<T> ref = entities.get(entity);
				if (ref != null) {
					return ref.get();
				}
				return null;
			} finally {
				lock.unlock();
			}
		}

		/**
		 * 在维持实体实例单一的条件下，将原始的实体实例放入实体缓存并返回增强的实例对象(写锁保护)
		 * @param entity
		 * @return
		 */
		private T putIfAbsentEntity(T entity) {
			Lock lock = entitiesLock.writeLock();
			lock.lock();
			try {
				WeakReference<T> value = entities.get(entity);
				if (value != null) {
					T prev = value.get();
					if (prev != null) {
						return prev;
					}
				}
				
				T prev = entity;
				if (enhancer != null) {
					entity = enhancer.transform(entity);
				}
				entities.put(prev, new WeakReference<T>(entity));
				return entity;
			} finally {
				lock.unlock();
			}
		}
	}

	/** 初始化标识 */
	private boolean initialize;

	/** 实体类型 */
	private Class<T> entityClz;
	/** 缓存实体配置信息 */
	private CachedEntityConfig config;
	/** 存储器 */
	private Accessor accessor;
	/** 查询器 */
	private Querier querier;
	/** 实体持久化处理器 */
	private Persister persister;
	/** 弱引用的实体持有者 */
	private WeakEntityHolder<T> entityHolder;

	/** 区域缓存 */
	private ConcurrentHashMap<String, ConcurrentMap<Object, ConcurrentHashMap<PK, T>>> cache;
	/** 索引区域内容更新锁 */
	private ConcurrentHashMap<IndexValue, ReentrantLock> indexLocks = new ConcurrentHashMap<IndexValue, ReentrantLock>();
	
	/** 主键操作锁 */
	private ConcurrentHashMap<PK, ReentrantLock> pkLocks = new ConcurrentHashMap<PK, ReentrantLock>();

	/** 正在被移除的实体的主键集合 */
	private ConcurrentHashSet<PK> removing = new ConcurrentHashSet<PK>();
	/** 正在被移除的实体的主键集合 */
	private ReentrantReadWriteLock removingLock = new ReentrantReadWriteLock();

	/** 初始化方法 */
	public synchronized void initialize(CachedEntityConfig config, Persister persister, Accessor accessor, Querier querier) {
		if (initialize) {
			throw new StateException("重复初始化异常");
		}

		// 初始化属性域
		initFileds(config, persister, accessor, querier);
		// 缓存初始化处理
		initCaches(config, querier);
		this.initialize = true;
	}

	/**
	 * 锁顺序说明: indexLock > removingLock.readLock
	 */
	@Override
	public Collection<T> load(IndexValue idx) {
		uninitializeThrowException();

		if (!config.hasIndexField(idx.getName())) {
			throw new StateException("实体[" + entityClz.getName() + "]没有索引属性域[" + idx.getName() + "]");
		}

		// 尝试从缓存中获取
		ConcurrentHashMap<PK, T> result = getIndexValueMap(idx);
		if (result != null) {
			return Collections.unmodifiableCollection(result.values());
		}

		// 锁定索引值:抑制索引值并发写
		Lock lock = getIndexValueLock(idx);
		lock.lock();
		try {
			result = getIndexValueMap(idx);
			if (result != null) {
				return Collections.unmodifiableCollection(result.values());
			}

			result = createIndexValueMap(idx, false);

			// 锁定被移除主键集合:避免读取到脏数据
			ReadLock removingLock = this.removingLock.readLock();
			removingLock.lock();
			try {
				// 执行查询
				List<T> current = querier.list(entityClz, config.getIndexQuery(idx.getName()), idx.getValue());
				if (current.isEmpty()) {
					return Collections.EMPTY_SET;
				}
				// 查询结果处理
				for (T entity : current) {
					// 跳过已经被移除的实体
					if (removing.contains(entity.getId())) {
						continue;
					}
					entity = entityHolder.putIfAbsentEntity(entity);
					result.put(entity.getId(), entity);
				}
			} finally {
				removingLock.unlock();
			}
			
			saveIndexValueMap(idx, result);
		} finally {
			lock.unlock();
			indexLocks.remove(idx);
		}

		return Collections.unmodifiableCollection(result.values());
	}

	/**
	 * 锁顺序说明: indexLock > removingLock.readLock
	 */
	@Override
	public T get(IndexValue idx, PK id) {
		uninitializeThrowException();

		if (!config.hasIndexField(idx.getName())) {
			throw new StateException("实体[" + entityClz.getName() + "]没有索引属性域[" + idx.getName() + "]");
		}

		// 已经被删除的元素直接返回
		ReadLock rLock = removingLock.readLock();
		rLock.lock();
		try {
			if (removing.contains(id)) {
				return null;
			}
		} finally {
			rLock.unlock();
		}

		// 尝试从缓存中获取
		ConcurrentHashMap<PK, T> result = getIndexValueMap(idx);
		if (result != null) {
			return result.get(id);
		}

		// 锁定索引值:抑制索引值并发写
		Lock lock = getIndexValueLock(idx);
		lock.lock();
		try {
			result = getIndexValueMap(idx);
			if (result != null) {
				return result.get(id);
			}

			result = createIndexValueMap(idx, false);

			// 锁定被移除主键集合:避免读取到脏数据
			rLock.lock();
			try {
				// 执行查询
				List<T> current = querier.list(entityClz, config.getIndexQuery(idx.getName()), idx.getValue());
				if (current.isEmpty()) {
					return null;
				}
				// 查询结果处理
				for (T entity : current) {
					// 跳过已经被移除的实体
					if (removing.contains(entity.getId())) {
						continue;
					}
					// 锁定实体缓存集合:保证实例的唯一性
					entity = entityHolder.putIfAbsentEntity(entity);
					result.put(entity.getId(), entity);
				}
			} finally {
				rLock.unlock();
			}
			
			saveIndexValueMap(idx, result);
		} finally {
			lock.unlock();
			indexLocks.remove(idx);
		}

		return result.get(id);
	}

	/**
	 * 锁顺序说明: pkLock > indexLock > removingLock.readLock
	 */
	@Override
	public T getOrCreate(IndexValue idx, PK id, EntityBuilder<PK, T> builder) {
		uninitializeThrowException();

		if (!config.hasIndexField(idx.getName())) {
			throw new StateException("实体[" + entityClz.getName() + "]没有索引属性域[" + idx.getName() + "]");
		}

		// 尝试从缓存中获取
		ConcurrentHashMap<PK, T> result = getIndexValueMap(idx);
		if (result == null) {
			// 锁定索引值:抑制索引值并发写
			Lock lock = getIndexValueLock(idx);
			lock.lock();
			try {
				result = getIndexValueMap(idx);
				if (result == null) {
					// 初始化索引
					result = createIndexValueMap(idx, false);

					// 锁定被移除主键集合:避免读取到脏数据
					ReadLock rLock = removingLock.readLock();
					rLock.lock();
					try {
						// 执行查询
						List<T> current = querier.list(entityClz, config.getIndexQuery(idx.getName()), idx.getValue());
						// 查询结果处理
						for (T entity : current) {
							// 跳过已经被移除的实体
							if (removing.contains(entity.getId())) {
								continue;
							}
							// 锁定实体缓存集合:保证实例的唯一性
							entity = entityHolder.putIfAbsentEntity(entity);
							result.put(entity.getId(), entity);
						}
					} finally {
						rLock.unlock();
					}
					
					saveIndexValueMap(idx, result);
				}
			} finally {
				lock.unlock();
				indexLocks.remove(idx);
			}
		}

		// 缓存中存在
		T entity = result.get(id);
		if (entity != null) {
			return entity;
		}

		// 不存在， 创建
		entity = builder.newInstance(id);
		
		// 获取当前的主键锁:抑制并发创建
		Lock pkLock = getPkLock(id);
		pkLock.lock();
		try {
			T prev = entityHolder.getPrevEntity(entity);
			if (prev != null) {
				return prev;
			}
			// 数据库检查
			T current = (T) accessor.load(entityClz, id);
			if (current != null) {
				throw new InvaildEntityException("实体主键[" + id + "]重复");
			}
			// 异步持久化
			persister.put(Element.saveOf(entity));
			// 添加实体缓存并进行增强
			entity = entityHolder.putIfAbsentEntity(entity);
			// 维护索引数据
			for (Entry<String, Object> entry : config.getIndexValues(entity).entrySet()) {
				// 锁定索引值:抑制索引值并发写
				IndexValue entryIdx = IndexValue.valueOf(entry.getKey(), entry.getValue());
				Lock idxLock = getIndexValueLock(entryIdx);
				idxLock.lock();
				try {
					ConcurrentHashMap<PK, T> currents = getIndexValueMap(entryIdx);
					if (currents == null) {
						currents = createIndexValueMap(entryIdx, false);
						// 锁定被移除主键集合:避免读取到脏数据
						ReadLock rLock = removingLock.readLock();
						rLock.lock();
						try {
							// 执行查询
							List<T> queryResult = querier.list(entityClz, config.getIndexQuery(entryIdx.getName()),
									entryIdx.getValue());
							// 处理查询结果
							for (T queryEntity : queryResult) {
								// 跳过已经被移除的实体
								if (removing.contains(queryEntity.getId())) {
									continue;
								}
								// 锁定实体缓存集合:保证实例的唯一性
								queryEntity = entityHolder.putIfAbsentEntity(queryEntity);
								currents.put(queryEntity.getId(), queryEntity);
							}
						} finally {
							rLock.unlock();
						}
						saveIndexValueMap(entryIdx, currents);
					}
					// 添加实体到索引集合
					currents.put(id, entity);
				} finally {
					idxLock.unlock();
					indexLocks.remove(entryIdx);
				}
			}
		} finally {
			pkLock.unlock();
			pkLocks.remove(id);
		}

		return entity;
	}

	/**
	 * 锁顺序说明: pkLock > indexLock > removingLock.readLock
	 */
	@Override
	public T create(T entity) {
		uninitializeThrowException();

		if (entity.getId() == null) {
			throw new InvaildEntityException("新创建的实体必须指定主键");
		}
		PK id = entity.getId();

		// 获取当前的主键锁:抑制并发创建
		Lock lock = getPkLock(id);
		lock.lock();
		try {
			T prev = entityHolder.getPrevEntity(entity);
			if (prev != null) {
				throw new InvaildEntityException("实体主键[" + entity.getId() + "]重复");
			}
			// 数据库检查
			T current = (T) accessor.load(entityClz, id);
			if (current != null) {
				throw new InvaildEntityException("实体主键[" + entity.getId() + "]重复");
			}
			// 异步持久化
			persister.put(Element.saveOf(entity));
			// 添加实体缓存并进行增强
			entity = entityHolder.putIfAbsentEntity(entity);
			// 维护索引数据
			for (Entry<String, Object> entry : config.getIndexValues(entity).entrySet()) {
				// 锁定索引值:抑制索引值并发写
				IndexValue idx = IndexValue.valueOf(entry.getKey(), entry.getValue());
				Lock idxLock = getIndexValueLock(idx);
				idxLock.lock();
				try {
					ConcurrentHashMap<PK, T> currents = getIndexValueMap(idx);
					if (currents == null) {
						currents = createIndexValueMap(idx, false);
						// 锁定被移除主键集合:避免读取到脏数据
						ReadLock rLock = removingLock.readLock();
						rLock.lock();
						try {
							// 执行查询
							List<T> queryResult = querier.list(entityClz, config.getIndexQuery(idx.getName()),
									idx.getValue());
							// 处理查询结果
							for (T queryEntity : queryResult) {
								// 跳过已经被移除的实体
								if (removing.contains(queryEntity.getId())) {
									continue;
								}
								// 锁定实体缓存集合:保证实例的唯一性
								queryEntity = entityHolder.putIfAbsentEntity(queryEntity);
								currents.put(queryEntity.getId(), queryEntity);
							}
						} finally {
							rLock.unlock();
						}
						
						saveIndexValueMap(idx, currents);
					}
					// 添加实体到索引集合
					currents.put(id, entity);
				} finally {
					idxLock.unlock();
					indexLocks.remove(idx);
				}
			}
			return entity;
		} finally {
			lock.unlock();
			pkLocks.remove(id);
		}
	}

	/**
	 * 锁顺序说明<br/>
	 * 加载:pkLock > removingLock
	 */
	@Override
	public void remove(T entity) {
		uninitializeThrowException();

		PK id = entity.getId();

		ReadLock rLock = removingLock.readLock();
		WriteLock wLock = removingLock.writeLock();

		rLock.lock();
		try {
			// 检查是否已经在删除中
			if (removing.contains(id)) {
				return;
			}
		} finally {
			rLock.unlock();
		}

		// 获取当前的主键写锁
		Lock lock = getPkLock(id);
		lock.lock();
		try {
			rLock.lock();
			try {
				// 检查是否已经在删除中
				if (removing.contains(id)) {
					return;
				}
			} finally {
				rLock.unlock();
			}

			wLock.lock();
			try {
				// 添加到删除中主键集合
				removing.add(id);
			} finally {
				wLock.unlock();
			}
			// 添加到更新队列
			persister.put(Element.removeOf(id, entityClz));
			// 维护索引数据
			for (Entry<String, Object> entry : config.getIndexValues(entity).entrySet()) {
				IndexValue idx = IndexValue.valueOf(entry.getKey(), entry.getValue());
				ConcurrentHashMap<PK, T> currents = getIndexValueMap(idx);
				if (currents == null) {
					continue;
				}
				currents.remove(id);
			}
		} finally {
			lock.unlock();
			pkLocks.remove(id);
		}
	}
	
	@Override
	public void clear(IndexValue idx) {
		// 锁定索引值:抑制索引值并发写
		Lock lock = getIndexValueLock(idx);
		lock.lock();
		try {
			cache.get(idx.getName()).remove(idx.getValue());
		} finally {
			lock.unlock();
			indexLocks.remove(idx);
		}
	}
	
	@Override
	public void truncate() {
		for (IndexValue indexValue : this.indexLocks.keySet()) {
			this.clear(indexValue);
		}
	}

	// 实体增强相关的方法

	@Override
	public void writeBack(PK id, T entity) {
		uninitializeThrowException();

		// 检查是否已经在删除中
		Lock rLock = removingLock.readLock();
		rLock.lock();
		try {
			if (removing.contains(id)) {
				if (logger.isWarnEnabled()) {
					logger.warn("尝试更新处于待删除状态的实体[{}:{}],操作将被忽略", entityClz.getSimpleName(), id);
				}
				return;
			}
		} finally {
			rLock.unlock();
		}

		// 异步回写
		persister.put(Element.updateOf(entity));
	}

	@Override
	public void changeIndexValue(String name, T entity, Object prev) {
		uninitializeThrowException();

		// 获取实体信息
		PK id = entity.getId();
		Object current = config.getIndexValues(entity).get(name);
		
		// 维护原索引值区域
		IndexValue idx = IndexValue.valueOf(name, prev);
		// 锁定索引值:抑制索引值并发写
		Lock lock = getIndexValueLock(idx);
		lock.lock();
		try {
			ConcurrentHashMap<PK, T> result = getIndexValueMap(idx);
			if (result == null) {
				// 索引值缓存没有命中
				result = createIndexValueMap(idx, false);
				// 锁定被移除主键集合:避免读取到脏数据
				ReadLock removingLock = this.removingLock.readLock();
				removingLock.lock();
				try {
					// 执行查询
					List<T> queryResult = querier.list(entityClz, config.getIndexQuery(idx.getName()), idx.getValue());
					// 查询结果处理
					for (T queryEntity : queryResult) {
						// 跳过已经被移除的实体
						if (removing.contains(queryEntity.getId())) {
							continue;
						}
						queryEntity = entityHolder.putIfAbsentEntity(queryEntity);
						result.put(queryEntity.getId(), queryEntity);
					}
				} finally {
					removingLock.unlock();
				}
				
				saveIndexValueMap(idx, result);
			}
			result.remove(id);
		} finally {
			lock.unlock();
			indexLocks.remove(idx);
		}

		// 维护新的索引值区域
		idx = IndexValue.valueOf(name, current);
		// 锁定索引值:抑制索引值并发写
		lock = getIndexValueLock(idx);
		lock.lock();
		try {
			ConcurrentHashMap<PK, T> result = getIndexValueMap(idx);
			if (result == null) {
				// 索引值缓存没有命中
				result = createIndexValueMap(idx, false);
				// 锁定被移除主键集合:避免读取到脏数据
				ReadLock removingLock = this.removingLock.readLock();
				removingLock.lock();
				try {
					// 执行查询
					List<T> queryResult = querier.list(entityClz, config.getIndexQuery(idx.getName()), idx.getValue());
					// 查询结果处理
					for (T queryEntity : queryResult) {
						// 跳过已经被移除的实体
						if (removing.contains(queryEntity.getId())) {
							continue;
						}
						queryEntity = entityHolder.putIfAbsentEntity(queryEntity);
						result.put(queryEntity.getId(), queryEntity);
					}
				} finally {
					removingLock.unlock();
				}
				
				saveIndexValueMap(idx, result);
			}
			
			// 锁定被移除主键集合:避免读取到脏数据
			ReadLock removingLock = this.removingLock.readLock();
			removingLock.lock();
			try {
				if (!removing.contains(id)) {
					result.put(id, entity);
				}
			} finally {
				removingLock.unlock();
			}
		} finally {
			lock.unlock();
			indexLocks.remove(idx);
		}
	}

	@Override
	public CachedEntityConfig getEntityConfig() {
		return config;
	}

	@Override
	public Persister getPersister() {
		return persister;
	}

	// 内部方法

	/**
	 * 创建指定索引值所对应的实体Map
	 * @param idx 索引值
	 * @return
	 */
	private ConcurrentHashMap<PK, T> createIndexValueMap(IndexValue idx, boolean save) {
		ConcurrentHashMap<PK, T> result = new ConcurrentHashMap<PK, T>();
		if (!save) {
			return result;
		}
		ConcurrentHashMap<PK, T> prev = cache.get(idx.getName()).putIfAbsent(idx.getValue(), result);
		return prev == null ? result : prev;
	}
	
	private void saveIndexValueMap(IndexValue idx, ConcurrentHashMap<PK, T> values) {
		cache.get(idx.getName()).putIfAbsent(idx.getValue(), values);
	}

	/**
	 * 获取指定索引值所对应的实体Map
	 * @param idx 索引值
	 * @return 有可能返回null
	 */
	private ConcurrentHashMap<PK, T> getIndexValueMap(IndexValue idx) {
		return cache.get(idx.getName()).get(idx.getValue());
	}
	
	/**
	 * 获取索引值区域的操作锁
	 * @param idx 索引值
	 * @return
	 */
	private Lock getIndexValueLock(IndexValue idx) {
		ReentrantLock result = indexLocks.get(idx);
		if (result == null) {
			result = new ReentrantLock();
			ReentrantLock prevLock = indexLocks.putIfAbsent(idx, result);
			result = prevLock != null ? prevLock : result;
		}
		return result;
	}

	/**
	 * 获取指定主键的操作锁
	 * @param id 主键值
	 * @return
	 */
	private Lock getPkLock(PK id) {
		ReentrantLock result = pkLocks.get(id);
		if (result == null) {
			result = new ReentrantLock();
			ReentrantLock prevLock = pkLocks.putIfAbsent(id, result);
			result = prevLock != null ? prevLock : result;
		}
		return result;
	}

	/** 检查是否已经完成初始化(未完成时会抛异常) */
	private void uninitializeThrowException() {
		if (!initialize) {
			throw new StateException("未完成初始化");
		}
	}

	/** 初始化可直接获取的域属性 */
	private void initFileds(final CachedEntityConfig config, Persister persister, Accessor accessor, Querier querier) {
		Cached cached = config.getCached();
		// 初始化属性域
		this.config = config;
		this.accessor = accessor;
		this.querier = querier;
		this.entityClz = (Class<T>) config.getClz();
		this.persister = persister;
		this.persister.addListener(entityClz, new AbstractListener() {
			private void remove(PK id) {
				Lock lock = removingLock.writeLock();
				lock.lock();
				try {
					removing.remove(id);
				} finally {
					lock.unlock();
				}
			}
			@Override
			protected void onRemoveSuccess(Serializable id) {
				remove((PK) id);
			}
			@Override
			protected void onRemoveError(Serializable id, RuntimeException ex) {
				remove((PK) id);
			}
		});
		// 初始化类持有者
		if (cached.enhanced()) {
			JavassistRegionEnhancer enhancer = new JavassistRegionEnhancer();
			enhancer.initialize(this);
			this.entityHolder = new WeakEntityHolder<T>(enhancer);
		} else {
			this.entityHolder = new WeakEntityHolder<T>(null);
		}
		// 初始化实体缓存空间
		this.cache = new ConcurrentHashMap<String, ConcurrentMap<Object,ConcurrentHashMap<PK,T>>>(config.getIndexNames().size());
		switch (cached.type()) {
		case LRU:
			for (String name : config.getIndexNames()) {
				Builder<Object, ConcurrentHashMap<PK, T>> builder = new Builder<Object, ConcurrentHashMap<PK, T>>()
						.initialCapacity(cached.initialCapacity()) 			// 设置调整大小因子
						.maximumWeightedCapacity(config.getCachedSize()) 	// 设置最大元素数量(可能会临时超出该数值)
						.concurrencyLevel(cached.concurrencyLevel()); 		// 设置并发更新线程数预计值
				this.cache.put(name, builder.build());
			}
			break;
		case MANUAL:
			for (String name : config.getIndexNames()) {
				this.cache.put(name, new ConcurrentHashMap<Object, ConcurrentHashMap<PK,T>>(cached.initialCapacity(), (float) 0.75, cached.concurrencyLevel()));
			}
			break;
		default:
			throw new ConfigurationException("未支持的缓存管理类型[" + cached.type() + "]");
		}
	}

	/** 初始化缓存配置 */
	private void initCaches(CachedEntityConfig config, Querier querier) {
		InitialConfig initial = config.getInitialConfig();
		if (initial == null) {
			return;
		}
		// 获取要初始化的实体列表
		List<T> entities = null;
		switch (initial.type()) {
		case ALL:
			entities = querier.all(entityClz);
			break;
		case QUERY:
			entities = querier.list(entityClz, initial.query());
			break;
		default:
			throw new ConfigurationException("无法按配置[" + initial + "]初始化实体[" + this.entityClz.getName() + "]的缓存");
		}
		// 初始化缓存数据
		for (T entity : entities) {
			PK pk = entity.getId();
			Map<String, Object> indexMap = config.getIndexValues(entity);
			entity = entityHolder.putIfAbsentEntity(entity);
			for (Entry<String, Object> entry : indexMap.entrySet()) {
				String name = entry.getKey();
				Object value = entry.getValue();
				IndexValue idx = IndexValue.valueOf(name, value);
				ConcurrentHashMap<PK, T> indexCache = getIndexValueMap(idx);
				if (indexCache == null) {
					indexCache = createIndexValueMap(idx, true);
				}
				indexCache.put(pk, entity);
			}
		}
	}

}
