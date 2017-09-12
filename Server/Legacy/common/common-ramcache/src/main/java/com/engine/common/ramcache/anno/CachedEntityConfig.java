package com.engine.common.ramcache.anno;

import java.io.Serializable;
import java.lang.reflect.Field;
import java.lang.reflect.Modifier;
import java.util.Collection;
import java.util.Collections;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Map.Entry;
import java.util.concurrent.locks.ReentrantReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock.ReadLock;
import java.util.concurrent.locks.ReentrantReadWriteLock.WriteLock;

import org.apache.commons.collections.bidimap.DualHashBidiMap;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.enhance.EnhancedEntity;
import com.engine.common.ramcache.exception.ConfigurationException;
import com.engine.common.ramcache.exception.StateException;
import com.engine.common.utils.reflect.ReflectionUtility;

/**
 * 缓存的实体配置信息对象
 * 
 */
@SuppressWarnings("rawtypes")
public class CachedEntityConfig implements Serializable {

	private static final long serialVersionUID = -6067788531240033388L;

	/**
	 * 检查一个类是否是有效的实体类<br/>
	 * @param clz
	 * @param constants 
	 * @return
	 * @throws ConfigurationException 配置非法时抛出
	 */
	public static boolean isVaild(Class<?> clz, Map<String, Integer> constants) {
		if (Modifier.isAbstract(clz.getModifiers())) {
			return false;
		}
		if (Modifier.isInterface(clz.getModifiers())) {
			return false;
		}
		if (!IEntity.class.isAssignableFrom(clz)) {
			return false;
		}
		if (!clz.isAnnotationPresent(Cached.class)) {
			return false;
		}
		Cached cached = clz.getAnnotation(Cached.class);
		if (!constants.containsKey(cached.size())) {
			throw new ConfigurationException("缓存实体[" + clz.getName() + "]要求的缓存数量定义[" + cached.size() + "]不存在");
		}
		switch (cached.unit()) {
		case ENTITY:
			if (ReflectionUtility.getDeclaredFieldsWith(clz, Idx.class).length > 0) {
				throw new ConfigurationException("缓存单位为[" + cached.unit() + "]的实体[" + clz.getName() + "]不支持索引属性配置");
			}
			break;
		case REGION:
			if (ReflectionUtility.getDeclaredFieldsWith(clz, Unique.class).length > 0) {
				throw new ConfigurationException("缓存单位为[" + cached.unit() + "]的实体[" + clz.getName() + "]不支持唯一值属性配置");
			}
			break;
		default:
			throw new ConfigurationException("实体[" + clz.getName() + "]使用了未支持的缓存单位[" + cached.unit() + "]配置");
		}
		return true;
	}

	/** 构造方法 */
	public static CachedEntityConfig valueOf(Class<? extends IEntity> clz, Map<String, Integer> constants) {
		CachedEntityConfig result = new CachedEntityConfig();
		result.clz = clz;
		result.cached = clz.getAnnotation(Cached.class);
		result.initialConfig = clz.getAnnotation(InitialConfig.class);
		result.cachedSize = constants.get(result.cached.size());
		
		// 初始化唯一属性域信息
		Field[] fields = ReflectionUtility.getDeclaredFieldsWith(clz, Unique.class);
		if (fields != null && fields.length > 0) {
			// 初始化存储空间
			HashMap<String, Unique> uniques = new HashMap<String, Unique>(fields.length);
			HashMap<String, Field> uniqueFields = new HashMap<String, Field>(fields.length);
			HashMap<String, ReentrantReadWriteLock> uniqueLocks = new HashMap<String, ReentrantReadWriteLock>(fields.length);
			
			for (Field field : fields) {
				Unique unique = field.getAnnotation(Unique.class);
				String name = field.getName();
				// 存储唯一属性域
				ReflectionUtility.makeAccessible(field);
				uniques.put(name, unique);
				uniqueFields.put(name, field);
				uniqueLocks.put(name, new ReentrantReadWriteLock());
			}
			
			// 保存唯一属性域信息
			result.uniques = uniques;
			result.uniqueFields = uniqueFields;
			result.uniqueLocks = uniqueLocks;
		}
		
		// 初始化索引属性域信息
		fields = ReflectionUtility.getDeclaredFieldsWith(clz, Idx.class);
		if (fields != null && fields.length > 0) {
			// 初始化存储空间
			HashMap<String, Idx> indexs = new HashMap<String, Idx>(fields.length);
			HashMap<String, Field> indexFields = new HashMap<String, Field>(fields.length);
			HashMap<String, ReentrantReadWriteLock> indexLocks = new HashMap<String, ReentrantReadWriteLock>(fields.length);
			
			for (Field field : fields) {
				Idx unique = field.getAnnotation(Idx.class);
				String name = field.getName();
				// 存储索引属性域
				ReflectionUtility.makeAccessible(field);
				indexs.put(name, unique);
				indexFields.put(name, field);
				indexLocks.put(name, new ReentrantReadWriteLock());
			}
			
			// 保存索引属性域信息
			result.indexs = indexs;
			result.indexFields = indexFields;
			result.indexLocks = indexLocks;
		}
		
		return result;
	}

