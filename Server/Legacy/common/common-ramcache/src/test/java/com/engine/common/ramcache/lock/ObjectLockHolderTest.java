package com.engine.common.ramcache.lock;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.locks.Lock;

import org.junit.Test;

import com.engine.common.ramcache.lock.ObjectLock;
import com.engine.common.ramcache.lock.ObjectLockHolder;

/**
 * 锁持有者测试
 * 
 * 
 */
public class ObjectLockHolderTest {

	/** 测试单一类(实体) */
	@Test
	public void test_single_class_one() throws InterruptedException {
		ObjectLockHolder holder = new ObjectLockHolder();

		EntityString entity = new EntityString("one");
		ObjectLock lock1 = holder.getLock(entity);
		ObjectLock lock2 = holder.getLock(entity);

		assertThat(lock1, sameInstance(lock2));

		System.gc();
		Thread.sleep(100);
		ObjectLock lock3 = holder.getLock(entity);
		assertThat(lock3, sameInstance(lock1));
	}

	/** 测试单一类(非实体) */
	@Test
	public void test_single_class_two() throws InterruptedException {
		ObjectLockHolder holder = new ObjectLockHolder();

		String one = new String("one");
		ObjectLock lock1 = holder.getLock(one);
		ObjectLock lock2 = holder.getLock(one);

		assertThat(lock1, sameInstance(lock2));

		System.gc();
		Thread.sleep(100);
		ObjectLock lock3 = holder.getLock(one);
		assertThat(lock3, sameInstance(lock1));
	}
	
	/** 测试单一类(非实体) */
	@Test
	public void test_single_class_three() throws InterruptedException {
		ObjectLockHolder holder = new ObjectLockHolder();

		String one = new String("one");
		ObjectLock lock1 = holder.getLock(one);
		ObjectLock lock2 = holder.getLock(one);

		assertThat(lock1, sameInstance(lock2));
		
		System.gc();
		Thread.sleep(100);
		one = new String("one");
		ObjectLock lock3 = holder.getLock(one);
		assertThat(lock3, sameInstance(lock1));
	}


	/** 测试类的唯一锁 */
	@Test
	public void test_class_tie() {
		ObjectLockHolder holder = new ObjectLockHolder();

		Lock lock1 = holder.getTieLock(String.class);
		Lock lock2 = holder.getTieLock(Integer.class);
		assertThat(lock1, not(sameInstance(lock2)));

		Lock lock3 = holder.getTieLock(String.class);
		assertThat(lock3, sameInstance(lock1));
	}

	/** 测试内存(锁)自动回收 */
	@Test
	public void test_recover() throws InterruptedException {
		ObjectLockHolder holder = new ObjectLockHolder();

		List<Object> list = new ArrayList<Object>();
		list.add(new Integer(1));
		list.add(new EntityInteger(1));

		Lock lock1 = holder.getLock(list.get(0));
		Lock lock2 = holder.getLock(list.get(1));
		assertThat(lock1, not(sameInstance(lock2)));
		assertThat(holder.count(Integer.class), is(1));
		assertThat(holder.count(EntityInteger.class), is(1));

		// 保持实例引用的测试
		System.gc();
		Thread.sleep(100);
		assertThat(holder.count(Integer.class), is(1));
		assertThat(holder.count(EntityInteger.class), is(1));

		// 清除对实例的引用再测试
		list.clear();
		System.gc();
		Thread.sleep(200);
		assertThat(holder.count(Integer.class), is(0));
		assertThat(holder.count(EntityInteger.class), is(0));
	}

}
