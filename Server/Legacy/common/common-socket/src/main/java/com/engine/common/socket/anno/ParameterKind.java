package com.engine.common.socket.anno;

/**
 * 参数类型
 * 
 */
public enum ParameterKind {

	/** 请求信息体 */
	BODY,
	/** 请求对象 */
	REQUEST,
	/** 抽象会话对象 */
	SESSION,
	/** 原生会话对象 */
	IO_SESSION,
	/** 在信息体中 */
	IN_BODY,
	/** 在请求对象中 */
	IN_REQUEST,
	/** 在会话对象中 */
	IN_SESSION;

}
