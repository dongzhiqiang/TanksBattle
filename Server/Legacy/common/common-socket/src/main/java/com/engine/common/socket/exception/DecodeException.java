package com.engine.common.socket.exception;

public class DecodeException extends SocketException {

	private static final long serialVersionUID = -802552390478658742L;

	public DecodeException() {
		super();
	}

	public DecodeException(String message, Throwable cause) {
		super(message, cause);
	}

	public DecodeException(String message) {
		super(message);
	}

	public DecodeException(Throwable cause) {
		super(cause);
	}

}
