package com.engine.common.socket.client;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.filter.session.SessionManager;

@Component
public class TestFacade implements TestFacadeInf {

	@Override
	public void timeout() throws InterruptedException {
		Thread.sleep(5000);
	}

	@Override
	public void test() {
	}
	
	@Autowired
	private SessionManager sessionManager;

	@Override
	public void start() {
		Thread thread = new Thread() {
			@Override
			public void run() {
				sessionManager.sendAll(Request.valueOf(Command.valueOf(2, 2), 1));
			}
		};
		thread.start();
	}

}
