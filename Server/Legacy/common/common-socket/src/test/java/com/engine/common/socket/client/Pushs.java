package com.engine.common.socket.client;

import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.anno.SocketPush;

@SocketPush
@SocketModule(2)
public interface Pushs {

	@SocketCommand(2)
	void test();
}
