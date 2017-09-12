package com.engine.common.socket.anno;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * 消息体原生类型(byte[])注释
 * 
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
public @interface Raw {
	
	/**
	 * 请求信息体是否使用原生类型
	 * @return
	 */
	boolean request() default false;

	/**
	 * 回应信息体是否使用原生类型
	 * @return
	 */
	boolean response() default false;

}
