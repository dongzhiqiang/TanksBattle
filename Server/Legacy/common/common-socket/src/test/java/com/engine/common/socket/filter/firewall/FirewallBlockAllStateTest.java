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
 * 防火墙过滤器测试:全部阻止状态
 * 
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class FirewallBlockAllStateTest {

	@Autowired
	private FirewallManager firewallManager;
	@Autowired
	private ClientFactory clientFactory;
	
	@Test
	public void test() {
		Request<Integer> request = Request.valueOf(Command.valueOf(1, 1), 100);
		
		Client client = clientFactory.createClient("192.168.10.127:8888");
		try {
			client.send(request, int.class);
			fail();
		} catch (Exception e) {
			assertThat(e, instanceOf(SocketException.class));
			assertThat(client.isConnected(), is(false));
			assertThat(firewallManager.getCurrentConnections(), is(0));
		}

		firewallManager.unblockAll();
		client = clientFactory.createClient("192.168.10.127:8888");
		Response<Integer> response = client.send(request, int.class);
		assertThat(response.getBody(), is(100));
		assertThat(firewallManager.getCurrentConnections(), is(1));
		client.close();
		assertThat(firewallManager.getCurrentConnections(), is(0));
		
		firewallManager.blockAll();
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
}
