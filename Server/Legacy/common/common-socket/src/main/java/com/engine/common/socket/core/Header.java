package com.engine.common.socket.core;

import static com.engine.common.socket.core.MessageConstant.*;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.utils.lang.ByteUtils;

/**
 * 信息头<br/>
 * 格式:[长度][格式][状态][序号][会话][指令][模块号]
 * 
 */
public class Header {

	/** 默认消息头长度 */
	public static final int DEFAULT_HEADER_LENGTH = 33;

	/** 格式 */
	private byte format;
	/** 状态 */
	private int state;
	/** 序号 */
	private long sn;
	/** 会话标识 */
	private long session;
	/**会话序列号(对于每个会话,服务器每次为发送的消息包递增序号)*/
	private int csn;
	/** 指令 */
	private Command command;

	/**
	 * 获取信息头信息，该方法会执行读取头长度
	 * 
	 * @param bytes 信息数据
	 * @return
	 */
	public static Header valueOf(byte[] bytes) {
		// 分析 byte[]
		int offset = 0;
		int length = ByteUtils.intFromByte(bytes, offset);
		byte format = bytes[offset += 4];
		int state = ByteUtils.intFromByte(bytes, ++offset);
		long sn = ByteUtils.longFromByte(bytes, offset += 4);
		int csn=ByteUtils.intFromByte(bytes, offset += 8);
//		long session = ByteUtils.longFromByte(bytes, offset += 8);
		long session =0;
//		Command command = Command.valueOf(bytes, offset += 8, length);
		Command command = Command.valueOf(bytes, offset += 4, length);
		// 构建对象
		Header result = new Header();
		result.format = format;
		result.state = state;
		result.sn = sn;
		result.csn=csn;
		result.session = session;
		result.command = command;
		return result;
	}

	public static Header valueOf(byte format, int state, long sn, int csn,long session, Command command) {
		Header result = new Header();
		result.format = format;
		result.state = state;
		result.sn = sn;
		result.csn=csn;
		result.session = session;
		result.command = command;
		return result;
	}

	/**
	 * 将信息头转换为 byte[] 表示格式
	 * 
	 * @param header
	 * @return
	 */
	public byte[] toBytes() {
		// 构建byte[]
		IoBuffer buffer = IoBuffer.allocate(DEFAULT_HEADER_LENGTH).setAutoExpand(true);
		buffer.putInt(0);
		buffer.put(format);
		buffer.putInt(state);
		buffer.putLong(sn);
		buffer.putInt(csn);
//		buffer.putLong(session);
		buffer.put(command.toBytes());
		buffer.flip();
		byte[] bytes = new byte[buffer.remaining()];
		buffer.get(bytes);
		// 更新正确的长度并返回
		ByteUtils.intToByte(bytes.length, bytes, 0);
		return bytes;
	}

	@Override
	public String toString() {
		return "[F=" + format + ", ST=" + state + ", SN=" + sn + ", CSN="+csn+", IO=" + session + ", C=" + command + "]";
	}

	// 状态处理方法


	/**
	 * 检查状态值中是否有指定状态
	 * 
	 * @param state 状态值
	 * @param check 被检查的状态
	 * @return
	 */
	public static boolean hasState(int state, int check) {
		return (state & check) == check ? true : false;
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

	/**
	 * 检查是否有指定状态
	 * 
	 * @param check 被检查的状态
	 * @return
	 */
	public boolean hasState(int check) {
		return hasState(state, check);
	}

	/**
	 * 是否回应信息
	 * 
	 * @return
	 */
	public boolean isResponse() {
		return hasState(state, STATE_RESPONSE);
	}

	// Getter and Setter ...

	public byte getFormat() {
		return format;
	}

	public void setFormat(byte format) {
		this.format = format;
	}

	public int getState() {
		return state;
	}

	public void setState(int state) {
		this.state = state;
	}

	public long getSn() {
		return sn;
	}

	public void setSn(long sn) {
		this.sn = sn;
	}

	public int getCsn() {
		return csn;
	}

	public void setCsn(int csn) {
		this.csn = csn;
	}

	public long getSession() {
		return session;
	}

	public void setSession(long session) {
		this.session = session;
	}

	public Command getCommand() {
		return command;
	}

	public void setCommand(Command command) {
		this.command = command;
	}

}
