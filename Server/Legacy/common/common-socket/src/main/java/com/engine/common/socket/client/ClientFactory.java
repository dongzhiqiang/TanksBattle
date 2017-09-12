package com.engine.common.socket.client;

import static com.engine.common.socket.client.ClientConfigConstant.*;

import java.io.IOException;
import java.lang.reflect.Type;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.util.Calendar;
import java.util.Date;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Properties;
import java.util.WeakHashMap;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.DelayQueue;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

import javax.annotation.PostConstruct;
import javax.annotation.PreDestroy;

import org.apache.commons.lang3.StringUtils;
import org.apache.mina.core.filterchain.IoFilter;
import org.apache.mina.core.future.CloseFuture;
import org.apache.mina.core.future.ConnectFuture;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.filter.codec.ProtocolCodecFilter;
import org.apache.mina.transport.socket.DefaultSocketSessionConfig;
import org.apache.mina.transport.socket.SocketConnector;
import org.apache.mina.transport.socket.SocketSessionConfig;
import org.apache.mina.transport.socket.nio.NioSocketConnector;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.beans.BeansException;
import org.springframework.context.ApplicationContext;
import org.springframework.context.ApplicationContextAware;
import org.springframework.core.io.Resource;

import com.engine.common.socket.codec.Coder;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Convertor;
import com.engine.common.socket.core.MessageCodecFactory;
import com.engine.common.socket.core.Processor;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;
import com.engine.common.socket.exception.SocketException;
import com.engine.common.socket.handler.AbstractListener;
import com.engine.common.socket.handler.CommandRegister;
import com.engine.common.socket.handler.Handler;
import com.engine.common.socket.handler.SnGenerator;
import com.engine.common.socket.handler.TypeDefinition;
import com.engine.common.socket.server.SocketServer;
import com.engine.common.utils.concurrent.DelayedElement;

/**
 * 客户端工厂对象
 * 
 */
public class ClientFactory implements ApplicationContextAware {

	private static final Logger logger = LoggerFactory.getLogger(ClientFactory.class);

	/** 连接失败最大重试次数, 超出次数未连接上则抛弃Client */
	private static final int MAX_RETRY = 10;

	/**
	 * 工厂创建的客户端对象
	 * 
	 */
	class SimpleClient implements Client {

		/** 连接地址 */
		private final InetSocketAddress address;
		/** 与 {@link SocketServer}的连接 */
		private final SocketConnector connector;
		/** 客户端控制器 */
		private final Handler handler;

		/** 当前与服务器连接的会话 */
		private IoSession session;
		/** 请求缓存 */
		private ConcurrentHashMap<Long, Request<?>> requests = new ConcurrentHashMap<Long, Request<?>>();
		/** 回应缓存 */
		private WeakHashMap<Request<?>, Response<?>> responses = new WeakHashMap<Request<?>, Response<?>>();
		/** 等待回应的哑对象 */
		private Object waitForResponse = new Object();
		/** 尝试连接次数 */
		private int retry;

		/** 构造方法 */
		public SimpleClient(InetSocketAddress address, CommandRegister register) {
			this.address = address;
			// 创建控制器与配置
			handler = new ClientHandler(register);
			if (clone) {
				handler.setConvertor(convertor.clone());
			} else {
				handler.setConvertor(convertor);
			}
			handler.addListener(new AbstractListener() {
				@Override
				public void sent(Request<?> request, IoSession... sessions) {
					requests.put(request.getSn(), request);
				}

				@Override
				public void received(Response<?> response, IoSession... sessions) {
					synchronized (waitForResponse) {
						Request<?> request = requests.get(response.getSn());
						if (request == null) {
							return;
						}
						responses.put(request, response);
						waitForResponse.notifyAll();
					}
				}
			});

			// 创建与配置连接器
			connector = new NioSocketConnector();
			// 设置会话配置
			connector.getSessionConfig().setAll(sessionConfig);
			// 设置控制器
			connector.setHandler(handler);
			// 设置过滤器集合
			if (filters != null) {
				for (Entry<String, IoFilter> entry : filters.entrySet()) {
					connector.getFilterChain().addLast(entry.getKey(), entry.getValue());
				}
			}
			// 设置编码
			connector.getFilterChain().addLast("codec", new ProtocolCodecFilter(new MessageCodecFactory()));
		}

		@Override
		public String getAddress() {
			return fromInetSocketAddress(address);
		}

