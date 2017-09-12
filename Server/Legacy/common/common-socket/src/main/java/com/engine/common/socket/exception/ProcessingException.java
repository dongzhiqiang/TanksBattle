package com.engine.common.socket.exception;

/**
 * 处理异常
 * 
 * 
 */
public class ProcessingException extends SocketException {

	private static final long serialVersionUID = 3027236671106441923L;

	public ProcessingException() {
		super();
	}

	public ProcessingException(String message, Throwable cause) {
		super(message, cause);
	}

	public ProcessingException(String message) {
		super(message);
	}

	public ProcessingException(Throwable cause) {
		super(cause);
	}

}
