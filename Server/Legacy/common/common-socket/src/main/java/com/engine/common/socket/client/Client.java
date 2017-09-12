package com.engine.common.socket.client;

import java.lang.reflect.Type;
import java.util.Date;

import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Processor;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;
import com.engine.common.socket.handler.TypeDefinition;

/**
 * 客户端接口
 * 
 */
public interface Client {

	/**
	 * 注册指令{@link Command}对应的处理器{@link Processor}和消息体定义{@link TypeDefinition}
	 * @param command 指令
	 * @param definition 消息体定义
	 * @param processor 处理器
	 * @throw {@link IllegalStateException} 重复注册时会抛出
	 */
	void register(Command command, TypeDefinition definition, Processor<?, ?> processor);

	/**
	 * 发送请求并接收回应
	 * @param <T>
	 * @param request 请求对象
	 * @param typeOfT 回应消息体类型
	 * @return
	 */
	<T> Response<T> send(Request<?> request, Type typeOfT);

	/**
	 * 以默认编码格式发送请求
	 * @param request
	 * @return
	 */
	Response<?> send(Request<?> request);

	/**
	 * 关闭与服务器的连接
	 */
	void close();

	/**
	 * 连接服务器
	 */
	void connect();

	/**
	 * 检查是否需要保持连接
	 * @return
	 */
	boolean isKeepAlive();

	/**
	 * 获取客户端的最后操作时间戳
	 * @return
	 */
	Date getTimestamp();

	/**
	 * 检查会话是否处于连接状态
	 * @return
	 */
	boolean isConnected();

	/**
	 * 连接是否有效
	 * @return
	 */
	boolean isDisposed();

	/**
	 * 取消保持连接状态
	 */
	void disableKeepAlive();

	/**
	 * 获取连接的地址
	 */
	String getAddress();

}
