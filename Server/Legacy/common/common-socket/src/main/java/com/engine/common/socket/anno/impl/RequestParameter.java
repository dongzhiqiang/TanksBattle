package com.engine.common.socket.anno.impl;

import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.anno.Parameter;
import com.engine.common.socket.anno.ParameterKind;
import com.engine.common.socket.core.Request;

public class RequestParameter implements Parameter {

	public static final RequestParameter instance = new RequestParameter();

	@Override
	public ParameterKind getKind() {
		return ParameterKind.REQUEST;
	}

	@Override
	public Object getValue(Request<?> request, IoSession session) {
		return request;
	}

}