		@Override
		public synchronized void connect() {
			if (session != null && session.isConnected()) {
				return;
			}
			if (logger.isDebugEnabled()) {
				logger.debug("开始连接服务器[{}]", address);
			}
			retry++;
			try {
				ConnectFuture future = connector.connect(address);
				future.awaitUninterruptibly();
				session = future.getSession();
				if (logger.isDebugEnabled()) {
					logger.debug("与服务器[{}]连接成功", address);
				}
				retry = 0;
			} catch (Exception e) {
				if (retry > MAX_RETRY) {
					close();
				}
				throw new SocketException(e);
			}
		}

		@Override
		public synchronized void close() {
			if (session == null || session.isClosing() || !session.isConnected()) {
				if (retry > MAX_RETRY) {
					// 释放资源
					connector.dispose();
					if (logger.isDebugEnabled()) {
						logger.debug("与服务器[{}]连接重试过多[{}]", address, retry);
					}
				}
				return;
			}
			if (logger.isDebugEnabled()) {
				logger.debug("开始关闭与服务器[{}]的连接", address);
			}
			CloseFuture close = session.close(false);
			close.awaitUninterruptibly();
			// 释放资源
			connector.dispose();
			if (logger.isDebugEnabled()) {
				logger.debug("已经关闭与服务器[{}]的连接", address);
			}
		}

		private boolean keepAlive = false;

		@Override
		public boolean isKeepAlive() {
			return keepAlive;
		}

		private Date timestamp = new Date();

		@Override
		public Date getTimestamp() {
			if (isKeepAlive()) {
				return new Date();
			}
			return timestamp;
		}

		public void setTimestamp(Date timestamp) {
			this.timestamp = timestamp;
		}

		@Override
		public boolean isConnected() {
			if (session == null) {
				return false;
			}
			if (session.isConnected()) {
				return true;
			}
			return false;
		}

		@Override
		@SuppressWarnings("unchecked")
		public <T> Response<T> send(Request<?> request, Type typeOfT) {
			Command command = request.getCommand();
			TypeDefinition definition = handler.getDefinition(command);
			Type requestType = definition.getResponse();
			if (requestType.equals(typeOfT)) {
				return (Response<T>) send(request);
			}

			FormattingTuple message = MessageFormatter.format("要求的信息体类型[{}]与定义类型[{}]不一致", typeOfT, requestType);
			logger.error(message.getMessage());
			throw new SocketException(message.getMessage());
		}

		@Override
		public Response<?> send(Request<?> request) {
			if (!isConnected()) {
				connect();
			}

			long sn = generator.next();
			request.setSn(sn);
			try {
				long send = System.currentTimeMillis();
				long now = System.currentTimeMillis();
				long timeout = now + responseTimeout;

				handler.send(request, session);
				setTimestamp(new Date());

				while (now < timeout) {
					synchronized (waitForResponse) {
						try {
							if (responses.containsKey(request)) {
								return responses.remove(request);
							}
							waitForResponse.wait(500);
							now = System.currentTimeMillis();
						} catch (InterruptedException e) {
							FormattingTuple error = MessageFormatter.format("请求[{}]在等待回应时被打断", request.getSn(), e);
							logger.error(error.getMessage());
							throw new SocketException(error.getMessage(), e);
						}
					}
				}

				// 超时, 重试一次
				if (responses.containsKey(request)) {
					return responses.remove(request);
				}

				FormattingTuple error = MessageFormatter.arrayFormat("请求[{}]发送失败, 发送时间[{}], 超时时间[{}]", new Object[] {
					request.getCommand(), new Date(send), new Date(timeout) });
				logger.error(error.getMessage());
				throw new SocketException(error.getMessage());
			} finally {
				requests.remove(sn);
			}
		}

		@Override
		public void register(Command command, TypeDefinition definition, Processor<?, ?> processor) {
			handler.register(command, definition, processor);
		}

		@Override
		public void disableKeepAlive() {
			keepAlive = false;
		}

		@Override
		public boolean isDisposed() {
			return connector.isDisposed() || connector.isDisposing();
		}
	}

	private final Properties properties = new Properties();

	/** 配置文件位置 */
	private String location;
	/** 默认连接地址 */
	private String defaultAddress;
	/** 全局会话配置 */
	private SocketSessionConfig sessionConfig;
	/** 回应超时时间(毫秒) */
	private int responseTimeout = 5000;
	/** 客户端过期时间(秒) */
	private int removeTimes = 300;

	/** 指令注册器(作为所有客户端的私有指令注册器的种子) */
	private CommandRegister commandRegister;
	/** 会话的过滤器 */
	private Map<String, IoFilter> filters;
	/** 编码转换器 */
	private Convertor convertor;
	/** 是否克隆编码转换器 */
	private boolean clone;

