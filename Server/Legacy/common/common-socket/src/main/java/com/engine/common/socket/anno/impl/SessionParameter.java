package com.engine.common.socket.anno.impl;

import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.anno.Parameter;
import com.engine.common.socket.anno.ParameterKind;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Session;

public class SessionParameter implements Parameter {
	
	public static final SessionParameter instance = new SessionParameter();

	@Override
	public ParameterKind getKind() {
		return ParameterKind.SESSION;
	}

	@Override
	public Object getValue(Request<?> request, IoSession session) {
		return Session.valueOf(session);
	}

}
