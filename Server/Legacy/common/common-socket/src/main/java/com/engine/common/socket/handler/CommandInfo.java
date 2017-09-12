package com.engine.common.socket.handler;

import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Processor;

/**
 * 指令信息对象
 * 
 * 
 */
@SuppressWarnings("rawtypes")
public class CommandInfo {

	/** 指令对象 */
	private final Command command;
	/** 消息体类型定义 */
	private final TypeDefinition definition;
	/** 指令处理器 */
	private final Processor processor;

	/**
	 * 构造方法
	 * @param command 指令对象(必须)
	 * @param definition 消息体类型定义(必须)
	 * @param processor 指令处理器(可选)
	 * @throws IllegalArgumentException 指令或消息体为空时抛出
	 */
	public CommandInfo(Command command, TypeDefinition definition, Processor processor) {
		if (command == null || definition == null) {
			throw new IllegalArgumentException("指令或消息体定义不能为空");
		}
		this.command = command;
		this.definition = definition;
		this.processor = processor;
	}

	/**
	 * 检查是否有指令处理器
	 * @return true:有, false:没有
	 */
	public boolean hasProcessor() {
		return processor != null;
	}

	/**
	 * 获取指令对象
	 * @return
	 */
	public Command getCommand() {
		return command;
	}

	/**
	 * 获取消息体类型定义
	 * @return
	 */
	public TypeDefinition getDefinition() {
		return definition;
	}

	/**
	 * 获取指令处理器
	 * @return
	 */
	public Processor getProcessor() {
		return processor;
	}
	
	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((command == null) ? 0 : command.hashCode());
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
		CommandInfo other = (CommandInfo) obj;
		if (command == null) {
			if (other.command != null)
				return false;
		} else if (!command.equals(other.command))
			return false;
		return true;
	}

	@Override
	public String toString() {
		return "指令:" + command + " 消息体:" + definition + " 处理器:" + (processor == null ? "不存在" : "存在");
	}
}
