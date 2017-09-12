package com.game.module.migration;

/**
 * 当重试达到最大次数时抛出的异常
 * 
 *
 */
class MaximumRetryException extends RuntimeException {

	private static final long serialVersionUID = -4791551041298800926L;

	public MaximumRetryException() {
		super();
	}

	public MaximumRetryException(String message, Throwable cause) {
		super(message, cause);
	}

	public MaximumRetryException(String message) {
		super(message);
	}

	public MaximumRetryException(Throwable cause) {
		super(cause);
	}
	
}
