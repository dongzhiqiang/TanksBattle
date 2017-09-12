package com.engine.common.ramcache.enhance;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * 方法增强注释<br/>
 * ps:增强方法在抛出异常时，默认都是不会回写数据到持久层的。
 * 如果需要抛出异常的同时也需要回写数据到持久层，需要使用<code>ignore</code>进行异常声明
 * 
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.METHOD)
public @interface Enhance {

	/** 仅在返回该值时才更新 */
	String value() default "";
	
	/** 运行时抛出该异常时也会通知数据更新 */
	Class<?> ignore() default void.class;
}
