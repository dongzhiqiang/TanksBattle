package com.engine.common.ramcache.persist;

import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.LinkedBlockingQueue;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.util.Assert;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.exception.ConfigurationException;
import com.engine.common.ramcache.exception.StateException;
import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.utils.collection.ConcurrentHashSet;

/**
 * 队列持久化处理器<br/>
 * 允许选择是否开启重复更新抑制
 * 
 */
@SuppressWarnings("rawtypes")
public class QueuePersister implements Persister {

	private static final Logger logger = LoggerFactory.getLogger(QueuePersister.class);
	
	private static final String SPLIT = ":";

	/** 名称 */
	private String name;
	/** 更新队列 */
	private BlockingQueue<Element> queue;
	/** 对应实体的处理监听器 */
	private ConcurrentHashMap<Class<? extends IEntity>, Listener> listeners = new ConcurrentHashMap<Class<? extends IEntity>, Listener>();
	/** 初始化标识 */
	private boolean initialize;
	/** 抑制重复更新状态 */
	private boolean flag;
	/** 正在等待更新的信息缓存 */
	private ConcurrentHashSet<String> updating = new ConcurrentHashSet<String>();

	/** 消费线程 */
	private QueueConsumer consumer;

	/** 初始化方法 */
	@Override
	public synchronized void initialize(String name, Accessor accessor, String config) {
		if (initialize) {
			throw new ConfigurationException("重复初始化异常");
		}
		Assert.notNull(accessor, "持久层数据访问器不能为 null");

		try {
			String[] array = config.split(SPLIT);
			int size = Integer.parseInt(array[0]);
			if (size >= 0) {
				this.queue = new ArrayBlockingQueue<Element>(size);
			} else {
				this.queue = new LinkedBlockingQueue<Element>();
			}
			
			this.flag = Boolean.parseBoolean(array[1]);
			this.name = name;
			this.consumer = new QueueConsumer(name, accessor, this.queue, this);
			this.initialize = true;
		} catch (Exception e) {
			throw new ConfigurationException("持久化处理器[" + name + "]初始化异常:" + e.getMessage());
		}
	}
	
	/** 添加监听器 */
	public void addListener(Class<? extends IEntity> clz, Listener listener) {
		if (listener == null) {
			throw new ConfigurationException("被添加的监听器实例不能为空");
		}
		listeners.put(clz, listener);
	}
	
	/**
	 * 将指定元素插入此队列中，将等待可用的空间（如果有必要）。
	 * @param element 被添加元素(元素为null时直接返回)
	 */
	public void put(Element element) {
		if (element == null) {
			return;
		}
		if (stop) {
			FormattingTuple message = MessageFormatter.format("实体更新队列[{}]已经停止,更新元素[{}]将不被接受", name, element);
			logger.error(message.getMessage());
			throw new StateException(message.getMessage());
		}
		
		try {
			if (flag && element.getType() == EventType.UPDATE) {
				// 仅当更新事件时才做抑制
				String identity = element.getIdentity();
				if (updating.contains(identity)) {
					return;
				}
				updating.add(identity);
			}
			queue.put(element);
		} catch (InterruptedException e) {
			// 这种情况是不应该会出现的
			logger.error("等待将元素[{}]添加到队列时被打断", new Object[] { element, e });
			if (element.getType() == EventType.UPDATE) {
				updating.remove(element.getIdentity());
			}
		}
	}

	@Override
	public Listener getListener(Class<? extends IEntity> clz) {
		return listeners.get(clz);
	}
	
	@Override
	public Map<String, String> getInfo() {
		HashMap<String, String> result = new HashMap<String, String>();
		result.put("size", Integer.toString(size()));
		result.put("error", Integer.toString(consumer.getError()));
		return result;
	}
	
	public void removeUpdating(String identity) {
		if (flag) {
			updating.remove(identity);
		}
	}

	/**
	 * 获取队列中的元素数量
	 * @return
	 */
	public int size() {
		return queue.size();
	}

	/** 停止状态 */
	private volatile boolean stop;

	/** 停止更新队列并等待全部*/
	public void shutdown() {
		if(logger.isDebugEnabled()) {
			logger.debug("停机回写队列[{}]开始...", name);
		}
		stop = true;
		for (;;) {
			if (queue.isEmpty()) {
				break;
			}
			Thread.yield();
		}
		logger.error("停机回写队列[{}]完成...", name);
	}

}

