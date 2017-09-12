package com.engine.common.socket.anno.session;

import com.engine.common.socket.anno.InSession;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;

/**
 * {@link InSession}测试接口
 * 
 */
@SocketModule(4)
public interface InSessionFacadeInf {

	@SocketCommand(1)
	String found(@InSession("FOUND") String value);

	@SocketCommand(2)
	String notFound(@InSession(value="not-found", required=false) String value);

	@SocketCommand(3)
	String error(@InSession(value="not-found") String value);

}
