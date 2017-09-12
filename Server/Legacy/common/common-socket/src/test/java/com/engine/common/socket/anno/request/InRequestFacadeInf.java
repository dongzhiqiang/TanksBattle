package com.engine.common.socket.anno.request;

import static com.engine.common.socket.anno.InRequest.Type.*;

import com.engine.common.socket.anno.InRequest;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.core.Command;

/**
 * {@link InRequest}测试接口
 * 
 * 
 */
@SocketModule(3)
public interface InRequestFacadeInf {

	@SocketCommand(1)
	long sn(@InRequest(SN) long sn);

	@SocketCommand(2)
	boolean command(@InRequest(COMMAND) Command command);

	@SocketCommand(3)
	int state(@InRequest(STATE) int state);

	@SocketCommand(4)
	String attachment(@InRequest(ATTACHMENT) byte[] attachment);
}
