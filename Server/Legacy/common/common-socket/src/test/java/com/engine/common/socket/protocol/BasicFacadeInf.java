package com.engine.common.socket.protocol;

import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.anno.Compress;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.core.Request;

/**
 * 被研究对象的儿子，被测试对象
 * 
 * 
 */
@SocketModule(1)
public interface BasicFacadeInf {

	/** 测试无消息体的方法 */
	@SocketCommand(1)
	void count();

	@SocketCommand(2)
	void session(IoSession session);
	
	@SocketCommand(3)
	void request(Request<String> request);
	
	@SocketCommand(4)
	void body(Person person);
	
	@SocketCommand(value = 5, compress = @Compress(request = true, response = true))
	String compress(String string);
}
