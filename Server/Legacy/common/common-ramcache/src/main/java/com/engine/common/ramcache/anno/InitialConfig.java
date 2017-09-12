package com.engine.common.ramcache.anno;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * 初始化配置信息
 * 
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.TYPE)
public @interface InitialConfig {

	/** 初始化类型 */
	InitialType type() default InitialType.ALL;

	/** 初始化查询名 */
	String query() default "";

}
