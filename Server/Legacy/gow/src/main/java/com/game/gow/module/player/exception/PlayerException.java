/**
 * 
 */
package com.game.gow.module.player.exception;

import com.engine.common.utils.ManagedException;

/**
 * @author wenkin
 *
 */
public class PlayerException extends ManagedException {

	private static final long serialVersionUID = -7915402735113985717L;
	
	public PlayerException(int code, String message, Throwable cause) {
		super(code, message, cause);
	}

	public PlayerException(int code, String message) {
		super(code, message);
	}

	public PlayerException(int code, Throwable cause) {
		super(code, cause);
	}

	public PlayerException(int code) {
		super(code);
	}

}
