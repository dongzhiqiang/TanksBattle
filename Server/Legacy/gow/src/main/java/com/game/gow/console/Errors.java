package com.game.gow.console;

/**
 * 指令错误状态码
 * 
 */
public interface Errors {

	/** 初始化应用上下文失败 */
	int APPLICATION_CONTEXT_START_FAIL = -1;
	
	/** 客户端工厂不存在 */
	int FACTORY_NOT_FOUND = -2;
	
	/** 无法创建与服务器的连接 */
	int CREATE_CLIENT_FAIL = -3;
	
	/** 请求发送失败 */
	int REQUEST_SEND_FAIL = -4;

	/** 回应有错误信息(服务器无法处理请求) */
	int RESPONSE_HAS_ERROR = -5;
	
	/** 配置信息缺失 */
	int PROPERTIES_NOT_FOUND = -6;

	/** 未知错误 */
	int UNKNOWN = -255;
	
}