	/** 实体类 */
	private Class<? extends IEntity> clz;
	/** 缓存配置 */
	private Cached cached;
	/** 初始化配置 */
	private InitialConfig initialConfig;
	/** 缓存数量 */
	private int cachedSize;

	/** 唯一属性域配置信息 */
	private transient Map<String, Unique> uniques;
	/** 唯一属性域 */
	private transient Map<String, Field> uniqueFields;
	/** 唯一属性域操作锁 */
	private transient Map<String, ReentrantReadWriteLock> uniqueLocks;
	
	/** 索引属性域配置信息 */
	private transient Map<String, Idx> indexs;
	/** 索引属性域 */
	private transient Map<String, Field> indexFields;
	/** 索引属性域操作锁 */
	private transient Map<String, ReentrantReadWriteLock> indexLocks;

	private CachedEntityConfig() {
	}
	
	// 索引值相关的方法
	
	/**
	 * 获取索引属性名集合
	 * @return
	 */
	@SuppressWarnings("unchecked")
	public Collection<String> getIndexNames() {
		if (indexs == null) {
			return Collections.EMPTY_SET;
		}
		
		HashSet<String> result = new HashSet<String>(indexs.size());
		for (String name : indexs.keySet()) {
			result.add(name);
		}
		return result;
	}

	/**
	 * 获取索引属性域的查询信息
	 * @param name 索引属性域名
	 * @return 查询名
	 */
	public String getIndexQuery(String name) {
		if (indexs == null) {
			throw new StateException("实体[" + clz.getName() + "]的索引属性名[" + name + "]无效");
		}
		Idx index = indexs.get(name);
		if (index == null) {
			throw new StateException("实体[" + clz.getName() + "]的索引属性名[" + name + "]无效");
		}
		return index.query();
	}

	/**
	 * 获取实体的索引键值
	 * @param entity
	 * @return
	 */
	public Map<String, Object> getIndexValues(IEntity entity) {
		if (indexFields == null) {
			throw new StateException("实体[" + clz.getName() + "]没有索引属性配置无法获取索引属性值");
		}
		
		if (entity instanceof EnhancedEntity) {
			entity = ((EnhancedEntity) entity).getEntity();
		}
		
		try {
			Map<String, Object> result = new HashMap<String, Object>(indexFields.size());
			for (Entry<String, Field> entry : indexFields.entrySet()) {
				Object value = entry.getValue().get(entity);
				result.put(entry.getKey(), value);
			}
			return result;
		} catch (Exception e) {
			throw new StateException("无法获取索引属性值:" + e.getMessage());
		}
	}

	/**
	 * 获取指定的索引属性值
	 * @param name 索引属性名
	 * @param entity 实体
	 * @return
	 */
	public Object getIndexValue(String name, IEntity entity) {
		Map<String, Object> values = getIndexValues(entity);
		if (values.containsKey(name)) {
			return values.get(name);
		}
		throw new StateException("索引属性[" + name + "]不存在");
	}

	/**
	 * 获取指定索引属性域的读锁
	 * @param name 索引属性名
	 * @return
	 */
	public ReadLock getIndexReadLock(String name) {
		ReentrantReadWriteLock lock = indexLocks.get(name);
		if (lock == null) {
			throw new StateException("实体[" + clz.getName() + "]的索引属性名[" + name + "]无效");
		}
		return lock.readLock();
	}

