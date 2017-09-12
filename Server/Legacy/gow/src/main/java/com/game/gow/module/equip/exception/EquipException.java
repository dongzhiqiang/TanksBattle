package com.game.gow.module.equip.exception;

import com.engine.common.utils.ManagedException;

/**
 * 装备异常
 */
public class EquipException extends ManagedException {
	
	private static final long serialVersionUID = -6432123345579305696L;

	public EquipException(int code, String message, Throwable cause) {
		super(code, message, cause);
	}

	public EquipException(int code, String message) {
		super(code, message);
	}

	public EquipException(int code, Throwable cause) {
		super(code, cause);
	}

	public EquipException(int code) {
		super(code);
	};


}
