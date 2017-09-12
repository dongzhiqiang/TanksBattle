package com.engine.common.socket.core;

import java.util.Arrays;

import org.apache.commons.lang3.ArrayUtils;

import com.engine.common.utils.lang.ByteUtils;

/**
 * 通信指令
 * 
 * 
 */
public class Command {

	/** 指令所属模块 */
	private Module module;
	/** 指令标识 */
	private int command;

	/**
	 * 创建通信指令对象实例
	 * @param command 指令标识
	 * @param modules 所属模块标识
	 * @return
	 */
	public static Command valueOf(int command, byte...modules) {
		Command result = new Command();
		result.command = command;
		result.module = Module.valueOf(modules);
		return result;
	}

	/**
	 * 创建通信指令对象实例
	 * @param command 指令标识
	 * @param modules 所属模块标识
	 * @return
	 */
	public static Command valueOf(int command, int...modules) {
		Command result = new Command();
		result.command = command;
		result.module = Module.valueOf(modules);
		return result;
	}
	
	/**
	 * 创建通信指令对象实例
	 * @param bytes 原始通信信息数组
	 * @param start 开始位置
	 * @param end 结束位置
	 * @return
	 */
	public static Command valueOf(byte[] bytes, int start, int end) {
		int command = ByteUtils.intFromByte(bytes, start);
		byte[] modules = ArrayUtils.subarray(bytes, start + 4, end);

		Command result = new Command();
		result.command = command;
		result.module = Module.valueOf(modules);
		return result;
	}

	/**
	 * 将当前对象转换为通信表示格式
	 * @return
	 */
	public byte[] toBytes() {
		int deep = module.getDeep();
		byte[] result = new byte[deep + 4];
		ByteUtils.intToByte(command, result, 0);
		
		int offset = 4;
		Module m = module;
		while (m != null) {
			result[offset++] = (byte) m.getId();
			m = m.getNext();
		}
		return result;
	}
	
	// Getter and Setter ...
	
	public Module getModule() {
		return module;
	}

	public int getCommand() {
		return command;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + command;
		result = prime * result + ((module == null) ? 0 : module.hashCode());
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
		Command other = (Command) obj;
		if (command != other.command)
			return false;
		if (module == null) {
			if (other.module != null)
				return false;
		} else if (!module.equals(other.module))
			return false;
		return true;
	}

	@Override
	public String toString() {
		return "M:" + Arrays.toString(module.toBytes()) + " C:" + this.command;
	}
}
