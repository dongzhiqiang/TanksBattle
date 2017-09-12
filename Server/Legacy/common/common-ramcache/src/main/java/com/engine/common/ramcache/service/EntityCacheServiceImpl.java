package com.engine.common.ramcache.service;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;
import java.util.concurrent.locks.ReentrantReadWriteLock.ReadLock;
import java.util.concurrent.locks.ReentrantReadWriteLock.WriteLock;

import org.apache.commons.collections.bidimap.DualHashBidiMap;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.CachedEntityConfig;
import com.engine.common.ramcache.anno.InitialConfig;
import com.engine.common.ramcache.enhance.Enhancer;
import com.engine.common.ramcache.enhance.JavassistEntityEnhancer;
import com.engine.common.ramcache.exception.ConfigurationException;
import com.engine.common.ramcache.exception.InvaildEntityException;
import com.engine.common.ramcache.exception.StateException;
import com.engine.common.ramcache.exception.UniqueFieldException;
import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.ramcache.orm.Querier;
import com.engine.common.ramcache.persist.AbstractListener;
import com.engine.common.ramcache.persist.Element;
import com.engine.common.ramcache.persist.Persister;
import com.engine.common.utils.collection.ConcurrentHashSet;
import com.googlecode.concurrentlinkedhashmap.ConcurrentLinkedHashMap.Builder;
import com.googlecode.concurrentlinkedhashmap.EvictionListener;

