package com.engine.common.ramcache.lock;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.concurrent.locks.Lock;

import org.junit.Test;

import com.engine.common.ramcache.lock.ObjectLock;

public class ObjectLockTest {

	/**
	 * 测试非实体对象锁排序
	 */
	@Test
	public void test_object() {
		{ // 同一类型测试
			List<ObjectLock> locks = new ArrayList<ObjectLock>();
			Collections.addAll(locks, new ObjectLock("1"), new ObjectLock("5"), new ObjectLock("3"),
					new ObjectLock("7"));
			Collections.sort(locks);
			List<Integer> sorts = new ArrayList<Integer>();
			Collections.addAll(sorts, System.identityHashCode("1"), System.identityHashCode("5"),
					System.identityHashCode("3"), System.identityHashCode("7"));
			Collections.sort(sorts);

			assertThat((Integer) locks.get(0).getValue(), is(sorts.get(0)));
			assertThat((Integer) locks.get(1).getValue(), is(sorts.get(1)));
			assertThat((Integer) locks.get(2).getValue(), is(sorts.get(2)));
			assertThat((Integer) locks.get(3).getValue(), is(sorts.get(3)));
		}

		{ // 不同类型测试
			List<ObjectLock> locks = new ArrayList<ObjectLock>();
			Collections.addAll(locks, new ObjectLock("1"), new ObjectLock(2), new ObjectLock(3L), new ObjectLock(4.0));
			Collections.sort(locks);
			List<Integer> sorts = new ArrayList<Integer>();
			Collections.addAll(sorts, String.class.hashCode(), Integer.class.hashCode(), Long.class.hashCode(),
					Double.class.hashCode());
			Collections.sort(sorts);

			assertThat((Integer) locks.get(0).getClz().hashCode(), is(sorts.get(0)));
			assertThat((Integer) locks.get(1).getClz().hashCode(), is(sorts.get(1)));
			assertThat((Integer) locks.get(2).getClz().hashCode(), is(sorts.get(2)));
			assertThat((Integer) locks.get(3).getClz().hashCode(), is(sorts.get(3)));
		}

		{
			// TODO 类型 hashCode 相同的测试
		}
	}

	/**
	 * 测试实体对象锁排序
	 */
	@Test
	public void test_entity() {

		{ // 同一类型测试
			List<ObjectLock> locks = new ArrayList<ObjectLock>();
			Collections.addAll(locks, new ObjectLock(new EntityString("1")), new ObjectLock(new EntityString("7")),
					new ObjectLock(new EntityString("5")), new ObjectLock(new EntityString("3")));
			Collections.sort(locks);
			List<String> sorts = new ArrayList<String>();
			Collections.addAll(sorts, "1", "5", "3", "7");
			Collections.sort(sorts);

			assertThat((String) locks.get(0).getValue(), is(sorts.get(0)));
			assertThat((String) locks.get(1).getValue(), is(sorts.get(1)));
			assertThat((String) locks.get(2).getValue(), is(sorts.get(2)));
			assertThat((String) locks.get(3).getValue(), is(sorts.get(3)));
		}

		{ // 不同类型的测试
			List<ObjectLock> locks = new ArrayList<ObjectLock>();
			Collections.addAll(locks, new ObjectLock(new EntityString("1")), new ObjectLock(new EntityInteger(2)),
					new ObjectLock(new EntityLong(3L)), new ObjectLock(new EntityDouble(4.0)));
			Collections.sort(locks);
			List<Integer> sorts = new ArrayList<Integer>();
			Collections.addAll(sorts, EntityString.class.hashCode(), EntityInteger.class.hashCode(),
					EntityLong.class.hashCode(), EntityDouble.class.hashCode());
			Collections.sort(sorts);

			assertThat((Integer) locks.get(0).getClz().hashCode(), is(sorts.get(0)));
			assertThat((Integer) locks.get(1).getClz().hashCode(), is(sorts.get(1)));
			assertThat((Integer) locks.get(2).getClz().hashCode(), is(sorts.get(2)));
			assertThat((Integer) locks.get(3).getClz().hashCode(), is(sorts.get(3)));
		}
	}

