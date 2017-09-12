package com.engine.common.socket.filter.firewall;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

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
import com.engine.common.socket.exception.SocketException;
import com.engine.common.socket.filter.firewall.FirewallManager;

/**
 * 防火墙过滤器测试:黑白名单
 * 
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class FirewallBlockAndAllowTest {

	@Autowired
	private FirewallManager firewallManager;
	@Autowired
	private ClientFactory clientFactory;
	
	@Test
	public void test_block_and_unblock() {
		Request<Integer> request = Request.valueOf(Command.valueOf(1, 1), 100);
		
		Client client = clientFactory.createClient("192.168.10.127:8888");
		Response<Integer> response = client.send(request, int.class);
		assertThat(response.getBody(), is(100));
		assertThat(firewallManager.getCurrentConnections(), is(1));
		client.close();
		assertThat(firewallManager.getCurrentConnections(), is(0));
		
		firewallManager.block("192.168.10.127");
		client = clientFactory.createClient("192.168.10.127:8888");
		try {
			client.send(request, int.class);
			fail();
		} catch (Exception e) {
			assertThat(e, instanceOf(SocketException.class));
			assertThat(client.isConnected(), is(false));
			assertThat(firewallManager.getCurrentConnections(), is(0));
		}
		
		firewallManager.unblock("192.168.10.127");
		client = clientFactory.createClient("192.168.10.127:8888");
		response = client.send(request, int.class);
		assertThat(response.getBody(), is(100));
		assertThat(firewallManager.getCurrentConnections(), is(1));
		client.close();
	}
	
	@Test
	public void test_allow_and_disallow() {
		firewallManager.allow("192.168.10.127");
		firewallManager.block("192.168.10.127");
		
		Request<Integer> request = Request.valueOf(Command.valueOf(1, 1), 100);
		Client client = clientFactory.createClient("192.168.10.127:8888");
		Response<Integer> response = client.send(request, int.class);
		assertThat(response.getBody(), is(100));
		assertThat(firewallManager.getCurrentConnections(), is(1));
		client.close();
		assertThat(firewallManager.getCurrentConnections(), is(0));
		
		firewallManager.disallow("192.168.10.127");
		client = clientFactory.createClient("192.168.10.127:8888");
		try {
			client.send(request, int.class);
			fail();
		} catch (Exception e) {
			assertThat(e, instanceOf(SocketException.class));
			assertThat(client.isConnected(), is(false));
			assertThat(firewallManager.getCurrentConnections(), is(0));
		}
	}
	
	@Test
	public void test_block_times() throws InterruptedException {
		firewallManager.block("192.168.10.127");
		Thread.sleep(10000);
		
		Request<Integer> request = Request.valueOf(Command.valueOf(1, 1), 100);
		
		Client client = clientFactory.createClient("192.168.10.127:8888");
		Response<Integer> response = client.send(request, int.class);
		assertThat(response.getBody(), is(100));
		assertThat(firewallManager.getCurrentConnections(), is(1));
		client.close();
		Thread.sleep(1000);
		assertThat(firewallManager.getCurrentConnections(), is(0));
	}
}
