package com.engine.common.socket.client;

import org.springframework.stereotype.Component;

import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;

@Component
@SocketModule(2)
public class OtherFacade {

	@SocketCommand(1)
	public String test(String in) {
		return in;
	}
}
