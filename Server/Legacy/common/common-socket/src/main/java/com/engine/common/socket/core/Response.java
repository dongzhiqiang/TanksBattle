package com.engine.common.socket.core;

/**
 * 回应对象
 * 
 * 
 */
public class Response<T> implements ResponseConstants {

	/**
	 * 构建没有附加信息体的回应对象实例
	 * @param message
	 * @param body
	 * @return
	 */
	public static <T> Response<T> valueOfMessage(Message message, T body) {
		Response<T> result = new Response<T>();
		result.sn = message.getSn();
		result.command = message.getCommand();
		result.state = message.getState();
		result.body = body;
		return result;
	}

	/**
	 * 构建有附加信息体的回应对象实例
	 * @param message
	 * @param body
	 * @param attachment
	 * @return
	 */
	public static <T> Response<T> valueOfMessage(Message message, T body, byte[] attachment) {
		Response<T> result = new Response<T>();
		result.sn = message.getSn();
		result.command = message.getCommand();
		result.state = message.getState();
		result.body = body;
		result.setAttachment(attachment);
		return result;
	}

	/**
	 * 构建回应对象实例<br/>
	 * 用于通信层返回{@link Response}对象，在该情况下设置{@link Response#sn}/{@link Response#command}/{@link Response#state}
	 * 均无效，这些值会由通信处理器进行设置。
	 * 
	 * @param <T>
	 * @param body 信息体
	 * @param attachment 附加信息
	 * @return
	 */
	public static <T> Response<T> valueOf(T body, byte[] attachment) {
		Response<T> result = new Response<T>();
		result.body = body;
		result.setAttachment(attachment);
		return result;
	}

	/** 对应的请求序号 */
	private long sn;
	/** 指令定义 */
	private Command command;
	/** 回应内容 */
	private T body;
	/** 附加信息体 */
	private byte[] attachment;
	/** 状态 */
	private int state;

	/**
	 * 添加状态
	 * @param added 被添加的状态
	 */
	public void addState(int added) {
		state = state | added;
	}

	/**
	 * 移除状态
	 * @param removed 被移除的状态
	 */
	public void removeState(int removed) {
		state = state ^ removed;
	}

	/**
	 * 检查是否有指定状态
	 * @param check 被检查的状态
	 * @return
	 */
	public boolean hasState(int check) {
		return Header.hasState(state, check);
	}
	
	/**
	 * 检查请求是否不正确
	 * @return
	 */
	public boolean hasError() {
		return Header.hasState(state, MessageConstant.STATE_ERROR);
	}
	
	// Getter and Setter...

	public long getSn() {
		return sn;
	}

	public Command getCommand() {
		return command;
	}

	public T getBody() {
		return body;
	}

	public byte[] getAttachment() {
		return attachment;
	}

	public void setAttachment(byte[] attachment) {
		this.attachment = attachment;
		if (attachment != null && attachment.length > 0) {
			this.addState(MessageConstant.STATE_ATTACHMENT);
		} else {
			this.removeState(MessageConstant.STATE_ATTACHMENT);
		}
	}

	public int getState() {
		return state;
	}

	public void setState(int state) {
		this.state = state;
	}

}
