package com.engine.common.socket;

import java.util.concurrent.TimeUnit;

import org.apache.mina.filter.executor.ExecutorFilter;

import com.engine.common.utils.thread.NamedThreadFactory;

/**
 * 执行器配置信息<br/>
 * 用于创建一个受配置的{@link ExecutorFilter}
 * 
 * 
 */
public class ExecutorConfig {
	
	public static final String NAME = "executor";

	private int min;
	private int max;
	private long idel;

	public static ExecutorConfig valueOf(int min, int max, long idel) {
		ExecutorConfig result = new ExecutorConfig();
		result.min = min;
		result.max = max;
		result.idel = idel;
		return result;
	}

	/**
	 * 创建 {@link ExecutorFilter} 并返回
	 * 
	 * @return
	 */
	public ExecutorFilter build() {
		ThreadGroup group = new ThreadGroup("通信模块");
		NamedThreadFactory threadFactory = new NamedThreadFactory(group, "通信线程");
		return new ExecutorFilter(min, max, idel, TimeUnit.MILLISECONDS, threadFactory);
	}
	
	// Getter and Setter ...

	public int getMin() {
		return min;
	}

	public int getMax() {
		return max;
	}

	public long getIdel() {
		return idel;
	}

}
