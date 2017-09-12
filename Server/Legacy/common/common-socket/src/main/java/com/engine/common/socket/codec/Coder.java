package com.engine.common.socket.codec;

import com.engine.common.socket.anno.InBody;
import com.engine.common.socket.anno.impl.InBodyParameter;
import com.engine.common.socket.core.Message;

/**
 * 内容/消息体{@link Message#getBody()}编码器接口
 * 
 */
public interface Coder {

	/**
	 * 编码方法，负责将消息体从对象转换为通信格式
	 * @param obj 对象实例
	 * @param type 对象类型，为空时要求编解码器自行提供默认类型
	 * @return
	 */
	byte[] encode(Object obj, Object type);
	
	/**
	 * 将消息体内容转换为对象实例
	 * @param bytes 信息体
	 * @param type 对象类型，为空时要求编解码器自行提供默认类型
	 * @return
	 */
	Object decode(byte[] bytes, Object type);

	/**
	 * 获取{@link InBody}标注的参数值
	 * @param body 当前的消息体
	 * @param parameter {@link InBodyParameter}实例
	 * @return
	 */
	Object getInBody(Object body, InBodyParameter parameter);
	
	/**
	 * 获取默认的解码类型
	 * @return
	 */
	Object getDefaultDecodeType();

	/**
	 * 获取默认的编码类型
	 * @return
	 */
	Object getDefaultEncodeType();
	
	/**
	 * 获取该编码器的克隆实例(如果编码器不需要克隆可直接返回自身)
	 * @return
	 */
	Coder getClone();
}
