package com.engine.common.socket.filter.session;

import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.anno.InBody;
import com.engine.common.socket.anno.InSession;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.filter.session.SessionManager;

@SocketModule(1)
public interface IdentityFacadeInf {
	
	String USER_ID = SessionManager.IDENTITY;
	
	Command cmdLogin = Command.valueOf(1, 1);

	@SocketCommand(1)
	boolean login(@InBody("username")String username, @InBody("password")String password, IoSession session);

	Command cmdGetUserId = Command.valueOf(2, 1);

	@SocketCommand(2)
	String getUserId(@InSession(USER_ID) Integer id);
	
}
