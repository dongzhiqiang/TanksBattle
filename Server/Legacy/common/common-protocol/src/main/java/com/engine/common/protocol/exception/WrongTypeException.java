package com.engine.common.protocol.exception;

import java.io.IOException;

/**
 * 错误的代理类型
 * 
 * 
 */
public class WrongTypeException extends IOException {
	private static final long serialVersionUID = 4014279564958034497L;

	/**
	 * 未知类型
	 * 
	 * @param type
	 */
	public WrongTypeException(int type) {
		super("未定义代理类型[" + type + "]");
	}

	public WrongTypeException(int need, int except) {
		super("代理类型[" + need + "]与当前类型[" + except + "]不匹配");
	}
}
