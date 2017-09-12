package com.engine.common.socket.anno.session;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.socket.client.Client;
import com.engine.common.socket.client.ClientFactory;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;
import com.engine.common.socket.core.ResponseConstants;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
@SuppressWarnings("unchecked")
public class InSessionTest {

	@Autowired
	private ClientFactory clientFactory;
	private Client client;
	
	@Before
	public void before() {
		client = clientFactory.getClient(true);
	}
	
	@Test
	public void test_found() {
		Command command = Command.valueOf(1, 4);
		Request<?> request = Request.valueOf(command, null);
		Response<String> response = (Response<String>) client.send(request);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(InSessionTestFilter.VALUE));
	}

	@Test
	public void test_not_found() {
		Command command = Command.valueOf(2, 4);
		Request<?> request = Request.valueOf(command, null);
		Response<String> response = (Response<String>) client.send(request);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), nullValue());
	}

	@Test
	public void test_error() {
		Command command = Command.valueOf(3, 4);
		Request<?> request = Request.valueOf(command, null);
		Response<String> response = (Response<String>) client.send(request);
		
		assertThat(response.hasError(), is(true));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.hasState(ResponseConstants.PROCESSING_EXCEPTION), is(true));
	}
}
