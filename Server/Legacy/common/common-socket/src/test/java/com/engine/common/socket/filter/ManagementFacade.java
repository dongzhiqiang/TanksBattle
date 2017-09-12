package com.engine.common.socket.filter;

import org.springframework.stereotype.Component;

@Component
public class ManagementFacade implements ManagementFacadeInf {

	@Override
	public boolean isManagement(String name) {
		if (name == null) {
			return false;
		}
		return true;
	}

	@Override
	public String getName(String name) {
		return name;
	}

}
