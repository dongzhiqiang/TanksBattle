package com.engine.common.protocol.exception;

import java.io.IOException;

/**
 * 类型代理异常
 * 
 * 
 *
 */
public class ObjectProxyException extends IOException{
	private static final long serialVersionUID = -671045745597774362L;

	public ObjectProxyException(Exception e) {
		super("类型代理异常", e);
	}

}
