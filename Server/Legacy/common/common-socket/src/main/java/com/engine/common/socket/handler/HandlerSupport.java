package com.engine.common.socket.handler;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.TimeUnit;

import org.apache.mina.core.service.IoHandlerAdapter;
import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.protocol.utils.QuickLZUtils;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Convertor;
import com.engine.common.socket.core.Header;
import com.engine.common.socket.core.Message;
import com.engine.common.socket.core.MessageConstant;
import com.engine.common.socket.core.Processor;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;
import com.engine.common.socket.core.Session;
import com.engine.common.socket.exception.DecodeException;
import com.engine.common.socket.exception.EncodeException;
import com.engine.common.socket.exception.ProcessingException;
import com.engine.common.socket.exception.ProcessorNotFound;
import com.engine.common.socket.exception.SessionParameterException;
import com.engine.common.socket.exception.TypeDefinitionNotFound;
import com.engine.common.socket.filter.session.SessionManager;

/**
 * 控制器抽象类，提供部分控制器代码的标准实现，以简化控制器的开发
 * 
 */
public abstract class HandlerSupport extends IoHandlerAdapter implements Handler {

	private static final Logger logger = LoggerFactory.getLogger(HandlerSupport.class);

	/** 设置编码转换器的属性名 */
	public static final String PROP_CONVERTOR = "convertor";

	private final CommandRegister commandRegister;
	private final SyncSupport syncSupport = new SyncSupport();
	private final ListenerSupport listenerSupport = new ListenerSupport();

	// 解压超时配置值
	private final long timeout = 1000;
	private final TimeUnit unit = TimeUnit.MILLISECONDS;
	
	//断线重连消息包补发
	
	/** 序列号生成器缓存<key:玩家id,value:玩家序号生成器>*/
	protected ConcurrentHashMap<Long, CsnGenerator> csnGenerators = new ConcurrentHashMap<Long, CsnGenerator>();	
	/** sent玩家的消息缓存<key:玩家id,value:消息> */
	protected ConcurrentHashMap<Long, List<Message>> pushCache = new ConcurrentHashMap<Long, List<Message>>();

	/**
	 * 构造方法
	 * @param register 指令注册器(必须)
	 */
	public HandlerSupport(CommandRegister register) {
		if (register == null) {
			throw new IllegalArgumentException("指令注册器不能为空");
		}
		this.commandRegister = register;
	}

	// 接收的方法部分

	@Override
	public void messageReceived(IoSession session, Object in) throws Exception {
		if (in == null) {
			logger.warn("收到的消息为空");
			return;
		}
		if (!(in instanceof Message)) {
			logger.warn("无效的消息类型[{}]", in.getClass().getName());
			return;
		}
		if (session.isClosing() || !session.isConnected()) {
			if (logger.isDebugEnabled()) {
				logger.debug("会话[{}]已经关闭或关闭中,忽略信息处理", session.getId());
			}
			return;
		}

		Message message = (Message) in;
		receive(message, session);
	}

	@Override
	@SuppressWarnings("rawtypes")
	public void receive(Message message, IoSession session) {
		// 压缩信息处理
		if (message.hasState(MessageConstant.STATE_COMPRESS)) {
			if (logger.isDebugEnabled()) {
				logger.debug("通信信息[{}]进行解压", message);
			}
			byte[] bytes = null;
			try {
				bytes = QuickLZUtils.unzip(message.getBody(), timeout, unit);
				message.setBody(bytes);
			} catch (Exception e) {
				send(MessageHelper.decodeException(message, new DecodeException(e.getMessage(), e)), session);
				return;
			}
		}

		if (message.isResponse()) {
			// 回应的处理
			Response response = messageToResponse(message);
			receive(response, session);
		} else {
			// 请求的处理
			doReceive(message, session);
		}
	}

	/**
	 * 将{@link Message}转换为{@link Request}(不做验证)
	 * @param message
	 * @return
	 */
	@SuppressWarnings("rawtypes")
	private Request messageToRequest(Message message) {
		// 原生数据的处理
		if (message.hasState(MessageConstant.STATE_RAW)) {
			return Request.valueOf(message, message.getBody());
		}

		// 根据类型编码的处理
		Command command = message.getCommand();
		TypeDefinition definition = getDefinition(command);
		// 调试信息
		if (logger.isDebugEnabled()) {
			logger.debug("对信息体[{}]进行解码", new String(message.getBody()));
		}
		Object body = convertor.decode(definition.getFormat(), message.getBody(), definition.getRequest());
		Request result = Request.valueOf(message, body);
		result.setFormat(definition.getFormat());
		return result;
	}

