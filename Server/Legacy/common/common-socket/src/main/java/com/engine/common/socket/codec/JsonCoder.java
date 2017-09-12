package com.engine.common.socket.codec;

import java.io.IOException;
import java.lang.reflect.Type;

import org.codehaus.jackson.JsonNode;
import org.codehaus.jackson.JsonParseException;
import org.codehaus.jackson.JsonParser;
import org.codehaus.jackson.map.JsonMappingException;
import org.codehaus.jackson.map.ObjectMapper;
import org.codehaus.jackson.map.type.TypeFactory;
import org.codehaus.jackson.type.JavaType;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.socket.anno.impl.InBodyParameter;
import com.engine.common.socket.exception.DecodeException;
import com.engine.common.socket.exception.ParameterException;

/**
 * <code>JSON</code>格式的编解码器
 * 
 */
public class JsonCoder extends CoderSupport {

	private static final Logger logger = LoggerFactory.getLogger(JsonCoder.class);

	private static final ObjectMapper mapper = new ObjectMapper();
	private static final TypeFactory typeFactory = TypeFactory.defaultInstance();

	@Override
	protected byte[] doEncode(Object obj, Object type) {
		try {
			return mapper.writeValueAsBytes(obj);
		} catch (Exception e) {
			String message = "JSON编码失败:" + e.getMessage();
			if (logger.isErrorEnabled())
				logger.info(message, e);
			throw new DecodeException(message, e);
		}
	}

	@Override
	protected Object doDecode(byte[] bytes, Object type) {
		if (type.equals(void.class)) {
			return null;
		}
		try {
			if (type instanceof JavaType) {
				return decodeByJavaType(bytes, (JavaType) type);
			} else if (type instanceof Type) {
				return decodeByType(bytes, (Type) type);
			}
		} catch (Exception e) {
			String message = "JSON解码失败:" + e.getMessage();
			if (logger.isErrorEnabled())
				logger.info(message);
			throw new DecodeException(message, e);
		}

		FormattingTuple message = MessageFormatter.format("不支持的类型参数[{}]", type);
		if (logger.isErrorEnabled())
			logger.info(message.getMessage());
		throw new IllegalArgumentException(message.getMessage());
	}

	/** 使用 {@link Type} 描述进行解码 */
	private Object decodeByType(byte[] bytes, Type type) throws JsonParseException, JsonMappingException, IOException {
		return mapper.readValue(bytes, 0, bytes.length, typeFactory.constructType(type));
	}

	/** 使用 {@link JavaType} 描述进行解码 */
	private Object decodeByJavaType(byte[] bytes, JavaType type) throws JsonParseException, JsonMappingException,
			IOException {
		return mapper.readValue(bytes, 0, bytes.length, type);
	}

	@Override
	public Object getInBody(Object body, InBodyParameter parameter) {
		JsonNode node = (JsonNode) body;
		JsonParser parser;
		if (node.isArray()) {
			int index = Integer.parseInt(parameter.getValue());
			if (node.has(index)) {
				parser = node.get(index).traverse();
			} else if (parameter.isRequired()) {
				FormattingTuple message = MessageFormatter.format("注释[{}]请求的参数不存在", parameter.getAnnotation());
				if (logger.isErrorEnabled())
					logger.info(message.getMessage());
				throw new ParameterException(message.getMessage());
			} else {
				return null;
			}
		} else {
			String name = parameter.getValue();
			if (node.has(name)) {
				parser = node.get(name).traverse();
			} else if (parameter.isRequired()) {
				FormattingTuple message = MessageFormatter.format("注释[{}]请求的参数不存在", parameter.getAnnotation());
				if (logger.isErrorEnabled())
					logger.info(message.getMessage());
				throw new ParameterException(message.getMessage());
			} else {
				return null;
			}
		}
		parser.setCodec(mapper);
		try {
			return parser.readValueAs(parameter.getClazz());
		} catch (Exception e) {
			String message = "处理InBody数据提取时出现异常";
			if (logger.isErrorEnabled())
				logger.info(message, e);
			throw new ParameterException(message, e);
		}
	}

	@Override
	public Object getDefaultDecodeType() {
		return JsonNode.class;
	}

	@Override
	public Object getDefaultEncodeType() {
		return null;
	}

	@Override
	public Coder getClone() {
		return this;
	}

}
