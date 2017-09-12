package com.engine.common.socket.anno.session;

import com.engine.common.socket.anno.InBody;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.core.Session;
import com.engine.common.socket.filter.session.SessionManager;

@SocketModule(5)
public interface SessionFacadeInf {

	String USER_ID = SessionManager.IDENTITY;

	@SocketCommand(1)
	boolean login(@InBody("username")String username, @InBody("password")String password, Session session);

	@SocketCommand(2)
	void checkAfterLogin(Session session);
}
