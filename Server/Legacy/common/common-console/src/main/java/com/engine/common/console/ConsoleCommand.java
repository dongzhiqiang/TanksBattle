package com.engine.common.console;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

@Target(ElementType.METHOD)
@Retention(RetentionPolicy.RUNTIME)
public @interface ConsoleCommand {

	/**
	 * 命令名
	 * @return
	 */
	String name();
	
	/**
	 * 命令描述
	 * @return
	 */
	String description() default "";
}
