package com.engine.common.socket.forward;

import static com.engine.common.socket.client.ClientConfigConstant.*;

import java.net.InetSocketAddress;

import org.apache.mina.integration.beans.InetSocketAddressEditor;
import org.apache.mina.transport.socket.DefaultSocketSessionConfig;
import org.apache.mina.transport.socket.SocketSessionConfig;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.socket.client.ClientConfigConstant;
import com.engine.common.socket.handler.ConfigSupport;

/**
 * 转发配置，负责提供转发的业务服的配置信息
 * TODO 未完成
 * 
 */
public class ForwardConfig extends ConfigSupport {

	@SuppressWarnings("unused")
	private static final Logger logger = LoggerFactory.getLogger(ForwardConfig.class);

	@Override
	public String[] getPropertyKeys() {
		return ClientConfigConstant.KEYS;
	}

	private InetSocketAddress address;
	private SocketSessionConfig sessionConfig;

	@Override
	protected void doInitialize() {
		initializeSessionConfig();
		initializeAddress();
	}

	/** 初始化连接会话配置 */
	private void initializeSessionConfig() {
		sessionConfig = new DefaultSocketSessionConfig();
		String value = getProperty(KEY_BUFFER_READ);
		sessionConfig.setReadBufferSize(Integer.parseInt(value));
		value = getProperty(KEY_BUFFER_WRITE);
		sessionConfig.setWriteTimeout(Integer.parseInt(value));
		value = getProperty(KEY_TIMEOUT);
		sessionConfig.setBothIdleTime(Integer.parseInt(value));
	}

	/** 初始化地址 */
	private void initializeAddress() {
		String value = getProperty(KEY_DEFAULT_ADDRESS);
		InetSocketAddressEditor editor = new InetSocketAddressEditor();
		editor.setAsText(value);
		address = (InetSocketAddress) editor.getValue();
	}

	public ForwardClient buildClient(CommunicateHandler handler) {
		ForwardClient client = new ForwardClient();
		client.setConfig(this);
		client.setCommunicateHandler(handler);
		return client;
	}

	public InetSocketAddress getAddress() {
		return address;
	}

	public SocketSessionConfig getSessionConfig() {
		return sessionConfig;
	}

}
