package com.engine.common.ramcache.orm.exception;

/**
 * 实体不存在异常
 * 
 */
public class EntityNotFoundException extends OrmException {

	private static final long serialVersionUID = -2034117261159389354L;

	public EntityNotFoundException() {
		super();
	}

	public EntityNotFoundException(String message, Throwable cause) {
		super(message, cause);
	}

	public EntityNotFoundException(String message) {
		super(message);
	}

	public EntityNotFoundException(Throwable cause) {
		super(cause);
	}
}
