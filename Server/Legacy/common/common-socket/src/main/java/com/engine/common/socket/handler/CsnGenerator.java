package com.engine.common.socket.handler;

import java.util.concurrent.atomic.AtomicInteger;

/** 
 * 序列号生成器，用于生成以1开始的正整数，当到达最大值将重新从1开始自增
 *
 */

public class CsnGenerator {
	
	/** 开始值 */
	private static final int START = 1;

	private AtomicInteger sequence = new AtomicInteger(START);

	/**
	 * 获取下一个序列号
	 * 
	 * @return
	 */
	public int next() {
		int result = 0;
		while (result <= 0) {
			result = sequence.getAndIncrement();
			if (result == Integer.MAX_VALUE) {
				sequence.compareAndSet(Integer.MIN_VALUE, START);
			}
		}
		return result;
	}
}
