package com.engine.common.socket.handler;

import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;

public abstract class AbstractListener implements Listener {

	@Override
	public void received(Response<?> response, IoSession... sessions) {
	}

	@Override
	public void received(Request<?> request, IoSession... sessions) {
	}

	@Override
	public void sent(Request<?> request, IoSession... sessions) {
	}

	@Override
	public void sent(Response<?> response, IoSession... sessions) {
	}

}
