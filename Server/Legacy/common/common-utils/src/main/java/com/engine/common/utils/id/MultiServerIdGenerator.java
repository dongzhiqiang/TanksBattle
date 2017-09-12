package com.engine.common.utils.id;

import java.util.HashMap;
import java.util.concurrent.locks.ReentrantReadWriteLock;

/**
 * 多服的主键生成器
 * 
 */
public class MultiServerIdGenerator {

	/** 主键生成器映射 */
	private HashMap<Short, IdGenerator> generators = new HashMap<Short, IdGenerator>();
	
	private ReentrantReadWriteLock lock = new ReentrantReadWriteLock();

	/**
	 * 添加指定服标识的主键生成器
	 * @param server 服标识
	 * @param max 当前的主键最大值
	 */
	public void add(short server, Long max) {
		lock.writeLock().lock();
		try {
			if (generators.containsKey(server)) {
				throw new IllegalStateException("服务器标识为[" + server + "]的主键生成器已经存在");
			}
			IdGenerator generator = new IdGenerator(server, max);
			generators.put(server, generator);
		} finally {
			lock.writeLock().unlock();
		}
	}
	
	/**
	 * 获取下一个自增主键
	 * @param server 服标识
	 * @return
	 */
	public long getNext(short server) {
		lock.readLock().lock();
		try {
			IdGenerator generator = generators.get(server);
			if (generator == null) {
				throw new IllegalStateException("服务器标识为[" + server + "]的主键生成器不存在");
			}
			return generator.getNext();
		} finally {
			lock.readLock().unlock();
		}
	}
}
