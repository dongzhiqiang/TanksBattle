package com.engine.common.socket.anno.session;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.util.HashMap;
import java.util.Map;

import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.socket.client.Client;
import com.engine.common.socket.client.ClientFactory;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;
import com.engine.common.socket.core.Session;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class SessionTest {

	@Component
	public static class Target implements SessionFacadeInf {
		@Override
		public boolean login(String username, String password, Session session) {
			if ("frank".equals(username) && "123456".equals(password)) {
				session.put(USER_ID, 1);
				return true;
			}
			return false;
		}
		@Override
		public void checkAfterLogin(Session session) {
			Integer id = (Integer) session.get(USER_ID);
			assertThat(id, is(1));
		}
	}

	@Autowired
	private ClientFactory clientFactory;
	private Client client;
	
	@Before
	public void before() {
		client = clientFactory.getClient(true);
	}
	
	@Test
	public void test_login() throws InterruptedException {
		Command command = Command.valueOf(1, 5);
		Map<String, String> body = new HashMap<String, String>(2);
		body.put("username", "frank");
		body.put("password", "654321");
		Request<Map<String, String>> request = Request.valueOf(command, body);
		Response<Boolean> response = client.send(request, Boolean.TYPE);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getBody(), is(false));
		
		body.put("password", "123456");
		request = Request.valueOf(command, body);
		response = client.send(request, Boolean.TYPE);
		assertThat(response.getBody(), is(true));
	}
	
	@Test
	public void test_after_login() {
		Command command = Command.valueOf(2, 5);
		Request<?> request = Request.valueOf(command, null);
		Response<?> response = client.send(request);
		
		assertThat(response.hasError(), is(false));
	}

}
