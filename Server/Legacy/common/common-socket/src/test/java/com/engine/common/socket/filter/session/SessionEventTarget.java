package com.engine.common.socket.filter.session;

import javax.annotation.PostConstruct;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.socket.filter.session.SessionManager;
import com.engine.common.socket.filter.session.SessionEvent.Type;

@Component
public class SessionEventTarget {

	@Autowired
	private SessionManager sessionManager;

	private MockListener identified = new MockListener(Type.IDENTIFIED);
	private MockListener closed = new MockListener(Type.CLOSED);
	private MockListener replaced = new MockListener(Type.REPLACED);

	@PostConstruct
	public void initilize() {
		sessionManager.addListener(identified);
		sessionManager.addListener(closed);
		sessionManager.addListener(replaced);
	}

	public SessionManager getSessionManager() {
		return sessionManager;
	}

	public MockListener getIdentified() {
		return identified;
	}

	public MockListener getClosed() {
		return closed;
	}

	public MockListener getReplaced() {
		return replaced;
	}

}
