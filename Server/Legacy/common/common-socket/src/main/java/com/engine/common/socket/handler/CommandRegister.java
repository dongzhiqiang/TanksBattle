package com.engine.common.socket.handler;

import java.util.HashSet;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Processor;
import com.engine.common.socket.exception.ProcessorNotFound;
import com.engine.common.socket.exception.TypeDefinitionNotFound;

/**
 * 指令注册器
 * 
 */
public class CommandRegister implements Cloneable {
	
	private static final Logger logger = LoggerFactory.getLogger(CommandRegister.class);

	/** 指令->指令信息，映射表 */
	private ConcurrentHashMap<Command, CommandInfo> infos = new ConcurrentHashMap<Command, CommandInfo>();
	
	/**
	 * 默认构造方法
	 */
	public CommandRegister() {
		this.infos = new ConcurrentHashMap<Command, CommandInfo>();
	}
	
	/**
	 * 构造方法
	 * @param infos
	 */
	private CommandRegister(Map<Command, CommandInfo> infos) {
		this.infos = new ConcurrentHashMap<Command, CommandInfo>(infos);
	}
	
	/**
	 * 注册指令{@link Command}对应的处理器{@link Processor}和消息体定义{@link TypeDefinition}
	 * @param command 指令
	 * @param definition 消息体定义
	 * @param processor 消息处理器
	 * @throw {@link IllegalStateException} 重复注册时会抛出
	 */
	public void register(Command command, TypeDefinition definition, Processor<?, ?> processor) {
		CommandInfo info = new CommandInfo(command, definition, processor);
		
		CommandInfo prev = infos.putIfAbsent(info.getCommand(), info);
		if (prev != null) {
			FormattingTuple message = MessageFormatter.format("指令[{}]重复注册:{}", command, prev);
			logger.error(message.getMessage());
			throw new IllegalStateException(message.getMessage());
		}
		
		if (logger.isDebugEnabled()) {
			logger.debug("完成指令注册:{}", info);
		}
	}
	
	/**
	 * 移除已经注册的指令
	 * @param command 指令
	 */
	public void unregister(Command command) {
		infos.remove(command);
	}
	
	/**
	 * 检查是否包含指定的指令
	 * @param command 指令
	 * @return
	 */
	public boolean contain(Command command) {
		return infos.contains(command);
	}

	/**
	 * 获取对应的{@link Processor}
	 * @param command 指令
	 * @return 指定对应的处理器，不会返回null
	 * @throws ProcessorNotFound 处理器不存在时抛出
	 */
	public Processor<?, ?> getProcessor(Command command) {
		CommandInfo info = infos.get(command);
		if (info == null || !info.hasProcessor()) {
			FormattingTuple message = MessageFormatter.format("指令[{}]对应的业务处理器不存在", command);
			if (logger.isDebugEnabled()) {
				logger.debug(message.getMessage());
			}
			throw new ProcessorNotFound(message.getMessage());
		}
		return info.getProcessor();
	}
	
	/**
	 * 获取对应的{@link TypeDefinition}
	 * @param command 指令
	 * @return 指定对应的消息体定义，不会返回null
	 * @throws TypeDefinitionNotFound 消息体不存在时抛出
	 */
	public TypeDefinition getDefinition(Command command) {
		CommandInfo info = infos.get(command);
		if (info == null) {
			FormattingTuple message = MessageFormatter.format("指令[{}]对应的消息体类型定义不存在", command);
			if (logger.isDebugEnabled()) {
				logger.debug(message.getMessage());
			}
			throw new TypeDefinitionNotFound(message.getMessage());
		}
		return info.getDefinition();
	}

	/**
	 * 获取全部已经注册的指令集合
	 * @return
	 */
	public Set<Command> getCommands() {
		return new HashSet<Command>(infos.keySet());
	}

	@Override
	public CommandRegister clone() {
		return new CommandRegister(this.infos);
	}
}
