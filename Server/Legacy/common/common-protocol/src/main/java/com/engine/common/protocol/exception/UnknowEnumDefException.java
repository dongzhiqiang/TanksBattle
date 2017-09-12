package com.engine.common.protocol.exception;

import java.io.IOException;

/**
 * 未定义的传输枚举类型
 * 
 * 
 *
 */
public class UnknowEnumDefException extends IOException{
	private static final long serialVersionUID = 4895297111772939299L;

	public UnknowEnumDefException(int rawType) {
		super("未定义的传输枚举类型[" + rawType + "]");
	}

	public UnknowEnumDefException(Class<? extends Object> clz) {
		super("未定义的传输枚举类型[" + clz.getName() + "]");
	}
	
}
