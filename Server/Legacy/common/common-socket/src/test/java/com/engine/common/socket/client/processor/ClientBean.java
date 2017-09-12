package com.engine.common.socket.client.processor;

import org.springframework.stereotype.Component;

import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;

@Component
@SocketModule(2)
public class ClientBean {

	@SocketCommand(1)
	public void call(String string) {
		System.out.println(string);
	}
}
