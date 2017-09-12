package com.engine.common.socket.handler;

import java.lang.reflect.GenericArrayType;
import java.lang.reflect.Method;
import java.lang.reflect.ParameterizedType;
import java.lang.reflect.Type;

import org.apache.mina.core.session.IoSession;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.socket.anno.Parameter;
import com.engine.common.socket.anno.ParameterBuilder;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.Sync;
import com.engine.common.socket.anno.impl.BodyParameter;
import com.engine.common.socket.anno.impl.RequestParameter;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;

/**
 * 注释方法的消息体定义
 * 
 */
public class MethodDefinition extends TypeDefinition {
	
	/** 构造方法 */
	public static MethodDefinition valueOf(byte format, Class<?> clz, Method method, ParameterBuilder builder) {
		Parameter[] parameters = builder.buildParameters(method);
		Type request = findRequestType(method, parameters);
		Type response = findResponseType(method);
		
		MethodDefinition result = new MethodDefinition();
		result.format = format;
		result.clazz = clz;
		result.method = method;
		result.parameters = parameters;
		result.request = request;
		result.response = response;
		
		SocketCommand command = method.getAnnotation(SocketCommand.class);
		if (command != null) {
			result.requestCompress = command.compress().request();
			result.responseCompress = command.compress().response();
			result.requestRaw = command.raw().request();
			result.responseRaw = command.raw().response();
		}
		// 正确性判断
		if (result.requestRaw) {
			boolean flag = false;
			if (result.request instanceof GenericArrayType) {
				Type type = ((GenericArrayType) result.request).getGenericComponentType();
				if (type != byte.class) {
					flag = true;
				}
			} else if (result.request != byte[].class) {
				flag = true;
			}
			if (flag) {
				FormattingTuple message = MessageFormatter.format("方法[{}]的请求信息体[{}]应为 byte[]", method.getName(), result.request.getClass().getName());
				throw new IllegalArgumentException(message.getMessage());
			}
		}
		if (result.responseRaw) {
			boolean flag = false;
			if (result.response instanceof GenericArrayType) {
				Type type = ((GenericArrayType) result.response).getGenericComponentType();
				if (type != byte.class) {
					flag = true;
				}
			} else if (result.response != byte[].class) {
				flag = true;
			}
			if (flag) {
				FormattingTuple message = MessageFormatter.format("方法[{}]的回应信息体[{}]应为 byte[]", method.getName(), result.response.getClass().getName());
				throw new IllegalArgumentException(message.getMessage());
			}
		}
		
		// 方法为同步执行
		Sync sync = method.getAnnotation(Sync.class);
		if (sync != null) {
			result.sync = sync;
		}
		
		return result;
	}

	/**
	 * 查找请求信息体类型
	 * @param parameters
	 * @return
	 */
	private static Type findRequestType(Method method, Parameter[] parameters) {
		if (parameters.length == 0) {
			return void.class;
		}
		
		for (int i = 0; i < parameters.length; i++) {
			Parameter p = parameters[i];
			if (p instanceof BodyParameter) {
				return method.getGenericParameterTypes()[i];
			}
			if (p instanceof RequestParameter) {
				Type type = method.getGenericParameterTypes()[i];
				if (type instanceof ParameterizedType) {
					return ((ParameterizedType) type).getActualTypeArguments()[0];
				}
				return Object.class;
			}
		}
		return null;
	}
	
	/**
	 * 查找回应信息体类型
	 * @param method
	 * @return
	 */
	public static Type findResponseType(Method method) {
		Type type = method.getGenericReturnType();
		if (type.equals(Response.class)) {
			return Object.class;
		}
		if (type instanceof ParameterizedType) {
			ParameterizedType pType = (ParameterizedType) type;
			if (pType.getRawType().equals(Response.class)) {
				return pType.getActualTypeArguments()[0];
			}
		}
		return type;
	}

	private Class<?> clazz;
	private Method method;
	private Parameter[] parameters;
	
	/**
	 * 创建请求参数数组
	 * @param request
	 * @param session
	 * @return
	 */
	public Object[] buildParameters(Request<?> request, IoSession session) {
		Object[] result = new Object[parameters.length];
		for (int i = 0; i < parameters.length; i++) {
			Parameter p = parameters[i];
			result[i] = p.getValue(request, session);
		}
		return result;
	}

	// Getter and Setter ...

	public Class<?> getClazz() {
		return clazz;
	}

	public Method getMethod() {
		return method;
	}

}
