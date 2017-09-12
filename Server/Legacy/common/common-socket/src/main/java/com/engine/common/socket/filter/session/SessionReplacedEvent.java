package com.engine.common.socket.filter.session;

import org.apache.mina.core.session.IoSession;

/**
 * 会话替换事件
 * 
 * 
 */
public class SessionReplacedEvent extends SessionEvent {

	/** 被替换的会话 */
	private final IoSession prev;

	public SessionReplacedEvent(int cause, Object identity, IoSession session, IoSession prev) {
		super(cause, SessionEvent.Type.REPLACED, identity, session);
		this.prev = prev;
	}

	/**
	 * 获取被替换的会话
	 * 
	 * @return
	 */
	public IoSession getPrev() {
		return prev;
	}

}
