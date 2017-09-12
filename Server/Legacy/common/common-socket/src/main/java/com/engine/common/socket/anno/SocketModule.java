package com.engine.common.socket.anno;

import java.lang.annotation.Documented;
import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * 控制器声明
 * 
 */
@Target({ElementType.TYPE})
@Retention(RetentionPolicy.RUNTIME)
@Documented
public @interface SocketModule {

	/**
	 * 模块声明
	 * @return
	 */
	byte[] value();
	
	/**
	 * 模块的编码格式(默认为:0)
	 * @return
	 */
	byte format() default 0;
}
