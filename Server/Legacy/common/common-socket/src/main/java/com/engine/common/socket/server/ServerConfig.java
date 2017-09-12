package com.engine.common.socket.server;

import static com.engine.common.socket.server.ServerConfigConstant.*;

import java.io.IOException;
import java.net.InetSocketAddress;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

import javax.annotation.PostConstruct;

import org.apache.mina.integration.beans.InetSocketAddressEditor;
import org.apache.mina.transport.socket.DefaultSocketSessionConfig;
import org.apache.mina.transport.socket.SocketSessionConfig;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.beans.BeansException;
import org.springframework.context.ApplicationContext;
import org.springframework.context.ApplicationContextAware;
import org.springframework.core.io.Resource;

import com.engine.common.socket.ExecutorConfig;

/**
 * 服务器配置信息对象
 * 
 */
public class ServerConfig implements ApplicationContextAware {
	
	/** Spring 中的 Bean Name后缀 */
	public static final String BEAN_NAME_SUFFIX = "-config";
	/** 资源属性名 */
	public static final String PROP_NAME_LOCATION = "location";
	
	private static final Logger logger = LoggerFactory.getLogger(ServerConfig.class);
	
	/** 配置文件位置 */
	private String location;
	/** 配置文件 */
	private Properties properties;

	private List<InetSocketAddress> address;
	private SocketSessionConfig sessionConfig;
	private ExecutorConfig executorConfig;
	/** 自动启动 */
	private boolean autoStart;
	
	@PostConstruct
	public void initialize() {
		// 加载配置文件
		Resource resource = this.applicationContext.getResource(location);
		properties = new Properties();
		try {
			properties.load(resource.getInputStream());
		} catch (IOException e) {
			FormattingTuple message = MessageFormatter.format("资源[{}]加载失败", location);
			logger.error(message.getMessage(), e);
			throw new RuntimeException(message.getMessage(), e);
		}
		
		// 检查配置是否完整
		for (String key : KEYS) {
			if (!properties.containsKey(key)) {
				FormattingTuple message = MessageFormatter.format("配置缺失，配置键[{}]", key);
				logger.error(message.getMessage());
				throw new RuntimeException(message.getMessage());
			}
		}
		
		initializeAddress();
		initializeExecutorConfig();
		initializeSessionConfig();
		
		// 可选配置项的处理
		String value = getProperty(KEY_AUTO_START);
		if (value != null) {
			autoStart = Boolean.valueOf(value);
		}
	}

	/** 初始化连接会话配置 */
	private void initializeSessionConfig() {
		sessionConfig = new DefaultSocketSessionConfig();
		String value = properties.getProperty(KEY_BUFFER_READ);
		sessionConfig.setReadBufferSize(Integer.parseInt(value));
		value = properties.getProperty(KEY_BUFFER_WRITE);
		sessionConfig.setWriteTimeout(Integer.parseInt(value));
		value = properties.getProperty(KEY_TIMEOUT);
		sessionConfig.setBothIdleTime(Integer.parseInt(value));
	}

	/** 初始化线程池配置 */
	private void initializeExecutorConfig() {
		String min = properties.getProperty(KEY_POOL_MIN);
		String max = properties.getProperty(KEY_POOL_MAX);
		String idel = properties.getProperty(KEY_POOL_IDLE);
		executorConfig = ExecutorConfig.valueOf(Integer.parseInt(min), Integer.parseInt(max), Long.parseLong(idel));
	}

	/** 初始化地址 */
	private void initializeAddress() {
		String value = properties.getProperty(KEY_ADDRESS);
		address = new ArrayList<InetSocketAddress>();
		InetSocketAddressEditor editor = new InetSocketAddressEditor();
		for (String s : value.split(SPLIT)) {
			editor.setAsText(s);
			address.add((InetSocketAddress) editor.getValue());
		}
	}
	
	/**
	 * 获取配置值
	 * @param key 配置键
	 * @return
	 */
	public String getProperty(String key) {
		return properties.getProperty(key);
	}
	
	/**
	 * 是否自动启动
	 * @return
	 */
	public boolean isAutoStart() {
		return autoStart;
	}

	// Getter and Setter ...
	
	public void setLocation(String location) {
		this.location = location;
	}
	
	public List<InetSocketAddress> getAddress() {
		return address;
	}
	
	public SocketSessionConfig getSessionConfig() {
		return sessionConfig;
	}
	
	public ExecutorConfig getExecutorConfig() {
		return executorConfig;
	}
	
	// 实现接口的方法

	private ApplicationContext applicationContext;
	
	@Override
	public void setApplicationContext(ApplicationContext applicationContext) throws BeansException {
		this.applicationContext = applicationContext;
	}

}
