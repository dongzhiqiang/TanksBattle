package com.engine.common.socket.anno;

import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;

/**
 * 消息体压缩注释
 * 
 */
@Retention(RetentionPolicy.RUNTIME)
public @interface Compress {

	/**
	 * 请求信息体是否开启压缩
	 * @return
	 */
	boolean request() default false;

	/**
	 * 回应信息体是否开启压缩
	 * @return
	 */
	boolean response() default false;
}