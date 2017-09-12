package com.engine.common.socket.anno.response;

import static com.engine.common.socket.anno.InRequest.Type.*;

import com.engine.common.socket.anno.InRequest;
import com.engine.common.socket.anno.Raw;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.core.Response;

/**
 * {@link Response} 返回值测试
 * 
 * 
 */
@SocketModule(1)
public interface ResponseFacadeInf {

	@SocketCommand(1)
	Response<String> attachment(byte[] args);

	@SocketCommand(2)
	Response<Integer> inToOut(int value, @InRequest(ATTACHMENT) byte[] attachment);

	@SocketCommand(value = 3, raw = @Raw(request = true))
	Response<Boolean> rawToOut(byte[] raw);
}
