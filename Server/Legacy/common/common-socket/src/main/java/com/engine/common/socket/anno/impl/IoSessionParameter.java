package com.engine.common.socket.anno.impl;

import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.anno.Parameter;
import com.engine.common.socket.anno.ParameterKind;
import com.engine.common.socket.core.Request;

public class IoSessionParameter implements Parameter {

	public static final IoSessionParameter instance = new IoSessionParameter();

	@Override
	public ParameterKind getKind() {
		return ParameterKind.IO_SESSION;
	}

	@Override
	public Object getValue(Request<?> request, IoSession session) {
		return session;
	}

}
