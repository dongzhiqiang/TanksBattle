package com.engine.common.scheduler.impl;

/**
 * 无法由 FixThreadPoolExecutor 执行的任务的处理程序。<br/>
 * 修改 ThreadPoolExecutor 的附带产物
 * 
 */
public interface RejectedExecutionHandler {

	/**
	 * 当 execute 不能接受某个任务时，可以由 ThreadPoolExecutor
	 * 调用的方法。因为超出其界限而没有更多可用的线程或队列槽时，或者关闭 Executor 时就可能发生这种情况。
	 * 在没有其他替代方法的情况下，该方法可能抛出未经检查的 RejectedExecutionException，而该异常将传播到 execute
	 * 的调用者。
	 * @param r 所请求执行的可运行任务。
	 * @param executor 试图执行此任务的执行程序。
	 */
	void rejectedExecution(Runnable r, FixThreadPoolExecutor executor);
}
