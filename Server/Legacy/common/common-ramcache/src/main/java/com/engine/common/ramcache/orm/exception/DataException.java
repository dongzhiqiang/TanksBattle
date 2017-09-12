package com.engine.common.ramcache.orm.exception;

/**
 * 实体数据异常,当持久化实体时或更新实体时,实体的数据与存储层要求不符合时抛出
 * 
 */
public class DataException extends OrmException {

	private static final long serialVersionUID = -8396525701135532677L;

	public DataException() {
		super();
	}

	public DataException(String message, Throwable cause) {
		super(message, cause);
	}

	public DataException(String message) {
		super(message);
	}

	public DataException(Throwable cause) {
		super(cause);
	}
}
