package com.engine.common.ramcache.schema;

import java.util.Map;
import java.util.Set;

import javax.annotation.PreDestroy;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.FactoryBean;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Qualifier;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.ServiceManager;
import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.ramcache.orm.Querier;
import com.engine.common.ramcache.persist.PersisterConfig;

/**
 * 缓存服务管理器工厂
 * 
 */
@SuppressWarnings("rawtypes")
public class ServiceManagerFactory implements FactoryBean<ServiceManager> {
	
	private Logger logger = LoggerFactory.getLogger(getClass());
	
	public static final String ENTITY_CLASSES_NAME = "entityClasses";
	public static final String PERSISTER_CONFIG_NAME = "persisterConfig";

	private Accessor accessor;
	private Querier querier;
	private Set<Class<IEntity>> entityClasses;
	private Map<String, PersisterConfig> persisterConfig;
	private Map<String, Integer> constants;
	
	private ServiceManager cacheServiceManager;

	@Autowired(required = false)
	@Qualifier("shutdownDelay")
	private long shutdownDelay = 5000;

	@Override
	public ServiceManager getObject() throws Exception {
		cacheServiceManager = new ServiceManager(entityClasses, accessor, querier, constants, persisterConfig);
		return cacheServiceManager;
	}

	@PreDestroy
	public void shutdown() {
		if (cacheServiceManager == null) {
			return;
		}
		
		logger.error("开始回写缓存数据...");
		cacheServiceManager.shutdown();
		try {
			Thread.sleep(shutdownDelay);
		} catch (InterruptedException e) {
		}
		logger.error("回写缓存数据完成...");
	}
	
	// Setter Methods ...
	
	public void setAccessor(Accessor accessor) {
		this.accessor = accessor;
	}


	public void setQuerier(Querier querier) {
		this.querier = querier;
	}
	
	public void setEntityClasses(Set<Class<IEntity>> entityClasses) {
		this.entityClasses = entityClasses;
	}
	
	public void setPersisterConfig(Map<String, PersisterConfig> persisterConfig) {
		this.persisterConfig = persisterConfig;
	}
	
	public void setConstants(Map<String, Integer> constants) {
		this.constants = constants;
	}
	
	// Other Methods

	@Override
	public Class<?> getObjectType() {
		return ServiceManager.class;
	}

	@Override
	public boolean isSingleton() {
		return true;
	}

}
