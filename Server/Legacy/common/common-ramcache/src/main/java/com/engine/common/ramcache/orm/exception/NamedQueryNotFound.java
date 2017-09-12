package com.engine.common.ramcache.orm.exception;


/**
 * 命名查询不存在异常
 * 
 */
public class NamedQueryNotFound extends OrmException {

	private static final long serialVersionUID = -4358843497647274575L;

	public NamedQueryNotFound() {
		super();
	}

	public NamedQueryNotFound(String message, Throwable cause) {
		super(message, cause);
	}

	public NamedQueryNotFound(String message) {
		super(message);
	}

	public NamedQueryNotFound(Throwable cause) {
		super(cause);
	}

}
