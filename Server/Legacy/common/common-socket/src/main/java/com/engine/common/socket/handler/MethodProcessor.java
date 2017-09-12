package com.engine.common.socket.handler;

import java.lang.reflect.Method;

import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.util.ReflectionUtils;

import com.engine.common.socket.anno.ParameterBuilder;
import com.engine.common.socket.core.Processor;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.exception.ProcessingException;

/**
 * 注释方法的{@link Processor}
 * 
 */
/**
 * 
 */
@SuppressWarnings("rawtypes")
public class MethodProcessor implements Processor {

	private static final Logger logger = LoggerFactory.getLogger(MethodProcessor.class);

	/**
	 * 创建 {@link MethodProcessor}实例
	 * @param format 编码格式
	 * @param target 方法所在对象
	 * @param method 代理方法
	 * @return
	 */
	public static MethodProcessor valueOf(byte format, Object target, Method method, ParameterBuilder builder) {
		MethodProcessor result = new MethodProcessor();
		ReflectionUtils.makeAccessible(method);
		MethodDefinition definition = MethodDefinition.valueOf(format, target.getClass(), method, builder);

		result.target = target;
		result.method = method;
		result.definition = definition;
		return result;
	}

	/**
	 * 创建 {@link MethodProcessor}实例
	 * @param format 编码格式
	 * @param clz 方法所在的类
	 * @param target 方法所在对象
	 * @param method 代理方法
	 * @return
	 */
	public static MethodProcessor valueOf(byte format, Class<?> clz, Object target, Method method, ParameterBuilder builder) {
		MethodProcessor result = new MethodProcessor();
		ReflectionUtils.makeAccessible(method);
		MethodDefinition definition = MethodDefinition.valueOf(format, clz, method, builder);

		result.target = target;
		result.method = method;
		result.definition = definition;
		return result;
	}

	private Object target;
	private Method method;
	private MethodDefinition definition;

	@Override
	public Object process(Request request, IoSession session) {
		try {
			Object[] args = definition.buildParameters(request, session);
			return method.invoke(target, args);
		} catch (Exception e) {
			if (e instanceof ProcessingException) {
				throw (ProcessingException) e;
			}
			FormattingTuple message = MessageFormatter.format("对象[{}]的方法[{}]访问异常", target.getClass().getName(),
					method.getName());
			logger.error(message.getMessage(), e);
			throw new ProcessingException(message.getMessage(), e);
		}
	}

	public TypeDefinition getDefinition() {
		return definition;
	}

	@Override
	public String toString() {
		return target.getClass().getName() + "." + method.getName();
	}

}
