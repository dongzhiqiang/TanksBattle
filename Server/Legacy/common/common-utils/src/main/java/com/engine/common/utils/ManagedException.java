package com.engine.common.utils;

/**
 * 受管理异常
 * 
 */
public class ManagedException extends RuntimeException {

	private static final long serialVersionUID = -5566075318388205571L;

	/** 错误代码 */
	private final int code;

	public ManagedException(int code) {
		super();
		this.code = code;
	}

	public ManagedException(int code, String message, Throwable cause) {
		super(message, cause);
		this.code = code;
	}

	public ManagedException(int code, String message) {
		super(message);
		this.code = code;
	}

	public ManagedException(int code, Throwable cause) {
		super(cause);
		this.code = code;
	}

	/**
	 * 获取错误代码
	 * @return
	 */
	public int getCode() {
		return code;
	}
	
}
