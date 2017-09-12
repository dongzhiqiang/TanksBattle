package com.engine.common.event;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.util.concurrent.atomic.AtomicInteger;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.event.AbstractReceiver;
import com.engine.common.event.Event;
import com.engine.common.event.EventBus;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class EventBusTest {

	@Autowired
	private EventBus eventBus;
	
	@Component
	public static class TestReceiver extends AbstractReceiver<String> {

		@Override
		public String[] getEventNames() {
			return new String[] {"TEST"};
		}

		@Override
		public void doEvent(String event) {
			assertThat(event, is("CONTENT"));
			count.incrementAndGet();
		}

	}
	
	private static AtomicInteger count = new AtomicInteger();
	
	@Test(timeout = 1000)
	public void test() throws InterruptedException {
		for (int i = 0; i < 10000; i++) {
			eventBus.post(new Event<String>("TEST", "CONTENT"));
		}
		Thread.sleep(100);
		assertThat(count.get(), is(10000));
	}
}
