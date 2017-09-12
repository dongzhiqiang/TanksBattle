package com.engine.common.socket.filter.session;

/**
 * 事件原因标识
 * <pre>
 * 正数:发送事件通知
 * 负数:不发送事件通知
 * </pre>
 * 
 */
public interface SessionEventCause {

	/** 一般情况 */
	int NORMAL = 0;
	
	/** 踢下线 */
	int KICK = 1;
	
	/** 强制退出 */
	int ENFORCE_LOGOUT = 2;
	
	/** 封停 */
	int INVALID = 3;

}
