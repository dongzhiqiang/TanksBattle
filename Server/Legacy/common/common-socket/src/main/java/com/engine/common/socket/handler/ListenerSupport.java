package com.engine.common.socket.handler;

import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;

import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;

public class ListenerSupport {

	private final List<Listener> listeners = new CopyOnWriteArrayList<Listener>();
	
	public void addListener(Listener listener) {
		this.listeners.add(listener);
	}
	
	public void removeListener(Listener listener) {
		this.listeners.remove(listener);
	}

	/**
	 * 向指定会话发送通信信息
	 * @param session
	 * @param request
	 */
	public void fireSentRequest(Request<?> request, IoSession... sessions) {
		for (Listener listener : listeners) {
			listener.sent(request, sessions);
		}
	}

	/**
	 * 向指定会话发送通信信息
	 * @param session
	 * @param response
	 */
	public void fireSentResponse(Response<?> response, IoSession... sessions ) {
		for (Listener listener : listeners) {
			listener.sent(response, sessions);
		}
	}

	/**
	 * 接收到指定会话的通信信息
	 * @param session
	 * @param message
	 */
	public void fireReceivedRequest(Request<?> request, IoSession... sessions) {
		for (Listener listener : listeners) {
			listener.received(request, sessions);
		}
	}
	
	public void fireReceivedResponse(Response<?> response, IoSession... sessions) {
		for (Listener listener : listeners) {
			listener.received(response, sessions);
		}
	}

}
