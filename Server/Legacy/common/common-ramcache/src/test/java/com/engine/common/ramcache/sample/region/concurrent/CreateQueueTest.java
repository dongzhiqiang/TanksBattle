package com.engine.common.ramcache.sample.region.concurrent;

import java.util.Collection;
import java.util.List;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.CyclicBarrier;
import java.util.concurrent.atomic.AtomicInteger;

import org.hamcrest.CoreMatchers;
import org.junit.Assert;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.orm.Querier;
import com.engine.common.ramcache.persist.QueuePersister;
import com.engine.common.ramcache.service.IndexValue;
import com.engine.common.ramcache.service.RegionCacheService;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class CreateQueueTest {

	@Inject
	private RegionCacheService<Integer, BasicItem> itemService;
	
	@Autowired
	private Querier querier;
	
	private int times = 10000;
	
	@Test
	public void test() throws InterruptedException {
		final AtomicInteger errors = new AtomicInteger();
		for (int i = 0; i < times; i++) {
			final int id = i;
			final CountDownLatch signal = new CountDownLatch(2);
			final CyclicBarrier barrier = new CyclicBarrier(2);
			Thread thread1 = new Thread() {
				public void run() {
					try {
						barrier.await();
					} catch (Exception e) {
					}
					
					try {
						itemService.create(BasicItem.valueOf(id, 1, 0));
					} catch (Exception e) {
						errors.incrementAndGet();
					}
					signal.countDown();
				};
			};
			Thread thread2 = new Thread() {
				public void run() {
					try {
						barrier.await();
					} catch (Exception e) {
					}
					
					try {
						itemService.create(BasicItem.valueOf(id, 2, 0));
					} catch (Exception e) {
						errors.incrementAndGet();
					}
					signal.countDown();
				};
			};
			thread1.start();
			thread2.start();
			signal.await();
		}
		
		Assert.assertThat(errors.get(), CoreMatchers.is(times));
		Collection<BasicItem> item1 = itemService.load(IndexValue.valueOf("owner", 1));
		Collection<BasicItem> item2 = itemService.load(IndexValue.valueOf("owner", 2));
		Assert.assertThat(item1.size() + item2.size(), CoreMatchers.is(times));
		
		wait4queueEmpty();
		int count1 = 0;
		int count2 = 0;
		List<BasicItem> items = querier.all(BasicItem.class);
		for (BasicItem item : items) {
			if (item.getOwner() == 1) {
				count1++;
			}
			if (item.getOwner() == 2) {
				count2++;
			}
		}
		Assert.assertThat(count1, CoreMatchers.is(item1.size()));
		Assert.assertThat(count2, CoreMatchers.is(item2.size()));
	}
	
	/** 等待更新队列清空 */
	private void wait4queueEmpty() throws InterruptedException {
		QueuePersister persister = (QueuePersister) itemService.getPersister();
		while (persister.size() > 0) {
			Thread.yield();
		}
		Thread.sleep(100);
	}
}
