package com.engine.common.socket.client;

import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;

@SocketModule(1)
public interface TestFacadeInf {

	@SocketCommand(1)
	void timeout() throws InterruptedException;

	@SocketCommand(2)
	void test();
	
	@SocketCommand(3)
	void start();
}
