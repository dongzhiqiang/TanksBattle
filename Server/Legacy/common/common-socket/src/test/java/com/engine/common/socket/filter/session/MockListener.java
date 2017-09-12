package com.engine.common.socket.filter.session;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import com.engine.common.socket.filter.session.SessionEvent;
import com.engine.common.socket.filter.session.SessionListener;
import com.engine.common.socket.filter.session.SessionEvent.Type;

/**
 * 测试用的伪监听器
 * 
 */
public class MockListener implements SessionListener {
	
	private Type type;
	private Integer id;

	public MockListener(Type type) {
		this.type = type;
	}

	@Override
	public void onEvent(SessionEvent event) {
		assertThat(event.getIdentity(), instanceOf(Integer.class));
		id = (Integer) event.getIdentity();
	}

	@Override
	public Type getType() {
		return type;
	}

	public Integer getId() {
		return id;
	}
}
