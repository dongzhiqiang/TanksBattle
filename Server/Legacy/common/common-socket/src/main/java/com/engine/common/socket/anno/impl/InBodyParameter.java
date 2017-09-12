package com.engine.common.socket.anno.impl;

import java.lang.reflect.Method;

import javassist.ClassPool;
import javassist.CtClass;
import javassist.CtMethod;
import javassist.Modifier;
import javassist.NotFoundException;
import javassist.bytecode.CodeAttribute;
import javassist.bytecode.LocalVariableAttribute;
import javassist.bytecode.MethodInfo;

import org.apache.commons.lang3.StringUtils;
import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.socket.anno.InBody;
import com.engine.common.socket.anno.Parameter;
import com.engine.common.socket.anno.ParameterKind;
import com.engine.common.socket.codec.Coder;
import com.engine.common.socket.core.Convertor;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.exception.ParameterException;

/**
 * {@link InBody}注释的处理器
 * 
 */
public class InBodyParameter implements Parameter {
	
	private static final Logger logger = LoggerFactory.getLogger(InBodyParameter.class);
	
	public static InBodyParameter valueOf(InBody annotation, Method method, int index, int inBodyIndex, Convertor convertor) throws NotFoundException {
		String name = annotation.value();
		
		InBodyParameter result = new InBodyParameter();
		
		if (StringUtils.isEmpty(name)) {
			
			//name = getParamaterName(method, index);
			// use function index
			name = ""+inBodyIndex;
			result.indexed = true;
		}
		
		result.annotation = annotation;
		result.clazz = method.getParameterTypes()[index];
		result.value = name;
		result.required = annotation.required();
		result.convertor = convertor;
		return result;
	}

	/** 获取参数名 */
	@SuppressWarnings("unused")
	private static String getParamaterName(Method method, int index) throws NotFoundException {
		Class<?> clazz = method.getDeclaringClass();
		if (clazz.isInterface()) {
			throw new ParameterException("接口方法的参数必须指定 InBody 注释的 value");
		}
		
		ClassPool pool = ClassPool.getDefault();
		CtClass clz = pool.get(clazz.getName());
		CtClass[] params = new CtClass[method.getParameterTypes().length];
		for (int i = 0; i < method.getParameterTypes().length; i++) {
			params[i] = pool.getCtClass(method.getParameterTypes()[i].getName());
		}
		CtMethod cm = clz.getDeclaredMethod(method.getName(), params);
		MethodInfo methodInfo = cm.getMethodInfo();
		CodeAttribute codeAttribute = methodInfo.getCodeAttribute();
		LocalVariableAttribute attr = (LocalVariableAttribute) codeAttribute.getAttribute(LocalVariableAttribute.tag);
		int pos = Modifier.isStatic(cm.getModifiers()) ? 0 : 1;
		String name = attr.variableName(index + pos);
		return name;
	}

	private InBody annotation;
	private Class<?> clazz;
	private String value;
	private boolean required;
	private Convertor convertor;
	private boolean indexed = false;
	
	@Override
	public ParameterKind getKind() {
		return ParameterKind.IN_BODY;
	}

	@Override
	public Object getValue(Request<?> request, IoSession session) {
		byte format = request.getHeader().getFormat();
		Coder coder = convertor.getCoder(format);
		try {
			return coder.getInBody(request.getBody(), this);
		} catch (Exception e) {
			FormattingTuple message = MessageFormatter.format("命令[{}]请求的参数[{}]不存在", request.getCommand(), annotation);
			logger.error(message.getMessage());
			throw new ParameterException(message.getMessage(), e);
		}
	}

	// Getter and Setter ...
	
	public Class<?> getClazz() {
		return clazz;
	}

	public String getValue() {
		return value;
	}

	public boolean isRequired() {
		return required;
	}

	public InBody getAnnotation() {
		return annotation;
	}
	
	public boolean isIndexed() {
		return indexed;
	}
	
}
