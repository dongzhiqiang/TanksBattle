package com.game.gow.module.account.event;

import org.apache.commons.lang3.builder.ReflectionToStringBuilder;
import org.apache.mina.core.session.IoSession;

import com.engine.common.event.Event;

/**
 * 登录事件体,
 * 其它模块若在登录时需要进行相关处理，可以通过创建事件接收者处理
 * 
 * @author wenkin
 */
public class LoginEvent implements IdentityEvent {

	/** 事件名 */
	public static final String NAME = "common:login";

	/** 登录的用户标识 */
	private long id;
	
	/** 会话 */
	private IoSession session;

	// Getter and Setter ...
	
	@Override
	public long getOwner() {
		return id;
	}

	@Override
	public String getName() {
		return NAME;
	}

	public void setId(long id) {
		this.id = id;
	}

	public void setSession(IoSession session) {
		this.session = session;
	}

	public IoSession getSession() {
		return session;
	}

	@Override
	public String toString() {
		return ReflectionToStringBuilder.toString(this);
	}
	
	// Static Method's ...

	/** 构造方法 */
	public static Event<LoginEvent> valueOf(long id, IoSession session) {
		LoginEvent body = new LoginEvent();
		body.id = id;
		body.session = session;
		return Event.valueOf(NAME, body);
	}
	
}
