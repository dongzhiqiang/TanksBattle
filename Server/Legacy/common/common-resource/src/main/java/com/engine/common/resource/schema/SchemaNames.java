package com.engine.common.resource.schema;

/**
 * 配置名定义
 * 
 */
public interface SchemaNames {
	
	/** 配置元素 */
	String CONFIG_ELEMENT = "config";
	
	/** 包声明元素 */
	String PACKAGE_ELEMENT = "package";

	/** 类声明元素 */
	String CLASS_ELEMENT = "class";

	/** 资源格式声明元素 */
	String FORMAT_ELEMENT = "format";
	
	/** 名称属性 */
	String PACKAGE_ATTRIBUTE_NAME = "name";

	/** 名称属性 */
	String CLASS_ATTRIBUTE_NAME = "name";

	/** 资源位置 */
	String FORMAT_ATTRIBUTE_LOCATION = "location";

	/** 资源类型 */
	String FORMAT_ATTRIBUTE_TYPE = "type";
	
	/** 资源后缀 */
	String FORMAT_ATTRIBUTE_SUFFIX = "suffix";

}
