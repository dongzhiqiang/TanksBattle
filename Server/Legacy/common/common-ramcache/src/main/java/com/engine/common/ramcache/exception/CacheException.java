package com.engine.common.ramcache.exception;

/**
 * 内存缓存层的异常父类
 * 
 */
public abstract class CacheException extends RuntimeException {

	private static final long serialVersionUID = -1115472626914115272L;

	public CacheException() {
		super();
	}

	public CacheException(String message, Throwable cause) {
		super(message, cause);
	}

	public CacheException(String message) {
		super(message);
	}

	public CacheException(Throwable cause) {
		super(cause);
	}
}