	/**
	 * 将{@link Message}转换为{@link Response}(不做验证)
	 * @param message
	 * @return
	 */
	@SuppressWarnings("rawtypes")
	private Response messageToResponse(Message message) {

		// 有错误状态
		if (message.hasState(MessageConstant.STATE_ERROR)) {
			return (Response) Response.valueOfMessage(message, null);
		}
		// 原生数据类型
		if (message.hasState(MessageConstant.STATE_RAW)) {
			return Response.valueOfMessage(message, message.getBody());
		}
		// 根据类型解码
		Command command = message.getCommand();
		TypeDefinition definition = getDefinition(command);
		Object body = convertor.decode(definition.getFormat(), message.getBody(), definition.getResponse());
		// 创建回应对象
		Response response = null;
		if (message.hasState(MessageConstant.STATE_ATTACHMENT)) {
			response = Response.valueOfMessage(message, body, message.getAttachment());
		} else {
			response = Response.valueOfMessage(message, body);
		}
		return response;
	}

	private void doReceive(final Message message, final IoSession session) {
		@SuppressWarnings("rawtypes")
		final Request request = messageToRequest(message);
		TypeDefinition definition = getDefinition(request.getCommand());
		if (definition.isSync()) {
			// 方法为同步执行，加入到同步队列
			syncSupport.execute(definition.getSyncKey(), new Runnable() {
				public void run() {
					doReceive(message, request, session);
				}
			});
		} else {
			doReceive(message, request, session);
		}
	}

	private void doReceive(Message message, Request<?> request, final IoSession session) {
		try {
			receive(request, session);
		} catch (TypeDefinitionNotFound e) {
			if(logger.isInfoEnabled()) {
				logger.info("消息体类型定义不存在", e);
			}
			send(MessageHelper.commandNotFound(message, e), session);
		} catch (ProcessorNotFound e) {
			if(logger.isInfoEnabled()) {
				logger.info("消息处理器不存在", e);
			}
			send(MessageHelper.commandNotFound(message, e), session);
		} catch (DecodeException e) {
			if(logger.isInfoEnabled()) {
				logger.info("接受消息解码异常", e);
			}
			send(MessageHelper.decodeException(message, e), session);
		} catch (EncodeException e) {
			if(logger.isInfoEnabled()) {
				logger.info("发送消息编码异常", e);
			}
			send(MessageHelper.encodeException(message, e), session);
		} catch (ProcessingException e) {
			if (e instanceof SessionParameterException) {
				send(MessageHelper.parameterException(message, e), session);
				logger.warn("会话[{}]SESSION参数异常，连接[{}]将被强制关闭", session.getId(), session.getRemoteAddress());
				session.close(false);
				return;
			}
			if(logger.isInfoEnabled()) {
				logger.info("消息处理逻辑异常", e);
			}
			send(MessageHelper.processingException(message, e), session);
		} catch (Exception e) {
			if(logger.isInfoEnabled()) {
				logger.info("消息处理未知异常", e);
			}
			send(MessageHelper.unknownException(message, e), session);
		}
	}

	@Override
	public void receive(Request<?> request, IoSession session) {
		fireReceivedRequest(request, session);
		// 处理请求
		byte[][] bytes = processRequest(request, session);
		// 封装回应信息体
		TypeDefinition definition = getDefinition(request.getCommand());
		Message message = Message.toResponse(request);
		message.clearBody().changeToNormalResponse();
		message.setBody(bytes[0]);
		// 设置状态标识
		if (definition.isResponseCompress()) {
			message.addState(MessageConstant.STATE_COMPRESS);
		}
		if (definition.isResponseRaw()) {
			message.addState(MessageConstant.STATE_RAW);
		}
		if (bytes[1] != null && bytes[1].length > 0) {
			message.setAttachment(bytes[1]);
			message.addState(MessageConstant.STATE_ATTACHMENT);
		}

		if (logger.isDebugEnabled()) {
			if (message.hasState(MessageConstant.STATE_COMPRESS) || message.hasState(MessageConstant.STATE_RAW)) {
				logger.debug("[会话:{} 请求{}]的回应，信息体[长度:{}]", new Object[] { session.getId(), request.getCommand(),
					message.getBody().length });
			} else {
				logger.debug("[会话:{} 请求{}]的回应，信息体[{}]", new Object[] { session.getId(), request.getCommand(),
					new String(message.getBody()) });
			}
			if (message.hasState(MessageConstant.STATE_ATTACHMENT)) {
				logger.debug("附加信息内容[{}]", Arrays.toString(message.getAttachment()));
			}
		}

		// 向会话回写回应
		send(message, session);
	}