	/**
	 * 测试实体与非实体混合
	 */
	@Test
	public void test_mix_entity_and_object() {
		{ // 不同类型测试
			List<ObjectLock> locks = new ArrayList<ObjectLock>();
			Collections.addAll(locks, new ObjectLock("1"), new ObjectLock(2), new ObjectLock(3L), new ObjectLock(4.0),
					new ObjectLock(new EntityString("1")), new ObjectLock(new EntityInteger(2)), new ObjectLock(
							new EntityLong(3L)), new ObjectLock(new EntityDouble(4.0)));
			Collections.sort(locks);
			List<Integer> sortsOne = new ArrayList<Integer>();
			Collections.addAll(sortsOne, String.class.hashCode(), Integer.class.hashCode(), Long.class.hashCode(),
					Double.class.hashCode());
			Collections.sort(sortsOne);
			List<Integer> sortsTwo = new ArrayList<Integer>();
			Collections.addAll(sortsTwo, EntityString.class.hashCode(), EntityInteger.class.hashCode(),
					EntityLong.class.hashCode(), EntityDouble.class.hashCode());
			Collections.sort(sortsTwo);

			assertThat((Integer) locks.get(0).getClz().hashCode(), is(sortsOne.get(0)));
			assertThat((Integer) locks.get(1).getClz().hashCode(), is(sortsOne.get(1)));
			assertThat((Integer) locks.get(2).getClz().hashCode(), is(sortsOne.get(2)));
			assertThat((Integer) locks.get(3).getClz().hashCode(), is(sortsOne.get(3)));
			assertThat((Integer) locks.get(4).getClz().hashCode(), is(sortsTwo.get(0)));
			assertThat((Integer) locks.get(5).getClz().hashCode(), is(sortsTwo.get(1)));
			assertThat((Integer) locks.get(6).getClz().hashCode(), is(sortsTwo.get(2)));
			assertThat((Integer) locks.get(7).getClz().hashCode(), is(sortsTwo.get(3)));
		}

		{ // 不同类型混合同类型测试
			List<ObjectLock> locks = new ArrayList<ObjectLock>();
			Collections.addAll(locks, new ObjectLock("1"), new ObjectLock("5"), new ObjectLock("3"),
					new ObjectLock("7"), new ObjectLock(new EntityString("1")), new ObjectLock(new EntityString("7")),
					new ObjectLock(new EntityString("5")), new ObjectLock(new EntityString("3")));
			Collections.sort(locks);
			List<Integer> sortsOne = new ArrayList<Integer>();
			Collections.addAll(sortsOne, System.identityHashCode("1"), System.identityHashCode("5"),
					System.identityHashCode("3"), System.identityHashCode("7"));
			Collections.sort(sortsOne);
			List<String> sortsTwo = new ArrayList<String>();
			Collections.addAll(sortsTwo, "1", "5", "3", "7");
			Collections.sort(sortsTwo);

			assertThat((Integer) locks.get(0).getValue().hashCode(), is(sortsOne.get(0)));
			assertThat((Integer) locks.get(1).getValue().hashCode(), is(sortsOne.get(1)));
			assertThat((Integer) locks.get(2).getValue().hashCode(), is(sortsOne.get(2)));
			assertThat((Integer) locks.get(3).getValue().hashCode(), is(sortsOne.get(3)));
			assertThat((String) locks.get(4).getValue(), is(sortsTwo.get(0)));
			assertThat((String) locks.get(5).getValue(), is(sortsTwo.get(1)));
			assertThat((String) locks.get(6).getValue(), is(sortsTwo.get(2)));
			assertThat((String) locks.get(7).getValue(), is(sortsTwo.get(3)));
		}
	}

	private class TestRunner implements Runnable {

		private Integer count = 0;
		private Lock lock;
		private final int times;

		public TestRunner(Lock lock, int times) {
			this.lock = lock;
			this.times = times;
		}

		@Override
		public void run() {
			for (int i = 0; i < times; i++) {
				lock.lock();
				try {
					count++;
				} finally {
					lock.unlock();
				}
			}
		}

		public Integer getCount() {
			return count;
		}

		public void setCount(Integer count) {
			this.count = count;
		}
	}

	@Test
	public void test_lock() throws InterruptedException {
		ObjectLock lock = new ObjectLock(1);
		int times = 100000;

		TestRunner runner = new TestRunner(lock, times);
		Thread t = new Thread(runner);
		t.start();

		for (int i = 0; i < times; i++) {
			lock.lock();
			try {
				int count = runner.getCount();
				count--;
				runner.setCount(count);
			} finally {
				lock.unlock();
			}
		}

		Thread.sleep(1000);
		assertThat(runner.getCount(), is(0));
	}
}
