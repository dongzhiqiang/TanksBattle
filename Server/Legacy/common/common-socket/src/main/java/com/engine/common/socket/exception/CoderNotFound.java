package com.engine.common.socket.exception;

/**
 * 编码器不存在的异常
 * 
 */
public class CoderNotFound extends SocketException {

	private static final long serialVersionUID = -1015542101173152766L;

	public CoderNotFound() {
		super();
	}

	public CoderNotFound(String message, Throwable cause) {
		super(message, cause);
	}

	public CoderNotFound(String message) {
		super(message);
	}

	public CoderNotFound(Throwable cause) {
		super(cause);
	}

}
