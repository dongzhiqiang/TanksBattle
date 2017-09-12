package com.engine.common.ramcache.orm.exception;

/**
 * 存储层异常的抽象父类，用于统一管理存储层异常
 * 
 */
public abstract class OrmException extends RuntimeException {

	private static final long serialVersionUID = -5480573723994246089L;

	public OrmException() {
		super();
	}

	public OrmException(String message, Throwable cause) {
		super(message, cause);
	}

	public OrmException(String message) {
		super(message);
	}

	public OrmException(Throwable cause) {
		super(cause);
	}

}
