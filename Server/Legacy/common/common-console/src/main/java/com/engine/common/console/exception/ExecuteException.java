package com.engine.common.console.exception;

/**
 * 命令执行异常
 * 
 */
public class ExecuteException extends CommandException {

	private static final long serialVersionUID = -5676858996083129957L;

	public ExecuteException() {
		super();
	}

	public ExecuteException(String message, Throwable cause) {
		super(message, cause);
	}

	public ExecuteException(String message) {
		super(message);
	}

	public ExecuteException(Throwable cause) {
		super(cause);
	}

}
