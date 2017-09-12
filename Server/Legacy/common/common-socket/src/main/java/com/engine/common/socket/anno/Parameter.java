package com.engine.common.socket.anno;

import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.core.Request;

/**
 * 参数对象,负责获取指定注释({@link InBody} / {@link InRequest} / {@link InSession})声明的参数
 * 
 */
public interface Parameter {

	/**
	 * 获取对应的参数类型
	 * @return
	 */
	ParameterKind getKind();
	
	/**
	 * 获取参数值
	 * @param request
	 * @param session
	 * @return
	 */
	Object getValue(Request<?> request, IoSession session);

}
