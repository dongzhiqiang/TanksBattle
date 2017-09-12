package com.engine.common.protocol.exception;

/** 
 * 对象标识定义异常
 *
 *
 */

public class ObjecIndexDefException extends RuntimeException{
	
	private static final long serialVersionUID = -5480536480972325820L;

	public ObjecIndexDefException(int index){
		super("对象标识[" + index + "]");
	}
	
	public ObjecIndexDefException(int index,String name){
		super("对象标识[" + index + "]与当前类[" + name + "]不匹配");
	}
}
