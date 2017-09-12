package com.engine.common.socket.exception;

/**
 * 处理器不存在的异常
 * 
 */
public class ProcessorNotFound extends SocketException {

	private static final long serialVersionUID = -4349222660303288826L;

	public ProcessorNotFound() {
		super();
	}

	public ProcessorNotFound(String message, Throwable cause) {
		super(message, cause);
	}

	public ProcessorNotFound(String message) {
		super(message);
	}

	public ProcessorNotFound(Throwable cause) {
		super(cause);
	}

}
