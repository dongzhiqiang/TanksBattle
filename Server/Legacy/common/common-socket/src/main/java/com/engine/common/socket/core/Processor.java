package com.engine.common.socket.core;

import org.apache.mina.core.session.IoSession;

/**
 * 通信业务处理接口，是通信内容对应业务处理的抽象定义
 * @param I {@link Request#getBody()}类型
 * @param O {@link Response#getBody()}类型
 * 
 */
public interface Processor<I, O> {

	/**
	 * 处理通信请求
	 * @param request 请求
	 * @param session 通信会话
	 * @return 回应的信息体对象实例
	 */
	O process(Request<I> request, IoSession session);

}
