package com.engine.common.socket.anno;

import java.lang.annotation.Documented;
import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * 指令声明注释
 * 
 */
@Target(ElementType.METHOD)
@Retention(RetentionPolicy.RUNTIME)
@Documented
public @interface SocketCommand {
	
	/**
	 * 指令值，默认:0
	 * @return
	 */
	int value() default 0;
	
	/**
	 * 指令所属模块声明，可选
	 * @return
	 */
	byte[] modules() default {};
	
	/**
	 * 标识消息体是否采用压缩
	 * @return
	 */
	Compress compress() default @Compress();
	
	/**
	 * 标识消息体是否使用原生类型
	 * @return
	 */
	Raw raw() default @Raw();
	
}
