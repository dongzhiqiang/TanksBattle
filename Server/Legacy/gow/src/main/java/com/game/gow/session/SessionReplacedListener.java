package com.game.gow.session;

import java.util.Set;

import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

import com.engine.common.socket.filter.session.SessionEvent;
import com.engine.common.socket.filter.session.SessionListener;
import com.engine.common.socket.filter.session.SessionReplacedEvent;
import com.engine.common.socket.filter.session.SessionEvent.Type;

/**
 * 通信会话替换事件监听器，用于接收会话的替换事件
 * 
 */
@Component
public class SessionReplacedListener implements SessionListener {

	private static final Logger logger = LoggerFactory.getLogger(SessionReplacedListener.class);

	@Override
	public Type getType() {
		return Type.REPLACED;
	}

	@Override
	public void onEvent(SessionEvent event) {
		if (logger.isDebugEnabled()) {
			logger.debug("用户[{}]发生断线重连", event.getIdentity());
		}

		// 清空被T的session属性
		IoSession kicked = ((SessionReplacedEvent) event).getPrev();
		Set<Object> attributeKeys = kicked.getAttributeKeys();
		for (Object key : attributeKeys) {
			kicked.removeAttribute(key);
		}
		if (logger.isDebugEnabled()) {
			logger.debug("清空SESSION[{}]属性[{}=>{}]", new Object[] { kicked.getId(), attributeKeys.size(),
				kicked.getAttributeKeys().size() });
		}
	}

}