	/**
	 * 处理请求并返回消息体(不做参数验证)
	 * @param request 请求对象
	 * @param session 请求会话
	 * @return [0]:信息体,[1]:附加信息
	 */
	@SuppressWarnings({ "rawtypes", "unchecked" })
	private byte[][] processRequest(Request request, IoSession session) {
		// 获取请求定义
		Command command = request.getCommand();
		TypeDefinition definition = getDefinition(command);
		Processor<?, ?> processor = getProcessor(command);

		if (logger.isDebugEnabled()) {
			logger.debug("收到指令[{}]请求, 处理器[{}]", command, processor);
		}

		// 处理请求
		Object processRet = processor.process(request, session);
		// 处理结果编码
		byte[][] body = { null, null };
		if (definition.isResponseRaw()) {
			// 原生类型
			body[0] = (byte[]) processRet;
		} else if (processRet != null && processRet.getClass() == Response.class) {
			Response response = (Response) processRet;
			Object realBody = response.getBody();
			body[0] = convertor.encode(request.getFormat(), realBody, definition.getResponse());
			if (response.hasState(MessageConstant.STATE_ATTACHMENT)) {
				body[1] = response.getAttachment();
			}
		} else {
			body[0] = convertor.encode(request.getFormat(), processRet, definition.getResponse());
		}
		// 对回应信息体进行压缩
		if (definition.isResponseCompress()) {
			long s = System.nanoTime();
			byte[] ziped = QuickLZUtils.zip(body[0]);
			if (logger.isDebugEnabled()) {
				logger.debug("对指令[{}]的回应信息体进行压缩:[压缩前:{} 压缩后:{}]时间:[{}]NS", new Object[] { request.getCommand(),
					body[0].length, ziped.length, (System.nanoTime() - s) });
			}
			body[0] = ziped;
		}
		return body;
	}

	@Override
	public void receive(Response<?> response, IoSession session) {
		fireReceivedResponse(response, session);
	}

	// 发送的方法部分

	@Override
	public void send(Message message, IoSession... sessions) {
		for (IoSession session : sessions) {
			doMessageStore(session,message);
			if (!session.isConnected() || session.isClosing()) {
				continue;
			}
			session.write(message);
		}
	}

	@Override
	public void send(Request<?> request, IoSession... sessions) {
		if (logger.isDebugEnabled()) {
			logger.debug("推送指令[{}]信息, 目标数量[{}]", request.getCommand(), sessions.length);
		}
		Message message = encodeRequest(request);
		fireSentRequest(request, sessions);
		send(message, sessions);
	}

	// 转换器相关的方法实现

	private Convertor convertor;

	@Override
	public Convertor getConvertor() {
		return convertor;
	}

	@Override
	public void setConvertor(Convertor convertor) {
		this.convertor = convertor;
	}

	@Override
	public Message encodeRequest(Request<?> request) {
		Header header = request.getHeader();
		TypeDefinition definition = getDefinition(request.getCommand());

		header.setFormat(definition.getFormat());
		byte[] body = null;
		if (definition.isRequestRaw()) {
			// 原生类型
			body = (byte[]) request.getBody();
			header.addState(MessageConstant.STATE_RAW);
		} else {
			// Java类型，需要编码
			body = convertor.encode(definition.getFormat(), request.getBody(), definition.getRequest());
			if (logger.isDebugEnabled()) {
				logger.debug("信息体编码后结果是[{}]", new String(body));
			}
		}

		// 对信息体做压缩
		if (definition.isRequestCompress()) {
			byte[] bytes = QuickLZUtils.zip(body);
			if (logger.isDebugEnabled()) {
				logger.debug("对指令[{}]的请求信息体进行压缩:[压缩前:{} 压缩后:{}],", new Object[] { request.getCommand(), body.length,
					bytes.length });
			}
			header.addState(MessageConstant.STATE_COMPRESS);
			body = bytes;
		}

		if (request.hasState(MessageConstant.STATE_ATTACHMENT)) {
			return Message.valueOf(header, body, request.getAttachment());
		} else {
			return Message.valueOf(header, body);
		}
	}

	@Override
	public Response<?> decodeResponse(Message message) {
		Command command = message.getCommand();
		TypeDefinition definition = getDefinition(command);

		byte[] bytes = message.getBody();
		if (message.hasState(MessageConstant.STATE_COMPRESS)) {
			if (logger.isDebugEnabled()) {
				logger.debug("对指令[{}]的回应信息体进行解压", message.getCommand());
			}
			try {
				bytes = QuickLZUtils.unzip(bytes, timeout, unit);
			} catch (Exception e) {
				throw new DecodeException(e.getMessage(), e);
			}
		}

		Object content = convertor.decode(message.getFormat(), bytes, definition.getResponse());
		return Response.valueOfMessage(message, content);
	}

