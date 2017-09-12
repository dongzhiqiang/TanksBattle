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
import com.engine.common.socket.core.MessageConstant;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;
import com.engine.common.socket.core.ResponseConstants;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
@SuppressWarnings("unchecked")
public class IdentityTest {
	
	@Autowired
	private ClientFactory clientFactory;
	private Client client;
	
	@Before
	public void before() {
		client = clientFactory.getClient(true);
	}
	
	@Test
	public void test_before_login() {
		Request<?> request = Request.valueOf(IdentityFacadeInf.cmdGetUserId, null);
		Response<String> response = (Response<String>) client.send(request);
		
		assertThat(response.hasError(), is(true));
		assertThat(response.hasState(MessageConstant.STATE_ERROR), is(true));
		assertThat(response.hasState(ResponseConstants.PROCESSING_EXCEPTION), is(true));
		assertThat(response.getBody(), nullValue());
	}
	
	@Test
	public void test_login() {
		Map<String, String> body = new HashMap<String, String>(2);
		body.put("username", "frank");
		body.put("password", "654321");
		Request<Map<String, String>> request = Request.valueOf(IdentityFacadeInf.cmdLogin, body);
		Response<Boolean> response = (Response<Boolean>) client.send(request);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getBody(), is(false));
		
		body.put("password", "123456");
		request = Request.valueOf(IdentityFacadeInf.cmdLogin, body);
		response = (Response<Boolean>) client.send(request);
		assertThat(response.getBody(), is(true));
	}
	
	@Test
	public void test_after_login() {
		Request<?> request = Request.valueOf(IdentityFacadeInf.cmdGetUserId, null);
		Response<String> response = (Response<String>) client.send(request);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getBody(), is("1"));
	}

}