	/**
	 * 获取指定索引属性域的写锁
	 * @param name 索引属性名
	 * @return
	 */
	public WriteLock getIndexWriteLock(String name) {
		ReentrantReadWriteLock lock = indexLocks.get(name);
		if (lock == null) {
			throw new StateException("实体[" + clz.getName() + "]的索引属性名[" + name + "]无效");
		}
		return lock.writeLock();
	}

	/**
	 * 检查是否有索引属性域
	 * @param name 索引属性名
	 * @return
	 */
	public boolean hasIndexField(String name) {
		if (indexFields == null) {
			return false;
		}
		if (indexFields.containsKey(name)) {
			return true;
		}
		return false;
	}

	// 唯一属性值相关的方法
	
	/**
	 * 获取唯一属性域的查询信息
	 * @param name 唯一属性域名
	 * @return 查询名
	 */
	public String getUniqueQuery(String name) {
		if (uniques == null) {
			throw new StateException("实体[" + clz.getName() + "]的唯一属性名[" + name + "]无效");
		}
		Unique unique = uniques.get(name);
		if (unique == null) {
			throw new StateException("实体[" + clz.getName() + "]的唯一属性名[" + name + "]无效");
		}
		return unique.query();
	}
	
	/**
	 * 获取指定唯一属性域的读锁
	 * @param name 唯一属性域名
	 * @return
	 */
	public ReadLock getUniqueReadLock(String name) {
		ReentrantReadWriteLock lock = uniqueLocks.get(name);
		if (lock == null) {
			throw new StateException("实体[" + clz.getName() + "]的唯一属性名[" + name + "]无效");
		}
		return lock.readLock();
	}

	/**
	 * 获取指定唯一属性域的写锁
	 * @param name 唯一属性域名
	 * @return
	 */
	public WriteLock getUniqueWriteLock(String name) {
		ReentrantReadWriteLock lock = uniqueLocks.get(name);
		if (lock == null) {
			throw new StateException("实体[" + clz.getName() + "]的唯一属性名[" + name + "]无效");
		}
		return lock.writeLock();
	}

	/** 创建唯一属性缓存 */
	public HashMap<String, DualHashBidiMap> buildUniqueCache() {
		HashMap<String, DualHashBidiMap> result = new HashMap<String, DualHashBidiMap>(uniqueFields.size());
		for (String name : uniqueFields.keySet()) {
			DualHashBidiMap map = new DualHashBidiMap();
			result.put(name, map);
		}
		return result;
	}

	/**
	 * 获取实体的唯一键值
	 * @param entity
	 * @return
	 */
	public Map<String, Object> getUniqueValues(IEntity entity) {
		if (uniqueFields == null) {
			throw new StateException("实体[" + clz.getName() + "]没有唯一属性配置无法获取唯一属性值");
		}
		
		if (entity instanceof EnhancedEntity) {
			entity = ((EnhancedEntity) entity).getEntity();
		}
		
		try {
			Map<String, Object> result = new HashMap<String, Object>(uniqueFields.size());
			for (Entry<String, Field> entry : uniqueFields.entrySet()) {
				Object value = entry.getValue().get(entity);
				result.put(entry.getKey(), value);
			}
			return result;
		} catch (Exception e) {
			throw new StateException("无法获取唯一属性值:" + e.getMessage());
		}
	}

	/**
	 * 检查是否有唯一键属性域
	 * @return
	 */
	public boolean hasUniqueField() {
		if (uniqueFields == null) {
			return false;
		}
		return true;
	}

	// 其它方法
	
	/** 检查缓存单位是否是指定的单位 */
	public boolean cacheUnitIs(CacheUnit unit) {
		if (cached.unit() == unit) {
			return true;
		}
		return false;
	}

	// Getter and Setter ...
	
	/**
	 * 获取实体类
	 * @return
	 */
	public Class<? extends IEntity> getClz() {
		return clz;
	}

	/**
	 * 获取缓存配置信息
	 * @return
	 */
	public Cached getCached() {
		return cached;
	}

	/**
	 * 获取缓存初始化配置信息
	 * @return
	 */
	public InitialConfig getInitialConfig() {
		return initialConfig;
	}

	public int getCachedSize() {
		return cachedSize;
	}

	/**
	 * 获取更新队列名
	 * @return
	 */
	public String getPersisterName() {
		return cached.persister().value();
	}

}
