package com.engine.common.ramcache.lock;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

import org.junit.Test;

import com.engine.common.ramcache.lock.ChainLock;
import com.engine.common.ramcache.lock.LockUtils;
import com.engine.common.ramcache.lock.ObjectLock;

/**
 * 锁工具测试
 * 
 * 
 */
public class LockUtilsTest {

	/**
	 * 测试锁排序是否正确，是否能正确添加类型唯一锁
	 */
	@Test
	public void test_loadLocks() {
		List<? extends Lock> locks = LockUtils.loadLocks(1, 1);
		assertThat(locks.size(), is(3));
		locks = LockUtils.loadLocks(1, "1", 1, "1");
		assertThat(locks.size(), is(6));
		locks = LockUtils.loadLocks(new Integer(1), new Integer(1));
		assertThat(locks.size(), is(3));

		locks = LockUtils.loadLocks(new EntityInteger(5), new EntityInteger(1), new EntityInteger(3),
				new EntityInteger(7));
		assertThat(locks.size(), is(4));
		assertThat((Integer) ((ObjectLock) locks.get(0)).getValue(), is(1));
		assertThat((Integer) ((ObjectLock) locks.get(1)).getValue(), is(3));
		assertThat((Integer) ((ObjectLock) locks.get(2)).getValue(), is(5));
		assertThat((Integer) ((ObjectLock) locks.get(3)).getValue(), is(7));

		locks = LockUtils.loadLocks(new EntityInteger(5), new EntityInteger(1), new EntityInteger(3),
				new EntityInteger(1));
		assertThat(locks.size(), is(5));
		assertThat(locks.get(0).getClass().getName(), is(ReentrantLock.class.getName()));
		assertThat((Integer) ((ObjectLock) locks.get(1)).getValue(), is(1));
		assertThat((Integer) ((ObjectLock) locks.get(2)).getValue(), is(1));
		assertThat((Integer) ((ObjectLock) locks.get(3)).getValue(), is(3));
		assertThat((Integer) ((ObjectLock) locks.get(4)).getValue(), is(5));
	}

	class MyObject {
		private int count;

		public int getCount() {
			return count;
		}

		public void setCount(int count) {
			this.count = count;
		}
	}

	/** 测试锁的使用 */
	@Test
	public void test_lock_use() {
		final MyObject obj1 = new MyObject();
		final MyObject obj2 = new MyObject();
		final ChainLock lock1 = LockUtils.getLock(obj1, obj2);
		final ChainLock lock2 = LockUtils.getLock(obj2, obj1);
		final int times = 1000000;
		final AtomicBoolean end1 = new AtomicBoolean(false);
		final AtomicBoolean end2 = new AtomicBoolean(false);
		
		Thread t1 = new Thread() {
			@Override
			public void run() {
				for (int i = 0; i < times; i++) {
					lock1.lock();
					try {
						obj1.setCount(obj1.getCount() + 1);
						obj2.setCount(obj2.getCount() - 1);
						assertThat(obj1.getCount(), is(-obj2.getCount()));
					} finally {
						lock1.unlock();
					}
				}
				end1.set(true);
			}
		};
		t1.start();

		Thread t2 = new Thread() {
			@Override
			public void run() {
				for (int i = 0; i < times; i++) {
					lock2.lock();
					try {
						obj1.setCount(obj1.getCount() - 1);
						obj2.setCount(obj2.getCount() + 1);
						assertThat(obj1.getCount(), is(-obj2.getCount()));
					} finally {
						lock2.unlock();
					}
				}
				end2.set(true);
			}
		};
		t2.start();

		while (true) {
			if (end1.get() && end2.get()) {
				break;
			}
			Thread.yield();
		}
		assertThat(obj1.getCount(), is(0));
		assertThat(obj2.getCount(), is(0));
	}
	
	/** 压力测试运行类 */
	class PressureRunner implements Runnable {
		private final ChainLock lock;
		private final AtomicBoolean end;
		private final int times;
		
		public PressureRunner(ChainLock lock, AtomicBoolean end, int times) {
			this.lock = lock;
			this.end = end;
			this.times = times;
		}
		
		@Override
		public void run() {
			for (int i = 0; i < times; i++) {
				lock.lock();
				try {
					Thread.yield();
				} finally {
					lock.unlock();
				}
			}
			end.set(true);
		}
	}
	
	@Test(timeout = 5000)
	public void test_pressure() {
		List<Object> objects = new ArrayList<Object>();
		{	// 填充数据
			objects.add(new String("1"));
			objects.add(new Integer(1));
			objects.add(new Long(1L));
			objects.add(new Byte((byte) 1));
			objects.add(new Short((short) 1));
			objects.add(new String("2"));
			objects.add(new Integer(2));
			objects.add(new Long(2L));
			objects.add(new Byte((byte) 2));
			objects.add(new Short((short) 2));
			objects.add(new Date());
			objects.add(new HashMap<Object, Object>());
			objects.add(new ArrayList<Object>());

			objects.add(new EntityString("s1"));
			objects.add(new EntityString("s2"));
			objects.add(new EntityString("s3"));
			objects.add(new EntityString("s1"));
			objects.add(new EntityInteger(1));
			objects.add(new EntityInteger(2));
			objects.add(new EntityInteger(3));
			objects.add(new EntityInteger(1));
			objects.add(new EntityLong(1L));
			objects.add(new EntityLong(2L));
			objects.add(new EntityLong(3L));
			objects.add(new EntityLong(1L));
		}
		
		int count = 500;	// 线程数
		int times = 10000;	// 每线程的执行数
		List<AtomicBoolean> ends = new ArrayList<AtomicBoolean>();
		
		int size = 3;
		for (int i = 0; i < count; i++) {
			Object[] objs = new Object[size];
			for (int j = 0; j < objs.length; j++) {
				int idx = (int) (Math.random() + 1) * size;
				objs[j] = objects.get(idx);
			}
			ChainLock lock = LockUtils.getLock(objs);
			AtomicBoolean end = new AtomicBoolean(false);
			Thread t = new Thread(new PressureRunner(lock, end, times));
			t.start();
			ends.add(end);
		}
		
		while(true) {
			boolean flag = true;
			for (AtomicBoolean end : ends) {
				flag &= end.get();
			}
			if (flag) {
				break;
			}
			Thread.yield();
		}
	}

}
