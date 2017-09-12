package com.engine.common.scheduler.impl;

import java.util.Date;
import java.util.concurrent.Delayed;
import java.util.concurrent.TimeUnit;

/**
 * 抽象的延迟对象, 定义了开始时间和延迟时长
 * 
 * 
 *
 */
public class DelayedElement implements Delayed {
	
	/** 开始时间 */
	private Date start;
	/** 时长，单位秒 */
	private long len;
	
	/**
	 * 获得开始时间
	 */
	public Date getStart() {
		return start;
	}

	/**
	 * 获得时长
	 */
	public long getLen() {
		return len;
	}
	
	/**
	 * 增加时长到原有的时长
	 * @param add
	 */
	public void addLen(long add) {
		len += add;
	}
	
	/**
	 * 开始时间
	 */
	protected void setStart(Date start) {
		this.start = start;
	}

	/** 时长，单位秒 */
	protected void setLen(long len) {
		this.len = len;
	}

	@Override
	public long getDelay(TimeUnit timeUnit) {
		long now = System.currentTimeMillis();
		long millis = start == null ? 0 : start.getTime();
		long delay = (millis + len * 1000) - now;
		switch (timeUnit) {
		case MILLISECONDS:
			return delay;
		case SECONDS:
			return TimeUnit.MILLISECONDS.toSeconds(delay);
		case MINUTES:
			return TimeUnit.MILLISECONDS.toMinutes(delay);
		case HOURS:
			return TimeUnit.MILLISECONDS.toHours(delay);
		case DAYS:
			return TimeUnit.MILLISECONDS.toDays(delay);
		case MICROSECONDS:
			return TimeUnit.MILLISECONDS.toMicros(delay);
		case NANOSECONDS:
			return TimeUnit.MILLISECONDS.toNanos(delay);
		}
		return delay;
	}
	
	@Override
	public int compareTo(Delayed o) {
		long delay1 = this.getDelay(TimeUnit.MILLISECONDS);
		long delay2 = o.getDelay(TimeUnit.MILLISECONDS);
		int result = Long.valueOf(delay1).compareTo(Long.valueOf(delay2));
		if (result != 0) {
			return result;
		}
		if (this.equals(o)) {
			return 0;
		}
		if (this.hashCode() < o.hashCode()) {
			return -1;
		} else {
			return 1;
		}
	}

}
