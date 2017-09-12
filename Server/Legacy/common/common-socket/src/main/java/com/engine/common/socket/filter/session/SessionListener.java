package com.engine.common.socket.filter.session;

import com.engine.common.socket.filter.session.SessionEvent.Type;

/**
 * 会话事件监听器接口
 * 
 */
public interface SessionListener {
	
	/**
	 * 获取该监听器负责监听的事件类型
	 * @return
	 */
	Type getType();

	/**
	 * 事件响应方法
	 * @param event 事件对象实例
	 */
	void onEvent(SessionEvent event);
}
