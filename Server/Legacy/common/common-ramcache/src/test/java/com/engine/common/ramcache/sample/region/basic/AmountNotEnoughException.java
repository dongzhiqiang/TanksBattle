package com.engine.common.ramcache.sample.region.basic;

/**
 * 数量不足异常(测试用)
 * 
 */
public class AmountNotEnoughException extends Exception {

	private static final long serialVersionUID = 1492216476041136777L;

	private final int value;
	
	public AmountNotEnoughException(int value) {
		this.value = value;
	}

	public int getValue() {
		return value;
	}

}
