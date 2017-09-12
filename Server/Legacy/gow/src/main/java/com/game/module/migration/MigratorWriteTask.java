package com.game.module.migration;

import java.io.Serializable;
import java.util.Collection;
import java.util.concurrent.Callable;
import java.util.concurrent.atomic.AtomicInteger;

import org.hibernate.Session;
import org.hibernate.Transaction;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.orm.EntityMetadata;
import com.engine.common.ramcache.orm.impl.HibernateAccessor;

public class MigratorWriteTask implements Callable<MigratorWriteTask> {
	
	private static final Logger logger = LoggerFactory.getLogger(MigratorWriteTask.class);
	
	private final MigratorCounter counter;
	
	private final MigratorContext context;
	
	private final EntityMetadata entityMetadata;
	
	private final Collection<IEntity<Serializable>> dataCollection;

	public MigratorWriteTask(EntityMetadata entityMetadata, MigratorContext context, MigratorCounter counter, Collection<IEntity<Serializable>> dataCollection) {
		this.entityMetadata = entityMetadata;
		this.context = context;
		this.counter = counter;
		this.dataCollection = dataCollection;
	}

	@Override
	public MigratorWriteTask call() throws Exception {
		final HibernateAccessor oldRawStore = context.getNewAccessor();
		final HibernateAccessor newRawStore = context.getNewAccessor();
		final Session session = newRawStore.getHibernateTemplate().getSessionFactory().openSession();
		final Transaction transaction = session.beginTransaction();
		try {
			final MigratorConverter converter = context.getMigratorConverter(entityMetadata);
			for(IEntity<Serializable> entity:dataCollection) {
				final AtomicInteger retryTimes = new AtomicInteger(10);
				do {
					try {
						if(converter!=null) {
							converter.convert(context, oldRawStore, newRawStore, entity);
						}
						session.saveOrUpdate(entity);
						this.counter.getWriteRecordNumber().incrementAndGet();
						break;
					} catch(IgnoreMigrateException exception) {
						logger.info("迁移数据忽略:[{}]", exception.getMessage(), exception);
						break;
					} catch(Exception exception) {
						logger.info("迁移数据重试[{}]", retryTimes.get(), exception);
						// TODO 可能存在次级键原因导致锁冲突,等待100毫秒进行重试
						Thread.sleep(100);
						if(retryTimes.decrementAndGet()<0) {
							throw new MaximumRetryException(exception);
						}
					}
				} while(true);
			}
//			logger.info("迁移实体[{}],剩余任务[{}],线程[{}]", new Object[] {this.entityMetadata.getEntityName(), this.counter.getCurrentTaskNumber().get(), context.getWriteExecutor().getActiveCount()});
		} finally {
			this.counter.getCurrentTaskNumber().decrementAndGet();
			try {
				transaction.commit();
				session.close();
			} catch(Exception exception) {
				logger.error("迁移写任务异常", exception);
			}
		}
		return this;
	}
	
}
