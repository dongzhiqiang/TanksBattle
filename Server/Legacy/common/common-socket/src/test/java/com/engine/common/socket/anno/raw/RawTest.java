package com.engine.common.socket.anno.raw;

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
import com.engine.common.socket.core.MessageConstant;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class RawTest {
	
	@Autowired
	private ClientFactory clientFactory;
	private Client client;

	@Before
	public void before() {
		client = clientFactory.getClient("127.0.0.1:8888", false);
	}
	
	@Test
	public void test_in() throws Exception {
		Command command = Command.valueOf(1, 1);
		byte[] body = {1, 2, 3};
		Request<byte[]> request = Request.valueOf(command, body);
		@SuppressWarnings("rawtypes")
		Response response = client.send(request);
		assertThat(response.hasError(), is(false));
		assertThat(response.getBody(), nullValue());
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.hasState(MessageConstant.STATE_RAW), is(false));
	}
	
	@Test
	public void test_out() {
		Command command = Command.valueOf(2, 1);
		byte[] body = {4, 5, 6};
		Request<byte[]> request = Request.valueOf(command, body);
		Response<byte[]> response = client.send(request, byte[].class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getBody(), notNullValue());
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.hasState(MessageConstant.STATE_RAW), is(true));
		
		body = response.getBody();
		assertThat(body.length, is(3));
		assertThat(body[0], is((byte) 4));
		assertThat(body[1], is((byte) 5));
		assertThat(body[2], is((byte) 6));
	}

	@Test
	public void test_in_out() {
		Command command = Command.valueOf(3, 1);
		byte[] body = {7, 8, 9};
		Request<byte[]> request = Request.valueOf(command, body);
		Response<byte[]> response = client.send(request, byte[].class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getBody(), notNullValue());
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.hasState(MessageConstant.STATE_RAW), is(true));
		
		body = response.getBody();
		assertThat(body.length, is(3));
		assertThat(body[0], is((byte) 9));
		assertThat(body[1], is((byte) 8));
		assertThat(body[2], is((byte) 7));
	}

	@SuppressWarnings("rawtypes")
	@Test
	public void test_attachment_to_raw() {
		Command command = Command.valueOf(4, 1);
		byte[] attachment = {10, 11, 12};
		Request request = Request.valueOf(command, null, attachment);
		Response<byte[]> response = client.send(request, byte[].class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getBody(), notNullValue());
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.hasState(MessageConstant.STATE_RAW), is(true));
		
		attachment = response.getBody();
		assertThat(attachment.length, is(3));
		assertThat(attachment[0], is((byte) 10));
		assertThat(attachment[1], is((byte) 11));
		assertThat(attachment[2], is((byte) 12));
	}

}
