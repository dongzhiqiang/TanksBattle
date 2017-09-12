package com.engine.common.ramcache.aop;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * 自动锁定方法注释
 * 
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.METHOD)
public @interface AutoLocked {
	
	/** 
	 * 是否强制使用锁<br/>
	 * true:不管外层方法是否已经加锁，都进行加锁 false:如果外层方法已经加了锁就不再加锁
	 */
	boolean value() default false;
}
