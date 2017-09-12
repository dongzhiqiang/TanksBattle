package com.engine.common.scheduler;

import java.util.Calendar;
import java.util.Date;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ScheduledFuture;
import java.util.concurrent.atomic.AtomicInteger;

import org.hamcrest.CoreMatchers;
import org.junit.Assert;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.scheduler.ScheduledTask;
import com.engine.common.scheduler.Scheduler;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class BasicTest {

	@Autowired
	private Scheduler scheduler;
	
	@Test
	public void test() throws InterruptedException, ExecutionException {
		final AtomicInteger counter = new AtomicInteger();
		Calendar calendar = Calendar.getInstance();
		calendar.add(Calendar.SECOND, 1);
		Date start = calendar.getTime();
		ScheduledFuture<?> futrue = scheduler.scheduleAtFixedRate(new ScheduledTask() {
			@Override
			public void run() {
				counter.incrementAndGet();
			}
			@Override
			public String getName() {
				return "TEST";
			}
		}, start, 2000);
		Thread.sleep(2000);
		futrue.cancel(false);
		Thread.sleep(2000);
		Assert.assertThat(counter.get(), CoreMatchers.is(1));
	}
}
