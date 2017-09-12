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
 * 防火墙过滤器测试:最大客户端连接数
 * 
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class FirewallMaxClientsTest {

	@Autowired
	private FirewallManager firewallManager;
	@Autowired
	private ClientFactory clientFactory;
	
	@Test
	public void test_max_clients() {
		Request<Integer> request = Request.valueOf(Command.valueOf(1, 1), 100);
		
		for (int i = 0; i < 4; i++) {
			Client client = clientFactory.createClient("192.168.10.127:8888");
			if (i != 3) {
				Response<Integer> response = client.send(request, int.class);
				assertThat(response.getBody(), is(100));
				assertThat(firewallManager.getCurrentConnections(), is(i + 1));
			} else {
				try {
					client.send(request, int.class);
					fail();
				} catch (Exception e) {
					assertThat(e, instanceOf(SocketException.class));
					assertThat(client.isConnected(), is(false));
					assertThat(firewallManager.getCurrentConnections(), is(3));
				}
			}
		}
	}
}
