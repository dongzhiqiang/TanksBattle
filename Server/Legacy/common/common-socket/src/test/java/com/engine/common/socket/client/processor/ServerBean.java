package com.engine.common.socket.client.processor;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.filter.session.SessionManager;

@Component
@SocketModule(1)
public class ServerBean {

	@Autowired
	private SessionManager sessionManager;
	
	@SocketCommand(1)
	public void send(String string) {
		Request<String> request = Request.valueOf(Command.valueOf(1, 2), "server:" + string);
		sessionManager.sendAll(request);
	}
}
