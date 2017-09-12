package com.engine.common.socket.anno;

import java.lang.annotation.Annotation;
import java.lang.reflect.Method;
import java.lang.reflect.ParameterizedType;
import java.lang.reflect.Type;

import javassist.NotFoundException;

import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.socket.anno.impl.BodyParameter;
import com.engine.common.socket.anno.impl.InBodyParameter;
import com.engine.common.socket.anno.impl.InRequestParameter;
import com.engine.common.socket.anno.impl.InSessionParameter;
import com.engine.common.socket.anno.impl.IoSessionParameter;
import com.engine.common.socket.anno.impl.RequestParameter;
import com.engine.common.socket.anno.impl.SessionParameter;
import com.engine.common.socket.core.Convertor;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Session;
import com.engine.common.socket.exception.ParameterException;

public class ParameterBuilder {

	private static final Logger logger = LoggerFactory.getLogger(Parameter.class);
	
	private Convertor convertor;

	/**
	 * 创建参数数组
	 * @param method 方法
	 * @return
	 */
	public Parameter[] buildParameters(Method method) {
		Type[] types = method.getGenericParameterTypes();
		Parameter[] result = new Parameter[types.length];
		int inBodyIndex = 0;
		for (int i = 0; i < types.length; i++) {
			result[i] = valueOf(method, i, inBodyIndex);
			if( result[i] instanceof InBodyParameter && ((InBodyParameter)result[i]).isIndexed() )
			{
				inBodyIndex ++;
			}
		}
		return result;
	}

	/**
	 * 创建参数对象
	 * @param method 方法
	 * @param index 参数下标
	 * @return
	 */
	@SuppressWarnings("rawtypes")
	public Parameter valueOf(Method method, int index, int inBodyIndex) {
		if (index >= method.getGenericParameterTypes().length) {
			FormattingTuple message = MessageFormatter.format("参数下标[{}]超过了方法[{}]的有效下标", index, method.getName());
			logger.error(message.getMessage());
			throw new IllegalArgumentException(message.getMessage());
		}
		
		Type type = method.getGenericParameterTypes()[index];
		Annotation[] annotations = method.getParameterAnnotations()[index];
		for (Annotation a : annotations) {
			if (a instanceof InBody) {
				try {
					return InBodyParameter.valueOf((InBody) a, method, index, inBodyIndex, convertor);
				} catch (NotFoundException e) {
					throw new ParameterException(e);
				}
			} else if (a instanceof InRequest) {
				return InRequestParameter.valueOf((InRequest) a);
			} else if (a instanceof InSession) {
				return InSessionParameter.valueOf((InSession) a);
			}
		}
		
		if (type instanceof Class && Session.class.isAssignableFrom((Class) type)) {
			return SessionParameter.instance;
		}

		if (type instanceof Class && IoSession.class.isAssignableFrom((Class) type)) {
			return IoSessionParameter.instance;
		}
		
		if (type instanceof ParameterizedType && ((ParameterizedType) type).getRawType().equals(Request.class)) {
			return RequestParameter.instance;
		}
		
		return BodyParameter.instance;
	}

	public void setConvertor(Convertor convertor) {
		this.convertor = convertor;
	}

}
