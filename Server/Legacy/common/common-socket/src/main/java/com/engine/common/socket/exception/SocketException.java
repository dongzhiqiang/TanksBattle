package com.engine.common.socket.exception;

/**
 * 通信异常
 * 
 */
public class SocketException extends RuntimeException {

	private static final long serialVersionUID = -721014715061800917L;

	public SocketException() {
		super();
	}

	public SocketException(String message, Throwable cause) {
		super(message, cause);
	}

	public SocketException(String message) {
		super(message);
	}

	public SocketException(Throwable cause) {
		super(cause);
	}
}
