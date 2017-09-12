package com.game.module.migration;

import java.util.Collection;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Properties;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;

import org.apache.commons.configuration.PropertiesConfiguration;
import org.apache.commons.dbcp.BasicDataSource;
import org.hibernate.SessionFactory;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.orm.hibernate3.annotation.AnnotationSessionFactoryBean;

import com.engine.common.ramcache.orm.EntityMetadata;
import com.engine.common.ramcache.orm.impl.HibernateAccessor;
import com.engine.common.utils.json.JsonUtils;

public class Migrator {
	
	private static final Logger logger = LoggerFactory.getLogger(Migrator.class);
	
	/** 新数据库路径 */
	private final PropertiesConfiguration newDatabaseConfiguration;
	
	/** 旧数据库路径 */
	private final PropertiesConfiguration oldDatabaseConfiguration;
	
	/** 实体名称与转换器的映射 */
	private final Map<String, MigratorConverter> entityConverterMap = new HashMap<String, MigratorConverter>();
	
	/** 实体与依赖的映射 */
	private final Map<String, Collection<String>> entityDependencyMap;

	 public Migrator(PropertiesConfiguration oldDatabaseDirectory, PropertiesConfiguration newDatabaseDirectory, Map<String, String> entityConverterMap, Map<String, Collection<String>> entityDependencyMap) throws InstantiationException, IllegalAccessException, ClassNotFoundException {
		this.newDatabaseConfiguration = newDatabaseDirectory;
		this.oldDatabaseConfiguration = oldDatabaseDirectory;
		this.entityDependencyMap = entityDependencyMap;
		for(Entry<String, String> entry : entityConverterMap.entrySet()) {
			this.entityConverterMap.put(entry.getKey(),  MigratorConverter.class.cast(Class.forName(entry.getValue()).newInstance()));
		}
	}
	 
	private SessionFactory buildSessionFactory(PropertiesConfiguration configuration) throws Exception {
		final BasicDataSource dataSource = new BasicDataSource();
		dataSource.setDriverClassName(configuration.getString("jdbc.driverClassName"));
		dataSource.setUrl(configuration.getString("jdbc.url"));
		dataSource.setUsername(configuration.getString("jdbc.username"));
		dataSource.setPassword(configuration.getString("jdbc.password"));
		dataSource.setValidationQuery("select ''");
		dataSource.setTimeBetweenEvictionRunsMillis(5000);
		dataSource.setNumTestsPerEvictionRun(10);
		dataSource.setTestOnBorrow(false);
		dataSource.setTestWhileIdle(false);
		dataSource.setInitialSize(10);
		//TODO 连接池过小很可能导致迁移过程死锁.
		dataSource.setMaxActive(5000);
		dataSource.setMaxIdle(5);
		dataSource.setMinIdle(1);
		
		final Properties hibernateProperties = new Properties();
		hibernateProperties.put("current_session_context_class", "thread");
		hibernateProperties.put("cache.provider_class", "org.hibernate.cache.NoCacheProvider");
		hibernateProperties.put("hibernate.dialect", configuration.getString("hibernate.dialect"));
		hibernateProperties.put("hibernate.show_sql", configuration.getString("hibernate.show_sql"));
		hibernateProperties.put("hibernate.hbm2ddl.auto", configuration.getString("hibernate.hbm2ddl.auto"));
		hibernateProperties.put("hibernate.cache.use_second_level_cache", "false");
		hibernateProperties.put("connection.autoReconnect", "true");
		hibernateProperties.put("connection.autoReconnectForPools", "true");
		hibernateProperties.put("connection.is-connection-validation-required", "true");
		hibernateProperties.put("hibernate.hbm2ddl.import_files", "/import.sql");
		
		final AnnotationSessionFactoryBean factoryBean = new AnnotationSessionFactoryBean();
		factoryBean.setDataSource(dataSource);
		factoryBean.setPackagesToScan(new String[] {"com.my9yu.sango2.**.manager"});
		factoryBean.setHibernateProperties(hibernateProperties);
		factoryBean.afterPropertiesSet();
		return factoryBean.getObject();
	}
	 
	/**
	 * 迁移流程
	 * @throws Exception 
	 */
	public void migrate() throws Exception {
		final SessionFactory newDatabaseEnvironment = buildSessionFactory(this.newDatabaseConfiguration); 
		final SessionFactory oldDatabaseEnvironment = buildSessionFactory(this.oldDatabaseConfiguration);
		
		final HibernateAccessor newAccessor = new HibernateAccessor();
		newAccessor.setSessionFactory(newDatabaseEnvironment);
		newAccessor.postConstruct();
		final HibernateAccessor oldAccessor = new HibernateAccessor();
		oldAccessor.setSessionFactory(oldDatabaseEnvironment);
		oldAccessor.postConstruct();
	
		Map<String, EntityMetadata> newEntityMetadataMap = new HashMap<String, EntityMetadata>();
		Map<String, EntityMetadata> oleEntityMetadataMap = new HashMap<String, EntityMetadata>();
		for(EntityMetadata entityMetadata:newAccessor.getAllMetadata()) {
			newEntityMetadataMap.put(entityMetadata.getEntityName(), entityMetadata);
		}
		for(EntityMetadata entityMetadata:oldAccessor.getAllMetadata()) {
			oleEntityMetadataMap.put(entityMetadata.getEntityName(), entityMetadata);
		}
		
		logger.error("旧数据库实体列表[{}]", oleEntityMetadataMap.keySet());
		logger.error("新数据库实体列表[{}]", newEntityMetadataMap.keySet());
		
		final MigratorContext context = new MigratorContext(oleEntityMetadataMap, entityConverterMap, entityDependencyMap, oldAccessor, newAccessor);
		// 迁移实体
		for(String entityName:oleEntityMetadataMap.keySet()) {
			context.getMigrationReadTask(oleEntityMetadataMap.get(entityName));
		}
		
		while(true) {
			if(context.getReadExecutor().getActiveCount() == 0) {
				break;
			}
			logger.info("迁移实体读线程活跃数[{}]", new Object[] {context.getReadExecutor().getActiveCount()});
			logger.info("迁移实体写线程活跃数[{}]", new Object[] {context.getWriteExecutor().getActiveCount()});
			for(Entry<String, MigratorCounter> entry:context.getCounterMap().entrySet()) {
				Future<MigratorReadTask> task = context.getMigrationReadTask(oleEntityMetadataMap.get(entry.getKey()));
				if(task.isCancelled() || task.isDone()) {
					continue;
				}
				logger.info("迁移实体[{}]进度[{}]", new Object[] {entry.getKey(), JsonUtils.object2String(entry.getValue())});
			}
			Thread.sleep(10000);
		}
		
		context.getReadExecutor().shutdown();
		
		try {
			if(context.getReadExecutor().awaitTermination(60, TimeUnit.MINUTES)) {
				logger.debug("迁移成功");
			} else {
				logger.debug("迁移失败");
			}
		} catch (InterruptedException exception) {
			exception.printStackTrace();
		} finally {
			context.getWriteExecutor().shutdown();
			oldDatabaseEnvironment.close();
			newDatabaseEnvironment.close();
		}
	}
	
}
