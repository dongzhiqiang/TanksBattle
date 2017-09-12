package com.engine.common.socket.exception;

/**
 * 消息体类型定义不存在异常
 * 
 * 
 */
public class TypeDefinitionNotFound extends SocketException {

	private static final long serialVersionUID = -1585674859071997636L;

	public TypeDefinitionNotFound() {
		super();
	}

	public TypeDefinitionNotFound(String message, Throwable cause) {
		super(message, cause);
	}

	public TypeDefinitionNotFound(String message) {
		super(message);
	}

	public TypeDefinitionNotFound(Throwable cause) {
		super(cause);
	}

}
