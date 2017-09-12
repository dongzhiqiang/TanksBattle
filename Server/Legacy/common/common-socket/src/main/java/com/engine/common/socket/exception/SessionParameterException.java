package com.engine.common.socket.exception;

/**
 * 会话参数异常
 * 
 */
public class SessionParameterException extends ParameterException {

	private static final long serialVersionUID = -313993937695654466L;

	public SessionParameterException() {
		super();
	}

	public SessionParameterException(String message, Throwable cause) {
		super(message, cause);
	}

	public SessionParameterException(String message) {
		super(message);
	}

	public SessionParameterException(Throwable cause) {
		super(cause);
	}
}
