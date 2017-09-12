package com.engine.common.socket.schema;

/**
 * Socket Schema 定义的属性名
 * 
 */
public interface AttributeNames {

	/** 配置文件 */
	String CONFIG = "config";

	/** Bean Id */
	String ID = "id";

	/** 类名 */
	String CLASS = "class";

	/** 引用的 Bean Name */
	String REF = "ref";

	/** 名字 */
	String NAME = "name";

	/** 格式编号 */
	String FORMAT = "format";

	/** 是否克隆编码器 */
	String CLONE = "clone";

	/** 依赖 */
	String DEPENDS_ON = "depends-on";

	/** 扫描容器中的BEAN */
	String SCAN_BEANS = "scan-beans";

}
