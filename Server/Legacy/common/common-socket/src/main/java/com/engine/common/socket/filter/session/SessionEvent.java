package com.engine.common.socket.filter.session;

import org.apache.mina.core.session.IoSession;

/**
 * 会话事件对象
 * 
 * 
 */
public class SessionEvent {

	/**
	 * 会话事件类型
	 * 
	 * 
	 */
	public static enum Type {
		/** 会话完成身份验证 */
		IDENTIFIED,
		/** 会话关闭(如有延时关闭，则在延时关闭后触发) */
		CLOSED,
		/** 会话替换(在有延时关闭情况下，新会话完成旧会话属性复制后触发) */
		REPLACED;
	}

	/** 事件类型 */
	private final Type type;
	/** 原因标识 */
	private final int cause;
	/** 用户标识 */
	private final Object identity;
	/** 通信会话 */
	private final IoSession session;

	/**
	 * 构造方法
	 * 
	 * @param type 事件类型
	 * @param identity 身份标识
	 * @throws IllegalArgumentException 参数缺失时抛出
	 */
	public SessionEvent(int cause, Type type, Object identity, IoSession session) {
		if (type == null || identity == null) {
			throw new IllegalArgumentException("事件构造参数不能为空");
		}
		this.cause = cause;
		this.type = type;
		this.identity = identity;
		this.session = session;
	}

	/**
	 * 获取事件类型
	 * 
	 * @return
	 */
	public Type getType() {
		return type;
	}

	/**
	 * 获取身份标识
	 * 
	 * @return
	 */
	public Object getIdentity() {
		return identity;
	}

	/**
	 * 获取原因标识
	 * 
	 * @return
	 */
	public int getCause() {
		return Math.abs(cause);
	}

	/**
	 * 获取通信会话对象
	 * @return
	 */
	public IoSession getSession() {
		return session;
	}

}
