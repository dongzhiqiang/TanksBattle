package com.engine.common.socket.core;

import org.apache.commons.lang3.ArrayUtils;
import org.apache.mina.core.buffer.IoBuffer;
import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.utils.lang.ByteUtils;

/**
 * 通信信息对象
 * 
 * <pre>
 * 包：包头[包标识+包长度]+通信信息
 * 包头：是定长的，长度为:{@link MessageConstant#PACKAGE_LENGTH}
 * 
 * 通信信息：[信息头][信息体][附加内容]
 * 信息头:[长度][格式][状态][序号][会话][模块号][指令]
 * 信息体:[长度][内容]
 * 附加内容:[内容]
 * </pre>
 * 
 * 
 */
public class Message {

	private static final Logger logger = LoggerFactory.getLogger(Message.class);

	/** 信息头 */
	private Header header;
	/** 信息体 */
	private byte[] body = new byte[0];
	/** 通信附加信息体 */
	private byte[] attachment = new byte[0];

	/** 从原始包数据构建信息对象实例 */
	public static Message valueOf(byte[] array) {
		try {
			Message result = new Message();
			// 头长度
			int offset = ByteUtils.intFromByte(array);
			result.header = Header.valueOf(array);
			int length = ByteUtils.intFromByte(array, offset);
			result.body = ArrayUtils.subarray(array, offset += 4, offset += length - 4);
			result.attachment = ArrayUtils.subarray(array, offset, array.length);
			return result;
		} catch (Exception e) {
			String message = "创建通信信息对象异常";
			logger.error(message, e);
			throw new RuntimeException(message, e);
		}
	}

	public static Message valueOf(Header header, byte[] body) {
		Message result = new Message();
		result.header = header;
		result.body = body;
		return result;
	}

	public static Message valueOf(Header header, byte[] body, byte[] attachment) {
		Message result = new Message();
		result.header = header;
		result.body = body;
		result.attachment = attachment;
		return result;
	}
	
	public static Message toResponse(Request<?> request) {
		Message result = new Message();
		result.header = request.getHeader();
		return result;
	}

	/**
	 * 将对象转换为 byte[]
	 * 
	 * @return
	 */
	public byte[] toBytes() {
		byte[] header = this.header.toBytes();
		IoBuffer buffer = IoBuffer.allocate(header.length + 4 + (body == null ? 0 : body.length) + attachment.length);
		buffer.put(header);
		buffer.putInt((body == null ? 0 : body.length) + 4);
		if (body != null) {
			buffer.put(body);
		}
		buffer.put(attachment);
		return buffer.array();
	}

	/**
	 * 清空消息体
	 * 
	 * @return 对象自身
	 */
	public Message clearBody() {
		this.body = new byte[0];
		return this;
	}

	/**
	 * 将状态重置为正常{@link Response}的状态
	 * 
	 * @return 对象自身
	 */
	public Message changeToNormalResponse() {
		header.setState(MessageConstant.STATE_RESPONSE);
		return this;
	}

	/**
	 * 将状态重置为错误{@link Response}的状态
	 * @param state 错误状态值
	 * 
	 * @return 对象自身
	 */
	public Message changeToErrorResponse(int state) {
		header.setState(MessageConstant.STATE_ERROR + MessageConstant.STATE_RESPONSE + state);
		return this;
	}

	/**
	 * 将通信信息转为转发消息
	 * @param session
	 */
	public Message changeToForward(IoSession session) {
		header.addState(MessageConstant.STATE_FORWARD);
		this.setSession(session.getId());
		return this;
	}

	/**
	 * 检查是否有指定状态
	 * 
	 * @param checked 状态标识
	 * @return
	 */
	public boolean hasState(int checked) {
		return header.hasState(checked);
	}

	/**
	 * 添加状态
	 * 
	 * @param added 被添加的状态
	 */
	public void addState(int added) {
		header.addState(added);
	}

	/**
	 * 移除状态
	 * 
	 * @param removed 被移除的状态
	 */
	public void removeState(int removed) {
		header.removeState(removed);
	}
	
	/**
	 * 消息体加密（TEA算法）
	 */
	public void encrypt() {
		if(body != null){
//			byte[] copyBody = new byte[body.length];
//			System.arraycopy(body, 0, copyBody, 0, body.length);
			Tea.encrypt(body);
		}
	}
	
	
	/**
	 * 消息体解密（TEA算法）
	 */
	public void decrypt() {
		if(body != null){
			Tea.decrypt(body);
		}
	}

	@Override
	public String toString() {
		return "H:=" + header + " B:" + (body == null ? 0 : body.length) +" A:" + attachment.length;
	}

	// Getter and Setter ...

	public boolean isResponse() {
		return header.isResponse();
	}

	public byte getFormat() {
		return header.getFormat();
	}

	public int getState() {
		return header.getState();
	}

	public long getSn() {
		return header.getSn();
	}

	public Command getCommand() {
		return header.getCommand();
	}

	public byte[] getBody() {
		return body;
	}

	public void setBody(byte[] body) {
		this.body = body;
	}

	public byte[] getAttachment() {
		return attachment;
	}

	public void setAttachment(byte[] attachment) {
		this.attachment = attachment;
	}

	public long getSession() {
		return header.getSession();
	}

	public void setSession(long session) {
		header.setSession(session);
	}
	
	public int getCsn(){
		return header.getCsn();
	}
	
	public void setCsn(int csn) {
		header.setCsn(csn);
	}

}
