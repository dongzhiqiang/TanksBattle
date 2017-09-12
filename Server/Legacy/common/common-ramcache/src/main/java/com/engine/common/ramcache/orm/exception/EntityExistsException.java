package com.engine.common.ramcache.orm.exception;

/**
 * 实体已经存在异常<br/>
 * 当存储层持久化一个已经存在的实体时抛出(即实体的主键已经被占用)
 * 
 */
public class EntityExistsException extends OrmException {

	private static final long serialVersionUID = -4856143234643053387L;

	public EntityExistsException() {
		super();
	}

	public EntityExistsException(String message, Throwable cause) {
		super(message, cause);
	}

	public EntityExistsException(String message) {
		super(message);
	}

	public EntityExistsException(Throwable cause) {
		super(cause);
	}

}
