package com.game.gow.module.account.exception;

import com.engine.common.utils.ManagedException;

/**
 * 账号异常
 * 
 * @author wenkin
 */
public class AccountException extends ManagedException {

	private static final long serialVersionUID = -6432171345599305696L;

	public AccountException(int code, String message, Throwable cause) {
		super(code, message, cause);
	}

	public AccountException(int code, String message) {
		super(code, message);
	}

	public AccountException(int code, Throwable cause) {
		super(code, cause);
	}

	public AccountException(int code) {
		super(code);
	}
}
