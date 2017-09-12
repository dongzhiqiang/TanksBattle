package com.engine.common.ramcache.exception;

/**
 * 唯一键重复异常
 * 
 */
public class UniqueFieldException extends CacheException {

	private static final long serialVersionUID = 9191882390957957633L;

	public UniqueFieldException() {
		super();
	}

	public UniqueFieldException(String message, Throwable cause) {
		super(message, cause);
	}

	public UniqueFieldException(String message) {
		super(message);
	}

	public UniqueFieldException(Throwable cause) {
		super(cause);
	}

}
