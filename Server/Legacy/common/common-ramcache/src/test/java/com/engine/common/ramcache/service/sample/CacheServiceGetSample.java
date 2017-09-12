package com.engine.common.ramcache.service.sample;

import java.io.Serializable;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

import org.springframework.beans.factory.annotation.Autowired;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.orm.Accessor;
import com.googlecode.concurrentlinkedhashmap.ConcurrentLinkedHashMap;
import com.googlecode.concurrentlinkedhashmap.ConcurrentLinkedHashMap.Builder;

public class CacheServiceGetSample<PK extends Comparable<PK> & Serializable, T extends IEntity<PK>> {
	
	@Autowired
	private Accessor accessor;
	private ConcurrentLinkedHashMap<Serializable, Object> region = new Builder<Serializable, Object>()
			//.initialCapacity(16) 			// 设置调整大小因子
			.maximumWeightedCapacity(1000) 	// 设置最大元素数量(可能会临时超出该数值)
			//.concurrencyLevel(16)			// 设置并发更新线程数预计值
			//.listener()					// 设置元素清理的监听器
			.build();
	private Class<T> clz;

	@SuppressWarnings("unchecked")
	public T get(PK id) {
		T current = (T) region.get(id);
		if (current != null) {
			return current;
		}
		
		current = (T) accessor.load(clz, id);
		if (current != null) {
			T prev = (T) region.putIfAbsent(id, current);
			if (prev != null) {
				current = prev;
			}
		}
		return current;
	}
	
	private ConcurrentHashMap<PK, Lock> waitingLock = new ConcurrentHashMap<PK, Lock>();
	
	@SuppressWarnings("unchecked")
	public T getAndWait1(PK id) {
		T current = (T) region.get(id);
		if (current != null) {
			return current;
		}

		Lock lock = new ReentrantLock();
		Lock prevLock = waitingLock.putIfAbsent(id, lock);
		lock = prevLock != null ? prevLock : lock;
		
		lock.lock();
		try {
			current = (T) region.get(id);
			if (current != null) {
				return current;
			}
			current = (T) accessor.load(clz, id);
			if (current != null) {
				T prevEntity = (T) region.putIfAbsent(id, current);
				if (prevEntity != null) {
					current = prevEntity;
				}
			}
			return current;
		} finally {
			waitingLock.remove(id);
			lock.unlock();
		}
	}

	private ConcurrentHashMap<PK, Object> waitingObj = new ConcurrentHashMap<PK, Object>();
	
	@SuppressWarnings("unchecked")
	public T getAndWait2(PK id) {
		T current = (T) region.get(id);
		if (current != null) {
			return current;
		}

		Object lock = new Object();
		Object prevLock = waitingObj.putIfAbsent(id, lock);
		lock = prevLock != null ? prevLock : lock;
		
		synchronized (lock) {
			current = (T) region.get(id);
			if (current != null) {
				return current;
			}
			
			current = (T) accessor.load(clz, id);
			if (current != null) {
				T prevEntity = (T) region.putIfAbsent(id, current);
				if (prevEntity != null) {
					current = prevEntity;
				}
			}
			
			waitingObj.remove(id);
			return current;
		}
	}

	private ConcurrentHashMap<PK, ReentrantReadWriteLock> waitingLock2 = new ConcurrentHashMap<PK, ReentrantReadWriteLock>();

	@SuppressWarnings("unchecked")
	public T getAndWait3(PK id) {
		T current = (T) region.get(id);
		if (current != null) {
			return current;
		}

		ReentrantReadWriteLock lock = new ReentrantReadWriteLock();
		ReentrantReadWriteLock prevLock = waitingLock2.putIfAbsent(id, lock);
		lock = prevLock != null ? prevLock : lock;
		
		lock.writeLock().lock();
		try {
			current = (T) region.get(id);
			if (current != null) {
				return current;
			}
			current = (T) accessor.load(clz, id);
			if (current != null) {
				T prevEntity = (T) region.putIfAbsent(id, current);
				if (prevEntity != null) {
					current = prevEntity;
				}
			}
			return current;
		} finally {
			waitingLock2.remove(id);
			lock.writeLock().unlock();
		}
	}

	public void setClz(Class<T> clz) {
		this.clz = clz;
	}

}