	// 让 CommandSupport 代理指令相关的方法

	@Override
	public void register(Command command, TypeDefinition definition, Processor<?, ?> processor) {
		commandRegister.register(command, definition, processor);
	}

	@Override
	public void unregister(Command command) {
		commandRegister.unregister(command);
	}

	@Override
	public Processor<?, ?> getProcessor(Command command) {
		return commandRegister.getProcessor(command);
	}

	@Override
	public TypeDefinition getDefinition(Command command) {
		return commandRegister.getDefinition(command);
	}

	@Override
	public Set<Command> getCommands() {
		return commandRegister.getCommands();
	}
	
	//断线重连补发消息
	private void doMessageStore(IoSession session,Message message){
		@SuppressWarnings("unchecked")
		ConcurrentHashMap<String, Object> attributes = (ConcurrentHashMap<String, Object>) session
				.getAttribute(Session.MAIN_KEY);
		Object value = null;
		if (attributes != null)
		{
			value = attributes.get(SessionManager.IDENTITY);
		}
		if(value!=null){
			Long owner=(Long) value;
			CsnGenerator csnGernerator = csnGenerators.get(owner);
			if (csnGernerator == null)
			{
				csnGernerator = new CsnGenerator();
				csnGenerators.put(owner, csnGernerator);
			}
			message.setCsn(csnGernerator.next());	
			//TODO过滤不需要补发的消息(聊天)
			if (!message.getCommand().equals(Command.valueOf(-7, 2))
					&& !message.getCommand().equals(Command.valueOf(-1, 11)))
			{
				List<Message> list = pushCache.get(owner);
				if (list == null)
				{
					list = new ArrayList<Message>();
					list.add(message);
					pushCache.put(owner, list);
				}else{
					if (!list.contains(message))
					{
						if (list.size() > 10)
						{
							list.remove(0);
						}
						list.add(message);
					}
				}
			}		
		}
		
	}
	
	@Override
	public List<Message> getStoreMessage(final long owner, int csn, IoSession session) {

		List<Message> result = new ArrayList<Message>();
		List<Message> tmp = pushCache.get(owner);
		Comparator<? super Message> cmp = new Comparator<Message>() {

			@Override
			public int compare(Message o1, Message o2) {
				if (o1.getCsn() > o2.getCsn()) {
					return 1;
				} else if (o1.getCsn() < o2.getCsn()) {
					return -1;
				}
				return 0;
			}

		};

		if (tmp != null) {
			for (Message message : tmp) {
				if (message == null) {
					continue;
				}
				if (message.getCsn() > csn) {
					result.add(message);
				}
			}
		}

		Collections.sort(result, cmp);
		return result;
	}

	// Listener 相关的方法

	@Override
	public void addListener(Listener listener) {
		listenerSupport.addListener(listener);
	}

	@Override
	public void removeListener(Listener listener) {
		listenerSupport.removeListener(listener);
	}

	// 发送监听事件的相关方法

	public void fireSentRequest(Request<?> request, IoSession... sessions) {
		listenerSupport.fireSentRequest(request, sessions);
	}

	public void fireSentResponse(Response<?> response, IoSession... sessions) {
		listenerSupport.fireSentResponse(response, sessions);
	}

	public void fireReceivedRequest(Request<?> request, IoSession... sessions) {
		listenerSupport.fireReceivedRequest(request, sessions);
	}

	public void fireReceivedResponse(Response<?> response, IoSession... sessions) {
		listenerSupport.fireReceivedResponse(response, sessions);
	}
	
	//数据测试
	@Override
	  public void sessionCreated(IoSession session) throws Exception {
	        System.out.println("hava a session!!");
	    }

	/* (non-Javadoc)
	 * @see org.apache.mina.core.service.IoHandlerAdapter#sessionClosed(org.apache.mina.core.session.IoSession)
	 */
	@Override
	public void sessionClosed(IoSession session) throws Exception {
		// TODO Auto-generated method stub
		System.out.println("sessionClosed!!");
	}

	/* (non-Javadoc)
	 * @see org.apache.mina.core.service.IoHandlerAdapter#exceptionCaught(org.apache.mina.core.session.IoSession, java.lang.Throwable)
	 */
	@Override
	public void exceptionCaught(IoSession session, Throwable cause)
			throws Exception {
		// TODO Auto-generated method stub
		System.out.println("exceptionCaught");
	}
	
	
}
