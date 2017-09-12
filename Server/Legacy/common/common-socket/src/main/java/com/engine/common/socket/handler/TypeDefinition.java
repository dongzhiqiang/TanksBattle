package com.engine.common.socket.handler;

import java.lang.reflect.ParameterizedType;
import java.lang.reflect.Type;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.socket.anno.Sync;
import com.engine.common.socket.core.Processor;

/**
 * 消息体类型定义
 * <ul>
 * <li>用于定义 {@link Request#getBody()} / {@link Response#getBody()} 的类型， 即 {@link Message#getBody()} 的转码后类型。</li>
 * <li>request 和 response 定义允许为<code>null</code>，为<code>null</code>系统应该使用当前{@link Coder#getDefaultDecodeType()}或
 * {@link Coder#getDefaultEncodeType()}作为默认类型。</li>
 * </ul>
 * 
 */
public class TypeDefinition {
	
	private static final Logger logger = LoggerFactory.getLogger(TypeDefinition.class);

	/**
	 * 创建类实例
	 * @param format 编码格式
	 * @param request 请求体类型，没有可用<code>void.class</code>代替
	 * @param response 回应体类型，没有可用<code>void.class</code>代替
	 * @return
	 */
	public static TypeDefinition valueOf(byte format, Type request, Type response) {
		TypeDefinition result = new TypeDefinition();
		result.format = format;
		result.request = request;
		result.response = response;
		return result;
	}

	/**
	 * 创建类实例
	 * @param format 编码格式
	 * @param request 请求体类型，没有可用<code>void.class</code>代替
	 * @param response 回应体类型，没有可用<code>void.class</code>代替
	 * @param requestCompress 请求信息体是否压缩
	 * @param responseCompress 回应信息体是否压缩
	 * @return
	 */
	public static TypeDefinition valueOf(byte format, Type request, Type response, boolean requestCompress, boolean responseCompress) {
		TypeDefinition result = new TypeDefinition();
		result.format = format;
		result.request = request;
		result.response = response;
		result.requestCompress = requestCompress;
		result.responseCompress = responseCompress;
		return result;
	}
	
	/**
	 * 创建类实例
	 * @param format 编码格式
	 * @param request 请求体类型，没有可用<code>void.class</code>代替
	 * @param response 回应体类型，没有可用<code>void.class</code>代替
	 * @param requestCompress 请求信息体是否压缩
	 * @param responseCompress 回应信息体是否压缩
	 * @param requestRaw 请求信息体是否原生类型
	 * @param responseRaw 回应信息体是否原生类型
	 * @return
	 */
	public static TypeDefinition valueOf(byte format, Type request, Type response, boolean requestCompress, boolean responseCompress, boolean requestRaw, boolean responseRaw) {
		TypeDefinition result = new TypeDefinition();
		result.format = format;
		result.request = request;
		result.response = response;
		result.requestCompress = requestCompress;
		result.responseCompress = responseCompress;
		result.requestRaw = requestRaw;
		result.responseRaw = responseRaw;
		// 正确性判断
		if (result.requestRaw && result.request != byte[].class) {
			throw new IllegalArgumentException("在请求信息体为原生类型的条件下，请求信息体类型应为 byte[]");
		}
		if (result.responseRaw && result.response != byte[].class) {
			throw new IllegalArgumentException("在回应信息体为原生类型的条件下，回应信息体类型应为 byte[]");
		}
		return result;
	}

	/**
	 * 根据{@link Processor}实例的类型声明来创建类型定义实例
	 * @param format 编码格式
	 * @param handler 控制器实例
	 * @return
	 */
	public static TypeDefinition valueOf(byte format, Processor<?, ?> handler) {
		@SuppressWarnings({ "rawtypes", "unchecked" })
		Class<Processor> clz = (Class<Processor>) handler.getClass();
		Type[] interfaces = clz.getGenericInterfaces();
		for (Type type : interfaces) {
			if (type instanceof ParameterizedType && ((ParameterizedType) type).getRawType().equals(Processor.class)) {
				Type[] types = ((ParameterizedType) type).getActualTypeArguments();
				return TypeDefinition.valueOf(format, types[0], types[1]);
			}
		}
		
		FormattingTuple message = MessageFormatter.format("无法识别的控制器声明类型[{}]", clz);
		logger.error(message.getMessage());
		throw new RuntimeException(message.getMessage());
	}

	/**
	 * 根据{@link Processor}实例的类型声明来创建类型定义实例
	 * @param format 编码格式
	 * @param processor 控制器实例
	 * @param requestCompress 请求信息体是否压缩
	 * @param responseCompress 回应信息体是否压缩
	 * @return
	 */
	public static TypeDefinition valueOf(byte format, Processor<?, ?> processor, boolean requestCompress, boolean responseCompress) {
		@SuppressWarnings({ "rawtypes", "unchecked" })
		Class<Processor> clz = (Class<Processor>) processor.getClass();
		Type[] interfaces = clz.getGenericInterfaces();
		for (Type type : interfaces) {
			if (type instanceof ParameterizedType && ((ParameterizedType) type).getRawType().equals(Processor.class)) {
				Type[] types = ((ParameterizedType) type).getActualTypeArguments();
				return TypeDefinition.valueOf(format, types[0], types[1], requestCompress, responseCompress);
			}
		}
		
		FormattingTuple message = MessageFormatter.format("无法识别的控制器声明类型[{}]", clz);
		logger.error(message.getMessage());
		throw new RuntimeException(message.getMessage());
	}

	/** 编码定义 */
	protected byte format;
	protected Type request;
	protected Type response;
	// 压缩标识
	protected boolean requestCompress;
	protected boolean responseCompress;
	// 原生类型标识
	protected boolean requestRaw;
	protected boolean responseRaw;
	// 方法加到同步队列执行
	protected Sync sync;
	
	// Getter and Setter ...

	
	/**
	 * 获取请求体类型
	 * @return
	 */
	public Type getRequest() {
		return request;
	}
	
	/**
	 * 获取编码格式
	 * @return
	 */
	public byte getFormat() {
		return format;
	}

	/**
	 * 获取回应体类型
	 * @return
	 */
	public Type getResponse() {
		return response;
	}

	/**
	 * 检查请求信息体是否采用压缩
	 * @return
	 */
	public boolean isRequestCompress() {
		return requestCompress;
	}

	/**
	 * 检查回应信息体是否采用压缩
	 * @return
	 */
	public boolean isResponseCompress() {
		return responseCompress;
	}
	
	/**
	 * 请求体是否原生类型
	 * @return
	 */
	public boolean isRequestRaw() {
		return requestRaw;
	}

	/**
	 * 回应体是否原生类型
	 * @return
	 */
	public boolean isResponseRaw() {
		return responseRaw;
	}

	/**
	 * 是否同步执行
	 * @return
	 */
	public boolean isSync() {
		if (sync != null) {
			return true;
		}
		return false;
	}

	/**
	 * 获取同步键值
	 * @return
	 */
	public String getSyncKey() {
		if (sync != null) {
			return sync.value();
		}
		return null;
	}
	
	@Override
	public String toString() {
		String s = sync == null ? "同步:false" : "同步:true(" + sync.value() + ")";
		return "请求[类型:" + request + " 压缩:" + requestCompress + " 原生:" + requestRaw + " " + s + "] " +
				"回应[类型:" + response + " 压缩:" + responseCompress + " 原生:" + responseRaw + "]";
	}
}
