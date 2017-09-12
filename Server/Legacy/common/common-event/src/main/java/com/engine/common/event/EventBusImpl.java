package com.engine.common.event;

import java.lang.management.ManagementFactory;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CopyOnWriteArraySet;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.RejectedExecutionException;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

import javax.annotation.PostConstruct;
import javax.management.MBeanServer;
import javax.management.ObjectName;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.context.ApplicationListener;
import org.springframework.context.event.ContextClosedEvent;
import org.springframework.stereotype.Component;

import com.engine.common.utils.thread.NamedThreadFactory;

/**
 * 事件总线接口的实现类
 * 
 */
@Component
public class EventBusImpl implements EventBus, EventBusImplMBean, ApplicationListener<ContextClosedEvent> {

	private static final Logger logger = LoggerFactory.getLogger(EventBusImpl.class);

	/** 注册的事件接收者 */
	private ConcurrentHashMap<String, CopyOnWriteArraySet<Receiver<?>>> receivers = new ConcurrentHashMap<String, CopyOnWriteArraySet<Receiver<?>>>();

	@Autowired(required = false)
	@Qualifier("event_queue_size")
	private Integer queueSize = 10000;
	private BlockingQueue<Event<?>> eventQueue;

	@Autowired(required = false)
	@Qualifier("event_pool_size")
	private Integer poolSize = 5;
	@Autowired(required = false)
	@Qualifier("event_pool_max_size")
	private Integer poolMaxSize = 10;
	@Autowired(required = false)
	@Qualifier("event_pool_alive_time")
	private Integer poolKeepAlive = 60;

	private ExecutorService pool;

	/** 事件消费线程执行代码 */
	private Runnable consumerRunner = new Runnable() {

		@Override
		public void run() {
			while (true) {
				try {
					Event<?> event = eventQueue.take();
					String name = event.getName();
					if (!receivers.containsKey(name)) {
						logger.warn("事件[{}]没有对应的接收器", name);
						continue;
					}
					for (Receiver<?> receiver : receivers.get(name)) {
						Runnable runner = createRunner(receiver, event);
						try {
							pool.submit(runner);
						} catch (RejectedExecutionException e) {
							logger.error("事件线程池已满，请尽快调整配置参数");
							onRejected(receiver, event);
						}
					}
				} catch (InterruptedException e) {
					logger.error("获取事件对象时出现异常", e);
				}
			}
		}

		@SuppressWarnings({ "rawtypes", "unchecked" })
		private void onRejected(Receiver receiver, Event event) {
			try {
				receiver.onEvent(event);
			} catch (ClassCastException e) {
				logger.error("事件[" + event.getName() + "]对象类型不符合接收器声明", e);
			} catch (Throwable t) {
				logger.error("事件[" + event.getName() + "]处理时发生异常", t);
			}
		}

		@SuppressWarnings({ "rawtypes", "unchecked" })
		private Runnable createRunner(final Receiver receiver, final Event event) {
			return new Runnable() {
				@Override
				public void run() {
					try {
						receiver.onEvent(event);
					} catch (ClassCastException e) {
						logger.error("事件[" + event.getName() + "]对象类型不符合接收器[" + receiver.getClass() + "]声明", e);
					} catch (Throwable t) {
						logger.error("事件[" + event.getName() + "]处理器[" + receiver.getClass() + "]运行时发生异常", t);
					}
				}
			};
		}
	};

	/**
	 * 根据配置初始化
	 */
	@PostConstruct
	protected void initialize() {
		ThreadGroup threadGroup = new ThreadGroup("事件模块");
		NamedThreadFactory threadFactory = new NamedThreadFactory(threadGroup, "事件处理");
		eventQueue = new LinkedBlockingQueue<Event<?>>(queueSize);
		pool = new ThreadPoolExecutor(poolSize, poolMaxSize, poolKeepAlive, TimeUnit.SECONDS,
				new LinkedBlockingQueue<Runnable>(queueSize), threadFactory);
		// 创建并启动事件消费线程
		Thread consumer = new Thread(consumerRunner, "消费事件后台线程");
		consumer.setDaemon(true);
		consumer.start();

		// 注册MBean
		try {
			MBeanServer mbs = ManagementFactory.getPlatformMBeanServer();
			ObjectName name = new ObjectName("com.engine.common:type=EventBusMBean");
			mbs.registerMBean(this, name);
		} catch (Exception e) {
			logger.error("注册[common-event]的JMX管理接口失败", e);
		}
	}

	/** 销毁方法 */
	@Override
	public void onApplicationEvent(ContextClosedEvent event) {
		shutdown();
	}

	/** 停止状态 */
	private volatile boolean stop;

	/**
	 * 关闭事件总线，阻塞方法会等待总线中的全部事件都发送完后再返回
	 */
	public void shutdown() {
		if (isStop())
			return;
		stop = true;
		for (;;) {
			if (eventQueue.isEmpty()) {
				break;
			}
			Thread.yield();
		}
		pool.shutdown();
		for (;;) {
			if (pool.isTerminated()) {
				break;
			}
			Thread.yield();
		}

	}

	/**
	 * 检查该事件总线是否已经停止服务
	 * @return
	 */
	public boolean isStop() {
		return stop;
	}

	@Override
	public void post(Event<?> event) {
		if (event == null) {
			throw new IllegalArgumentException("事件对象不能为空");
		}
		if (stop) {
			throw new IllegalStateException("事件总线已经停止，不能再接收事件");
		}
		try {
			eventQueue.put(event);
		} catch (InterruptedException e) {
			logger.error("在添加事件对象时产生异常", e);
		}
	}

	@Override
	public void register(String name, Receiver<?> receiver) {
		if (name == null || receiver == null) {
			throw new IllegalArgumentException("事件名和接收者均不能为空");
		}

		CopyOnWriteArraySet<Receiver<?>> set = receivers.get(name);
		if (set == null) {
			set = new CopyOnWriteArraySet<Receiver<?>>();
			CopyOnWriteArraySet<Receiver<?>> prev = receivers.putIfAbsent(name, set);
			set = prev != null ? prev : set;
		}

		set.add(receiver);
	}

	@Override
	public void unregister(String name, Receiver<?> receiver) {
		if (name == null || receiver == null) {
			throw new IllegalArgumentException("事件名和接收者均不能为空");
		}

		CopyOnWriteArraySet<Receiver<?>> set = receivers.get(name);
		if (set != null) {
			set.remove(receiver);
		}
	}

	@SuppressWarnings({ "rawtypes", "unchecked" })
	@Override
	public void syncPost(Event<?> event) {
		String name = event.getName();
		if (!receivers.containsKey(name)) {
			logger.warn("事件'{}'没有对应的接收器", name);
			return;
		}
		for (Receiver receiver : receivers.get(name)) {
			try {
				receiver.onEvent(event);
			} catch (Exception e) {
				logger.error("事件[" + event.getName() + "]处理时发生异常", e);
			}
		}
	}

	// JMX管理接口的实现方法

	@Override
	public int getEventQueueSize() {
		return eventQueue.size();
	}

	@Override
	public int getPoolActiveCount() {
		return ((ThreadPoolExecutor) pool).getActiveCount();
	}

	@Override
	public int getPollQueueSize() {
		return ((ThreadPoolExecutor) pool).getQueue().size();
	}

	@SuppressWarnings("rawtypes")
	@Override
	public List<String> getEvents() {
		List<Event> dump = new ArrayList<Event>(eventQueue);
		ArrayList<String> result = new ArrayList<String>(dump.size());
		for (Event e : dump) {
			result.add(e.getName());
		}
		return result;
	}
}
