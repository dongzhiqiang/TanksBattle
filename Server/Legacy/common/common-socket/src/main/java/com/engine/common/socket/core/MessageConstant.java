package com.engine.common.socket.core;

/**
 * {@link Message}的常量定义
 * 
 */
public interface MessageConstant extends ResponseConstants {
	
	// 包定义部分

	/** 包标识 */
	int PACKAGE_INDETIFIER = 0xFFFFFFFF;
	/** 包长度 */
	int PACKAGE_LENGTH = 8;

	// 信息状态部分
	
	/** 状态:正常(请求状态) */
	int STATE_NORMAL = 0;

	/** 状态:回应(不是回应就是请求) */
	int STATE_RESPONSE = 1;

	/** 压缩标记位(没有该状态代表未经压缩) */
	int STATE_COMPRESS = 1 << 1;
	
	/** 转发标记位 */
	int STATE_FORWARD = 1 << 2;

	/** 附加信息标记位(存在就标识除<code>Body</code>还有附加信息<code>Attachment</code>) */
	int STATE_ATTACHMENT = 1 << 3;
	
	/** 原生信息标记位(有该状态代表信息体为原生类型，即不进行编解码) */
	int STATE_RAW = 1 << 4;

	/** 错误标记位(没有该状态代表正常) */
	int STATE_ERROR = 1 << 16;
	
}
