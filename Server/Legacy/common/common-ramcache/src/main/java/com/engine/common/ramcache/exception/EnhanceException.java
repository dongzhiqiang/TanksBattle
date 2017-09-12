package com.engine.common.ramcache.exception;

import com.engine.common.ramcache.IEntity;

/**
 * 类增强异常
 * 
 */
@SuppressWarnings("rawtypes")
public class EnhanceException extends CacheException {

	private static final long serialVersionUID = -8348735808262762811L;

	/** 尝试被增强的实体 */
	private final IEntity entity;

	public EnhanceException(IEntity entity) {
		super();
		this.entity = entity;
	}

	public EnhanceException(IEntity entity, String message, Throwable cause) {
		super(message, cause);
		this.entity = entity;
	}

	public EnhanceException(IEntity entity, String message) {
		super(message);
		this.entity = entity;
	}

	public EnhanceException(IEntity entity, Throwable cause) {
		super(cause);
		this.entity = entity;
	}

	public IEntity getEntity() {
		return entity;
	}

}
