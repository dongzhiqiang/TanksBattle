package com.engine.common.ramcache;

import java.lang.management.ManagementFactory;
import java.lang.reflect.Field;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Set;

import javax.management.MBeanServer;
import javax.management.ObjectName;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.util.Assert;

import com.engine.common.ramcache.anno.CacheUnit;
import com.engine.common.ramcache.anno.CachedEntityConfig;
import com.engine.common.ramcache.exception.ConfigurationException;
import com.engine.common.ramcache.exception.StateException;
import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.ramcache.orm.Querier;
import com.engine.common.ramcache.persist.Persister;
import com.engine.common.ramcache.persist.PersisterConfig;
import com.engine.common.ramcache.persist.PersisterType;
import com.engine.common.ramcache.persist.QueuePersister;
import com.engine.common.ramcache.persist.TimingPersister;
import com.engine.common.ramcache.service.EntityCacheService;
import com.engine.common.ramcache.service.EntityCacheServiceImpl;
import com.engine.common.ramcache.service.RegionCacheService;
import com.engine.common.ramcache.service.RegionCacheServiceImpl;

/**
 * 缓存服务管理器
 * 
 */
@SuppressWarnings("rawtypes")
public class ServiceManager implements ServiceManagerMBean {

	private static final Logger logger = LoggerFactory.getLogger(ServiceManager.class);

	/** 存储器 */
	private final Accessor accessor;
	/** 查询器 */
	private final Querier querier;
	/** 持久化缓存配置信息 */
	private final Map<String, PersisterConfig> persisterConfigs;
	/** 实体对应的配置信息 */
	private final Map<Class<? extends IEntity>, CachedEntityConfig> entityConfigs = new HashMap<Class<? extends IEntity>, CachedEntityConfig>();

	/** 队列配置名对应的队列实例 */
	private final Map<String, Persister> persisters = new HashMap<String, Persister>();
	/** 实体对应的缓存服务对象 */
	private final Map<Class<? extends IEntity>, EntityCacheService> entityServices = new HashMap<Class<? extends IEntity>, EntityCacheService>();
	/** 实体对应的缓存服务对象 */
	private final Map<Class<? extends IEntity>, RegionCacheService> regionServices = new HashMap<Class<? extends IEntity>, RegionCacheService>();

	/** 初始化方法 */
	public ServiceManager(Set<Class<IEntity>> entityClasses, Accessor accessor, Querier querier,
			Map<String, Integer> constants, Map<String, PersisterConfig> persisterConfigs) {
		Assert.notNull(accessor, "存储器不能为空");
		Assert.notNull(querier, "查询器不能为空");
		Assert.notNull(entityClasses, "实体类配置集合不能为空");

		this.accessor = accessor;
		this.querier = querier;
		this.persisterConfigs = persisterConfigs;

		for (Class<? extends IEntity> clz : entityClasses) {
			if (!CachedEntityConfig.isVaild(clz, constants)) {
				throw new ConfigurationException("无效的缓存实体[" + clz.getName() + "]配置");
			}

			// 获取实体缓存配置信息
			CachedEntityConfig config = CachedEntityConfig.valueOf(clz, constants);
			entityConfigs.put(clz, config);
		}

		// 注册MBean
		try {
			MBeanServer mbs = ManagementFactory.getPlatformMBeanServer();
			ObjectName name = new ObjectName("com.engine.common:type=CacheServiceMBean");
			mbs.registerMBean(this, name);
		} catch (Exception e) {
			logger.error("注册[common-ramcache]的JMX管理接口失败", e);
		}
	}

	/**
	 * 获取指定实体的缓存服务对象{@link EntityCacheService}
	 * @param clz 实体类
	 * @return 不存在会返回null
	 */
	public EntityCacheService getEntityService(Class<? extends IEntity> clz) {
		CachedEntityConfig config = entityConfigs.get(clz);
		if (config == null) {
			throw new StateException("类[" + clz.getName() + "]不是有效的缓存实体");
		}
		if (!config.cacheUnitIs(CacheUnit.ENTITY)) {
			throw new StateException("实体[" + clz.getName() + "]的缓存单位不是[" + CacheUnit.ENTITY + "]");
		}

		EntityCacheService result = entityServices.get(clz);
		if (result != null) {
			return result;
		}
		return createEntityService(clz);
	}

	/**
	 * 获取指定实体的缓存服务对象{@link RegionCacheService}
	 * @param clz 实体类
	 * @return 不存在会返回null
	 */
	public RegionCacheService getRegionService(Class<? extends IEntity> clz) {
		CachedEntityConfig config = entityConfigs.get(clz);
		if (config == null) {
			throw new StateException("类[" + clz.getName() + "]不是有效的缓存实体");
		}
		if (!config.cacheUnitIs(CacheUnit.REGION)) {
			throw new StateException("实体[" + clz.getName() + "]的缓存单位不是[" + CacheUnit.REGION + "]");
		}

		RegionCacheService result = regionServices.get(clz);
		if (result != null) {
			return result;
		}
		return createRegionService(clz);
	}

