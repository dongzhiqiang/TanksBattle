package com.engine.common.socket.client;

/**
 * 客户端配置常量约定
 * 
 * 
 */
public interface ClientConfigConstant {

	// 配置键定义部分
	
	/** 读取缓存大小设置 */
	String KEY_BUFFER_READ = "client.socket.buffer.read";
	/** 写入缓存大小设置 */
	String KEY_BUFFER_WRITE = "client.socket.buffer.write";
	/** 连接超时设置 */
	String KEY_TIMEOUT = "client.socket.timeout";
	
	/** 等待回应的超时时间,单位毫秒(可选,默认:5000) */
	String KEY_RESPONSE_TIMEOUT = "client.socket.response.timeout";
	/** 默认服务器的地址与端口配置(可选) */
	String KEY_DEFAULT_ADDRESS = "client.default.address";
	/** 非活跃客户端移除延时,单位秒(可选,默认:300) */
	String KEY_REMOVE_TIME = "client.remove.times";

	/** 必须的配置键 */
	String[] KEYS = { KEY_BUFFER_READ, KEY_BUFFER_WRITE, KEY_TIMEOUT };

}
