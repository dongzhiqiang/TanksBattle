package com.engine.common.socket.anno.request;

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

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
@SuppressWarnings("unchecked")
public class InRequestTest {

	@Autowired
	private ClientFactory clientFactory;
	private Client client;
	
	@Before
	public void before() {
		client = clientFactory.getClient(true);
	}
	
	@Test
	public void test_sn() {
		Command command = Command.valueOf(1, 3);
		Request<?> request = Request.valueOf(command, null);
		Response<Long> response = (Response<Long>) client.send(request);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(response.getSn()));
	}

	@Test
	public void test_command() {
		Command command = Command.valueOf(2, 3);
		Request<?> request = Request.valueOf(command, null);
		Response<Boolean> response = (Response<Boolean>) client.send(request);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(true));
	}

	@Test
	public void test_state() {
		Command command = Command.valueOf(3, 3);
		Request<?> request = Request.valueOf(command, new Double[]{1.0, 2.0});
		Response<Integer> response = (Response<Integer>) client.send(request);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(0));
	}
	
	@Test
	public void test_attachment() throws InterruptedException {
		// 有附加信息
		Command command = Command.valueOf(4, 3);
		Request<?> request = Request.valueOf(command, null, new byte[]{1, 2, 3});
		Response<String> response = client.send(request, String.class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is("[1, 2, 3]"));
		
		// 没有附加信息
		request = Request.valueOf(command, null, null);
		response = client.send(request, String.class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is("null"));
	}

}
