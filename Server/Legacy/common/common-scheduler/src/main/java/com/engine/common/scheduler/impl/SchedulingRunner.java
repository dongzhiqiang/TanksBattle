package com.engine.common.scheduler.impl;

import java.util.Date;
import java.util.concurrent.Delayed;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ScheduledFuture;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.scheduler.ScheduledTask;
import com.engine.common.scheduler.Trigger;

/**
 * 计划任务执行者
 * 
 */
@SuppressWarnings("rawtypes")
public class SchedulingRunner implements ScheduledTask, ScheduledFuture<Object> {
	
	private static final Logger log = LoggerFactory.getLogger(SchedulingRunner.class);

	/** 委托的执行任务  */
	private final ScheduledTask delegate;
	/** 触发器，用于获取下次执行时间 */
	private final Trigger trigger;
	/** 任务上下文，用于记录任务完成时间 */
	private final SimpleTaskContext taskContext = new SimpleTaskContext();
	
	private final FixScheduledThreadPoolExecutor executor;
	private volatile ScheduledFuture currentFuture;
	private volatile Date scheduledTime;

	public SchedulingRunner(ScheduledTask delegate, Trigger trigger, FixScheduledThreadPoolExecutor executor) {
		this.delegate = delegate;
		this.trigger = trigger;
		this.executor = executor;
	}

	private final Object mutex = new Object();

	public ScheduledFuture schedule() {
		synchronized (this.mutex) {
			this.scheduledTime = this.trigger.nextTime(this.taskContext);
			if (this.scheduledTime == null) {
				return null;
			}
			long initialDelay = this.scheduledTime.getTime() - System.currentTimeMillis();
			this.currentFuture = this.executor.schedule(this, initialDelay, TimeUnit.MILLISECONDS);
			// 日志信息
			if (log.isDebugEnabled()) {
				log.debug("任务[{}]的下次计划执行时间[{}]", getName(), this.scheduledTime);
			}
			return this;
		}
	}

	// 委托的 Task 方法
	@Override
	public void run() {
		Date actualExecutionTime = new Date();
		try {
			delegate.run();
		} catch (Exception e) {
			log.error("定时任务:{},执行异常", this.getName(), e);
		}
		Date completionTime = new Date();
		synchronized (this.mutex) {
			this.taskContext.update(this.scheduledTime, actualExecutionTime,
					completionTime);
		}
		if (!this.currentFuture.isCancelled()) {
			schedule();
		}
	}

	@Override
	public String getName() {
		return delegate.getName();
	}
	
	// 委托的 ScheduledFuture 方法
	
	public boolean cancel(boolean mayInterruptIfRunning) {
		return this.currentFuture.cancel(mayInterruptIfRunning);
	}

	public boolean isCancelled() {
		return this.currentFuture.isCancelled();
	}

	public boolean isDone() {
		return this.currentFuture.isDone();
	}

	public Object get() throws InterruptedException, ExecutionException {
		return this.currentFuture.get();
	}

	public Object get(long timeout, TimeUnit unit) throws InterruptedException, ExecutionException,
			TimeoutException {
		return this.currentFuture.get(timeout, unit);
	}

	public long getDelay(TimeUnit unit) {
		return this.currentFuture.getDelay(unit);
	}

	public int compareTo(Delayed other) {
		if (this == other) {
			return 0;
		}
		long diff = getDelay(TimeUnit.MILLISECONDS) - other.getDelay(TimeUnit.MILLISECONDS);
		return (diff == 0 ? 0 : ((diff < 0) ? -1 : 1));
	}

	@Override
	public boolean equals(Object obj) {
		return (this == obj);
	}

}
