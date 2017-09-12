package com.engine.common.console.exception;

/**
 * 参数异常
 * 
 */
public class ArgumentException extends CommandException {

	private static final long serialVersionUID = 3691784737559119946L;

	public ArgumentException() {
		super();
	}

	public ArgumentException(String message, Throwable cause) {
		super(message, cause);
	}

	public ArgumentException(String message) {
		super(message);
	}

	public ArgumentException(Throwable cause) {
		super(cause);
	}
	
}
