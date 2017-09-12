package com.engine.common.socket.client;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.apache.mina.core.session.IoSession;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.socket.client.Client;
import com.engine.common.socket.client.ClientFactory;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Processor;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;
import com.engine.common.socket.exception.SocketException;
import com.engine.common.socket.exception.TypeDefinitionNotFound;
import com.engine.common.socket.handler.TypeDefinition;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class ClientTest {
	
	@Autowired
	private ClientFactory clientFactory;

	/** 测试非活跃客户端移除  */
	@Test
	public void test_client_instance() throws InterruptedException {
		Client client1 = clientFactory.getClient("127.0.0.1:11111", false);
		Client client2 = clientFactory.getClient("127.0.0.1:11111", false);
		assertThat(client1, sameInstance(client2));
		assertThat(client1.isConnected(), is(false));
		assertThat(client2.isConnected(), is(false));
		
		client1.send(Request.valueOf(Command.valueOf(2, 1), null));
		assertThat(client1.isConnected(), is(true));
		assertThat(client2.isConnected(), is(true));
		
		Thread.sleep(2000);
		Client client3 = clientFactory.getClient("127.0.0.1:9999", false);
		assertThat(client3, not(sameInstance(client1)));
		assertThat(client3, not(sameInstance(client2)));
		assertThat(client1.isConnected(), is(false));
		assertThat(client2.isConnected(), is(false));
		assertThat(client3.isConnected(), is(false));
	}
	
	/** 测试保持连接状态  */
	@Test
	public void test_keepAlive() throws InterruptedException {
		Client client = clientFactory.getClient("127.0.0.1:9999", true);
		assertThat(client.isKeepAlive(), is(true));
		client.connect();
		
		Thread.sleep(2000);
		assertThat(client.isConnected(), is(true));
		Client other = clientFactory.getClient("127.0.0.1:9999", false);
		assertThat(other, sameInstance(client));
		
		client.disableKeepAlive();
		Thread.sleep(3000);
		assertThat(client.isConnected(), is(false));
		other = clientFactory.getClient("127.0.0.1:9999", false);
		assertThat(other, not(sameInstance(client)));
	}
	
	/** 回应超时测试 */
	@Test
	public void test_timeout() {
		Client client = clientFactory.getClient("127.0.0.1:9999", false);
		Response<?> response = client.send(Request.valueOf(Command.valueOf(2, 1), null), void.class);
		assertThat(response.getBody(), nullValue());
		
		try {
			response = client.send(Request.valueOf(Command.valueOf(1, 1), null), void.class);
			fail();
		} catch (Exception e) {
			assertThat(e, instanceOf(SocketException.class));
		}
	}
	
	/** 测试指令注册 */
	@SuppressWarnings("unchecked")
	@Test
	public void test_register_command() {
		Command command = Command.valueOf(1, 2);
		Client client = clientFactory.getClient("127.0.0.1:9999", false);
		try {
			client.send(Request.valueOf(command, "123"), String.class);
			fail();
		} catch (Exception e) {
			assertThat(e, instanceOf(TypeDefinitionNotFound.class));
		}
		try {
			client.send(Request.valueOf(command, "456"));
		} catch (Exception e) {
			assertThat(e, instanceOf(TypeDefinitionNotFound.class));
		}
		
		client.register(command, TypeDefinition.valueOf((byte) 0, String.class, String.class), null);
		Response<String> response = client.send(Request.valueOf(command, "123"), String.class);
		assertThat(response.getBody(), is("123"));
		response = (Response<String>) client.send(Request.valueOf(command, "456"));
		assertThat(response.getBody(), is("456"));
	}

	/** 测试处理器注册 */
	@Test
	@SuppressWarnings("rawtypes")
	public void test_register_processor() throws InterruptedException {
		class TestProcessor implements Processor {
			public int count = 0;
			@Override
			public Object process(Request request, IoSession session) {
				count++;
				return null;
			}
		}
		TestProcessor processor = new TestProcessor();
		
		Client client = clientFactory.getClient("127.0.0.1:9999", false);
		client.send(Request.valueOf(Command.valueOf(3, 1), null));
		Thread.sleep(100);
		assertThat(processor.count, is(0));
		
		client.register(Command.valueOf(2, 2), TypeDefinition.valueOf((byte) 0, null, null), processor);
		client.send(Request.valueOf(Command.valueOf(3, 1), null));
		Thread.sleep(100);
		assertThat(processor.count, is(1));
	}
}
