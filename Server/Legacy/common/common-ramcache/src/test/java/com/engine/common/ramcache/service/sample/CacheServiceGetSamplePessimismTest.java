package com.engine.common.ramcache.service.sample;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.util.concurrent.BrokenBarrierException;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.CyclicBarrier;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import org.junit.Ignore;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.BeansException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationContext;
import org.springframework.context.ApplicationContextAware;
import org.springframework.context.ConfigurableApplicationContext;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.orm.Accessor;

@SuppressWarnings("unchecked")
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class CacheServiceGetSamplePessimismTest implements ApplicationContextAware {
	
	@Autowired
	private Accessor accessor;
	
	@Test
	public void init() {
		for (int i = 0; i < times; i++) {
			Person person = new Person(i, "name-" + i);
			accessor.save(Person.class, person);
		}
	}
	
	private final int times = 1000;		// 测试数据批次
	private final int multiple = 10;	// 测试并发数

	@Test
	@Ignore
	public void test_get() throws InterruptedException, BrokenBarrierException {
		final CountDownLatch signal = new CountDownLatch(times * multiple);
		final CacheServiceGetSample<Integer, Person> cacheService = applicationContext.getBeanFactory().createBean(CacheServiceGetSample.class);
		cacheService.setClz(Person.class);
		
		long start = System.currentTimeMillis();
		for (int i = 0; i < times; i++) {
			final CountDownLatch innerSignal = new CountDownLatch(multiple);
			final ExecutorService pool = Executors.newFixedThreadPool(multiple);
			final CyclicBarrier barrier = new CyclicBarrier(multiple);
			final int id = i;
			for (int j = 0; j < multiple; j++) {
				pool.submit(new Runnable() {
					@Override
					public void run() {
						try {
							barrier.await();
						} catch (Exception e) {
						}
						Person person = cacheService.get(id);
						assertThat(person, notNullValue());
						innerSignal.countDown();
						signal.countDown();
					}
				});
			}
			innerSignal.await();
		}
		signal.await();
		System.out.println("get 全部执行完成:" + (System.currentTimeMillis() - start));
	}
	
	@Test
	@Ignore
	public void test_getAndWait3() throws InterruptedException, BrokenBarrierException {
		final CountDownLatch signal = new CountDownLatch(times * multiple);
		final CacheServiceGetSample<Integer, Person> cacheService = applicationContext.getBeanFactory().createBean(CacheServiceGetSample.class);
		cacheService.setClz(Person.class);
		
		long start = System.currentTimeMillis();
		for (int i = 0; i < times; i++) {
			final CountDownLatch innerSignal = new CountDownLatch(multiple);
			final ExecutorService pool = Executors.newFixedThreadPool(multiple);
			final CyclicBarrier barrier = new CyclicBarrier(multiple);
			final int id = i;
			for (int j = 0; j < multiple; j++) {
				pool.submit(new Runnable() {
					@Override
					public void run() {
						try {
							barrier.await();
						} catch (Exception e) {
						}
						Person person = cacheService.getAndWait3(id);
						assertThat(person, notNullValue());
						innerSignal.countDown();
						signal.countDown();
					}
				});
			}
			innerSignal.await();
		}
		signal.await();
		System.out.println("getAndWait3 全部执行完成:" + (System.currentTimeMillis() - start));
	}

	@Test
	@Ignore
	public void test_getAndWait1() throws InterruptedException, BrokenBarrierException {
		final CountDownLatch signal = new CountDownLatch(times * multiple);
		final CacheServiceGetSample<Integer, Person> cacheService = applicationContext.getBeanFactory().createBean(CacheServiceGetSample.class);
		cacheService.setClz(Person.class);
		
		long start = System.currentTimeMillis();
		for (int i = 0; i < times; i++) {
			final CountDownLatch innerSignal = new CountDownLatch(multiple);
			final ExecutorService pool = Executors.newFixedThreadPool(multiple);
			final CyclicBarrier barrier = new CyclicBarrier(multiple);
			final int id = i;
			for (int j = 0; j < multiple; j++) {
				pool.submit(new Runnable() {
					@Override
					public void run() {
						try {
							barrier.await();
						} catch (Exception e) {
						}
						Person person = cacheService.getAndWait1(id);
						assertThat(person, notNullValue());
						innerSignal.countDown();
						signal.countDown();
					}
				});
			}
			innerSignal.await();
		}
		signal.await();
		System.out.println("getAndWait1 全部执行完成:" + (System.currentTimeMillis() - start));
	}

	@Test
	public void test_getAndWait2() throws InterruptedException, BrokenBarrierException {
		final CountDownLatch signal = new CountDownLatch(times * multiple);
		final CacheServiceGetSample<Integer, Person> cacheService = applicationContext.getBeanFactory().createBean(CacheServiceGetSample.class);
		cacheService.setClz(Person.class);
		
		long start = System.currentTimeMillis();
		for (int i = 0; i < times; i++) {
			final CountDownLatch innerSignal = new CountDownLatch(multiple);
			final ExecutorService pool = Executors.newFixedThreadPool(multiple);
			final CyclicBarrier barrier = new CyclicBarrier(multiple);
			final int id = i;
			for (int j = 0; j < multiple; j++) {
				pool.submit(new Runnable() {
					@Override
					public void run() {
						try {
							barrier.await();
						} catch (Exception e) {
						}
						Person person = cacheService.getAndWait2(id);
						assertThat(person, notNullValue());
						innerSignal.countDown();
						signal.countDown();
					}
				});
			}
			innerSignal.await();
		}
		signal.await();
		System.out.println("getAndWait2 全部执行完成:" + (System.currentTimeMillis() - start));
	}

	private ConfigurableApplicationContext applicationContext;
	
	@Override
	public void setApplicationContext(ApplicationContext applicationContext) throws BeansException {
		this.applicationContext = (ConfigurableApplicationContext) applicationContext;
	}

}
