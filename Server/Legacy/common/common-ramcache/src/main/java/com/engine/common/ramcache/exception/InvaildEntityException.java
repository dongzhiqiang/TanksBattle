package com.engine.common.ramcache.exception;

/**
 * 无效的实体异常<br/>
 * 当实体主键已经被占用时抛出
 * 
 */
public class InvaildEntityException extends CacheException {

	private static final long serialVersionUID = 1622804717507686589L;

	public InvaildEntityException() {
		super();
	}

	public InvaildEntityException(String message, Throwable cause) {
		super(message, cause);
	}

	public InvaildEntityException(String message) {
		super(message);
	}

	public InvaildEntityException(Throwable cause) {
		super(cause);
	}
}
