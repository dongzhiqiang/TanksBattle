package com.engine.common.socket.filter.session;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.util.HashMap;
import java.util.Map;

import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.socket.client.Client;
import com.engine.common.socket.client.ClientFactory;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;

/**
 * 会话事件的单元测试
 * 
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class SessionEventTest {
	
	@Autowired
	private SessionEventTarget target;
	
	@Autowired
	private ClientFactory clientFactory;
	private Client client;
	
	@Before
	public void before() {
		client = clientFactory.getClient(true);
	}
	
	@Test
	public void test_identified() throws InterruptedException {
		Map<String, String> body = new HashMap<String, String>(2);
		body.put("username", "frank");
		body.put("password", "123456");
		Request<Map<String, String>> request = Request.valueOf(IdentityFacadeInf.cmdLogin, body);
		Response<Boolean> response = client.send(request, Boolean.TYPE);
		assertThat(response.hasError(), is(false));
		
		assertThat(target.getIdentified().getId(), is(1));
	}
	
}
