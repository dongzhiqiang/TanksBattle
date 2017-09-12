package com.game.gow.module.account.event;

import org.apache.commons.lang3.builder.ReflectionToStringBuilder;
import org.apache.mina.core.session.IoSession;

import com.engine.common.event.Event;
import com.engine.common.socket.other.IpUtils;

/**
 * 登出事件体,
 * 其它模块若在登出时需要进行相关处理，可以通过创建事件接收者处理
 * 
 * @author wenkin
 */
public class LogoutEvent implements IdentityEvent {

	/** 事件名 */
	public static final String NAME = "common:logout";

	/** 登出的用户标识 */
	private long id;
	/** 登出事件触发原因 */
	private int cause;
	/** 会话 */
	private IoSession session;
	/** 会话对应的IP地址 */
	private String ip;

	// Getter and Setter ...

	@Override
	public long getOwner() {
		return id;
	}

	public int getCause() {
		return cause;
	}

	@Override
	public String getName() {
		return NAME;
	}

	public IoSession getSession() {
		return session;
	}

	public String getIp() {
		return ip;
	}

	@Override
	public String toString() {
		return ReflectionToStringBuilder.toString(this);
	}

	// Static Method's ...

	/** 构造方法 */
	public static Event<LogoutEvent> valueOf(long id, int cause, IoSession session) {
		LogoutEvent body = new LogoutEvent();
		body.id = id;
		body.session = session;
		body.cause = cause;
		body.ip = IpUtils.getIp(session);
		return Event.valueOf(NAME, body);
	}

}
