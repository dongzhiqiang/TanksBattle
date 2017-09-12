package com.engine.common.socket.server;

import java.io.IOException;
import java.net.InetSocketAddress;
import java.util.Map;
import java.util.Map.Entry;

import javax.annotation.PostConstruct;

import org.apache.mina.core.filterchain.IoFilter;
import org.apache.mina.filter.codec.ProtocolCodecFilter;
import org.apache.mina.filter.executor.ExecutorFilter;
import org.apache.mina.transport.socket.SocketAcceptor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.context.ApplicationEvent;
import org.springframework.context.ApplicationListener;
import org.springframework.context.event.ContextClosedEvent;
import org.springframework.context.event.ContextRefreshedEvent;

import com.engine.common.socket.ExecutorConfig;
import com.engine.common.socket.core.MessageCodecFactory;
import com.engine.common.socket.handler.Handler;

/**
 * 可配置的 {@link SocketAcceptor},该类为所包含的 {@link SocketAcceptor}提供更丰富的配置功能
 * 
 */
public class SocketServer implements ApplicationListener<ApplicationEvent> {

	private static final Logger logger = LoggerFactory.getLogger(SocketServer.class);

	private SocketAcceptor acceptor;
	private ServerConfig config;
	private Handler handler;
	private Map<String, IoFilter> filters;
	private ExecutorFilter executorFilter;

	@PostConstruct
	protected void initialize() {
		// 设置会话配置
		acceptor.getSessionConfig().setAll(config.getSessionConfig());
		// 设置控制器
		acceptor.setHandler(handler);
		// 设置过滤器集合
		if (filters != null) {
			for (Entry<String, IoFilter> entry : filters.entrySet()) {
				acceptor.getFilterChain().addLast(entry.getKey(), entry.getValue());
			}
		}
		acceptor.getFilterChain().addLast("codec", new ProtocolCodecFilter(new MessageCodecFactory()));
		// 设置处理线程池
		ExecutorConfig executorConfig = config.getExecutorConfig();
		executorFilter = executorConfig.build();
		acceptor.getFilterChain().addLast(ExecutorConfig.NAME, executorFilter);
	}

	@Override
	public void onApplicationEvent(ApplicationEvent event) {
		if (event instanceof ContextRefreshedEvent) {
			if (config.isAutoStart() && !acceptor.isActive()) {
				start();
			}
		} else if (event instanceof ContextClosedEvent) {
			close();
		}
	}

	/** 关闭服务器 */
	public void close() {
		if (logger.isInfoEnabled()) {
			logger.info("关闭服务器");
		}
		acceptor.unbind();
		acceptor.dispose(false);
		executorFilter.destroy();
	}

	/** 开启服务器 */
	public void start() {
		if (logger.isErrorEnabled()) {
			logger.error("开启服务器");
		}
		try {
			acceptor.setReuseAddress(true);
			for (InetSocketAddress address : config.getAddress()) {
				acceptor.bind(address);
				if (logger.isErrorEnabled()) {
					logger.error("绑定服务地址和端口到[{}:{}]", address.getHostName(), address.getPort());
				}
			}
		} catch (IOException e) {
			String message = "启动服务器失败";
			logger.error(message, e);
			throw new RuntimeException(message, e);
		}
	}

	// Getter and Setter ...

	public void setAcceptor(SocketAcceptor acceptor) {
		this.acceptor = acceptor;
	}

	public void setConfig(ServerConfig config) {
		this.config = config;
	}

	public void setHandler(Handler handler) {
		this.handler = handler;
	}

	public void setFilters(Map<String, IoFilter> filters) {
		this.filters = filters;
	}

}
