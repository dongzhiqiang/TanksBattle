package com.engine.common.scheduler;

import java.util.Date;

/**
 * 触发器接口，用于确定下次任务的执行时间
 */
public interface Trigger {

	/**
	 * 获取下次执行时间
	 * @param triggerContext
	 * @return
	 */
	Date nextTime(TaskContext triggerContext);

}