	// 客户端工厂自身的生命周期方法

	/** 初始化方法 */
	@PostConstruct
	public void initialize() {
		// 加载配置文件
		Resource resource = this.applicationContext.getResource(location);
		try {
			properties.load(resource.getInputStream());
		} catch (IOException e) {
			FormattingTuple message = MessageFormatter.format("通信客户端配置文件[{}]加载失败", location);
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

		initConfig();

		// 启动非活跃客户端清理线程
		Thread removeClientThread = new Thread(new Runnable() {
			@Override
			public void run() {
				while (true) {
					try {
						DelayedElement<? extends Client> e = removeQueue.take();
						Client client = e.getContent();
						if (client == null || !client.isDisposed()) {
							continue; // 客户端已经不存在
						}
						String address = client.getAddress();
						if (!client.isKeepAlive()) {
							Lock lock = loadClientLock(address);
							lock.lock();
							try {
								// 检查是否超时没有活动
								Calendar calendar = Calendar.getInstance();
								calendar.setTime(client.getTimestamp());
								calendar.add(Calendar.SECOND, removeTimes);
								if (calendar.getTime().before(new Date())) {
									clients.remove(address);
									client.close();
									continue;
								}
							} finally {
								lock.unlock();
							}
						}

						// 客户端还处于活跃状态，延时再进行检查
						Calendar calendar = Calendar.getInstance();
						calendar.add(Calendar.SECOND, removeTimes);
						DelayedElement<Client> element = DelayedElement.valueOf(client, calendar.getTime());
						removeQueue.put(element);
					} catch (InterruptedException e) {
						logger.error("过期客户端清理线程被打断", e);
					} catch (Exception e) {
						logger.error("过期客户端清理线程出现未知异常", e);
					}
				}
			}
		}, "过期客户端清理");
		removeClientThread.setDaemon(true);
		removeClientThread.start();
	}

	/** 销毁方法 */
	@PreDestroy
	public void destory() {
		for (Client client : clients.values()) {
			client.close();
		}
	}

	// 通信客户端管理方法

	/** 序列号生成器(全部客户端共用) */
	private SnGenerator generator = new SnGenerator();
	/** 当前的全部可用客户端 */
	private ConcurrentHashMap<String, SimpleClient> clients = new ConcurrentHashMap<String, SimpleClient>();
	/** 客户端操作锁 */
	private ConcurrentHashMap<String, Lock> locks = new ConcurrentHashMap<String, Lock>();
	/** 过期客户端删除队列 */
	private final DelayQueue<DelayedElement<? extends Client>> removeQueue = new DelayQueue<DelayedElement<? extends Client>>();

	/**
	 * 直接创建一个不受客户端工厂管理的客户端
	 * @param address 服务器地址加端口(ip:port)
	 * @return
	 */
	public Client createClient(String address) {
		InetSocketAddress socketAddress = toInetSocketAddress(address);
		return new SimpleClient(socketAddress, commandRegister.clone());
	}

	/**
	 * 获取默认客户端
	 * @param keepAlive 是否维持客户端的生命
	 * @return
	 * @throws IllegalStateException 默认客户端连接服务器的配置缺失时抛出
	 */
	public Client getClient(boolean keepAlive) {
		if (defaultAddress == null) {
			throw new IllegalStateException("默认客户端连接服务器的配置缺失");
		}
		return getClient(defaultAddress, keepAlive);
	}

	/**
	 * 获取指定地址对应的通信客户端
	 * @param address 服务器地址加端口(ip:port)
	 * @param keepAlive 是否维持客户端的生命
	 * @return
	 */
	public Client getClient(String address, boolean keepAlive) {
		Lock lock = loadClientLock(address);
		lock.lock();
		try {
			SimpleClient client = clients.get(address);
			if (client != null && !client.isDisposed()) {
				client.setTimestamp(new Date());
			} else {
				InetSocketAddress socketAddress = toInetSocketAddress(address);
				client = new SimpleClient(socketAddress, commandRegister.clone());
				SimpleClient prev = clients.putIfAbsent(address, client);
				if (prev != null) {
					// 重试次数过多, 丢弃旧连接
					if (prev.retry > MAX_RETRY) {
						if (!clients.replace(address, prev, client)) {
							client = clients.get(address);
						}
					} else {
						client = prev;
					}
				} else {
					Calendar calendar = Calendar.getInstance();
					calendar.add(Calendar.SECOND, removeTimes);
					removeQueue.put(DelayedElement.valueOf(client, calendar.getTime()));
				}
			}

			if (keepAlive) {
				client.keepAlive = true;
			}
			return client;
		} catch (Exception e) {
			removeClientLock(address);
			// 异常处理
			FormattingTuple message = MessageFormatter.format("无法获取指定服务器地址的客户端对象[{}]", address, e);
			logger.error(message.getMessage());
			throw new SocketException(message.getMessage(), e);
		} finally {
			lock.unlock();
		}
	}

	// 内部方法

	/** 初始化配置信息 */
	private void initConfig() {
		// 初始化会话配置
		sessionConfig = new DefaultSocketSessionConfig();
		String value = properties.getProperty(KEY_BUFFER_READ);
		sessionConfig.setReadBufferSize(Integer.parseInt(value));
		value = properties.getProperty(KEY_BUFFER_WRITE);
		sessionConfig.setWriteTimeout(Integer.parseInt(value));
		value = properties.getProperty(KEY_TIMEOUT);
		sessionConfig.setBothIdleTime(Integer.parseInt(value));
		// 初始化可选配置
		defaultAddress = properties.getProperty(KEY_DEFAULT_ADDRESS);
		if (properties.getProperty(KEY_RESPONSE_TIMEOUT) != null) {
			responseTimeout = Integer.parseInt(properties.getProperty(KEY_RESPONSE_TIMEOUT));
		}
		if (properties.getProperty(KEY_REMOVE_TIME) != null) {
			removeTimes = Integer.parseInt(properties.getProperty(KEY_REMOVE_TIME));
		}
	}

	/**
	 * 获取客户端操作锁
	 * @param address 服务器地址
	 * @return 不会返回null
	 */
	private Lock loadClientLock(String address) {
		Lock result = locks.get(address);
		if (result != null) {
			return result;
		}

		result = new ReentrantLock();
		Lock prev = locks.putIfAbsent(address, result);
		return prev == null ? result : prev;
	}

	/**
	 * 移除客户端操作锁
	 * @param address 服务器地址
	 */
	private void removeClientLock(String address) {
		locks.remove(address);
	}

	/** 连接地址 */
	private InetSocketAddress toInetSocketAddress(String text) {
		if (StringUtils.isEmpty(text)) {
			throw new IllegalArgumentException("无效的地址字符串: " + text);
		}

		int colonIndex = text.lastIndexOf(":");
		if (colonIndex > 0) {
			String host = text.substring(0, colonIndex);
			if (!"*".equals(host)) {
				int port = parsePort(text.substring(colonIndex + 1));
				return new InetSocketAddress(host, port);
			}
		}

		int port = parsePort(text.substring(colonIndex + 1));
		return new InetSocketAddress(port);
	}

	/** 连接地址 */
	private String fromInetSocketAddress(InetSocketAddress address) {
		InetAddress inetAddress = address.getAddress();
		return inetAddress == null ? "UNKNOW" : inetAddress.getHostAddress() + ":" + address.getPort();
	}

	/** 获取端口值 */
	private int parsePort(String s) {
		try {
			return Integer.parseInt(s);
		} catch (NumberFormatException nfe) {
			throw new IllegalArgumentException("无效的端口值: " + s);
		}
	}

	// Getter and Setter ...

	private ApplicationContext applicationContext;

	@Override
	public void setApplicationContext(ApplicationContext applicationContext) throws BeansException {
		this.applicationContext = applicationContext;
	}

	public void setCommandRegister(CommandRegister commandRegister) {
		this.commandRegister = commandRegister;
	}

	public void setFilters(Map<String, IoFilter> filters) {
		this.filters = filters;
	}

	public void setLocation(String location) {
		this.location = location;
	}

	public void setConvertor(Convertor convertor) {
		this.convertor = convertor;
	}

	public void setClone(boolean clone) {
		this.clone = clone;
	}

	// 需要设置的配置属性名

	public static final String PROP_COMMAND_REGISTER = "commandRegister";
	public static final String PROP_FILTERS = "filters";
	public static final String PROP_LOCATION = "location";
	public static final String PROP_CONVERTOR = "convertor";
	public static final String PROP_CLONE = "clone";

	// 静态构造方法

	public static ClientFactory valueOf(SocketSessionConfig sessionConfig, CommandRegister register,
			Map<Byte, Coder> coders) {
		ClientFactory factory = new ClientFactory();
		Convertor convertor = new Convertor();
		convertor.setCoders(coders);
		factory.setConvertor(convertor);
		factory.sessionConfig = sessionConfig;
		factory.setCommandRegister(register);
		factory.setClone(false);
		return factory;
	}

}
