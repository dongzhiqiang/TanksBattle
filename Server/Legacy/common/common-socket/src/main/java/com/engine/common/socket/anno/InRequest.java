package com.engine.common.socket.anno;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

import com.engine.common.socket.core.Request;

/**
 * 提取{@link Request}的属性作为参数的注释
 * <pre>
 * 不能和{@link InBody}与{@link InSession}同时声明在同一个参数上，
 * 如出现同时声明的情况，将以:
 * {@link InBody} > {@link InRequest} > {@link InSession} 的次序生效
 * </pre>
 * 
 */
@Target(ElementType.PARAMETER)
@Retention(RetentionPolicy.RUNTIME)
public @interface InRequest {
	
	/**
	 * 可以获取的 {@link InRequest} 类型
	 * 
	 */
	public static enum Type {
		/** 序列号 */
		SN,
		/** 指令 */
		COMMAND,
		/** 状态 */
		STATE,
		/** 附加内容 */
		ATTACHMENT;
	}

	/**
	 * {@link Request}的属性名
	 * @return
	 */
	Type value();

}
