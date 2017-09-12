package com.engine.common.socket.forward;

import java.net.InetSocketAddress;

import org.apache.mina.core.future.ConnectFuture;
import org.apache.mina.core.future.IoFutureListener;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.filter.codec.ProtocolCodecFilter;
import org.apache.mina.transport.socket.SocketConnector;
import org.apache.mina.transport.socket.nio.NioSocketConnector;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.socket.core.Message;
import com.engine.common.socket.core.MessageCodecFactory;

/**
 * 转发客户端
 * TODO 未完成
 * 
 */
public class ForwardClient {
	
	private static final Logger logger = LoggerFactory.getLogger(ForwardClient.class);
	
	private ForwardConfig config;
	@SuppressWarnings("unused")
	private CommunicateHandler communicateHandler;
	
	private ForwardHandler handler;
	private IoSession session;
	
	private SocketConnector connector;

	public void send(Message message) {
		session.write(message);
	}

	public void forward(IoSession session, Message message) {
		message.changeToForward(session);
		this.session.write(message);
	}

	public void setConfig(ForwardConfig config) {
		this.config = config;
	}

	public void setCommunicateHandler(CommunicateHandler handler) {
		this.communicateHandler = handler;
	}
	
	public void initialize() {
		connector = new NioSocketConnector();
		handler = new ForwardHandler();
	}
	
	public void start() {
		// 设置会话配置
		connector.getSessionConfig().setAll(config.getSessionConfig());
		// 设置控制器
		connector.setHandler(handler);
		// 设置编码
		connector.getFilterChain().addLast(MessageCodecFactory.NAME, new ProtocolCodecFilter(new MessageCodecFactory()));
	}
	
	/** 客户端关闭标记 */
	private volatile boolean closed;
	/** 连接中的标记位 */
	private volatile boolean connecting;
	
	/** 连接服务器 */
	@SuppressWarnings("unused")
	private synchronized void connect() {
		if (connecting || closed) {
			if (logger.isDebugEnabled()) {
				logger.debug("正在连接中或客户端已经关闭，忽略连接请求");
			}
			return;
		}
		
		InetSocketAddress address = config.getAddress();
		if (logger.isInfoEnabled()) {
			logger.info("开始连接服务器[{}]", config.getAddress());
		}
		ConnectFuture future = connector.connect(address);
		future.addListener(connectedListener);
	}
	
	/** 连接成功后的监听器 */
	private IoFutureListener<ConnectFuture> connectedListener = new IoFutureListener<ConnectFuture>() {
		@Override
		public void operationComplete(ConnectFuture future) {
			if (future.isConnected()) {
				session = future.getSession();
				connecting = false;
				
				
			}
		}
	};

}
