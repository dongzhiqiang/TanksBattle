package com.game.gow.module.system;

import java.util.HashMap;
import java.util.concurrent.locks.ReentrantReadWriteLock;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.orm.Querier;
import com.engine.common.utils.id.IdGenerator;
import com.engine.common.utils.id.MultiServerIdGenerator;

@Component
@SuppressWarnings("rawtypes")
public class MultiServerIdGeneratorHolder {

	@Autowired
	private Querier querier;
	@Autowired
	private SystemConfig config;
	
	private HashMap<Class<? extends IEntity>, MultiServerIdGenerator> generators = new HashMap<Class<? extends IEntity>, MultiServerIdGenerator>();

	private ReentrantReadWriteLock lock = new ReentrantReadWriteLock();
	
	/**
	 * 初始化指定实体的{@link MultiServerIdGenerator}
	 * @param clz 实例类
	 * @param queryName 主键最大值查询名
	 */
	public void initialize(Class<? extends IEntity> clz, String queryName) {
		lock.writeLock().lock();
		try {
			if (generators.containsKey(clz)) {
				throw new IllegalStateException("实体[" + clz.getSimpleName() + "]的主键生成器已经存在");
			}
			MultiServerIdGenerator generator = new MultiServerIdGenerator();
			for (Short server : config.getServers()) {
				long[] limits = IdGenerator.getLimits(server);
				Long max = querier.unique(clz, Long.class, queryName, limits[0], limits[1]);
				generator.add(server, max);
			}
			generators.put(clz, generator);
		} finally {
			lock.writeLock().unlock();
		}
	}
	
	/**
	 * 获取指定实体的多服主键生成器
	 * @param clz 实体类
	 * @return
	 */
	public MultiServerIdGenerator getGenerator(Class<? extends IEntity> clz) {
		lock.readLock().lock();
		try {
			MultiServerIdGenerator result = generators.get(clz);
			if (result == null) {
				throw new IllegalStateException("实体[" + clz.getSimpleName() + "]的主键生成器尚未初始化");
			}
			return result;
		} finally {
			lock.readLock().unlock();
		}
	}
}
