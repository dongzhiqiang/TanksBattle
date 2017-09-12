package com.game.gow.session;

import javax.annotation.PostConstruct;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.socket.filter.session.SessionManager;

/**
 * 会话监听器管理者，只是用于简化配置，没啥特别用途
 * 
 */
@Component
public class SessionListenerManager {

	@Autowired(required = false)
	private SessionCloseListener closeListener;
	@Autowired(required = false)
	private SessionReplacedListener replacedListener;

	@Autowired
	private SessionManager sessionManager;

	@PostConstruct
	public void initilize() {
		if (closeListener != null) {
			sessionManager.addListener(closeListener);
		}
		if (replacedListener != null) {
			sessionManager.addListener(replacedListener);
		}
	}

}
