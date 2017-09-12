package com.engine.common.ramcache.service.concurrent;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.NoSuchElementException;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.CyclicBarrier;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.locks.ReentrantLock;

import org.hamcrest.CoreMatchers;
import org.junit.After;
import org.junit.Assert;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.exception.UniqueFieldException;
import com.engine.common.ramcache.lock.ChainLock;
import com.engine.common.ramcache.lock.LockUtils;
import com.engine.common.ramcache.persist.QueuePersister;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;
import com.engine.common.utils.collection.ConcurrentHashSet;
import com.engine.common.utils.math.RandomUtils;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class UniqueQueueTest {

	@Inject
	private EntityCacheService<Integer, ConcurrentEntityTwo> cacheService;
	
	private int total = 100;
	private int times = 10000;	// 测试线程数
	
	private ConcurrentHashSet<Integer> entities = new ConcurrentHashSet<Integer>();
	
	private List<String> name1 = new ArrayList<String>();
	private List<String> name2 = new ArrayList<String>();
	
	@Before
	public void before() {
		for (int i = 0; i < total; i++) {
			final String name = "init:" + i;
			ConcurrentEntityTwo entity = cacheService.loadOrCreate(i, new EntityBuilder<Integer, ConcurrentEntityTwo>() {
				@Override
				public ConcurrentEntityTwo newInstance(Integer id) {
					return ConcurrentEntityTwo.valueOf(id, name);
				}
			});
			entities.add(entity.getId());
		}
		for (int i = 0; i < times / 2; i++) {
			name1.add("name1:" + i);
		}
	}
	
	@After
	public void after() throws InterruptedException {
		for (int i = 0; i < total; i++) {
			cacheService.remove(i);
		}
		name1.clear();
		name2.clear();
		wait4queueEmpty();
	}
	
	private ReentrantLock nameLock = new ReentrantLock();
	
	@Test
	public void test_one() throws InterruptedException {
		System.out.println("[test_one]");
		long start = System.currentTimeMillis();
		final Iterator<String> name1It = name1.iterator();
		final CountDownLatch signal = new CountDownLatch(times);
		final ExecutorService pool = Executors.newFixedThreadPool(4);
		final AtomicInteger count = new AtomicInteger();
		for (int i = 0; i < times; i++) {
			pool.submit(new Runnable() {
				@Override
				public void run() {
					int id = RandomUtils.betweenInt(0, total - 1, true);
					ConcurrentEntityTwo entity = cacheService.load(id);
					if (entity == null) {
						return;
					}
					ChainLock lock = LockUtils.getLock(entity);
					lock.lock();
					try {
						String name = null;
						nameLock.lock();
						try {
							name = name1It.next();
							name2.add(name);
						} catch (NoSuchElementException e) {
							name = name2.get(name2.size() - 1);
						} finally {
							nameLock.unlock();
						}
						entity.setName(name);
					} catch (UniqueFieldException e) {
						count.incrementAndGet();
					} finally {
						lock.unlock();
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
		
		System.out.println("唯一键冲突次数:" + count.get());
		Assert.assertThat(count.get(), CoreMatchers.is((times / 2)));
	}
	
	@Test
	public void test_two() throws InterruptedException {
		System.out.println("[test_two]");
		
		long start = System.currentTimeMillis();
		
		final Iterator<String> name1It = name1.iterator();
		final AtomicInteger count = new AtomicInteger();
		
		for (int i = 0; i < times / 2; i++) {
			final CountDownLatch signal = new CountDownLatch(2);
			final CyclicBarrier barrier = new CyclicBarrier(2);
			final String name = name1It.next();
			Thread thread1 = new Thread() {
				public void run() {
					try {
						barrier.await();
					} catch (Exception e) {
					}
					int id = RandomUtils.betweenInt(0, total - 1, true);
					ConcurrentEntityTwo entity = cacheService.load(id);
					if (entity == null) {
						return;
					}
					ChainLock lock = LockUtils.getLock(entity);
					lock.lock();
					try {
						entity.setName(name);
					} catch (UniqueFieldException e) {
						count.incrementAndGet();
					} finally {
						lock.unlock();
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
					int id = RandomUtils.betweenInt(0, total - 1, true);
					ConcurrentEntityTwo entity = cacheService.load(id);
					if (entity == null) {
						return;
					}
					ChainLock lock = LockUtils.getLock(entity);
					lock.lock();
					try {
						entity.setName(name);
					} catch (UniqueFieldException e) {
						count.incrementAndGet();
					} finally {
						lock.unlock();
					}
					signal.countDown();
				};
			};
			thread1.start();
			thread2.start();

			signal.await();
		}
		
		System.out.println("任务执行完成耗时:" + (System.currentTimeMillis() - start));
		wait4queueEmpty();
		System.out.println("数据持久化完成耗时:" + (System.currentTimeMillis() - start));
		
		System.out.println("唯一键冲突次数:" + count.get());
		Assert.assertThat(count.get(), CoreMatchers.is((times / 2)));
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
