package com.engine.common.socket.client;

import com.engine.common.socket.handler.CommandRegister;
import com.engine.common.socket.handler.HandlerSupport;

/**
 * 客户端控制器
 * 
 * 
 */
public class ClientHandler extends HandlerSupport {

	public ClientHandler(CommandRegister register) {
		super(register);
	}

}