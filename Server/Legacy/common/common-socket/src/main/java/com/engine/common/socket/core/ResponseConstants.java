package com.engine.common.socket.core;

/**
 * 回应状态
 * 
 */
public interface ResponseConstants {
	
	/** 用于推送的消息序号 */
	long DEFAULT_SN = -1;
	
	/** 请求指令不存在 */
	int COMMAND_NOT_FOUND = 1 << 17;
	
	/** 解码异常 */
	int DECODE_EXCEPTION = 1 << 18;

	/** 编码异常 */
	int ENCODE_EXCEPTION = 1 << 19;
	
	/** 参数异常 */
	int PARAMETER_EXCEPTION = 1 << 20;
	
	/** 处理异常 */
	int PROCESSING_EXCEPTION = 1 << 21;

	/** 未知异常 */
	int UNKNOWN_EXCEPTION = 1 << 25;

}
