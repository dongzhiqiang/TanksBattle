package com.engine.common.ramcache.lock;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

import org.hamcrest.CoreMatchers;
import org.junit.Assert;
import org.junit.Test;

import com.engine.common.ramcache.lock.ChainLock;

/**
 * 锁链的测试
 * 
 */
public class ChainLockTest {
	
	/**
	 * 基本功能测试
	 * @throws InterruptedException
	 */
	@Test
	public void test_basic() throws InterruptedException {
		final AtomicInteger count = new AtomicInteger();
		final Lock lock1 = new ReentrantLock();
		final Lock lock2 = new ReentrantLock();
		List<Lock> locks = new ArrayList<Lock>();
		locks.add(lock1);
		locks.add(lock2);
		ChainLock lock = new ChainLock(locks);
		
		Thread t1 = new Thread() {
			@Override
			public void run() {
				try {
					Thread.sleep(100);
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
				lock1.lock();
				try {
					Thread.sleep(100);
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
				try {
					count.addAndGet(10);
				} finally {
					lock1.unlock();
				}
			}
		};
		t1.start();
		Thread t2 = new Thread() {
			@Override
			public void run() {
				try {
					Thread.sleep(100);
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
				lock2.lock();
				try {
					Thread.sleep(100);
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
				try {
					count.addAndGet(20);
				} finally {
					lock2.unlock();
				}
			}
		};
		t2.start();
		
		lock.lock();
		try {
			Assert.assertThat(count.get(), CoreMatchers.is(0));
			Thread.sleep(500);
			Assert.assertThat(count.get(), CoreMatchers.is(0));
		} finally {
			lock.unlock();
			Thread.sleep(150);
			Assert.assertThat(count.get(), CoreMatchers.is(30));
		}
	}

}
