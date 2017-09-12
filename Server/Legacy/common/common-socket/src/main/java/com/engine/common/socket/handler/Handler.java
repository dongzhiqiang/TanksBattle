package com.engine.common.socket.handler;

import java.util.List;
import java.util.Set;

import org.apache.mina.core.service.IoHandler;
import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Convertor;
import com.engine.common.socket.core.Message;
import com.engine.common.socket.core.Processor;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;

/**
 * 通信控制器的抽象表示
 * 
 */
public interface Handler extends IoHandler {
	
	/**
	 * 接收到指定会话的请求
	 * @param request
	 * @param session
	 */
	void receive(Request<?> request, IoSession session);

	/**
	 * 接收到指定会话的回应
	 * @param response
	 * @param session
	 */
	void receive(Response<?> response, IoSession session);

	/**
	 * 接收到指定会话的通信信息
	 * @param message
	 * @param session
	 */
	void receive(Message message, IoSession session);

	/**
	 * 向指定会话发送请求
	 * @param request
	 * @param sessions
	 */
	void send(Request<?> request, IoSession...sessions);
	
	/**
	 * 向指定会话发送通信信息
	 * @param message
	 * @param sessions
	 */
	void send(Message message, IoSession...sessions);
	
	/**
	 * 获取当前的{@link Convertor}
	 * @return
	 */
	Convertor getConvertor();

	/**
	 * 设置当前的{@link Convertor}
	 * @param convertor
	 */
	void setConvertor(Convertor convertor);

	/**
	 * 注册指令{@link Command}对应的处理器{@link Processor}和消息体定义{@link TypeDefinition}
	 * @param command 指令
	 * @param definition 消息体定义
	 * @param processor 处理器
	 * @throw {@link IllegalStateException} 重复注册时会抛出
	 */
	void register(Command command, TypeDefinition definition, Processor<?, ?> processor);
	
	/**
	 * 移除已经注册的指令
	 * @param command 指令
	 */
	void unregister(Command command);

	/**
	 * 获取对应的{@link Processor}
	 * @param command
	 * @return
	 */
	Processor<?, ?> getProcessor(Command command);
	
	/**
	 * 获取对应的{@link TypeDefinition}
	 * @param command
	 * @return
	 */
	TypeDefinition getDefinition(Command command);

	/**
	 * 获取全部已经注册的指令集合
	 * @return
	 */
	Set<Command> getCommands();

	/**
	 * 将请求对象编码为通信信息
	 * @param request 请求对象
	 * @return
	 */
	Message encodeRequest(Request<?> request);

	/**
	 * 将通信信息解码为回应对象
	 * @param message
	 * @return
	 */
	Response<?> decodeResponse(Message message);
	
	/**
	 * 添加监听器
	 * @param listener
	 */
	void addListener(Listener listener);
	
	/**
	 * 移除监听器
	 * @param listener
	 */
	void removeListener(Listener listener);
	
	/**
	 * 获取存储的消息
	 * @param owner
	 * @param csn
	 * @param session
	 * @return
	 */
	List<Message> getStoreMessage(long owner, int csn, IoSession session);

}
