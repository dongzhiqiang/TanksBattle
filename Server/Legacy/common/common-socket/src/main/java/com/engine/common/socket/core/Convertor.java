package com.engine.common.socket.core;

import java.lang.reflect.Type;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.socket.codec.Coder;
import com.engine.common.socket.exception.CoderNotFound;

/**
 * 消息体转换器，负责将{@link Message#getBody()}转换为具体的类实例
 * 
 */
public class Convertor implements Cloneable {
	
	private static final Logger logger = LoggerFactory.getLogger(Convertor.class);
	
	private Map<Byte, Coder> coders = new HashMap<Byte, Coder>();
	
	/**
	 * 编码方法，负责将对象转换为通信格式
	 * @param format 格式
	 * @param obj 对象实例
	 * @param type 类型信息
	 * @return
	 */
	public byte[] encode(byte format, Object obj, Type type) {
		Coder coder = getCoder(format);
		if (obj == null) {
			return new byte[0];
		}
		return type != null ? coder.encode(obj, type) : coder.encode(obj, coder.getDefaultEncodeType());
	}
	
	/**
	 * 将通信内容 byte[] 转换为对象实例
	 * @param format 格式
	 * @param bytes 信息体
	 * @param type 类型信息
	 * @return
	 */
	public Object decode(byte format, byte[] bytes, Type type) {
		Coder coder = getCoder(format);
		if (bytes.length == 0) {
			return null;
		}
		return type != null ? coder.decode(bytes, type) : coder.decode(bytes, coder.getDefaultDecodeType());
	}
	
	/**
	 * 获取指定格式的{@link Coder}
	 * @param format 格式代码
	 * @return
	 */
	public Coder getCoder(byte format) {
		Coder result = coders.get(format);
		if (result == null) {
			FormattingTuple message = MessageFormatter.format("格式[{}]对应的编码器不存在", format);
			logger.error(message.getMessage());
			throw new CoderNotFound(message.getMessage());
		}
		return result;
	}
	
	@Override
	public Convertor clone() {
		Convertor result = new Convertor();
		for (Entry<Byte, Coder> entry : coders.entrySet()) {
			result.coders.put(entry.getKey(), entry.getValue().getClone());
		}
		return result;
	}
	
	// Getter and Setter ...

	public void setCoders(Map<Byte, Coder> coders) {
		this.coders = coders;
	}

}
