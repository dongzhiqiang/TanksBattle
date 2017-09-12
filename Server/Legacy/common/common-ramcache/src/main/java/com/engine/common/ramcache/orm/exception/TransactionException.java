package com.engine.common.ramcache.orm.exception;

/**
 * 事务异常
 * 
 */
public class TransactionException extends OrmException {

	private static final long serialVersionUID = -8396525701135532677L;

	public TransactionException() {
		super();
	}

	public TransactionException(String message, Throwable cause) {
		super(message, cause);
	}

	public TransactionException(String message) {
		super(message);
	}

	public TransactionException(Throwable cause) {
		super(cause);
	}
}
