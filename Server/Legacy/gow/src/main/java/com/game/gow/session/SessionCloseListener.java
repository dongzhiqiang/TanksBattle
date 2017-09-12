package com.game.gow.session;

import java.util.Set;

import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.event.EventBus;
import com.engine.common.socket.filter.session.SessionEvent;
import com.engine.common.socket.filter.session.SessionEventCause;
import com.engine.common.socket.filter.session.SessionListener;
import com.engine.common.socket.filter.session.SessionEvent.Type;
import com.game.gow.module.account.event.LogoutEvent;

/**
 * 通信会话关闭事件监听器，用于接收已鉴权的会话关闭事件，再抛出{@link LogoutEvent}事件到{@link EventBus}
 * 
 */
@Component
public class SessionCloseListener implements SessionListener {

	private static final Logger logger = LoggerFactory.getLogger(SessionCloseListener.class);

	@Autowired
	private EventBus eventBus;

	@Override
	public Type getType() {
		return Type.CLOSED;
	}

	@Override
	public void onEvent(SessionEvent event) {
		int cause = event.getCause();
		IoSession session = event.getSession();
		if (cause == SessionEventCause.NORMAL) {
			if (logger.isDebugEnabled()) {
				logger.debug("玩家[{}]登出，发出登出事件", event.getIdentity());
			}
			eventBus.post(LogoutEvent.valueOf((Long) event.getIdentity(), cause, session));
		}
		if (cause == SessionEventCause.KICK) {
			if (logger.isDebugEnabled()) {
				logger.debug("踢玩家[{}]下线，发出登出事件", event.getIdentity());
			}
			eventBus.post(LogoutEvent.valueOf((Long) event.getIdentity(), cause, session));
		} else if (cause == SessionEventCause.ENFORCE_LOGOUT) {
			if (logger.isDebugEnabled()) {
				logger.debug("玩家[{}]被强制登出，不发出登出事件", event.getIdentity());
			}
		} else {
			if (logger.isDebugEnabled()) {
				logger.debug("未知的会话关闭原因[{}]", event.getCause());
			}
		}

		// 清空被T的session属性
		Set<Object> attributeKeys = session.getAttributeKeys();
		for (Object key : attributeKeys) {
			session.removeAttribute(key);
		}
		if (logger.isDebugEnabled()) {
			logger.debug("清空SESSION[{}]属性[{}=>{}]", new Object[] { session.getId(), attributeKeys.size(),
				session.getAttributeKeys().size() });
		}
	}

}
