package com.game.auth.module.auth.exception;

import com.engine.common.utils.ManagedException;

/**
 * 管理异常
 * 
 */
public class AuthException extends ManagedException {

	private static final long serialVersionUID = -8405040202825296509L;

	public AuthException(int code, Throwable cause) {
		super(code, cause);
	}

	public AuthException(int code) {
		super(code);
	}

	public AuthException(String message, int code, Throwable cause) {
		super(code, message, cause);
	}

	public AuthException(String message, int code) {
		super(code, message);
	}

}
