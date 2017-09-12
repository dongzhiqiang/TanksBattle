package com.engine.common.utils.chain.impl;

import java.lang.annotation.Annotation;
import java.lang.reflect.Method;

import org.springframework.core.BridgeMethodResolver;
import org.springframework.core.annotation.AnnotationUtils;

import com.engine.common.utils.chain.NodeProcessor;
import com.engine.common.utils.chain.anno.InNotice;
import com.engine.common.utils.chain.anno.Processing;
import com.engine.common.utils.chain.anno.Type;

/**
 * 处理节点创建器
 * 
 */
public abstract class AnnotationMethodNodeBuilder {
	
	/**
	 * 检查是否一个合法的
	 * @param method
	 * @return
	 */
	public static boolean isValid(Method method) {
		Processing anno = AnnotationUtils.getAnnotation(method, Processing.class);
		if (anno != null) {
			return true;
		}
		return false;
	}

	/**
	 * 创建方法处理节点
	 * @param target 方法所在对象
	 * @param method 处理方法
	 * @return
	 */
	public static NodeProcessor build(Object target, Method method) {
		method = BridgeMethodResolver.findBridgedMethod(method);
		Processing anno = method.getAnnotation(Processing.class);
		Type[] types = getInjectTypes(method);
		return new AnnotationMethodProcessor(target, method, anno, types);
	}

	/**
	 * 获取方法上，要注入的参数类型
	 * @param method
	 * @return
	 */
	private static Type[] getInjectTypes(Method method) {
		Class<?>[] classes = method.getParameterTypes();
		Annotation[][] annotations = method.getParameterAnnotations();
		Type[] result = new Type[classes.length];
		for (int i = 0; i < classes.length; i++) {
			InNotice inNotice = getInNotice(annotations[i]);
			if (inNotice != null) {
				result[i] = inNotice.type();
			} else {
				result[i] = Type.NOTICE;
			}
		}
		return result;
	}

	/**
	 * 获取注入声明信息
	 * @param annotations
	 * @return
	 */
	private static InNotice getInNotice(Annotation[] annotations) {
		for (Annotation a : annotations) {
			if (a instanceof InNotice) {
				return (InNotice) a;
			}
		}
		return null;
	}
}
