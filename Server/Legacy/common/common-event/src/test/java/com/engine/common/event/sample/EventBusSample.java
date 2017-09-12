package com.engine.common.event.sample;

import static org.junit.Assert.*;

import java.util.Date;
import java.util.Random;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.event.Event;
import com.engine.common.event.EventBus;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class EventBusSample {

	@Autowired
	EventBus bus;

	@Test
	public void test() {
		System.out.println("start enter logic!");
		Random random = new Random();
		Event<TestOneEvent> event = TestOneEvent.valueOf(random.nextInt(),
				new Date(), "one test event logic!");
		bus.post(event);
		bus.post(TestTwoEvent.valueOf((byte)1));
	}
	
	@Test
	public void syncTest() {
		System.out.println("start enter logic!");
		Random random = new Random();
		Event<TestOneEvent> event = TestOneEvent.valueOf(random.nextInt(),
				new Date(), "one test event sync logic!");
		bus.syncPost(event);
	}

}
