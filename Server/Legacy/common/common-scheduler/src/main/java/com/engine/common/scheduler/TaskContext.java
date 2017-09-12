package com.engine.common.scheduler;

import java.util.Date;

/**
 * 任务上下文对象，封装任务的计划时间和完成时间
 * 
 */
public interface TaskContext {

	/**
	 * 返回最后排定的任务执行时间，或者为null，如果没有安排过。
	 * @return
	 */
	Date lastScheduledTime();

	/**
	 * 返回的最后一个任务的实际执行时间，或者为null，如果没有安排过。
	 * @return
	 */
	Date lastActualTime();

	/**
	 * 返回该任务的最后完成时间，或NULL如果没有安排过。
	 * @return
	 */
	Date lastCompletionTime();

}
