package com.engine.common.ramcache.schema;

/**
 * RamCache Schema 定义的属性名
 * 
 */
public interface AttributeNames {

	/** Bean Id */
	String ID = "id";
	
	/** 引用的 Bean Name */
	String REF = "ref";
	
	/** 名称 */
	String NAME = "name";
	
	/** 大小 */
	String SIZE = "size";
	
	/** 类型 */
	String TYPE = "type";
	
	/** 配置 */
	String CONFIG = "config";
	
	/** 是否开启锁的AOP切面 */
	String LOCK_ASPECT = "lockAspect";
}
