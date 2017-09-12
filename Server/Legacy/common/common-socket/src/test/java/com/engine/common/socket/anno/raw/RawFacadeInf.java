package com.engine.common.socket.anno.raw;

import static com.engine.common.socket.anno.InRequest.Type.*;

import com.engine.common.socket.anno.InRequest;
import com.engine.common.socket.anno.Raw;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;

@SocketModule(1)
public interface RawFacadeInf {

	@SocketCommand(value = 1, raw = @Raw(request = true))
	void in(byte[] raw);
	
	@SocketCommand(value = 2, raw = @Raw(response = true))
	byte[] out(byte[] args);
	
	@SocketCommand(value = 3, raw = @Raw(request = true, response = true))
	byte[] inAndOut(byte[] args);
	
	@SocketCommand(value = 4, raw = @Raw(response = true))
	byte[] attachmentToRaw(@InRequest(ATTACHMENT) byte[] attachment);
}
