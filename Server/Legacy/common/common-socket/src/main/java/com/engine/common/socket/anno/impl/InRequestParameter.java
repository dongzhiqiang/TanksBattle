package com.engine.common.socket.anno.impl;

import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.socket.anno.InRequest;
import com.engine.common.socket.anno.Parameter;
import com.engine.common.socket.anno.ParameterKind;
import com.engine.common.socket.core.Request;

public class InRequestParameter implements Parameter {
	
	private static final Logger logger = LoggerFactory.getLogger(InRequestParameter.class);
	
	private InRequest annotation;

	public static InRequestParameter valueOf(InRequest annotation) {
		InRequestParameter result = new InRequestParameter();
		result.annotation = annotation;
		return result;
	}

	@Override
	public ParameterKind getKind() {
		return ParameterKind.IN_REQUEST;
	}

	@Override
	public Object getValue(Request<?> request, IoSession session) {
		switch (annotation.value()) {
			case SN:
				return request.getSn();
			case COMMAND:
				return request.getCommand();
			case STATE:
				return request.getState();
			case ATTACHMENT:
				return request.getAttachment();
			default:
				FormattingTuple message = MessageFormatter.format("无法处理的 InRequest 类型[{}]", annotation.value());
				logger.error(message.getMessage());
				throw new IllegalStateException(message.getMessage());
		}
	}

}
