package com.engine.common.socket.anno;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

import org.apache.mina.core.session.AttributeKey;
import org.apache.mina.core.session.IoSession;

/**
 * 提取{@link IoSession}的内容作为参数的注释
 * <pre>
 * 不能和{@link InBody}与{@link InRequest}同时声明在同一个参数上，
 * 如出现同时声明的情况，将以:
 * {@link InBody} > {@link InRequest} > {@link InSession} 的次序生效
 * </pre>
 * 
 */
@Target(ElementType.PARAMETER)
@Retention(RetentionPolicy.RUNTIME)
public @interface InSession {

	/**
	 * {@link AttributeKey}的字符串表示格式
	 * <pre>
	 * [类名]@[键]
	 * </pre>
	 * @return
	 */
	String value();

	/**
	 * 是否要求参数必须非空
	 * @return
	 */
	boolean required() default true;

}
