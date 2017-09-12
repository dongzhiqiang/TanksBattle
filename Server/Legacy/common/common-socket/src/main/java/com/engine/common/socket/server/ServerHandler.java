package com.engine.common.socket.server;

import java.io.IOException;

import org.apache.mina.core.service.IoHandler;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.core.write.WriteException;
import org.apache.mina.filter.logging.MdcInjectionFilter;
import org.apache.mina.filter.logging.MdcInjectionFilter.MdcKey;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;
import com.engine.common.socket.exception.TypeDefinitionNotFound;
import com.engine.common.socket.handler.CommandRegister;
import com.engine.common.socket.handler.HandlerSupport;
import com.engine.common.socket.handler.SnGenerator;

/**
 * 服务端{@link IoHandler}
 * 
 */
public class ServerHandler extends HandlerSupport {

	private static final Logger logger = LoggerFactory.getLogger(ServerHandler.class);

	/** 序列号生成器 */
	private SnGenerator generator = new SnGenerator();

	public ServerHandler(CommandRegister register) {
		super(register);
	}

	@Override
	public void send(Request<?> request, IoSession... sessions) {
		request.setSn(generator.next());
		super.send(request, sessions);
	}

	@Override
	public void receive(Response<?> response, IoSession session) {
		if (logger.isDebugEnabled()) {
			logger.debug("回应信息[{}]被忽略", response);
		}
	}

	// 只是为了调试实现的方法

	@Override
	public void sessionCreated(IoSession session) throws Exception {
		if (logger.isDebugEnabled()) {
			if (session.getFilterChain().contains(MdcInjectionFilter.class)) {
				logger.debug("服务端会话[{}]被创建，客户是:{}", session.getId(),
						MdcInjectionFilter.getProperty(session, MdcKey.remoteAddress.name()));
			} else {
				logger.debug("服务端会话[{}]被创建", session.getId());
			}
		}
		super.sessionCreated(session);
	}

	@Override
	public void sessionClosed(IoSession session) throws Exception {
		if (logger.isDebugEnabled()) {
			if (session.getFilterChain().contains(MdcInjectionFilter.class)) {
				logger.debug("服务端会话[{}]被关闭，的客户是:{}", session.getId(),
						MdcInjectionFilter.getProperty(session, MdcKey.remoteAddress.name()));
			} else {
				logger.debug("服务端会话[{}]被关闭", session.getId());
			}
		}
		super.sessionClosed(session);
	}

	@Override
	public void exceptionCaught(IoSession session, Throwable throwable) throws Exception {

		// write to closed socket channel
		if (throwable instanceof WriteException || throwable instanceof TypeDefinitionNotFound) {
			logger.error("{}\tError: {}", throwable.getClass().getName(), throwable.getMessage());
			return;
		}

		// 生成错误堆栈信息
		StringBuilder sb = new StringBuilder();
		Throwable ex = throwable;
		while (ex != null) {
			StackTraceElement[] stackTrace = ex.getStackTrace();
			for (StackTraceElement st : stackTrace) {
				if (throwable instanceof IOException && st.getClassName().equals("sun.nio.ch.SocketChannelImpl")) {
					// connection reset by peer ...
					// logger.error("{}\tError: {}", throwable.getClass().getName(), throwable.getMessage());
					return;
				}
				sb.append("\t").append(st.toString()).append("\n");
			}

			if (ex != ex.getCause()) {
				ex = ex.getCause();
				if (ex != null) {
					sb.append("CAUSE\n").append(ex.getMessage()).append(ex).append("\n");;
				}

			} else {
				break;
			}
		}

		logger.error("{}\tError: {}\n{}",
				new Object[] { throwable.getClass().getName(), throwable.getMessage(), sb.toString() });
	}

}
