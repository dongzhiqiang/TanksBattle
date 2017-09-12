package com.engine.common.socket.handler;

import java.util.concurrent.TimeUnit;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.socket.core.Convertor;
import com.engine.common.socket.core.Message;
import com.engine.common.socket.core.MessageConstant;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.ResponseConstants;
import com.engine.common.socket.exception.DecodeException;
import com.engine.common.socket.exception.EncodeException;
import com.engine.common.socket.exception.ProcessingException;
import com.engine.common.socket.exception.SocketException;
import com.engine.common.utils.codec.ZlibUtils;

/**
 * 通信消息处理帮助类
 * 
 */
public final class MessageHelper {
	
	private static final Logger logger = LoggerFactory.getLogger(MessageHelper.class);

	/** 获取解码后的请求对象实例 */
	public static Request<?> decodeRequest(Message message, byte format, TypeDefinition definition, Convertor convertor, long timeout, TimeUnit unit) {
		byte[] bytes = message.getBody();
		if (message.hasState(MessageConstant.STATE_COMPRESS)) {
			if (logger.isDebugEnabled()) {
				logger.debug("对指令[{}]的请求信息体进行解压", message.getCommand());
			}
			try {
				bytes = ZlibUtils.unzip(message.getBody(), timeout, unit);
			} catch (Exception e) {
				throw new DecodeException(e.getMessage(), e);
			}
		}
		Object in = convertor.decode(format, bytes, definition.getRequest());
		return Request.valueOf(message, in);
	}

	/**
	 * 处理异常回应
	 * @param message
	 * @param ex
	 */
	public static Message parameterException(Message message, ProcessingException ex) {
		// 日志
		if (logger.isWarnEnabled()) {
			logger.warn("处理通信信息[{}]时发生异常", message, ex);
		}
		// 处理
		return message.clearBody().changeToErrorResponse(ResponseConstants.PARAMETER_EXCEPTION);
	}
	
	/**
	 * 处理异常回应
	 * @param message
	 * @param ex
	 */
	public static Message processingException(Message message, ProcessingException ex) {
		// 日志
		if (logger.isWarnEnabled()) {
			logger.warn("处理通信信息[{}]时发生异常", message, ex);
		}
		// 处理
		return message.clearBody().changeToErrorResponse(ResponseConstants.PROCESSING_EXCEPTION);
	}

	/**
	 * 编码异常回应
	 * @param message
	 * @param ex
	 */
	public static Message encodeException(Message message, EncodeException ex) {
		// 日志
		if (logger.isWarnEnabled()) {
			logger.warn("处理通信信息[{}]时发生编码异常", message, ex);
		}
		// 处理
		return message.clearBody().changeToErrorResponse(ResponseConstants.ENCODE_EXCEPTION);
	}

	/**
	 * 解码异常回应
	 * @param message
	 * @param session
	 * @param ex
	 */
	public static Message decodeException(Message message, DecodeException ex) {
		// 日志
		if (logger.isWarnEnabled()) {
			logger.warn("处理通信信息[{}]时发生解码异常", message, ex);
		}
		// 处理
		return message.clearBody().changeToErrorResponse(ResponseConstants.DECODE_EXCEPTION);
	}

	/**
	 * 指令不存在的回应
	 * @param message
	 */
	public static Message commandNotFound(Message message) {
		// 日志
		if (logger.isWarnEnabled()) {
			logger.warn("通信信息[{}]请求的指令不存在", message);
		}
		// 处理
		return message.clearBody().changeToErrorResponse(ResponseConstants.COMMAND_NOT_FOUND);
	}

	/**
	 * 指令不存在的回应
	 * @param message
	 * @param e 异常原因
	 */
	public static Message commandNotFound(Message message, SocketException e) {
		// 日志
		if (logger.isWarnEnabled()) {
			logger.warn("通信信息[{}]请求的指令不存在", message);
		}
		// 处理
		return message.clearBody().changeToErrorResponse(ResponseConstants.COMMAND_NOT_FOUND);
	}

	/**
	 * 未知异常的回应
	 * @param message
	 * @param e 异常原因
	 * @return
	 */
	public static Message unknownException(Message message, Exception e) {
		// 日志
		if (logger.isWarnEnabled()) {
			logger.warn("通信信息[{}]请求的指令不存在", message);
		}
		// 处理
		return message.clearBody().changeToErrorResponse(ResponseConstants.UNKNOWN_EXCEPTION);
	}

}
