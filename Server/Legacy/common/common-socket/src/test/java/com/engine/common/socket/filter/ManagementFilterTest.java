package com.engine.common.socket.filter;

import org.hamcrest.CoreMatchers;
import org.junit.Assert;
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
public class ManagementFilterTest {

	@Autowired
	private ClientFactory clientFactory;
	private Client client;

	@Before
	public void before() {
		client = clientFactory.getClient(true);
	}
	
	@Test
	public void test_isManagement() {
		Request<?> request = Request.valueOf(Command.valueOf(1, 2), null);
		Response<Boolean> response = client.send(request, Boolean.TYPE);
		Assert.assertThat(response.getBody(), CoreMatchers.is(true));
	}
	
	@Test
	public void test_getName() {
		Request<?> request = Request.valueOf(Command.valueOf(2, 2), null);
		Response<String> response = client.send(request, String.class);
		Assert.assertThat(response.getBody(), CoreMatchers.is("TEST"));
	}

}
