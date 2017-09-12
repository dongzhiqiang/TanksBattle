package com.engine.common.socket.client.processor;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.socket.client.Client;
import com.engine.common.socket.client.ClientFactory;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Request;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class ProcessorTest {
	
	@Autowired
	private ClientFactory clientFactory;

	/** 测试非活跃客户端移除  */
	@Test
	public void test() throws InterruptedException {
		Client client = clientFactory.getClient("127.0.0.1:9999", false);
		
		client.send(Request.valueOf(Command.valueOf(1, 1), "hello"));
		Thread.sleep(2000);
	}
	
}
