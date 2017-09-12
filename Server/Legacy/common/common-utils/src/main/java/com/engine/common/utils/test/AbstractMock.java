package com.engine.common.utils.test;

import java.util.Queue;
import java.util.concurrent.ConcurrentLinkedQueue;

/**
 * 测试用的假对象抽象
 * 
 * 
 */
public abstract class AbstractMock {

	protected Queue<Object> queue = new ConcurrentLinkedQueue<Object>(); 

	/**
	 * 获取设置的返回值
	 * @return
	 */
	public Object getRetValue() {
		Object e = queue.poll();
		if(e instanceof RuntimeException) {
			throw (RuntimeException)e;
		}
		return e;
	}

	// Getter and Setter ...

	public void addException(RuntimeException exception) {
		this.queue.add(exception);
	}

	public void addRetValue(Object retValue) {
		this.queue.add(retValue);
	}

}