	/** 停止全部实体更新队列 */
	public void shutdown() {
		if (logger.isDebugEnabled()) {
			logger.debug("开始停止实体更新队列");
		}
		for (Persister queue : persisters.values()) {
			queue.shutdown();
		}
	}

	// JMX 的管理方法实现

	@Override
	public Map<String, Map<String, String>> getAllPersisterInfo() {
		HashMap<String, Map<String, String>> result = new HashMap<String, Map<String, String>>();
		for (Entry<String, Persister> entry : persisters.entrySet()) {
			result.put(entry.getKey(), entry.getValue().getInfo());
		}
		return result;
	}

	@Override
	public Map<String, String> getPersisterInfo(String name) {
		Persister persister = persisters.get(name);
		if (persister == null) {
			return null;
		}
		return persister.getInfo();
	}

	@Override
	public Map<String, CachedEntityConfig> getAllCachedEntityConfig() {
		HashMap<String, CachedEntityConfig> result = new HashMap<String, CachedEntityConfig>();
		for (Entry<Class<? extends IEntity>, EntityCacheService> entry : entityServices.entrySet()) {
			result.put(entry.getKey().getName(), entry.getValue().getEntityConfig());
		}
		for (Entry<Class<? extends IEntity>, RegionCacheService> entry : regionServices.entrySet()) {
			result.put(entry.getKey().getName(), entry.getValue().getEntityConfig());
		}
		return result;
	}

	/**
	 * 获取存取访问器
	 * @return
	 */
	public Accessor getAccessor() {
		return this.accessor;
	}

	// 内部方法

	/** 创建缓存服务对象 */
	private synchronized RegionCacheService createRegionService(Class<? extends IEntity> clz) {
		if (regionServices.containsKey(clz)) {
			return regionServices.get(clz);
		}

		CachedEntityConfig config = entityConfigs.get(clz);
		Persister queue = getPersister(config.getPersisterName());

		// 创建实体缓存服务对象
		RegionCacheServiceImpl result = new RegionCacheServiceImpl();
		result.initialize(config, queue, accessor, querier);
		regionServices.put(config.getClz(), result);
		return result;
	}

	/** 创建缓存服务对象 */
	private synchronized EntityCacheService createEntityService(Class<? extends IEntity> clz) {
		if (entityServices.containsKey(clz)) {
			return entityServices.get(clz);
		}

		CachedEntityConfig config = entityConfigs.get(clz);
		Persister queue = getPersister(config.getPersisterName());

		// 创建实体缓存服务对象
		EntityCacheServiceImpl result = new EntityCacheServiceImpl();
		result.initialize(config, queue, accessor, querier);
		entityServices.put(config.getClz(), result);
		return result;
	}

	/** 获取持久化处理器实例 */
	private Persister getPersister(String name) {
		Persister result = persisters.get(name);
		if (result != null) {
			return result;
		}

		if (!persisterConfigs.containsKey(name)) {
			throw new ConfigurationException("持久化处理器[" + name + "]的配置信息不存在");
		}

		// 创建实体更新队列
		PersisterConfig config = persisterConfigs.get(name);
		if (config.getType() == PersisterType.QUEUE) {
			result = new QueuePersister();
		} else {
			result = new TimingPersister();
		}
		result.initialize(name, accessor, config.getValue());
		persisters.put(name, result);
		return result;
	}

	@Override
	public Map<String, Integer> getCachedEntityCount() {
		Map<String, Integer> result = new HashMap<String, Integer>(entityServices.size());
		for (Entry<Class<? extends IEntity>, EntityCacheService> e : entityServices.entrySet()) {
			Class<? extends IEntity> k = e.getKey();
			EntityCacheService v = e.getValue();
			int size = v.getFinder().all().size();
			result.put(k.getName(), size);
		}
		return result;
	}

	@Override
	@SuppressWarnings("unchecked")
	public Map<String, Map<String, Integer>> getCachedRegionCount() {
		Map<String, Map<String, Integer>> result = new HashMap<String, Map<String, Integer>>(regionServices.size());
		for (Entry<Class<? extends IEntity>, RegionCacheService> e : regionServices.entrySet()) {
			Class<? extends IEntity> k = e.getKey();
			RegionCacheService v = e.getValue();
			Map<String, Integer> r = new HashMap<String, Integer>();
			try {
				int total = 0;
				Field f = v.getClass().getDeclaredField("cache");
				f.setAccessible(true);
				Map<String, Map> cache = (Map<String, Map>) f.get(v);
				for (Entry<String, Map> s : cache.entrySet()) {
					String idx = s.getKey();
					Map<?, Map> map = s.getValue();
					for (Entry<?, Map> o : map.entrySet()) {
						total += o.getValue().size();
					}
					r.put(idx, map == null ? 0 : map.size());
				}
				r.put("ENTITYS", total);
			} catch (Exception ex) {
				logger.error("", ex);
			}
			result.put(k.getName(), r);
		}
		return result;
	}
}
