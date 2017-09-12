package com.engine.common.socket.anno.session;

import org.apache.mina.core.filterchain.IoFilterAdapter;
import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.core.Attribute;

public class InSessionTestFilter extends IoFilterAdapter {
	
	private Attribute<String> inSessionAttribute  = new Attribute<String>("FOUND");
	
	public static final String VALUE = "frank";

	@Override
	public void sessionCreated(NextFilter nextFilter, IoSession session) throws Exception {
		inSessionAttribute.setValue(session, VALUE);
		super.sessionCreated(nextFilter, session);
	}
}