public class EntityCacheServiceImpl<PK extends Comparable<PK> & Serializable, T extends IEntity<PK>> implements
		EntityCacheService<PK, T>, CacheFinder<PK, T>, EntityEnhanceService<PK, T> {

	private static final Logger logger = LoggerFactory.getLogger(EntityCacheServiceImpl.class);

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
	/** 持久化缓存器 */
	private Persister persister;
	
	/** 实体缓存 */
	private ConcurrentMap<PK, T> cache;
	/** 增强器 */
	private Enhancer enhancer;

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

	/** 实体操作读写锁 */
	private ConcurrentHashMap<PK, ReentrantLock> locks = new ConcurrentHashMap<PK, ReentrantLock>();
	/** 正在被移除的实体的主键集合 */
	private ConcurrentHashSet<PK> removing = new ConcurrentHashSet<PK>();
	
	/** 唯一键缓存(key:唯一键名 value:{key:唯一键值, value:PK}) */
	private HashMap<String, DualHashBidiMap> uniques;

	@Override
	public T load(PK id) {
		uninitializeThrowException();
		// 先判断主键是否有效
		if (removing.contains(id)) {
			return null;
		}
		// 尝试从缓存中获取
		T current = (T) cache.get(id);
		if (current != null) {
			return current;
		}

		// 获取主键锁,抑制并发操作
		Lock lock = lockPkLock(id);
		try {
			// 先判断主键是否有效
			if (removing.contains(id)) {
				return null;
			}
			// 尝试从缓存中获取
			current = (T) cache.get(id);
			if (current != null) {
				return current;
			}
			current = (T) accessor.load(entityClz, id);
			if (current != null) {
				if (config.hasUniqueField()) {
					// 添加唯一属性缓存信息
					Map<String, Object> uniqueValues = config.getUniqueValues(current);
					for (Entry<String, Object> entry : uniqueValues.entrySet()) {
						addUniqueValue(entry.getKey(), entry.getValue(), id);
					}
				}
				// 实例增强
				if (enhancer != null) {
					current = enhancer.transform(current);
				}
				cache.put(id, current);
			}
			return current;
		} finally {
			// 释放主键锁
			releasePkLock(id, lock);
		}
	}

	/** 释放主键锁 */
	private void releasePkLock(PK id, Lock lock) {
		lock.unlock();
		locks.remove(id);
	}

	/** 获取主键锁对象 */
	private Lock lockPkLock(PK id) {
		// 获取当前的主键写锁
		ReentrantLock lock = locks.get(id);
		if (lock == null) {
			lock = new ReentrantLock();
			ReentrantLock prevLock = locks.putIfAbsent(id, lock);
			lock = prevLock != null ? prevLock : lock;
		}
		lock.lock();
		return lock;
	}

	@Override
	public T loadOrCreate(PK id, EntityBuilder<PK, T> builder) {
		uninitializeThrowException();
		
		// 先判断主键是否有效
		if (!removing.contains(id)) {
			// 尝试从缓存中获取
			T current = (T) cache.get(id);
			if (current != null) {
				return current;
			}
		}
		// 抑制并发操作
		T current = null;
		// 获取主键锁,抑制并发操作
		Lock lock = lockPkLock(id);
		try {
			// 尝试从缓存中获取
			current = (T) cache.get(id);
			if (current != null) {
				return current;
			}
			
			current = (T) accessor.load(entityClz, id);
			boolean flag = removing.contains(id);
			if (current == null || flag) {
				// 创建实例
				current = builder.newInstance(id);
				
				if (current == null) {
					throw new InvaildEntityException("创建主键为[" + id + "]的实体[" + entityClz.getName() + "]时返回null");
				}
				if (current.getId() == null) {
					throw new InvaildEntityException("创建主键为[" + id + "]的实体[" + entityClz.getName() + "]时实体的主键值为null");
				}
				
				if (config.hasUniqueField()) {
					// 检查唯一属性值是否合法
					Map<String, Object> uniqueValues = config.getUniqueValues(current);
					
					boolean rollback = false;	// 是否需要回滚的标记
					List<Entry<String, Object>> temp = new ArrayList<Entry<String, Object>>(uniqueValues.size());
					// 迭代每一个唯一属性域
					for (Entry<String, Object> entry : uniqueValues.entrySet()) {
						
						String uniqueName = entry.getKey();		// 唯一属性域名
						Object uniqueValue = entry.getValue();	// 唯一属性域值
						WriteLock uniqueLock = config.getUniqueWriteLock(uniqueName);
						
						uniqueLock.lock();
						try {
							// 检查缓存数据
							DualHashBidiMap unique = uniques.get(uniqueName);
							if (unique.containsKey(uniqueValue)) {
								rollback = true;
								break;
							}
							// 检查数据库数据
							T chkEntity = querier.unique(entityClz, config.getUniqueQuery(uniqueName), uniqueValue);
							if (chkEntity != null) {
								rollback = true;
								break;
							}
							// 更新缓存数据
							@SuppressWarnings("unchecked")
							PK prev = (PK) unique.put(uniqueValue, id);
							if (prev != null) {
								logger.error("实体[{}]的唯一键值[{}]异常:原主键[{}],当前主键[{}]",
										new Object[] { entityClz.getName(), uniqueValue, prev, id });
							}
							
							// 添加回滚数据以备回滚需要
							temp.add(entry);
						} finally {
							uniqueLock.unlock();
						}
					}
					
					if (rollback) {
						// 回滚缓存数据处理
						for (Entry<String, Object> entry : temp) {
							removeUniqueValue(entry.getKey(), entry.getValue());
						}
						throw new UniqueFieldException();
					}
				}
				
				if (flag) {
					removing.remove(id);
				}
				// 异步持久化
				persister.put(Element.saveOf(current));
			} else {
				if (config.hasUniqueField()) {
					// 添加唯一属性缓存信息
					Map<String, Object> uniqueValues = config.getUniqueValues(current);
					for (Entry<String, Object> entry : uniqueValues.entrySet()) {
						addUniqueValue(entry.getKey(), entry.getValue(), id);
					}
				}
			}
			// 实例增强
			if (enhancer != null) {
				current = enhancer.transform(current);
			}
			cache.put(id, current);
			return current;
		} finally {
			// 释放主键锁
			releasePkLock(id, lock);
		}
	}

	@Override
	public void writeBack(PK id, T entity) {
		uninitializeThrowException();
		
		// 检查是否已经在删除中
		if (removing.contains(id)) {
			if (logger.isWarnEnabled()) {
				logger.warn("尝试更新处于待删除状态的实体[{}:{}],操作将被忽略", entityClz.getSimpleName(), id);
			}
			return;
		}

		// 异步回写
		persister.put(Element.updateOf(entity));
	}

	@Override
	public T remove(PK id) {
		uninitializeThrowException();
		// 检查是否已经在删除中
		if (removing.contains(id)) {
			return null;
		}

		// 获取主键锁,抑制并发操作
		Lock lock = lockPkLock(id);
		try {
			// 检查是否已经在删除中
			if (removing.contains(id)) {
				return null;
			}
			// 添加到删除中主键集合
			removing.add(id);
			// 从缓存中移除
			T current = cache.remove(id);
			if (current != null && config.hasUniqueField()) {
				// 关联清理唯一属性值
				Map<String, Object> uniqueValues = config.getUniqueValues(current);
				for (Entry<String, Object> entry : uniqueValues.entrySet()) {
					removeUniqueValue(entry.getKey(), entry.getValue());
				}
			}
			// 添加到更新队列
			persister.put(Element.removeOf(id, entityClz));
			return current;
		} finally {
			// 释放主键锁
			releasePkLock(id, lock);
		}
	}
	
	@Override
	public void clear(PK id) {
		// 尝试从缓存中获取
		T current = (T) cache.get(id);
		if (current == null) {
			return;	// 缓存中不存在不需要处理
		}
		
		Lock lock = lockPkLock(id);
		try {
			// 从缓存中移除
			current = cache.remove(id);
			if (current != null && config.hasUniqueField()) {
				// 关联清理唯一属性值
				Map<String, Object> uniqueValues = config.getUniqueValues(current);
				for (Entry<String, Object> entry : uniqueValues.entrySet()) {
					removeUniqueValue(entry.getKey(), entry.getValue());
				}
			}
		} finally {
			releasePkLock(id, lock);
		}
	}
	
	@Override
	public void truncate() {
		for(PK key:cache.keySet()) {
			this.clear(key);
		}
	}

	@SuppressWarnings("unchecked")
	@Override
	public T unique(String name, Object value) {
		uninitializeThrowException();
		
		if (!config.hasUniqueField()) {
			throw new UniqueFieldException("实体[" + entityClz.getName() + "]没有唯一属性域");
		}
		
		// 尝试从唯一属性值缓存中取得主键值
		PK id = null;
		ReadLock readLock = config.getUniqueReadLock(name);
		readLock.lock();
		try {
			DualHashBidiMap unique = uniques.get(name);
			id = (PK) unique.get(value);
		} finally {
			readLock.unlock();
		}
		if (id != null) {
			return load(id);
		}
		
		// 从数据库加载实体
		T current = querier.unique(entityClz, config.getUniqueQuery(name), value);
		if (current == null) {
			return current;
		}
		
		id = current.getId();
		// 获取主键锁,抑制并发操作
		Lock lock = lockPkLock(id);
		try {
			// 先判断主键是否有效
			if (removing.contains(id)) {
				return null;
			}
			// 尝试从缓存中获取
			T prev = (T) cache.get(id);
			if (prev != null) {
				return prev;
			}
			
			if (config.hasUniqueField()) {
				// 添加唯一属性缓存信息
				Map<String, Object> uniqueValues = config.getUniqueValues(current);
				for (Entry<String, Object> entry : uniqueValues.entrySet()) {
					addUniqueValue(entry.getKey(), entry.getValue(), id);
				}
			}
			// 实例增强
			if (enhancer != null) {
				current = enhancer.transform(current);
			}
			cache.put(id, current);
		} finally {
			// 释放主键锁
			releasePkLock(id, lock);
		}
		return current;
	}

	// 实体增强相关的方法
	
	@Override
	public boolean hasUniqueValue(String name, Object value) {
		DualHashBidiMap unique = uniques.get(name);
		// 已经取得写锁的处理
		if (config.getUniqueWriteLock(name).isHeldByCurrentThread()) {
			if (unique.containsKey(value)) {
				return true;
			}
			// 从数据库加载实体
			T current = querier.unique(entityClz, config.getUniqueQuery(name), value);
			if (current == null) {
				return false;
			}
			return true;
		}
		
		// 没有获得写锁的处理
		WriteLock lock = config.getUniqueWriteLock(name);
		lock.lock();
		try {
			if (unique.containsKey(value)) {
				return true;
			}
			// 从数据库加载实体
			T current = querier.unique(entityClz, config.getUniqueQuery(name), value);
			if (current == null) {
				return false;
			}
			return true;
		} finally {
			lock.unlock();
		}
	}

	/**
	 * 替换指定实体标识对应的唯一属性缓存值
	 * @param id 主键
	 * @param name 属性域名
	 * @param value 最新值
	 */
	@Override
	public void replaceUniqueValue(PK id, String name, Object value) {
		if (!config.getUniqueWriteLock(name).isHeldByCurrentThread()) {
			throw new StateException("非法执行该方法");
		}
		DualHashBidiMap unique = uniques.get(name);
		unique.removeValue(id);
		unique.put(value, id);
	}

	// 查询相关的方法实现
	
	@Override
	public CacheFinder<PK, T> getFinder() {
		uninitializeThrowException();
		return this;
	}

	@Override
	public Set<T> find(Filter<T> filter) {
		uninitializeThrowException();
		HashSet<T> result = new HashSet<T>();
		for (T entity : cache.values()) {
			if (filter.isExclude(entity)) {
				continue;
			}
			result.add(entity);
		}
		return result;
	}

	@Override
	public List<T> sort(Comparator<T> comparator) {
		uninitializeThrowException();
		ArrayList<T> result = new ArrayList<T>(cache.values());
		Collections.sort(result, comparator);
		return result;
	}

	@Override
	public List<T> find(Filter<T> filter, Comparator<T> comparator) {
		uninitializeThrowException();
		ArrayList<T> result = new ArrayList<T>();
		for (T entity : cache.values()) {
			if (filter.isExclude(entity)) {
				continue;
			}
			result.add(entity);
		}
		Collections.sort(result, comparator);
		return result;
	}

	@Override
	public Set<T> all() {
		uninitializeThrowException();
		HashSet<T> result = new HashSet<T>(cache.values());
		return result;
	}

	// 管理相关的方法实现
	
	@Override
	public CachedEntityConfig getEntityConfig() {
		uninitializeThrowException();
		return config;
	}

	@Override
	public Persister getPersister() {
		return persister;
	}

	// 内部方法
	
	/**
	 * 添加指定唯一属性值与主键的对应关系(属性名的写锁保护)
	 * @param name 唯一属性名
	 * @param value 唯一属性值
	 * @param id 主键值
	 */
	private void addUniqueValue(String name, Object value, PK id) {
		DualHashBidiMap unique = uniques.get(name);
		WriteLock uniqueLock = config.getUniqueWriteLock(name);
		uniqueLock.lock();
		try {
			@SuppressWarnings("unchecked")
			PK prev = (PK) unique.put(value, id);
			if (prev != null) {
				logger.error("实体[{}]的唯一键值[{}]异常:原主键[{}],当前主键[{}]",
						new Object[] { entityClz.getName(), value, prev, id });
			}
		} finally {
			uniqueLock.unlock();
		}
	}

	/**
	 * 移除指定唯一属性值(属性名的写锁保护)
	 * @param name 唯一属性名
	 * @param value 唯一属性值
	 */
	private void removeUniqueValue(String name, Object value) {
		DualHashBidiMap unique = uniques.get(name);
		WriteLock uniqueLock = config.getUniqueWriteLock(name);
		uniqueLock.lock();
		try {
			unique.remove(value);
		} finally {
			uniqueLock.unlock();
		}
	}

	/** 检查是否已经完成初始化(未完成时会抛异常) */
	private void uninitializeThrowException() {
		if (!initialize) {
			throw new StateException("未完成初始化");
		}
	}

	/** 初始化可直接获取的域属性 */
	@SuppressWarnings("unchecked")
	private void initFileds(final CachedEntityConfig config, Persister persister, Accessor accessor, Querier querier) {
		Cached cached = config.getCached();
		// 初始化属性域
		this.config = config;
		this.accessor = accessor;
		this.querier = querier;
		this.entityClz = (Class<T>) config.getClz();
		this.persister = persister;
		this.persister.addListener(entityClz, new AbstractListener() {
			@Override
			protected void onRemoveSuccess(Serializable id) {
				removing.remove(id);
			}
			@Override
			protected void onRemoveError(Serializable id, RuntimeException ex) {
				removing.remove(id);
			}
		});
		// 初始化类增强器
		if (cached.enhanced()) {
			JavassistEntityEnhancer enhancer = new JavassistEntityEnhancer();
			enhancer.initialize(this);
			this.enhancer = enhancer;
		}
		// 初始化唯一键信息
		if (config.hasUniqueField()) {
			this.uniques = config.buildUniqueCache();
		}
		// 初始化实体缓存空间
		switch (cached.type()) {
		case LRU:
			Builder<PK, T> builder = new Builder<PK, T>()
				.initialCapacity(cached.initialCapacity()) 			// 设置调整大小因子
				.maximumWeightedCapacity(config.getCachedSize())	// 设置最大元素数量(可能会临时超出该数值)
				.concurrencyLevel(cached.concurrencyLevel());		// 设置并发更新线程数预计值
			if (config.hasUniqueField()) {
				// 如果有唯一键值，则添加过期内容清理监听器
				builder.listener(new EvictionListener<PK, T>() {
					public void onEviction(PK key, T value) {
						for (Entry<String, DualHashBidiMap> entry : uniques.entrySet()) {
							WriteLock lock = config.getUniqueWriteLock(entry.getKey());
							lock.lock();
							try {
								entry.getValue().removeValue(value.getId());
							} finally {
								lock.unlock();
							}
						}
					};
				});
			}
			this.cache = builder.build();
			break;
		case MANUAL:
			this.cache =  new ConcurrentHashMap<PK, T>(cached.initialCapacity(), (float) 0.75, cached.concurrencyLevel());
			break;
		default:
			throw new ConfigurationException("未支持的缓存管理类型[" + cached.type() + "]");
		}
	}

	/** 初始化缓存数据 */
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
			PK id = entity.getId();
			if (enhancer != null) {
				entity = enhancer.transform(entity);
			}
			cache.put(id, entity);
			
			if (config.hasUniqueField()) {
				Map<String, Object> uniqueValues = config.getUniqueValues(entity);
				// 更新唯一键索引
				for (Entry<String, Object> entry : uniqueValues.entrySet()) {
					DualHashBidiMap unique = uniques.get(entry.getKey());
					unique.put(entry.getValue(), id);
				}
			}
		}
	}

}
