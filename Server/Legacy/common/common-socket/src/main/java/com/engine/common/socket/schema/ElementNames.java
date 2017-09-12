package com.engine.common.socket.schema;

/**
 * Socket Schema 定义的元素名
 * 
 */
public interface ElementNames {

	/** 服务器配置元素 */
	String SERVER = "server";
	
	/** 客户端配置元素 */
	String CLIENT = "client";

	/** 客户端工厂配置元素 */
	String CLIENT_FACTORY = "client-factory";

	/** 接收器配置元素 */
	String ACCEPTOR = "acceptor";
	
	/** 连接器配置元素 */
	String CONNECTOR = "connector";
	
	/** 控制器配置元素 */
	String HANDLER = "handler";

	/** 过滤器集合配置元素 */
	String FILTERS = "filters";

	/** 过滤器配置元素 */
	String FILTER = "filter";
	
	/** 编码器集合配置元素 */
	String CODERS = "coders";

	/** 编码器集合配置元素 */
	String CODER = "coder";

	/** 配置元素 */
	String CONFIG = "config";
	
	/** 地址配置元素 */
	String ADDRESS = "address";
	
	/** 指令定义元素 */
	String COMMANDS = "commands";
	
	/** 包定义元素 */
	String PACKAGE = "package";
	
	/** 接口定义元素 */
	String INTERFACE = "interface";
	
	/** 包含表达式元素 */
	String INCLUDE = "include";
	
	/** 排除表达式元素 */
	String EXCLUDE = "exclude";
	
}
