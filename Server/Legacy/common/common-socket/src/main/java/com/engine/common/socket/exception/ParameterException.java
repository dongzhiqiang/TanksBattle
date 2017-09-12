package com.engine.common.socket.exception;

/**
 * 参数处理异常
 * 
 */
public class ParameterException extends ProcessingException {

	private static final long serialVersionUID = 9032409980090561175L;

	public ParameterException() {
		super();
	}

	public ParameterException(String message, Throwable cause) {
		super(message, cause);
	}

	public ParameterException(String message) {
		super(message);
	}

	public ParameterException(Throwable cause) {
		super(cause);
	}

}
