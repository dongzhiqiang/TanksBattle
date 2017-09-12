package com.engine.common.ramcache.anno;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * 持久化缓存策略配置
 * 
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.ANNOTATION_TYPE)
public @interface Persister {
	
	/** 默认策略名 */
	public static final String DEFAULT = "default";
	
	/** 配置策略名 */
	String value() default DEFAULT;
}

