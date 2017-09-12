package com.engine.common.ramcache.persist;

import java.util.Collection;
import java.util.Date;
import java.util.concurrent.atomic.AtomicInteger;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.utils.time.DateUtils;

public class TimingConsumer implements Runnable {
	
	private static final Logger logger = LoggerFactory.getLogger(TimingConsumer.class);

	/** 更新队列名 */
	private String name;
	/** 定期入库CRON */
	private String cron;
	/** 持久层的存储器 */
	private Accessor accessor;
	/** 实体持久化缓存 */
	private TimingPersister owner;
	/** 当前消费者线程自身 */
	private Thread me;
	/** 状态 */
	private TimingConsumerState state;
	private volatile boolean stoped;
	/** 下次执行的时间 */
	private Date nextTime;
	/** 错误计数器 */
	private AtomicInteger error = new AtomicInteger();
	
	public TimingConsumer(String name, String cron, Accessor accessor, TimingPersister owner) {
		this.name = name;
		this.cron = cron;
		this.accessor = accessor;
		this.owner = owner;
		
		this.me = new Thread(this, "持久化[" + name + ":定时]");
		me.setDaemon(true);
		me.start();
	}
	
	@Override
	public void run() {
		while(!stoped) {
			Collection<Element> elements = null;
			synchronized (me) {
				state = TimingConsumerState.WAITING;
				if (!interrupted) {
					nextTime = DateUtils.getNextTime(cron, new Date());
					if (logger.isDebugEnabled()) {
						logger.debug("定时入库[{}]的下个执行时间点为[{}]", name, DateUtils.date2String(nextTime, DateUtils.PATTERN_DATE_TIME));
					}
					try {
						long ms = nextTime.getTime() - System.currentTimeMillis();
						if (ms > 0) {
							me.wait(ms);
						}
					} catch (InterruptedException e) {
						if (logger.isDebugEnabled()) {
							logger.debug("定时入库[{}]被迫使立即执行[{}]", name, DateUtils.date2String(new Date(), DateUtils.PATTERN_DATE_TIME));
						}
					}
				}
				elements = owner.clearElements();
				interrupted = false;
				state = TimingConsumerState.RUNNING;
			}
			Date start = new Date();
			if (logger.isDebugEnabled()) {
				logger.debug("定时入库[{}]开始[{}]执行", name, DateUtils.date2String(start, DateUtils.PATTERN_DATE_TIME));
			}
			persist(elements);
			if (logger.isDebugEnabled()) {
				logger.debug("定时入库[{}]入库[{}]条数据耗时[{}]", 
						new Object[]{name, elements.size(), System.currentTimeMillis() - start.getTime()});
			}
		}
		synchronized (me) {
			Date start = new Date();
			Collection<Element> elements = owner.clearElements();
			persist(elements);
			if (logger.isDebugEnabled()) {
				logger.debug("定时入库[{}]补全入库数据[{}]条数据耗时[{}]", 
						new Object[]{name, elements.size(), System.currentTimeMillis() - start.getTime()});
			}
			state = TimingConsumerState.STOPPED;
			interrupted = false;
		}
	}

	@SuppressWarnings({ "rawtypes", "unchecked" })
	private void persist(Collection<Element> elements) {
		for (Element element : elements) {
			try {
				Class clz = element.getEntityClass();
				switch (element.getType()) {
				case SAVE:
					accessor.save(clz, element.getEntity());
					break;
				case REMOVE:
					accessor.remove(clz, element.getId());
					break;
				case UPDATE:
					accessor.update(clz, element.getEntity());
					break;
				}
				
				Listener listener = owner.getListener(clz);
				if (listener != null) {
					listener.notify( element.getType(), true, element.getId(), element.getEntity(), null);
				}
			} catch (RuntimeException e) {
				error.getAndIncrement();
				logger.error("实体更新队列[{}]处理元素[{}]时出现异常:{}", new Object[] { name, element, e.getMessage() });
				Listener listener = owner.getListener(element.getEntityClass());
				if (listener != null) {
					listener.notify(element.getType(), false, element.getId(), element.getEntity(), e);
				}
			} catch (Exception e) {
				error.getAndIncrement();
				if (element == null) {
					logger.error("获取更新队列元素时线程被非法打断", e);
				} else {
					logger.error("更新队列处理出现未知异常", e);
				}
			}
		}
	}
	
	public TimingConsumerState getState() {
		synchronized (me) {
			return state;
		}
	}

	public void stop() {
		if (logger.isDebugEnabled()) {
			logger.debug("定时入库[{}]收到停止通知", name);
		}
		synchronized (me) {
			stoped = true;
			interrupted = true;
			me.notify();
		}
		while (interrupted) {
			Thread.yield();
		}
	}
	
	private volatile boolean interrupted;

	public void interrupt() {
		synchronized (me) {
			interrupted = true;
			me.notify();
		}
		while (interrupted) {
			Thread.yield();
		}
	}

	public Date getNextTime() {
		return nextTime;
	}

	public int getError() {
		return error.get();
	}
}
