package com.engine.common.socket.handler;

import java.util.concurrent.BlockingQueue;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * 同步请求处理支持类
 * 
 *  2011-10-26 将处理线程改为会自动结束，这样做是为了让SocketClient不会创建多余的线程
 */
public class SyncSupport {

	private final static Logger logger = LoggerFactory.getLogger(SyncSupport.class);

	/** 同步队列集合 */
	private final ConcurrentMap<String, BlockingQueue<Runnable>> queues = new ConcurrentHashMap<String, BlockingQueue<Runnable>>();
	/** 处理线程集合 */
	private final ConcurrentMap<String, Thread> threads = new ConcurrentHashMap<String, Thread>();
	/** 同步队列线程操作锁 */
	private final ConcurrentMap<String, ReentrantLock> locks = new ConcurrentHashMap<String, ReentrantLock>();

	/** 处理线程执行对象 */
	class SyncRunner implements Runnable {
		/** 同步键 */
		private final String key;
		/** 处理队列 */
		private final BlockingQueue<Runnable> queue;
		/** 构造方法 */
		public SyncRunner(String key, BlockingQueue<Runnable> queue) {
			this.key = key;
			this.queue = queue;
		}
		@Override
		public void run() {
			boolean flag = true;
			while (flag) {
				try {
					Runnable e = queue.take();
					e.run();
				} catch (InterruptedException e) {
					logger.error("同步队列[" + key + "]处理线程被非法打断", e);
				} catch (Exception e) {
					logger.error("同步队列[" + key + "]处理线程出现未知错误", e);
				}
				
				if (queue.isEmpty()) {
					Lock lock = loadSyncLock(key);
					lock.lock();
					try {
						if (queue.isEmpty()) {
							threads.remove(key);
							flag = false;
						}
					} catch (Exception e) {
						logger.debug("结束同步队列[" + key + "]处理线程时出现未知错误", e);
					} finally {
						lock.unlock();
					}
				}
			}
 		}
	}
	
	/**
	 * 通过同步队列执行任务
	 * 
	 * @param key 同步键
	 * @param task 任务
	 */
	public void execute(String key, Runnable task) {
		if (key == null || task == null) {
			throw new IllegalArgumentException("同步键或任务不能为空");
		}
		
		BlockingQueue<Runnable> queue = loadSyncQueue(key);
		queue.add(task);
		
		Lock lock = loadSyncLock(key);
		lock.lock();
		try {
			if (!threads.containsKey(key)) {
				SyncRunner runner = new SyncRunner(key, queue);
				Thread thread = new Thread(runner, "通信同步处理:" + key);
				thread.setDaemon(true);
				if (threads.putIfAbsent(key, thread) == null) {
					thread.start();
				}
			}
		} catch (Exception e) {
			logger.error("创建同步队列[" + key + "]处理线程时出现未知错误", e);
		} finally {
			lock.unlock();
		}
	}

	/**
	 * 获取同步操作锁
	 * @param key 同步键
	 * @return 不会返回null
	 */
	private Lock loadSyncLock(String key) {
		ReentrantLock result = locks.get(key);
		if (result != null) {
			return result;
		}
		
		result = new ReentrantLock();
		ReentrantLock prev = locks.putIfAbsent(key, result);
		return prev == null ? result : prev;
	}

	/**
	 * 获取同步处理队列
	 * @param key 同步键
	 * @return 不会返回null
	 */
	private BlockingQueue<Runnable> loadSyncQueue(String key) {
		BlockingQueue<Runnable> result = queues.get(key);
		if (result != null) {
			return result;
		}
		
		result = new LinkedBlockingQueue<Runnable>();
		BlockingQueue<Runnable> prev = queues.putIfAbsent(key, result);
		return prev == null ? result : prev;
	}
	
}
