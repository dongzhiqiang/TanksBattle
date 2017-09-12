package com.engine.common.socket.handler;

import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;

public interface Listener {

	void received(Response<?> response, IoSession... sessions);

	void received(Request<?> request, IoSession... sessions);
	
	/**
	 * 向指定会话发送通信信息
	 * @param request
	 * @param sessions
	 */
	void sent(Request<?> request, IoSession... sessions);

	/**
	 * 向指定会话发送通信信息
	 * @param response
	 * @param sessions
	 */
	void sent(Response<?> response, IoSession... sessions);

}
