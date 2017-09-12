package com.engine.common.socket.anno.session;

import org.springframework.stereotype.Component;

@Component
public class InSessionFacade implements InSessionFacadeInf {

	@Override
	public String found(String value) {
		return value;
	}

	@Override
	public String notFound(String value) {
		return value;
	}

	@Override
	public String error(String value) {
		return value;
	}
}
