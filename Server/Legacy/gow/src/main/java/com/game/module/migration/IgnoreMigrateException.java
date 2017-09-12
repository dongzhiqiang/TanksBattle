package com.game.module.migration;

/**
 * 当重试达到最大次数时抛出的异常
 * 
 *
 */
public class IgnoreMigrateException extends RuntimeException {

	private static final long serialVersionUID = -3425994082520188020L;

	public IgnoreMigrateException() {
		super();
	}

	public IgnoreMigrateException(String message, Throwable cause) {
		super(message, cause);
	}

	public IgnoreMigrateException(String message) {
		super(message);
	}

	public IgnoreMigrateException(Throwable cause) {
		super(cause);
	}
	
}
