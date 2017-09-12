package com.engine.common.socket.schema;

/**
 * 服务器常量定义
 * 
 */
public interface Names {

	/** 分隔符 */
	String SPLIT = "-";
	
	/** 接收器名称 */
	String ACCEPTOR_NAME = "acceptor";
	/** 连接器名称 */
	String CONNECTOR_NAME = "connector";
	
	/** 指令注册器名称 */
	String REGISTER_NAME = "register";
	/** 指令注册器工厂名称 */
	String REGISTER_FACTORY_NAME = "registerFactory";
	/** 控制器名称 */
	String HANDLER_NAME = "handler";
	/** 控制器工厂名称 */
	String HANDLER_FACTORY_NAME = "handlerFactory";
	/** 配置名称 */
	String CONFIG_NAME = "config";
	/** 转换器名称 */
	String CONVERTOR_NAME = "convertor";
	/** 控制器持有者名称 */
	String HOLDER_NAME = "holder";
	/** 请求分发器名称 */
	String DISPATCHER_NAME = "dispatcher";
	/** 参数创建器名 */
	String PARAMETER_BUILDER_NAME = "builder";
	
	/** 过滤器集合名 */
	String FILTERS_NAME = "filters";
	/** 内容编解码器集合名 */
	String CODERS_NAME = "coders";
	/** 包名集合 */
	String PACKAGES_NAME = "packages";
	/** 接口名集合 */
	String INTERFACES_NAME = "interfaces";
	/** 包含表达式集合 */
	String INCLUDES_NAME = "includes";
	/** 排除表达式集合 */
	String EXCLUDES_NAME = "excludes";
	
	/** 扫描 Beans */
	String SCAN_BEANS_NAME = "scanBeans";
	/** 扫描 Interfaces */
	String SCAN_INTERFACES_NAME = "scanInterfaces";

	/** 通信编码器名称 */
	String CODEC_FILTER_NAME = "codec";
}
