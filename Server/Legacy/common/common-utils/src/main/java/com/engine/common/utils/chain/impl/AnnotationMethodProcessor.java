package com.engine.common.utils.chain.impl;

import java.lang.reflect.Method;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.utils.chain.NodeProcessor;
import com.engine.common.utils.chain.Notice;
import com.engine.common.utils.chain.Way;
import com.engine.common.utils.chain.anno.Processing;
import com.engine.common.utils.chain.anno.Type;

/**
 * 声明方法处理器，用于将一个{@link Processing}声明的方法转化为{@link NodeProcessor}
 * 
 */
@SuppressWarnings("rawtypes")
public class AnnotationMethodProcessor extends AbstractNode {
	
	private static final Logger log = LoggerFactory.getLogger(AnnotationMethodProcessor.class);
	
	private final Object target;
	private final Method method;
	private final Type[] types;
	
	/**
	 * 构建对象
	 * @param target 处理方法所在的对象实例
	 * @param method 处理方法
	 * @param anno 处理方法上的声明
	 * @param types 要注入的参数类型
	 */
	public AnnotationMethodProcessor(Object target, Method method, Processing anno, Type[] types) {
		super(anno.name(), anno.index(), anno.way());
		this.target = target;
		this.method = method;
		this.types = types;
		method.setAccessible(true);
	}

	@Override
	public boolean in(Notice notice) {
		if (this.getWay() == Way.OUT) {
			return true;
		}
		return execute(notice);
	}

	@Override
	public boolean out(Notice notice) {
		if (this.getWay() == Way.IN) {
			return true;
		}
		return execute(notice);
	}

	/**
	 * 执行处理方法
	 * @param notice 处理通知对象实例
	 * @return 处理是否可继续进行，false代表处理将被打断
	 */
	private boolean execute(Notice notice) {
		Object[] params = getParams(notice);
		try {
			Object result = method.invoke(target, params);
			if (result == null) {
				return true;
			}
			if (result instanceof Boolean) {
				return (Boolean) result;
			}
		} catch (Exception e) {
			log.error("执行方法时出现异常", e);
			throw new IllegalStateException("无法执行处理方法", e);
		}
		return true;
	}

	/**
	 * 获取处理方法所需的参数
	 * @param notice 处理通知对象实例
	 * @return
	 */
	private Object[] getParams(Notice notice) {
		Object[] result = new Object[types.length];
		for (int i = 0; i < types.length; i++) {
			Type type = types[i];
			result[i] = getParam(notice, type);
		}
		return result;
	}

	/**
	 * 获取单一的方法参数
	 * @param notice 处理通知对象实例
	 * @param type 要获取的参数类型
	 * @return
	 */
	private Object getParam(Notice notice, Type type) {
		switch (type) {
			case NOTICE:
				return notice;
			case CONTENT:
				return notice.getContent();
			case STEP:
				return notice.getStep();
			case WAY:
				return notice.getWay();
			default:
				log.error("无法处理的参数类型[{}]", type);
				return null;
		}
	}

}
