package com.game.module.migration;

import java.io.Serializable;
import java.util.Collection;
import java.util.LinkedList;
import java.util.concurrent.Callable;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;

import org.apache.commons.lang.builder.ReflectionToStringBuilder;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.orm.EntityMetadata;
import com.engine.common.ramcache.orm.impl.CursorCallback;
import com.engine.common.ramcache.orm.impl.HibernateAccessor;

public class MigratorReadTask implements Callable<MigratorReadTask> {
	
	private static final Logger logger = LoggerFactory.getLogger(MigratorReadTask.class);
	
	private final int batchSize;
	
	private final MigratorCounter counter;

	private final MigratorContext context;
	
	private final EntityMetadata entityMetadata;
	
	public MigratorReadTask(EntityMetadata entityMetadata, MigratorContext context, MigratorCounter counter, int batchSize) {
		this.entityMetadata = entityMetadata;
		this.context = context;
		this.counter = counter;
		this.batchSize = batchSize;
	}
	
	public String toString() {
		return ReflectionToStringBuilder.toString(this);
	}

	@SuppressWarnings({ "unchecked", "rawtypes" })
	@Override
	public MigratorReadTask call() {
		final MigratorConverter converter = context.getMigratorConverter(entityMetadata);
		if(converter instanceof IgnoreConverter) {
			// 忽略转换器,所以无需执行迁移过程
			return this;
		}
		
		try {
			// 迁移任务所依赖的迁移任务的集合
			final Collection<Future<?>> dependencyTaskSet = new LinkedList<Future<?>>();
	
			// 指定依赖的实体
			if (context.getEntityDependencyMap().get(entityMetadata.getEntityName()) != null) {
				for (String dependencyClassName : context.getEntityDependencyMap().get(entityMetadata.getEntityName())) {
					final Future<?> future = this.context.getMigrationReadTask(context.getEntityMetadata(dependencyClassName));
					dependencyTaskSet.add(future);
				}
			}

			// 等待依赖的任务完成
			for (Future<?> dependencyTask : dependencyTaskSet) {
				dependencyTask.get();
			}
			
			// 开始迁移时间
			final long startTime = System.currentTimeMillis();
			
			final HibernateAccessor oldRawStore = context.getOldAccessor();
			
			final Collection<IEntity<Serializable>> dataCollection = new LinkedList<IEntity<Serializable>>();
			
			oldRawStore.cursor(this.entityMetadata.getEntityClass(), null, new CursorCallback<IEntity>() {

				@Override
				public void call(IEntity entity) {
					dataCollection.add(entity);
					counter.getReadRecordNumber().incrementAndGet();
					
					if(counter.getReadRecordNumber().get()%batchSize==0) {
						counter.getCurrentTaskNumber().incrementAndGet();
						context.getMigrationWriteTask(entityMetadata, new LinkedList<IEntity<Serializable>>(dataCollection));
						dataCollection.clear();
					}
					
					if(counter.getReadRecordNumber().get()%10000==0) {
						final Object[] parameters = new Object[] {entityMetadata.getEntityName(), counter.getReadRecordNumber().get(), context.getWriteExecutor().getQueue().size(), context.getWriteExecutor().getActiveCount()};
						logger.info("迁移实体[{}],数量[{}],队列[{}],线程[{}]", parameters);
					}
				}
			});
			
			if(!dataCollection.isEmpty()) {
				counter.getCurrentTaskNumber().incrementAndGet();
				context.getMigrationWriteTask(entityMetadata, new LinkedList<IEntity<Serializable>>(dataCollection));
				dataCollection.clear();
			}
			
			try {
				while(this.counter.getCurrentTaskNumber().get()!=0) {
					logger.info("迁移实体[{}],剩余任务[{}],线程[{}]", new Object[] {this.entityMetadata.getEntityName(), this.counter.getCurrentTaskNumber().get(), context.getWriteExecutor().getActiveCount()});
					Thread.sleep(TimeUnit.MILLISECONDS.convert(1, TimeUnit.SECONDS));
				}
				// 结束迁移时间
				long endTime = System.currentTimeMillis();
				logger.info("迁移实体:实体名称[{}],旧数量[{}],新数量[{}],消耗时间[{}]", new Object[] {this.entityMetadata.getEntityName(), this.counter.getReadRecordNumber().get(), this.counter.getWriteRecordNumber().get(), (endTime-startTime)});
			} catch (InterruptedException exception) {
				logger.info("警告!等待被中断");
				exception.printStackTrace();
			}
		} catch (Exception exception) {
			exception.printStackTrace();
			throw new RuntimeException(exception);
		}
		return this;
	}
	
}
