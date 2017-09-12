package com.engine.common.event;

import java.util.List;

import javax.management.MXBean;

/**
 * 事件总线的JMX管理接口
 * 
 */
@MXBean
public interface EventBusImplMBean {
	
	/**
	 * 当前事件队列大小
	 */
	int getEventQueueSize();
	
	/**
	 * 当前事件池队列大小
	 */
	int getPollQueueSize();
	
	/**
	 * 池正在执行的线程数
	 */
	int getPoolActiveCount();
	
	/**
	 * 获取事件队列中的事件名列表
	 * @return
	 */
	List<String> getEvents();
	
}
