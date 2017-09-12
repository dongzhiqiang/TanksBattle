package com.engine.common.ramcache.service.concurrent;

import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.atomic.AtomicInteger;

import org.hamcrest.CoreMatchers;
import org.junit.Assert;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.lock.ChainLock;
import com.engine.common.ramcache.lock.LockUtils;
import com.engine.common.ramcache.orm.Querier;
import com.engine.common.ramcache.persist.QueuePersister;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;
import com.engine.common.utils.collection.ConcurrentHashSet;
import com.engine.common.utils.math.RandomUtils;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class NormalQueueTest {

	@Inject
	private EntityCacheService<Integer, ConcurrentEntityOne> cacheService;
	
	@Autowired
	private Querier querier;
	
	private int total = 100;
	private int times = 10000;	// 测试线程数
	
	private ConcurrentHashSet<Integer> entities = new ConcurrentHashSet<Integer>();
	
	@Before
	public void before() {
		for (int i = 0; i < total; i++) {
			ConcurrentEntityOne entity = cacheService.loadOrCreate(i, new EntityBuilder<Integer, ConcurrentEntityOne>() {
				@Override
				public ConcurrentEntityOne newInstance(Integer id) {
					return ConcurrentEntityOne.valueOf(id);
				}
			});
			entities.add(entity.getId());
		}
	}
	
	@Test
	public void test() throws InterruptedException {
		long start = System.currentTimeMillis();
		final CountDownLatch signal = new CountDownLatch(times);
		final ExecutorService pool = Executors.newFixedThreadPool(4);
		final AtomicInteger count = new AtomicInteger();
		final AtomicInteger removeCount = new AtomicInteger();
		final int mod = times / total;
		for (int i = 0; i < times; i++) {
			final int num = i;
			pool.submit(new Runnable() {
				@Override
				public void run() {
					while(true) {
						int id = RandomUtils.betweenInt(0, total - 1, true);
						ConcurrentEntityOne entity = cacheService.load(id);
						if (entity == null) {
							break;
						}
						ChainLock lock = LockUtils.getLock(entity);
						lock.lock();
						try {
							if (!entities.contains(entity.getId())) {
								continue;
							}
							if (num % mod == 0) {
								entities.remove(entity.getId());
								removeCount.addAndGet(entity.getCount());
								cacheService.remove(id);
							} else {
								count.incrementAndGet();
								entity.setCount(entity.getCount() + 1);
							}
							break;
						} finally {
							lock.unlock();
						}
					}
					signal.countDown();
				}
			});
		}
		
		System.out.println("任务提交完成耗时:" + (System.currentTimeMillis() - start));
		signal.await();
		System.out.println("任务执行完成耗时:" + (System.currentTimeMillis() - start));
		wait4queueEmpty();
		System.out.println("数据持久化完成耗时:" + (System.currentTimeMillis() - start));
		
		int temp = 0;
		for (ConcurrentEntityOne entity : querier.all(ConcurrentEntityOne.class)) {
			temp += entity.getCount();
		}
		temp += removeCount.get();
		System.out.println("计数器值:" + temp);
		Assert.assertThat(temp, CoreMatchers.is(count.get()));
	}
	
	/** 等待更新队列清空 */
	private void wait4queueEmpty() throws InterruptedException {
		QueuePersister persister = (QueuePersister) cacheService.getPersister();
		while (persister.size() > 0) {
			Thread.yield();
		}
		Thread.sleep(100);
	}

}
