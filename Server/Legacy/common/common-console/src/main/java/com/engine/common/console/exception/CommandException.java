package com.engine.common.console.exception;

/**
 * 控制台命令异常，这是该模块内所有异常的父类
 * 
 */
public abstract class CommandException extends Exception {

	private static final long serialVersionUID = 8734255937189347706L;

	public CommandException() {
		super();
	}

	public CommandException(String message, Throwable cause) {
		super(message, cause);
	}

	public CommandException(String message) {
		super(message);
	}

	public CommandException(Throwable cause) {
		super(cause);
	}
	
}
