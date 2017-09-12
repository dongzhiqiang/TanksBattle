package com.game.module.migration;

import java.io.Serializable;
import java.util.Collection;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.Future;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.RejectedExecutionException;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.orm.EntityMetadata;
import com.engine.common.ramcache.orm.impl.HibernateAccessor;

/**
 * 迁移上下文对象
 * 
 *
 */
public class MigratorContext {
	
	private static final Logger logger = LoggerFactory.getLogger(MigratorContext.class);
	
	/** 设置任务队列大小(TODO 考虑配置项) */
	public static final int QUEUE_SIZE = 200;
	
	/** 设置批量数据大小(TODO 考虑配置项) */
	public static final int BATCH_SIZE = 50;
	
	/** 警戒数量(当出现拒绝任务异常以后,会停止任务提交,直到任务队列低于警戒数量)(TODO 考虑配置项) */
	public static final int ALERT_SIZE = MigratorContext.QUEUE_SIZE/2;
	
	private final ThreadPoolExecutor readeExecutor;
	
	private final ThreadPoolExecutor writeExecutor;
	
	/** 所有迁移任务的映射集合 */
	private final ConcurrentMap<String, Future<MigratorReadTask>> migrationTaskMap = new ConcurrentHashMap<String, Future<MigratorReadTask>>() ;
	
	/** 所有迁移计数器的映射集合 */
	private final ConcurrentMap<String, MigratorCounter> counterMap = new ConcurrentHashMap<String, MigratorCounter>();
	
	public ConcurrentMap<String, MigratorCounter> getCounterMap() {
		return counterMap;
	}

	/** 新实体名称与旧实体名称的映射 */
	private final Map<String, EntityMetadata> classNameMap;

	/** 新旧数据库环境 */
	private final HibernateAccessor oldAccessor, newAccessor;
	
	/** 实体名称与转换器的映射 */
	private final Map<String, MigratorConverter> entityConverterMap;
	
	/** 实体与依赖的映射 */
	private final Map<String, Collection<String>> entityDependencyMap;
	
	public MigratorContext(Map<String, EntityMetadata> classNameMap, Map<String, MigratorConverter> entityConverterMap, Map<String, Collection<String>> entityDependencyMap, HibernateAccessor oldAccessor, HibernateAccessor newAccessor) {
		this.classNameMap = classNameMap;
		for(String entityName:this.classNameMap.keySet()) {
			this.counterMap.put(entityName, new MigratorCounter());
		}
		this.entityConverterMap = entityConverterMap;
		this.entityDependencyMap = entityDependencyMap;
		this.readeExecutor = new ThreadPoolExecutor(0, classNameMap.size(), 0, TimeUnit.SECONDS, new LinkedBlockingQueue<Runnable>(1));
		this.writeExecutor = new ThreadPoolExecutor(Runtime.getRuntime().availableProcessors(), Runtime.getRuntime().availableProcessors()*8, 0, TimeUnit.SECONDS, new LinkedBlockingQueue<Runnable>(QUEUE_SIZE));
		this.oldAccessor = oldAccessor;
		this.newAccessor = newAccessor;
	}

	public ThreadPoolExecutor getReadExecutor() {
		return readeExecutor;
	}

	public ThreadPoolExecutor getWriteExecutor() {
		return writeExecutor;
	}

	public Map<String, EntityMetadata> getClassNameMap() {
		return classNameMap;
	}
	
	public Future<MigratorReadTask> getMigrationReadTask(EntityMetadata entityMetadata) {
		if(!this.classNameMap.containsKey(entityMetadata.getEntityName())) {
			return null;
		}
		synchronized (migrationTaskMap) {
			if(!migrationTaskMap.containsKey(entityMetadata.getEntityName())) {
				final MigratorReadTask task = new MigratorReadTask(entityMetadata, this, this.counterMap.get(entityMetadata.getEntityName()), BATCH_SIZE);
				final Future<MigratorReadTask> future = this.readeExecutor.submit(task);
				migrationTaskMap.put(entityMetadata.getEntityName(), future);
				logger.info("构建实体[{}]迁移读任务", entityMetadata.getEntityName());
			}
		}
		return migrationTaskMap.get(entityMetadata.getEntityName());
	}
	

	public Future<MigratorWriteTask> getMigrationWriteTask(EntityMetadata entityMetadata, Collection<IEntity<Serializable>> dataCollection) {
		while(true) {
			try {
				return this.writeExecutor.submit(new MigratorWriteTask(entityMetadata, this, this.counterMap.get(entityMetadata.getEntityName()), dataCollection));
			} catch(RejectedExecutionException exception) {
				logger.error("迁移实体[{}]拒绝任务异常", entityMetadata.getEntityName());
				// 任务队列超过警戒数量,停止提交任务,防止RejectedExecutionException
				while(this.writeExecutor.getQueue().size()>=ALERT_SIZE) {
					// TODO 比较yield与sleep的区别
					try {
						Thread.sleep(TimeUnit.MILLISECONDS.convert(10, TimeUnit.SECONDS));
						logger.info("迁移实体[{}]写任务等待队列[{}]", entityMetadata.getEntityName(), this.writeExecutor.getQueue().size());
					} catch (InterruptedException interruptedException) {
						interruptedException.printStackTrace();
					}
				}
				logger.info("迁移实体[{}]写任务恢复,队列[{}]", entityMetadata.getEntityName(), this.writeExecutor.getQueue().size());
			}
		}
	}

	public HibernateAccessor getOldAccessor() {
		return oldAccessor;
	}

	public HibernateAccessor getNewAccessor() {
		return newAccessor;
	}
	
	public MigratorConverter getMigratorConverter(EntityMetadata entityMetadata) {
		return this.entityConverterMap.get(entityMetadata.getEntityName());
	}

	public Map<String, Collection<String>> getEntityDependencyMap() {
		return entityDependencyMap;
	}
	
	public EntityMetadata getEntityMetadata(String name) {
		return this.classNameMap.get(name);
	} 
	
}
