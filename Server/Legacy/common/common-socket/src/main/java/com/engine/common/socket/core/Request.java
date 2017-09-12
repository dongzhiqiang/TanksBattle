package com.engine.common.socket.core;

/**
 * 请求对象
 * 
 * 
 */
public class Request<T> {

	/**
	 * 构建没有附加信息体的请求对象实例
	 * 
	 * @param command 通信指令
	 * @param body 消息体内容
	 * @return
	 */
	public static <T> Request<T> valueOf(Command command, T body) {
		Request<T> result = new Request<T>();
		result.command = command;
		result.body = body;
		return result;
	}
	
	/**
	 * 构建有附加信息体的请求对象实例
	 * @param command 通信指令
	 * @param body 消息内容
	 * @param attachment 附加信息体
	 * @return
	 */
	public static <T> Request<T> valueOf(Command command, T body, byte[] attachment) {
		Request<T> result = new Request<T>();
		result.command = command;
		result.body = body;
		if (attachment != null && attachment.length > 0) {
			result.attachment = attachment;
			result.addState(MessageConstant.STATE_ATTACHMENT);
		}
		return result;
	}

	/**
	 * 构建请求对象实例
	 * @param message
	 * @param body
	 * @return
	 */
	public static <T> Request<T> valueOf(Message message, T body) {
		Request<T> result = new Request<T>();
		result.sn = message.getSn();
		result.format = message.getFormat();
		result.command = message.getCommand();
		result.body = body;
		result.state = message.getState();
		if (message.hasState(MessageConstant.STATE_ATTACHMENT)) {
			result.attachment = message.getAttachment();
		}
		return result;
	}

	/** 序号 */
	private long sn;
	/** 格式:默认0 */
	private byte format = 0;
	/** 指令定义 */
	private Command command;
	/** 消息内容 */
	private T body;
	/** 附加信息体 */
	private byte[] attachment;
	/** 状态 */
	private int state;
	/**通道序列号*/
	private int csn;

	public Header getHeader() {
		return Header.valueOf(format, state, sn,csn, 0, command);
	}
	
	/**
	 * 检查是否有指定的状态
	 * @param checked
	 * @return
	 */
	public boolean hasState(int checked) {
		return Header.hasState(this.state, checked);
	}

	/**
	 * 添加状态
	 * 
	 * @param added 被添加的状态
	 */
	public void addState(int added) {
		state = state | added;
	}

	/**
	 * 移除状态
	 * 
	 * @param removed 被移除的状态
	 */
	public void removeState(int removed) {
		state = state ^ removed;
	}

	// Getter and Setter ...

	public Command getCommand() {
		return command;
	}

	public T getBody() {
		return body;
	}

	public long getSn() {
		return sn;
	}

	/**
	 * 设置请求序号<br/>
	 * 不需要指定，该属于由{@link SimpleClient}负责生成
	 * 
	 * @param sn
	 */
	public void setSn(long sn) {
		this.sn = sn;
	}
	
	
	public int getCsn() {
		return csn;
	}

	public void setCsn(int csn) {
		this.csn = csn;
	}

	public int getState() {
		return state;
	}

	public byte getFormat() {
		return format;
	}

	public void setFormat(byte format) {
		this.format = format;
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
	
	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + (int) (sn ^ (sn >>> 32));
		return result;
	}

	@SuppressWarnings("rawtypes")
	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		Request other = (Request) obj;
		if (sn != other.sn)
			return false;
		return true;
	}
}
