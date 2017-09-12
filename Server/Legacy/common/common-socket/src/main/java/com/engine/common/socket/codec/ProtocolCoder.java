package com.engine.common.socket.codec;

import java.io.IOException;
import java.lang.reflect.Type;
import java.util.Map;

import org.apache.commons.beanutils.PropertyUtils;
import org.apache.mina.core.buffer.IoBuffer;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.beans.factory.annotation.Autowired;

import com.engine.common.protocol.Transfer;
import com.engine.common.socket.anno.impl.InBodyParameter;
import com.engine.common.socket.exception.DecodeException;
import com.engine.common.socket.exception.ParameterException;

/**
 * <code>Prorocol</code>格式的编解码器
 * 
 */
public class ProtocolCoder extends CoderSupport {

	private final Logger logger = LoggerFactory.getLogger(getClass());

	@Autowired
	private Transfer transfer;

	@Override
	protected byte[] doEncode(Object obj, Object type) {
		try {
			IoBuffer buf = transfer.encode(obj);
			byte[] bytes = new byte[buf.remaining()];
			buf.get(bytes);
			return bytes;
		} catch (Exception e) {
			String message = "Protocol编码失败:" + e.getMessage();
			logger.error(message, e);
			throw new DecodeException(message, e);
		}
	}

	@Override
	protected Object doDecode(byte[] bytes, Object type) {
		if (type.equals(void.class)) {
			return null;
		}
		if (!(type instanceof Type)) {
			FormattingTuple message = MessageFormatter.format("不支持的类型参数[{}]", type);
			if (logger.isInfoEnabled())
				logger.info(message.getMessage());
			throw new IllegalArgumentException(message.getMessage());
		}

		try {
			IoBuffer buf = IoBuffer.wrap(bytes);
			return transfer.decode(buf, (Type) type);
		} catch (Exception e) {
			String message = "Protocol解码失败:" + e.getMessage();
			if (logger.isInfoEnabled())
				logger.info(message);
			throw new DecodeException(message, e);
		}
	}

	@Override
	public Object getInBody(Object body, InBodyParameter parameter) {
		Class<? extends Object> bodyClz = body.getClass();
		Class<?> paramClz = parameter.getClazz();
		if (bodyClz.isArray()) {
			int index = Integer.parseInt(parameter.getValue());
			Object[] values = (Object[]) body;
			if (index >= 0 && index < values.length) {
				Object value = values[index];
				try {
					return transfer.convert(value, paramClz);
				} catch (Exception e) {
					FormattingTuple message = MessageFormatter.format("注释[{}]参数类型[{}]与值[{}]不匹配", new Object[] {
						parameter.getAnnotation(), paramClz, value });
					if (logger.isInfoEnabled())
						logger.info(message.getMessage());
					throw new ParameterException(message.getMessage(), e);
				}
			} else if (parameter.isRequired()) {
				FormattingTuple message = MessageFormatter.format("注释[{}]请求的参数不存在", parameter.getAnnotation());
				if (logger.isInfoEnabled())
					logger.info(message.getMessage());
				throw new ParameterException(message.getMessage());
			} else {
				return null;
			}
		} else if (body instanceof Map) {
			String name = parameter.getValue();
			Map<?, ?> map = (Map<?, ?>) body;
			if (map.containsKey(name)) {
				Object value = map.get(name);
				try {
					return transfer.convert(value, paramClz);
				} catch (Exception e) {
					FormattingTuple message = MessageFormatter.format("注释[{}]参数类型[{}]与值[{}]不匹配", new Object[] {
						parameter.getAnnotation(), paramClz, value });
					if (logger.isInfoEnabled())
						logger.info(message.getMessage());
					throw new ParameterException(message.getMessage(), e);
				}
			} else if (parameter.isRequired()) {
				FormattingTuple message = MessageFormatter.format("注释[{}]请求的参数不存在", parameter.getAnnotation());
				if (logger.isInfoEnabled())
					logger.info(message.getMessage());
				throw new ParameterException(message.getMessage());
			} else {
				return null;
			}
		} else if (bodyClz.equals(paramClz)) {
			try {
				return transfer.convert(body, paramClz);
			} catch (Exception e) {
				FormattingTuple message = MessageFormatter.format("注释[{}]参数类型[{}]与值[{}]不匹配", new Object[] {
						parameter.getAnnotation(), paramClz, body });
					if (logger.isInfoEnabled())
						logger.info(message.getMessage());
					throw new ParameterException(message.getMessage(), e);
				}
		}
		else {
			String name = parameter.getValue();
			try {
				return PropertyUtils.getProperty(body, name);
			} catch (Exception e) {
				if (parameter.isRequired()) {
					FormattingTuple message = MessageFormatter.format("注释[{}]请求的参数不存在", parameter.getAnnotation());
					if (logger.isInfoEnabled())
						logger.info(message.getMessage());
					throw new ParameterException(message.getMessage());
				} else {
					return null;
				}
			}
		}
	}

	@Override
	public Object getDefaultDecodeType() {
		return Object.class;
	}

	@Override
	public Object getDefaultEncodeType() {
		return Object.class;
	}

	@Override
	public Coder getClone() {
		ProtocolCoder result = new ProtocolCoder();
		result.transfer = new Transfer();
		return result;
	}

	// 静态构造方法

	public static ProtocolCoder valueOf(Transfer transfer) {
		ProtocolCoder coder = new ProtocolCoder();
		coder.transfer = transfer;
		return coder;
	}
}
