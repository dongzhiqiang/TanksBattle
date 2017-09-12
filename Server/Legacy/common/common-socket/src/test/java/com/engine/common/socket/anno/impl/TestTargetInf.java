package com.engine.common.socket.anno.impl;

import static com.engine.common.socket.filter.session.SessionManager.IDENTITY;

import com.engine.common.socket.anno.InBody;
import com.engine.common.socket.anno.InSession;

public interface TestTargetInf {

	void testInBodyDefault(@InBody("name") String name, @InBody("password") String password);
	
	void testInBodyIndex(@InSession(IDENTITY) String id, @InBody String name, @InBody String password);
}
