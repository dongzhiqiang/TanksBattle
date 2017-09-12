package com.engine.common.ramcache.sample.region.change;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.util.Collection;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.atomic.AtomicInteger;

import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.orm.Querier;
import com.engine.common.ramcache.persist.TimingConsumer;
import com.engine.common.ramcache.persist.TimingConsumerState;
import com.engine.common.ramcache.persist.TimingPersister;
import com.engine.common.ramcache.service.IndexValue;
import com.engine.common.ramcache.service.RegionCacheService;
import com.engine.common.utils.math.RandomUtils;

/**
 * 并发修改测试
 * 
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class ConcurrentTimingTest {

	@Inject
	private RegionCacheService<Integer, BasicItem> itemService;
	
	@Autowired
	private Querier querier;
	
	/** 数据总量 */
	private int total = 10000;
	private ConcurrentHashMap<Integer, BasicItem> items = new ConcurrentHashMap<Integer, BasicItem>();

	/** 初始化创建测试数据 */
	@Before
	public void init() {
		for (int i = 0; i < total; i++) {
			BasicItem item = itemService.create(BasicItem.valueOf(i, 0, 0));
			items.put(i, item);
		}
	}
	
	private int times = 10000;	// 测试线程数

	private ExecutorService pool = Executors.newFixedThreadPool(4);
	
	@Test
	public void test_change() throws InterruptedException {
		long start = System.currentTimeMillis();
		final CountDownLatch signal = new CountDownLatch(times);
		for (int i = 0; i < times; i++) {
			final int owner = (i % 3) + 1;
			pool.submit(new Runnable() {
				@Override
				public void run() {
					int id = RandomUtils.betweenInt(0, total - 1, true);
					BasicItem item = items.get(id);
					synchronized (item) {
						item.setOwner(owner);
						item.increaseAmount(1);
					}
					signal.countDown();
				}
			});
		}
		
		System.out.println("任务提交完成耗时:" + (System.currentTimeMillis() - start));
		signal.await();
		System.out.println("任务执行完成耗时:" + (System.currentTimeMillis() - start));
		wait4queueEmpty();
		System.out.println("入库完成耗时:" + (System.currentTimeMillis() - start));
		
		AtomicInteger[] counts = new AtomicInteger[] { new AtomicInteger(), new AtomicInteger(), new AtomicInteger(),
				new AtomicInteger() };
		AtomicInteger totalCount = new AtomicInteger();
		
		List<BasicItem> chkList = querier.all(BasicItem.class);
		for (BasicItem chk : chkList) {
			BasicItem item = itemService.get(IndexValue.valueOf("owner", chk.getOwner()), chk.getId());
			assertThat(item.getAmount(), is(chk.getAmount()));
			assertThat(item.getOwner(), is(chk.getOwner()));
			
			AtomicInteger count = counts[chk.getOwner()];
			count.incrementAndGet();
			totalCount.addAndGet(chk.getAmount());
		}
		for (int i = 0; i < counts.length; i++) {
			Collection<BasicItem> items = itemService.load(IndexValue.valueOf("owner", i));
			assertThat(items.size(), is(counts[i].get()));
		}
		assertThat(totalCount.get(), is(times));
	}
	
	/** 等待更新队列清空 */
	private void wait4queueEmpty() throws InterruptedException {
		TimingPersister persister = (TimingPersister) itemService.getPersister();
		persister.flush();
		TimingConsumer consumer = persister.getConsumer();
		while (consumer.getState() == TimingConsumerState.RUNNING) {
			Thread.yield();
		}
	}
}
