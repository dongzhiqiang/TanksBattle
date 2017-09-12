package com.engine.common.socket.anno.response;

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
public class ResponseTest {

	@Autowired
	private ClientFactory clientFactory;
	private Client client;
	
	@Before
	public void before() {
		client = clientFactory.getClient(true);
	}
	
	@Test
	public void test_attachment() {
		Command command = Command.valueOf(1, 1);
		// 有附加信息
		Request<byte[]> request = Request.valueOf(command, new byte[]{1, 2, 3});
		Response<String> response = client.send(request, String.class);
		
		assertThat(response.getBody(), is("SUCCESS"));
		assertThat(response.hasState(MessageConstant.STATE_ATTACHMENT), is(true));
		assertThat(response.getAttachment(), notNullValue());
		byte[] bytes = response.getAttachment();
		assertThat(bytes[0], is((byte) 1));
		assertThat(bytes[1], is((byte) 2));
		assertThat(bytes[2], is((byte) 3));
		
		// 没有附加信息
		request = Request.valueOf(command, null);
		response = client.send(request, String.class);
		assertThat(response.getBody(), is("SUCCESS"));
		assertThat(response.hasState(MessageConstant.STATE_ATTACHMENT), is(false));
		assertThat(response.getAttachment(), nullValue());
	}
	
	@Test
	public void test_in_and_out() {
		Command command = Command.valueOf(2, 1);
		// 有附加信息
		Request<Integer> request = Request.valueOf(command, 1, new byte[]{4, 5, 6});
		Response<Integer> response = client.send(request, Integer.class);
		
		assertThat(response.getBody(), is(1));
		assertThat(response.hasState(MessageConstant.STATE_ATTACHMENT), is(true));
		assertThat(response.getAttachment(), notNullValue());
		byte[] bytes = response.getAttachment();
		assertThat(bytes[0], is((byte) 6));
		assertThat(bytes[1], is((byte) 5));
		assertThat(bytes[2], is((byte) 4));
		
		// 没有附加信息
		request = Request.valueOf(command, 2);
		response = client.send(request, Integer.class);
		assertThat(response.getBody(), is(2));
		assertThat(response.hasState(MessageConstant.STATE_ATTACHMENT), is(false));
		assertThat(response.getAttachment(), nullValue());
	}
	
	@Test
	public void test_raw_to_out() {
		Command command = Command.valueOf(3, 1);
		// 有附加信息
		Request<byte[]> request = Request.valueOf(command, new byte[]{7, 8, 9});
		Response<Boolean> response = client.send(request, Boolean.class);
		
		assertThat(response.getBody(), is(true));
		assertThat(response.hasState(MessageConstant.STATE_ATTACHMENT), is(true));
		assertThat(response.getAttachment(), notNullValue());
		byte[] bytes = response.getAttachment();
		assertThat(bytes[0], is((byte) 7));
		assertThat(bytes[1], is((byte) 8));
		assertThat(bytes[2], is((byte) 9));
		
		// 没有附加信息
		request = Request.valueOf(command, null);
		response = client.send(request, Boolean.class);
		assertThat(response.getBody(), is(false));
		assertThat(response.hasState(MessageConstant.STATE_ATTACHMENT), is(false));
		assertThat(response.getAttachment(), nullValue());
	}

}
