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
import com.engine.common.socket.exception.SocketException;
import com.engine.common.socket.filter.firewall.FirewallManager;

/**
 * 防火墙过滤器测试:违规测试
 * 
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class FirewallTest {

	@Autowired
	private FirewallManager firewallManager;
	@Autowired
	private ClientFactory clientFactory;
	
	@Test
	public void test_bytes_limit() {
		byte[] bytes = new byte[100];
		Request<byte[]> request = Request.valueOf(Command.valueOf(2, 1), bytes);
		
		Client client = clientFactory.createClient("192.168.10.127:8888");
		for (int i = 0; i < 3; i++) {
			if (i != 2) {
				client.send(request, void.class);
			} else {
				try {
					client.send(request, void.class);
					fail();
				} catch (Exception e) {
					assertThat(e, instanceOf(SocketException.class));
					assertThat(client.isConnected(), is(false));
				}
			}
		}
		assertThat(firewallManager.getCurrentConnections(), is(0));
	}
	
	@Test
	public void test_times_limit() throws InterruptedException {
		firewallManager.unblock("192.168.10.127");
		
		Request<Integer> request = Request.valueOf(Command.valueOf(1, 1), 1);
		
		Client client = clientFactory.createClient("192.168.10.127:8888");
		for (int i = 0; i < 5; i++) {
			if (i != 4) {
				client.send(request, int.class);
			} else {
				try {
					client.send(request, int.class);
					fail();
				} catch (Exception e) {
					assertThat(e, instanceOf(SocketException.class));
					assertThat(client.isConnected(), is(false));
				}
			}
		}
		assertThat(firewallManager.getCurrentConnections(), is(0));
	}

}
