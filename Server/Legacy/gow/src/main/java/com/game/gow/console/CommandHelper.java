package com.game.gow.console;

import java.lang.reflect.Type;

import org.springframework.context.support.ClassPathXmlApplicationContext;

import com.engine.common.socket.client.Client;
import com.engine.common.socket.client.ClientFactory;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Processor;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;
import com.engine.common.socket.exception.SocketException;
import com.engine.common.socket.handler.TypeDefinition;
import com.engine.common.utils.ManagedException;

/**
 * 命令帮助对象
 * 
 */
public class CommandHelper {

	/** 默认的上下文配置名 */
	private static final String DEFAULT_APPLICATION_CONTEXT = "consoleContext.xml";
	
	/**
	 * 获取命令帮助对象实例
	 * @param context
	 * @return
	 */
	public static CommandHelper getInstance(String context) {
		return new CommandHelper(context);
	}
	
	private final ClassPathXmlApplicationContext applicationContext;
	private final ClientFactory clientFactory;
	private final Client client;
	
	private CommandHelper(String context) {
		if (context == null) {
			context = DEFAULT_APPLICATION_CONTEXT;
		}
		try {
			applicationContext = new ClassPathXmlApplicationContext(context);
			applicationContext.registerShutdownHook();
			applicationContext.start();
		} catch (Exception e) {
			throw new ManagedException(Errors.APPLICATION_CONTEXT_START_FAIL, "初始化上下文失败", e);
		}
		try {
			clientFactory = applicationContext.getBean(ClientFactory.class);
		} catch (Exception e) {
			throw new ManagedException(Errors.FACTORY_NOT_FOUND, "获取连接工厂失败", e);
		}
		try {
			client = clientFactory.getClient(true);
		} catch (Exception e) {
			throw new ManagedException(Errors.CREATE_CLIENT_FAIL, "创建连接失败", e);
		}
	}
	
	public void register(Command command, TypeDefinition definition, Processor<?, ?> processor) {
		try {
			client.register(command, definition, processor);
		} catch (Exception e) {
			throw new ManagedException(Errors.REQUEST_SEND_FAIL, "注册命令失败", e);
		}
	}

	/**
	 * 发送请求并接收回应
	 * @param <T>
	 * @param request 请求对象
	 * @param typeOfT 回应消息体类型
	 * @return
	 */
	public <T> Response<T> send(Request<?> request, Type typeOfT) {
		Response<T> response;
		try {
			response = client.send(request, typeOfT);
		} catch (SocketException e) {
			throw new ManagedException(Errors.REQUEST_SEND_FAIL, "请求发送失败", e);
		}
		if (response.hasError()) {
			throw new ManagedException(Errors.RESPONSE_HAS_ERROR, "服务器拒绝处理请求，状态码:" + response.getState());
		}
		return response;
	}

	/**
	 * 以默认编码格式发送请求
	 * @param request
	 * @return 
	 */
	public Response<?> send(Request<?> request) {
		Response<?> response;
		try {
			response = client.send(request);
		} catch (SocketException e) {
			throw new ManagedException(Errors.REQUEST_SEND_FAIL, "请求发送失败", e);
		}
		if (response.hasError()) {
			throw new ManagedException(Errors.RESPONSE_HAS_ERROR, "服务器拒绝处理请求，状态码:" + response.getState());
		}
		return response;
	}

	/**
	 * 等待客户端断开连接
	 */
	public void waitForDisconnect() {
		while (client.isConnected()) {
			Thread.yield();
		}
	}

	public void destory() {
		this.applicationContext.close();
	}

}
