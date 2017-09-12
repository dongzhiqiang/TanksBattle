package com.engine.common.scheduler.impl;

import java.util.AbstractCollection;
import java.util.Collection;
import java.util.Iterator;
import java.util.List;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.Callable;
import java.util.concurrent.Delayed;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.FutureTask;
import java.util.concurrent.RejectedExecutionException;
import java.util.concurrent.RunnableScheduledFuture;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.ScheduledFuture;
import java.util.concurrent.ScheduledThreadPoolExecutor;
import java.util.concurrent.ThreadFactory;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicLong;

/**
 * 修改过的 ScheduledThreadPoolExecutor
 * @see ScheduledThreadPoolExecutor
 * 
 */
public class FixScheduledThreadPoolExecutor extends FixThreadPoolExecutor implements
		ScheduledExecutorService {

	public FixScheduledThreadPoolExecutor(long delayTime) {
		super(1, Integer.MAX_VALUE, 0, TimeUnit.NANOSECONDS, new DelayedWorkQueue(delayTime));
	}

	public FixScheduledThreadPoolExecutor(Integer poolSize, Long delayTime) {
		super(poolSize, Integer.MAX_VALUE, 0, TimeUnit.NANOSECONDS, new DelayedWorkQueue(delayTime));
	}

	public FixScheduledThreadPoolExecutor(Integer poolSize, Long delayTime, ThreadFactory threadFactory) {
		super(poolSize, Integer.MAX_VALUE, 0, TimeUnit.NANOSECONDS, new DelayedWorkQueue(delayTime), threadFactory);
	}

	/**
	 * False if should cancel/suppress periodic tasks on shutdown.
	 */
	private volatile boolean continueExistingPeriodicTasksAfterShutdown;

	/**
	 * False if should cancel non-periodic tasks on shutdown.
	 */
	private volatile boolean executeExistingDelayedTasksAfterShutdown = true;

	/**
	 * Sequence number to break scheduling ties, and in turn to guarantee FIFO
	 * order among tied entries.
	 */
	private static final AtomicLong sequencer = new AtomicLong(0);

	/**
	 * Returns millisecond time
	 */
	final long now() {
		return System.currentTimeMillis();
	}

	private class ScheduledFutureTask<V> extends FutureTask<V> implements
			RunnableScheduledFuture<V> {

		/** Sequence number to break ties FIFO */
		private final long sequenceNumber;
		/** The time the task is enabled to execute in nanoTime units */
		private long time;
		/**
		 * Period in nanoseconds for repeating tasks. A positive value indicates
		 * fixed-rate execution. A negative value indicates fixed-delay
		 * execution. A value of 0 indicates a non-repeating task.
		 */
		private final long period;

		/**
		 * Creates a one-shot action with given nanoTime-based trigger time.
		 */
		ScheduledFutureTask(Runnable r, V result, long ns) {
			super(r, result);
			this.time = ns;
			this.period = 0;
			this.sequenceNumber = sequencer.getAndIncrement();
		}

		/**
		 * Creates a periodic action with given nano time and period.
		 */
		ScheduledFutureTask(Runnable r, V result, long ns, long period) {
			super(r, result);
			this.time = ns;
			this.period = period;
			this.sequenceNumber = sequencer.getAndIncrement();
		}

		/**
		 * Creates a one-shot action with given nanoTime-based trigger.
		 */
		ScheduledFutureTask(Callable<V> callable, long ns) {
			super(callable);
			this.time = ns;
			this.period = 0;
			this.sequenceNumber = sequencer.getAndIncrement();
		}

		public long getDelay(TimeUnit unit) {
			long d = unit.convert(time - now(), TimeUnit.MILLISECONDS);
			return d;
		}

		public int compareTo(Delayed other) {
			if (other == this) // compare zero ONLY if same object
				return 0;
			if (other instanceof ScheduledFutureTask) {
				ScheduledFutureTask<?> x = (ScheduledFutureTask<?>) other;
				long diff = time - x.time;
				if (diff < 0)
					return -1;
				else if (diff > 0)
					return 1;
				else if (sequenceNumber < x.sequenceNumber)
					return -1;
				else
					return 1;
			}
			long d = (getDelay(TimeUnit.NANOSECONDS) - other.getDelay(TimeUnit.NANOSECONDS));
			return (d == 0) ? 0 : ((d < 0) ? -1 : 1);
		}
		
		@Override
		public boolean equals(Object obj) {
			return (this == obj);
		}

		/**
		 * Returns true if this is a periodic (not a one-shot) action.
		 * @return true if periodic
		 */
		public boolean isPeriodic() {
			return period != 0;
		}

		/**
		 * Runs a periodic task.
		 */
		private void runPeriodic() {
			boolean ok = ScheduledFutureTask.super.runAndReset();
			boolean down = isShutdown();
			// Reschedule if not cancelled and not shutdown or policy allows
			if (ok
					&& (!down || (getContinueExistingPeriodicTasksAfterShutdownPolicy() && !isStopped()))) {
				long p = period;
				if (p > 0)
					time += p;
				else
					time = now() - p;
				FixScheduledThreadPoolExecutor.super.getQueue().add(this);
			}
			// This might have been the final executed delayed
			// task. Wake up threads to check.
			else if (down)
				interruptIdleWorkers();
		}

		/**
		 * Overrides FutureTask version so as to reset/requeue if periodic.
		 */
		public void run() {
			if (isPeriodic())
				runPeriodic();
			else
				ScheduledFutureTask.super.run();
		}
	}

	/**
	 * Specialized variant of ThreadPoolExecutor.execute for delayed tasks.
	 */
	private void delayedExecute(Runnable command) {
		if (isShutdown()) {
			reject(command);
			return;
		}
		// Prestart a thread if necessary. We cannot prestart it
		// running the task because the task (probably) shouldn't be
		// run yet, so thread will just idle until delay elapses.
		if (getPoolSize() < getCorePoolSize())
			prestartCoreThread();

		super.getQueue().add(command);
	}

	/**
	 * Cancels and clears the queue of all tasks that should not be run due to
	 * shutdown policy.
	 */
	private void cancelUnwantedTasks() {
		boolean keepDelayed = getExecuteExistingDelayedTasksAfterShutdownPolicy();
		boolean keepPeriodic = getContinueExistingPeriodicTasksAfterShutdownPolicy();
		if (!keepDelayed && !keepPeriodic)
			super.getQueue().clear();
		else if (keepDelayed || keepPeriodic) {
			Object[] entries = super.getQueue().toArray();
			for (int i = 0; i < entries.length; ++i) {
				Object e = entries[i];
				if (e instanceof RunnableScheduledFuture) {
					RunnableScheduledFuture<?> t = (RunnableScheduledFuture<?>) e;
					if (t.isPeriodic() ? !keepPeriodic : !keepDelayed)
						t.cancel(false);
				}
			}
			purge();
		}
	}

	public boolean remove(Runnable task) {
		if (!(task instanceof RunnableScheduledFuture))
			return false;
		return getQueue().remove(task);
	}

	/**
	 * Modifies or replaces the task used to execute a runnable. This method can
	 * be used to override the concrete class used for managing internal tasks.
	 * The default implementation simply returns the given task.
	 * @param runnable the submitted Runnable
	 * @param task the task created to execute the runnable
	 * @return a task that can execute the runnable
	 * @since 1.6
	 */
	protected <V> RunnableScheduledFuture<V> decorateTask(Runnable runnable,
			RunnableScheduledFuture<V> task) {
		return task;
	}

	/**
	 * Modifies or replaces the task used to execute a callable. This method can
	 * be used to override the concrete class used for managing internal tasks.
	 * The default implementation simply returns the given task.
	 * @param callable the submitted Callable
	 * @param task the task created to execute the callable
	 * @return a task that can execute the callable
	 * @since 1.6
	 */
	protected <V> RunnableScheduledFuture<V> decorateTask(Callable<V> callable,
			RunnableScheduledFuture<V> task) {
		return task;
	}

	public ScheduledFuture<?> schedule(Runnable command, long delay, TimeUnit unit) {
		if (command == null || unit == null)
			throw new NullPointerException();
		if (delay < 0)
			delay = 0;
		long triggerTime = now() + unit.toMillis(delay);
		RunnableScheduledFuture<?> t = decorateTask(command, new ScheduledFutureTask<Boolean>(
				command, null, triggerTime));
		delayedExecute(t);
		return t;
	}

	public <V> ScheduledFuture<V> schedule(Callable<V> callable, long delay, TimeUnit unit) {
		if (callable == null || unit == null)
			throw new NullPointerException();
		if (delay < 0)
			delay = 0;
		long triggerTime = now() + unit.toMillis(delay);
		RunnableScheduledFuture<V> t = decorateTask(callable, new ScheduledFutureTask<V>(callable,
				triggerTime));
		delayedExecute(t);
		return t;
	}

	public ScheduledFuture<?> scheduleAtFixedRate(Runnable command, long initialDelay, long period,
			TimeUnit unit) {
		if (command == null || unit == null)
			throw new NullPointerException();
		if (period <= 0)
			throw new IllegalArgumentException();
		if (initialDelay < 0)
			initialDelay = 0;
		long triggerTime = now() + unit.toMillis(initialDelay);
		RunnableScheduledFuture<?> t = decorateTask(command, new ScheduledFutureTask<Object>(
				command, null, triggerTime, unit.toMillis(period)));
		delayedExecute(t);
		return t;
	}

	public ScheduledFuture<?> scheduleWithFixedDelay(Runnable command, long initialDelay,
			long delay, TimeUnit unit) {
		if (command == null || unit == null)
			throw new NullPointerException();
		if (delay <= 0)
			throw new IllegalArgumentException();
		if (initialDelay < 0)
			initialDelay = 0;
		long triggerTime = now() + unit.toMillis(initialDelay);
		RunnableScheduledFuture<?> t = decorateTask(command, new ScheduledFutureTask<Boolean>(
				command, null, triggerTime, unit.toMillis(-delay)));
		delayedExecute(t);
		return t;
	}

	/**
	 * Executes command with zero required delay. This has effect equivalent to
	 * <tt>schedule(command, 0, anyUnit)</tt>. Note that inspections of the
	 * queue and of the list returned by <tt>shutdownNow</tt> will access the
	 * zero-delayed {@link ScheduledFuture}, not the <tt>command</tt> itself.
	 * @param command the task to execute
	 * @throws RejectedExecutionException at discretion of
	 * <tt>RejectedExecutionHandler</tt>, if task cannot be accepted for
	 * execution because the executor has been shut down.
	 * @throws NullPointerException if command is null
	 */
	public void execute(Runnable command) {
		if (command == null)
			throw new NullPointerException();
		schedule(command, 0, TimeUnit.MILLISECONDS);
	}

	// Override AbstractExecutorService methods

	public Future<?> submit(Runnable task) {
		return schedule(task, 0, TimeUnit.MILLISECONDS);
	}

	public <T> Future<T> submit(Runnable task, T result) {
		return schedule(Executors.callable(task, result), 0, TimeUnit.MILLISECONDS);
	}

	public <T> Future<T> submit(Callable<T> task) {
		return schedule(task, 0, TimeUnit.MILLISECONDS);
	}

	/**
	 * Sets the policy on whether to continue executing existing periodic tasks
	 * even when this executor has been <tt>shutdown</tt>. In this case, these
	 * tasks will only terminate upon <tt>shutdownNow</tt>, or after setting the
	 * policy to <tt>false</tt> when already shutdown. This value is by default
	 * false.
	 * @param value if true, continue after shutdown, else don't.
	 * @see #getContinueExistingPeriodicTasksAfterShutdownPolicy
	 */
	public void setContinueExistingPeriodicTasksAfterShutdownPolicy(boolean value) {
		continueExistingPeriodicTasksAfterShutdown = value;
		if (!value && isShutdown())
			cancelUnwantedTasks();
	}

	/**
	 * Gets the policy on whether to continue executing existing periodic tasks
	 * even when this executor has been <tt>shutdown</tt>. In this case, these
	 * tasks will only terminate upon <tt>shutdownNow</tt> or after setting the
	 * policy to <tt>false</tt> when already shutdown. This value is by default
	 * false.
	 * @return true if will continue after shutdown
	 * @see #setContinueExistingPeriodicTasksAfterShutdownPolicy
	 */
	public boolean getContinueExistingPeriodicTasksAfterShutdownPolicy() {
		return continueExistingPeriodicTasksAfterShutdown;
	}

	/**
	 * Sets the policy on whether to execute existing delayed tasks even when
	 * this executor has been <tt>shutdown</tt>. In this case, these tasks will
	 * only terminate upon <tt>shutdownNow</tt>, or after setting the policy to
	 * <tt>false</tt> when already shutdown. This value is by default true.
	 * @param value if true, execute after shutdown, else don't.
	 * @see #getExecuteExistingDelayedTasksAfterShutdownPolicy
	 */
	public void setExecuteExistingDelayedTasksAfterShutdownPolicy(boolean value) {
		executeExistingDelayedTasksAfterShutdown = value;
		if (!value && isShutdown())
			cancelUnwantedTasks();
	}

	/**
	 * Gets the policy on whether to execute existing delayed tasks even when
	 * this executor has been <tt>shutdown</tt>. In this case, these tasks will
	 * only terminate upon <tt>shutdownNow</tt>, or after setting the policy to
	 * <tt>false</tt> when already shutdown. This value is by default true.
	 * @return true if will execute after shutdown
	 * @see #setExecuteExistingDelayedTasksAfterShutdownPolicy
	 */
	public boolean getExecuteExistingDelayedTasksAfterShutdownPolicy() {
		return executeExistingDelayedTasksAfterShutdown;
	}

	/**
	 * Initiates an orderly shutdown in which previously submitted tasks are
	 * executed, but no new tasks will be accepted. If the
	 * <tt>ExecuteExistingDelayedTasksAfterShutdownPolicy</tt> has been set
	 * <tt>false</tt>, existing delayed tasks whose delays have not yet elapsed
	 * are cancelled. And unless the
	 * <tt>ContinueExistingPeriodicTasksAfterShutdownPolicy</tt> has been set
	 * <tt>true</tt>, future executions of existing periodic tasks will be
	 * cancelled.
	 */
	public void shutdown() {
		cancelUnwantedTasks();
		super.shutdown();
	}

	/**
	 * Attempts to stop all actively executing tasks, halts the processing of
	 * waiting tasks, and returns a list of the tasks that were awaiting
	 * execution.
	 * <p>
	 * There are no guarantees beyond best-effort attempts to stop processing
	 * actively executing tasks. This implementation cancels tasks via
	 * {@link Thread#interrupt}, so any task that fails to respond to interrupts
	 * may never terminate.
	 * @return list of tasks that never commenced execution. Each element of
	 * this list is a {@link ScheduledFuture}, including those tasks submitted
	 * using <tt>execute</tt>, which are for scheduling purposes used as the
	 * basis of a zero-delay <tt>ScheduledFuture</tt>.
	 * @throws SecurityException {@inheritDoc}
	 */
	public List<Runnable> shutdownNow() {
		return super.shutdownNow();
	}

	/**
	 * Returns the task queue used by this executor. Each element of this queue
	 * is a {@link ScheduledFuture}, including those tasks submitted using
	 * <tt>execute</tt> which are for scheduling purposes used as the basis of a
	 * zero-delay <tt>ScheduledFuture</tt>. Iteration over this queue is
	 * <em>not</em> guaranteed to traverse tasks in the order in which they will
	 * execute.
	 * @return the task queue
	 */
	public BlockingQueue<Runnable> getQueue() {
		return super.getQueue();
	}

	/**
	 * An annoying wrapper class to convince javac to use a
	 * DelayQueue<RunnableScheduledFuture> as a BlockingQueue<Runnable>
	 */
	@SuppressWarnings("rawtypes")
	private static class DelayedWorkQueue extends AbstractCollection<Runnable> implements
			BlockingQueue<Runnable> {

		private final FixTimeDelayQueue<RunnableScheduledFuture> dq;

		public DelayedWorkQueue(long delayTime) {
			dq = new FixTimeDelayQueue<RunnableScheduledFuture>(delayTime);
		}

		public Runnable poll() {
			return dq.poll();
		}

		public Runnable peek() {
			return dq.peek();
		}

		public Runnable take() throws InterruptedException {
			return dq.take();
		}

		public Runnable poll(long timeout, TimeUnit unit) throws InterruptedException {
			return dq.poll(timeout, unit);
		}

		public boolean add(Runnable x) {
			return dq.add((RunnableScheduledFuture) x);
		}

		public boolean offer(Runnable x) {
			return dq.offer((RunnableScheduledFuture) x);
		}

		public void put(Runnable x) {
			dq.put((RunnableScheduledFuture) x);
		}

		public boolean offer(Runnable x, long timeout, TimeUnit unit) {
			return dq.offer((RunnableScheduledFuture) x, timeout, unit);
		}

		public Runnable remove() {
			return dq.remove();
		}

		public Runnable element() {
			return dq.element();
		}

		public void clear() {
			dq.clear();
		}

		public int drainTo(Collection<? super Runnable> c) {
			return dq.drainTo(c);
		}

		public int drainTo(Collection<? super Runnable> c, int maxElements) {
			return dq.drainTo(c, maxElements);
		}

		public int remainingCapacity() {
			return dq.remainingCapacity();
		}

		public boolean remove(Object x) {
			return dq.remove(x);
		}

		public boolean contains(Object x) {
			return dq.contains(x);
		}

		public int size() {
			return dq.size();
		}

		public boolean isEmpty() {
			return dq.isEmpty();
		}

		public Object[] toArray() {
			return dq.toArray();
		}

		public <T> T[] toArray(T[] array) {
			return dq.toArray(array);
		}

		public Iterator<Runnable> iterator() {
			return new Iterator<Runnable>() {
				private Iterator<RunnableScheduledFuture> it = dq.iterator();

				public boolean hasNext() {
					return it.hasNext();
				}

				public Runnable next() {
					return it.next();
				}

				public void remove() {
					it.remove();
				}
			};
		}
	}
}
