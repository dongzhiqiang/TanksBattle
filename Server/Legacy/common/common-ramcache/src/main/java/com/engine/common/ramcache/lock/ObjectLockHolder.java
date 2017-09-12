package com.engine.common.ramcache.lock;

import java.util.WeakHashMap;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

/**
 * 对象锁持有者
 * 
 */
@SuppressWarnings("rawtypes")
public class ObjectLockHolder {
	
	/**
	 * 单一类的锁持有者
	 * 
	 */
	public class Holder {

		/** 持有的对象类型，先放在应该会有用的 */
		@SuppressWarnings("unused")
		private final Class clz;
		/** 类型唯一锁 */
		private final Lock tieLock = new ReentrantLock();
		/** 对象实例与其对应的锁缓存 */
		private final WeakHashMap<Object, ObjectLock> locks = new WeakHashMap<Object, ObjectLock>();
		
		/**
		 * 创建一个持有者实例
		 * @param clz
		 */
		public Holder(Class clz) {
			this.clz = clz;
		}

		/**
		 * 获取对象锁
		 * @param object
		 * @return
		 */
		public ObjectLock getLock(Object object) {
			ObjectLock result = locks.get(object);
			if (result != null) {
				return result;
			}
			return createLock(object);
		}

		/**
		 * 创建对象锁
		 * @param object
		 * @return
		 */
		private synchronized ObjectLock createLock(Object object) {
			ObjectLock result = locks.get(object);
			if (result != null) {
				return result;
			}
			result = new ObjectLock(object);
			locks.put(object, result);
			return result;
		}

		/**
		 * 获取类型唯一锁
		 * @return
		 */
		public Lock getTieLock() {
			return tieLock;
		}

		/**
		 * 获取锁的数量
		 * @return
		 */
		public int count() {
			return locks.size();
		}
	}

	/** 持有者集合 */
	private ConcurrentHashMap<Class, Holder> holders = new ConcurrentHashMap<Class, Holder>();

	/**
	 * 获取指定对象实例的对象锁
	 * @param object 要获取锁的对象实例
	 * @return
	 */
	public ObjectLock getLock(Object object) {
		Holder holder = getHolder(object.getClass());
		ObjectLock lock = holder.getLock(object);
		return lock;
	}

	/**
	 * 获取某类实例的锁持有者
	 * @param clz 指定类型
	 * @return
	 */
	private Holder getHolder(Class clz) {
		Holder holder = holders.get(clz);
		if (holder != null) {
			return holder;
		}
		holders.putIfAbsent(clz, new Holder(clz));
		return holders.get(clz);
	}

	/**
	 * 获取指定类型的类型唯一锁
	 * @param clz 指定类型
	 * @return
	 */
	public Lock getTieLock(Class clz) {
		Holder holder = getHolder(clz);
		return holder.getTieLock();
	}

	/**
	 * 获取指定类型的锁的数量
	 * @param clz
	 * @return
	 */
	public int count(Class<?> clz) {
		if (holders.containsKey(clz)) {
			Holder holder = getHolder(clz);
			return holder.count();
		}
		return 0;
	}
}
