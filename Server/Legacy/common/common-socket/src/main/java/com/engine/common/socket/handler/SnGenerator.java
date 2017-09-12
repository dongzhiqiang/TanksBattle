package com.engine.common.socket.handler;

import java.util.concurrent.atomic.AtomicLong;

/**
 * 序列号生成器，用于生成以1开始的正整数，当到达最大值将重新从1开始自增
 * 
 */
public class SnGenerator {
	
	/** 开始值 */
	private static final long START = 1;
	
	private AtomicLong sequence = new AtomicLong(START);

	/**
	 * 获取下一个序列号
	 * @return
	 */
	public long next() {
		long result = 0;
		while (result <= 0) {
			result = sequence.getAndIncrement();
			if (result == Long.MAX_VALUE) {
				sequence.compareAndSet(Long.MIN_VALUE, START);
			}
		}
		return result;
	}
}
