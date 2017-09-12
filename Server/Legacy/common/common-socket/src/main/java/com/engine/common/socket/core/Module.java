package com.engine.common.socket.core;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

/**
 * 指令模块声明
 * 
 * 
 */
public class Module {
	
	private static final Logger logger = LoggerFactory.getLogger(Module.class);

	/** 模块标识 */
	private int id;
	/** 子模块 */
	private Module next;

	public static Module valueOf(int... array) {
		byte[] bytes = new byte[array.length];
		for (int i = 0; i < array.length; i++) {
			if (array[i] < Byte.MIN_VALUE || array[i] > Byte.MAX_VALUE) {
				FormattingTuple message = MessageFormatter
						.format("构造参数值必须在[{}]到[{}]之间", Byte.MIN_VALUE, Byte.MAX_VALUE);
				logger.error(message.getMessage());
				throw new IllegalArgumentException(message.getMessage());
			}
			bytes[i] = (byte) array[i];
		}
		return valueOf(bytes, 0);
	}
	
	public static Module valueOf(byte...array) {
		return valueOf(array, 0);
	}

	private static Module valueOf(byte[] array, int offset) {
		if (array == null || array.length == 0) {
			String message = "构造参数不能没有内容";
			logger.error(message);
			throw new IllegalArgumentException(message);
		}
		Module result = new Module();
		result.id = array[offset++];
		if (offset < array.length) {
			result.next = valueOf(array, offset);
		}
		return result;
	}
	
	/**
	 * 将当前对象转换为通信表示形式
	 * @return
	 */
	public byte[] toBytes() {
		byte[] result = new byte[getDeep()];
		Module m = this;
		for (int i = 0; m != null; i++) {
			result[i] = (byte) m.id;
			m = m.next;
		}
		return result;
	}

	/**
	 * 获取指令深度
	 * @return
	 */
	public int getDeep() {
		int count = 1;
		Module m = next;
		while (m != null) {
			count++;
			m = m.next;
		}
		return count;
	}

	// Getter and Setter ...

	public int getId() {
		return id;
	}

	public Module getNext() {
		return next;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + id;
		result = prime * result + ((next == null) ? 0 : next.hashCode());
		return result;
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		Module other = (Module) obj;
		if (id != other.id)
			return false;
		if (next == null) {
			if (other.next != null)
				return false;
		} else if (!next.equals(other.next))
			return false;
		return true;
	}

}
