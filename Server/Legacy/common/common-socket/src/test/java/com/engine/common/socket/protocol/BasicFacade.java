package com.engine.common.socket.protocol;

import java.util.concurrent.atomic.AtomicInteger;

import org.apache.mina.core.session.IoSession;
import org.springframework.stereotype.Component;

import com.engine.common.socket.core.Request;

@Component
public class BasicFacade implements BasicFacadeInf {

	private AtomicInteger count = new AtomicInteger();

	public void count() {
		count.incrementAndGet();
	}

	public int getCount() {
		return count.get();
	}

	private IoSession session;

	@Override
	public void session(IoSession session) {
		this.session = session;
	}

	public IoSession getSession() {
		return session;
	}

	private String request;

	@Override
	public void request(Request<String> request) {
		this.request = request.getBody();
	}

	public String getRequest() {
		return request;
	}

	private Person body;

	@Override
	public void body(Person person) {
		this.body = person;
	}

	public Person getBody() {
		return body;
	}

	@Override
	public String compress(String string) {
		return string;
	}
}
