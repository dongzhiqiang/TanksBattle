package com.engine.common.socket.filter.session;

import org.apache.mina.core.session.IoSession;
import org.springframework.stereotype.Component;

import com.engine.common.socket.core.Attribute;
import com.engine.common.socket.filter.session.SessionManager;

@Component
public class IdentityFacade implements IdentityFacadeInf {
	
	private Attribute<Object> identity = new Attribute<Object>(SessionManager.IDENTITY);
	
	private User[] users = {User.valueOf(1, "frank", "123456"), User.valueOf(2, "ramon", "654321")};

	@Override
	public boolean login(String username, String password, IoSession session) {
		for (User user : users) {
			if (user.getName().equals(username) && user.getPassword().equals(password)) {
				identity.setValue(session, user.getId());
				return true;
			}
		}
		return false;
	}

	@Override
	public String getUserId(Integer id) {
		if (id == null)
			return null;
		return id.toString();
	}

}
