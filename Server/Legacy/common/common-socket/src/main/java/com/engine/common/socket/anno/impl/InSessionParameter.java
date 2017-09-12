package com.engine.common.socket.anno.impl;

import java.util.concurrent.ConcurrentHashMap;

import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.socket.anno.InSession;
import com.engine.common.socket.anno.Parameter;
import com.engine.common.socket.anno.ParameterKind;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Session;
import com.engine.common.socket.exception.SessionParameterException;

public class InSessionParameter implements Parameter {
	
	private static final Logger logger = LoggerFactory.getLogger(InSessionParameter.class);
	
	private InSession annotation;

	public static InSessionParameter valueOf(InSession annotation) {
		InSessionParameter result = new InSessionParameter();
		result.annotation = annotation;
		return result;
	}

	@Override
	public ParameterKind getKind() {
		return ParameterKind.IN_SESSION;
	}

	@SuppressWarnings("unchecked")
	@Override
	public Object getValue(Request<?> request, IoSession session) {
		String key = annotation.value();
		ConcurrentHashMap<String, Object> attributes = (ConcurrentHashMap<String, Object>) session.getAttribute(Session.MAIN_KEY);
		Object value = null;
		if (attributes != null) {
			value = attributes.get(key);
		}
		if (value != null) {
			return value;
		}
		
		if (annotation.required()) {
			Command command = request.getCommand();
			FormattingTuple message = MessageFormatter.format("指令[{}]注释[{}]请求的参数不存在", command, annotation);
			logger.error(message.getMessage());
			throw new SessionParameterException(message.getMessage());
		} else {
			return null;
		}
	}

}
