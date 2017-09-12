package com.engine.common.ramcache.persist;

import java.util.ArrayList;
import java.util.Collection;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.util.Assert;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.exception.ConfigurationException;
import com.engine.common.ramcache.exception.StateException;
import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.utils.time.DateUtils;

/**
 * 定时持久化处理器<br/>
 * 该持久化处理器会以CRON表达指定的时间进行入库操作，未到达入库时间点的持久化操作会进行累积并去重。
 * 
 */
@SuppressWarnings("rawtypes")
public class TimingPersister implements Persister {

	private static final Logger logger = LoggerFactory.getLogger(TimingPersister.class);

	/** 正在等待更新的信息缓存 */
	private ConcurrentHashMap<String, Element> elements = new ConcurrentHashMap<String, Element>();
	/** 对应实体的处理监听器 */
	private ConcurrentHashMap<Class<? extends IEntity>, Listener> listeners = new ConcurrentHashMap<Class<? extends IEntity>, Listener>();
	/** 初始化标识 */
	private boolean initialize;

	/** 名称 */
	private String name;
	/** 消费线程 */
	private TimingConsumer consumer;
	
	// 实现接口的方法

	/** 初始化方法 */
	@Override
	public synchronized void initialize(String name, Accessor accessor, String cron) {
		if (initialize) {
			throw new ConfigurationException("重复初始化异常");
		}
		Assert.notNull(accessor, "持久层数据访问器不能为 null");
		try {
			DateUtils.getNextTime(cron, new Date());
			this.elements = new ConcurrentHashMap<String, Element>();
			this.consumer = new TimingConsumer(name, cron, accessor, this);
			this.name = name;
			initialize = true;
		} catch (Exception e) {
			throw new ConfigurationException("定时持久化处理器[" + name + "]初始化失败:" + e.getMessage());
		}
	}
	
	/** 添加监听器 */
	@Override
	public void addListener(Class<? extends IEntity> clz, Listener listener) {
		if (listener == null) {
			throw new ConfigurationException("被添加的监听器实例不能为空");
		}
		listeners.put(clz, listener);
	}

	/** 获取监听器 */
	public Listener getListener(Class<? extends IEntity> clz) {
		return listeners.get(clz);
	}

	/**
	 * 将指定元素插入此队列中，将等待可用的空间（如果有必要）。
	 * @param element 被添加元素(元素为null时直接返回)
	 */
	@Override
	public void put(Element element) {
		if (element == null) {
			return;
		}
		if (stop) {
			FormattingTuple message = MessageFormatter.format("实体更新队列已经停止,更新元素[{}]将不被接受", element);
			logger.error(message.getMessage());
			throw new StateException(message.getMessage());
		}
		
		String id = element.getIdentity();
		rwLock.readLock().lock();
		Lock lock = lockIdLock(id);
		try {
			Element prev = elements.get(id);
			
			// 更新元素不存在的场景
			if (prev == null) {
				elements.put(id, element);
				return;
			}
			
			// 更新元素已经存在的场景
			EventType prevType = prev.getType();
			if (!prev.update(element)) {
				elements.remove(id);
			} else {
				// 当从REMOVE合并为UPDATE的时候要让监听器通知缓存服务将内部的临时失效主键清除
				if (prevType == EventType.REMOVE && prev.getType() == EventType.UPDATE) {
					Listener listener = getListener(element.getEntityClass());
					if (listener != null) {
						listener.notify(EventType.REMOVE, true, prev.getId(), null, null);
					}
				}
			}
		} finally {
			releaseIdLock(id, lock);
			rwLock.readLock().unlock();
		}
	}
	
	@Override
	public Map<String, String> getInfo() {
		HashMap<String, String> result = new HashMap<String, String>();
		result.put("size", Integer.toString(size()));
		result.put("state", consumer.getState().name());
		result.put("nextTime", DateUtils.date2String(consumer.getNextTime(), DateUtils.PATTERN_DATE_TIME));
		return result;
	}

	// 自身的方法
	
	private ReentrantReadWriteLock rwLock = new ReentrantReadWriteLock();
	
	Collection<Element> clearElements() {
		rwLock.writeLock().lock();
		try {
			ArrayList<Element> result = new ArrayList<Element>(elements.values());
			elements.clear();
			return result;
		} finally {
			rwLock.writeLock().unlock();
		}
	}
	
	/**
	 * 获取队列中的元素数量
	 * @return
	 */
	public int size() {
		return elements.size();
	}
	
	/** 停止状态 */
	private volatile boolean stop;

	/** 停止更新队列并等待全部入库完成 */
	public void shutdown() {
		if(logger.isDebugEnabled()) {
			logger.debug("停机回写队列[{}]开始...", name);
		}
		stop = true;
		consumer.stop();
		while(consumer.getState() != TimingConsumerState.STOPPED) {
			Thread.yield();
		}
		logger.error("停机回写队列[{}]完成...", name);
	}
	
	public void flush() {
		consumer.interrupt();
	}

	public TimingConsumer getConsumer() {
		return consumer;
	}

	// 内部方法
	
	/** 队列内更新元素的操作锁 */
	private ConcurrentHashMap<String, ReentrantLock> locks = new ConcurrentHashMap<String, ReentrantLock>();

	/** 获取标识锁对象 */
	private Lock lockIdLock(String id) {
		// 获取当前的主键写锁
		ReentrantLock lock = locks.get(id);
		if (lock == null) {
			lock = new ReentrantLock();
			ReentrantLock prevLock = locks.putIfAbsent(id, lock);
			lock = prevLock != null ? prevLock : lock;
		}
		lock.lock();
		return lock;
	}

	/** 释放标识锁 */
	private void releaseIdLock(String id, Lock lock) {
		lock.unlock();
		locks.remove(id);
	}
}
