package com.engine.common.socket.anno;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

import com.engine.common.socket.core.Message;

/**
 * 提取{@link Message#getBody()}的内容作为参数的注释
 * <pre>
 * 不能和{@link InRequest}与{@link InSession}同时声明在同一个参数上，
 * 如出现同时声明的情况，将以:
 * {@link InBody} > {@link InRequest} > {@link InSession} 的次序生效
 * </pre>
 * 
 */
@Target(ElementType.PARAMETER)
@Retention(RetentionPolicy.RUNTIME)
public @interface InBody {
	
	/**
	 * 值来源配置
	 * <ul>
	 * <li>{@link Map}的Key</li>
	 * <li>POJO对象的属性名</li>
	 * <li>数组的元素下标</li>
	 * </ul>
	 * @return
	 */
	String value() default "";
	
	/**
	 * 是否要求参数必须非空
	 * @return
	 */
	boolean required() default true;
}
