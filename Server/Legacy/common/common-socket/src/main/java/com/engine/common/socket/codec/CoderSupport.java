package com.engine.common.socket.codec;

import com.engine.common.socket.exception.DecodeException;
import com.engine.common.socket.exception.EncodeException;

/**
 * 抽象编解码器，负责封装处理异常
 * 
 */
public abstract class CoderSupport implements Coder {

	@Override
	public byte[] encode(Object obj, Object type) {
		try {
			return doEncode(obj, type);
		} catch (Exception e) {
			throw new EncodeException(e);
		}
	}

	@Override
	public Object decode(byte[] bytes, Object type) {
		try {
			return doDecode(bytes, type);
		} catch (Exception e) {
			throw new DecodeException(e);
		}
	}

	/**
	 * 等价于{@link Coder#encode(Object, Object)},子类实现该方法完成编码工作
	 * @param obj
	 * @param type
	 * @return
	 */
	protected abstract byte[] doEncode(Object obj, Object type);
	
	/**
	 * 等价于{@link Coder#decode(byte[], Object)},子类实现该方法完成解码工作
	 * @param bytes
	 * @param type
	 * @return
	 */
	protected abstract Object doDecode(byte[] bytes, Object type);
	
}
