package com.engine.common.socket.server;

/**
 * 服务端配置常量约定
 * 
 * 
 */
public interface ServerConfigConstant {

	/** 分隔符定义 */
	String SPLIT = ",";

	/** 服务器的地址与端口配置，允许通过分隔符","指定多个地址 */
	String KEY_ADDRESS = "server.socket.address";
	/** 读取缓存大小设置 */
	String KEY_BUFFER_READ = "server.socket.buffer.read";
	/** 写入缓存大小设置 */
	String KEY_BUFFER_WRITE = "server.socket.buffer.write";
	/** 连接超时设置 */
	String KEY_TIMEOUT = "server.socket.timeout";
	/** 最小线程数设置 */
	String KEY_POOL_MIN = "server.socket.pool.min";
	/** 最大线程数设置 */
	String KEY_POOL_MAX = "server.socket.pool.max";
	/** 线程空闲时间设置，单位:millisecond */
	String KEY_POOL_IDLE = "server.socket.pool.idle";
	
	/** 设置服务器是否自动启动 */
	String KEY_AUTO_START = "server.config.auto_start";

	/** 必须的配置键 */
	String[] KEYS = { KEY_ADDRESS, KEY_BUFFER_READ, KEY_BUFFER_WRITE, KEY_TIMEOUT, KEY_POOL_MIN, KEY_POOL_MAX,
			KEY_POOL_IDLE };

}
