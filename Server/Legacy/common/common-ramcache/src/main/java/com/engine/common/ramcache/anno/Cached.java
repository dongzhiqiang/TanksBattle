package com.engine.common.ramcache.anno;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * 缓存配置声明
 * 
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.TYPE)
public @interface Cached {

	/** 缓存实体数量 */
	String size();

	/** 初始化容量(16) */
	int initialCapacity() default 16;

	/** 设置并发更新线程数预计值(16) */
	int concurrencyLevel() default 16;

	/** 使用的持久化处理器配置 */
	Persister persister() default @Persister();

	/** 缓存管理类型 */
	CacheType type() default CacheType.LRU;
	
	/** 缓存单位 */
	CacheUnit unit() default CacheUnit.ENTITY;

	/** 是否开启动态类增强 */
	boolean enhanced() default true;
}
