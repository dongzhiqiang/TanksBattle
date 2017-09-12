package com.engine.common.ramcache.schema;

/**
 * Socket Schema 定义的元素名
 * 
 */
public interface ElementNames {

	/** 根配置元素 */
	String RAMCACHE = "config";
	
	/** 实体定义元素 */
	String ENTITY = "entity";
	/** 包定义元素 */
	String PACKAGE = "package";
	/** 定义实体元素 */
	String CLASS = "class";
	
	/** 访问器定义元素 */
	String ACCESSOR = "accessor";
	/** 查询器定义元素 */
	String QUERIER = "querier";
	
	/** 持久化配置元素定义 */
	String PERSIST = "persist";
	/** 持久化缓存器元素定义 */
	String PERSISTER = "persister";
	
	/** 常量组元素定义 */
	String CONSTANTS = "constants";
	/** 常量元素定义 */
	String CONSTANT = "constant";
	
	
}
