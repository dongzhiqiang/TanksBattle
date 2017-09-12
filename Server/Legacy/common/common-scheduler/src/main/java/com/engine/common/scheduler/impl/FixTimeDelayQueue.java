package com.engine.common.scheduler.impl;

import java.util.AbstractQueue;
import java.util.Collection;
import java.util.Iterator;
import java.util.NoSuchElementException;
import java.util.PriorityQueue;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.DelayQueue;
import java.util.concurrent.Delayed;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.ReentrantLock;

/**
 * 带时间修正的延迟队列
 * @see DelayQueue
 * 
 */
@SuppressWarnings("rawtypes")
public class FixTimeDelayQueue<E extends Delayed> extends AbstractQueue<E> implements
		BlockingQueue<E> {

	private transient final ReentrantLock lock = new ReentrantLock();
	private transient final Condition available = lock.newCondition();

	private final PriorityQueue<E> q = new PriorityQueue<E>();

	/** 最大修正时间 */
	private final long delayTime;
	
	/**
	 * 构造方法
	 * @param delayTime 修正时间长度，单位毫秒
	 */
	public FixTimeDelayQueue(long delayTime) {
		this.delayTime = delayTime;
	}

	/**
	 * 构造方法
	 * @param delayTime 修正时间长度，单位毫秒
	 * @param c 初始化的元素集合
	 */
	public FixTimeDelayQueue(long delayTime, Collection<? extends E> c) {
		this.delayTime = delayTime;
		this.addAll(c);
	}
	
	// 修改的方法

	public E take() throws InterruptedException {
		final ReentrantLock lock = this.lock;
		lock.lockInterruptibly();
		try {
			for (;;) {
				E first = q.peek();
				if (first == null) {
					available.await();
				} else {
					long delay = first.getDelay(TimeUnit.MILLISECONDS);
					if (delay > 0) {
						// 修改前
						//long t1 = available.awaitNanos(delay);
						boolean flag = available.await(delay < delayTime ? delay : delayTime, TimeUnit.MILLISECONDS);
						if(!flag) {
							// 线程等待出错
							continue;
						}
					} else {
						E x = q.poll();
						assert x != null;
						if (q.size() != 0)
							available.signalAll(); // wake up other takers
						return x;
					}
				}
			}
		} finally {
			lock.unlock();
		}
	}

	public E poll(long timeout, TimeUnit unit) throws InterruptedException {
		final ReentrantLock lock = this.lock;
		lock.lockInterruptibly();
		long millis = unit.toMillis(timeout);
		try {
			for (;;) {
				E first = q.peek();
				if (first == null) {
					if (millis <= 0)
						return null;
					else {
						long s = System.currentTimeMillis();
						boolean flag = available.await(delayTime > millis ? millis : delayTime, TimeUnit.MILLISECONDS);
						millis -= System.currentTimeMillis() - s;
						if(!flag) {
							// 线程等待出错
							continue;
						}
					}
				} else {
					long delay = first.getDelay(TimeUnit.MILLISECONDS);
					if (delay > 0) {
						if (millis <= 0)
							return null;
						if (delay > millis)
							delay = millis;
						// 添加:开始
						if (delay > delayTime) {
							delay = delayTime;
						}
						// 添加:结束
						// 修改前
						// long timeLeft = available.awaitNanos(delay);
						// nanos -= delay - timeLeft;
						long s = System.currentTimeMillis();
						boolean flag = available.await(delay, TimeUnit.MILLISECONDS);
						millis -= System.currentTimeMillis() - s;
						if(!flag) {
							// 线程等待出错
							return null;
						}
					} else {
						E x = q.poll();
						assert x != null;
						if (q.size() != 0)
							available.signalAll();
						return x;
					}
				}
			}
		} finally {
			lock.unlock();
		}
	}

	// 和 DelayedQueue 一样的方法
	
	public boolean add(E e) {
		return offer(e);
	}

	public boolean offer(E e) {
		final ReentrantLock lock = this.lock;
		lock.lock();
		try {
			E first = q.peek();
			q.offer(e);
			if (first == null || e.compareTo(first) < 0)
				available.signalAll();
			return true;
		} finally {
			lock.unlock();
		}
	}

	public void put(E e) {
		offer(e);
	}

	public boolean offer(E e, long timeout, TimeUnit unit) {
		return offer(e);
	}

	public E poll() {
		final ReentrantLock lock = this.lock;
		lock.lock();
		try {
			E first = q.peek();
			if (first == null || first.getDelay(TimeUnit.NANOSECONDS) > 0)
				return null;
			else {
				E x = q.poll();
				assert x != null;
				if (q.size() != 0)
					available.signalAll();
				return x;
			}
		} finally {
			lock.unlock();
		}
	}

	public E peek() {
		final ReentrantLock lock = this.lock;
		lock.lock();
		try {
			return q.peek();
		} finally {
			lock.unlock();
		}
	}

	public int size() {
		final ReentrantLock lock = this.lock;
		lock.lock();
		try {
			return q.size();
		} finally {
			lock.unlock();
		}
	}

	public int drainTo(Collection<? super E> c) {
		if (c == null)
			throw new NullPointerException();
		if (c == this)
			throw new IllegalArgumentException();
		final ReentrantLock lock = this.lock;
		lock.lock();
		try {
			int n = 0;
			for (;;) {
				E first = q.peek();
				if (first == null || first.getDelay(TimeUnit.NANOSECONDS) > 0)
					break;
				c.add(q.poll());
				++n;
			}
			if (n > 0)
				available.signalAll();
			return n;
		} finally {
			lock.unlock();
		}
	}

	public int drainTo(Collection<? super E> c, int maxElements) {
		if (c == null)
			throw new NullPointerException();
		if (c == this)
			throw new IllegalArgumentException();
		if (maxElements <= 0)
			return 0;
		final ReentrantLock lock = this.lock;
		lock.lock();
		try {
			int n = 0;
			while (n < maxElements) {
				E first = q.peek();
				if (first == null || first.getDelay(TimeUnit.NANOSECONDS) > 0)
					break;
				c.add(q.poll());
				++n;
			}
			if (n > 0)
				available.signalAll();
			return n;
		} finally {
			lock.unlock();
		}
	}

	public void clear() {
		final ReentrantLock lock = this.lock;
		lock.lock();
		try {
			q.clear();
		} finally {
			lock.unlock();
		}
	}

	public int remainingCapacity() {
		return Integer.MAX_VALUE;
	}

	public Object[] toArray() {
		final ReentrantLock lock = this.lock;
		lock.lock();
		try {
			return q.toArray();
		} finally {
			lock.unlock();
		}
	}

	public <T> T[] toArray(T[] a) {
		final ReentrantLock lock = this.lock;
		lock.lock();
		try {
			return q.toArray(a);
		} finally {
			lock.unlock();
		}
	}

	public boolean remove(Object o) {
		final ReentrantLock lock = this.lock;
		lock.lock();
		try {
			return q.remove(o);
		} finally {
			lock.unlock();
		}
	}

	public Iterator<E> iterator() {
		return new Itr(toArray());
	}

	private class Itr implements Iterator<E> {
		final Object[] array; // Array of all elements
		int cursor; // index of next element to return;
		int lastRet; // index of last element, or -1 if no such

		Itr(Object[] array) {
			lastRet = -1;
			this.array = array;
		}

		public boolean hasNext() {
			return cursor < array.length;
		}

		@SuppressWarnings("unchecked")
		public E next() {
			if (cursor >= array.length)
				throw new NoSuchElementException();
			lastRet = cursor;
			return (E) array[cursor++];
		}

		public void remove() {
			if (lastRet < 0)
				throw new IllegalStateException();
			Object x = array[lastRet];
			lastRet = -1;
			lock.lock();
			try {
				for (Iterator it = q.iterator(); it.hasNext();) {
					if (it.next() == x) {
						it.remove();
						return;
					}
				}
			} finally {
				lock.unlock();
			}
		}
	}
}
