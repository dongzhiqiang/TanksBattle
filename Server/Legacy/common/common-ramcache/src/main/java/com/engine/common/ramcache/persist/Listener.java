package com.engine.common.ramcache.persist;

import java.io.Serializable;

import com.engine.common.ramcache.IEntity;

/**
 * 队列更新结果监听器
 * 
 */
public interface Listener {

	/**
	 * 队列更新的回调
	 * @param type 更新类型
	 * @param isSuccess 是否成功
	 * @param id 主键
	 * @param entity 实体(可能为null)
	 * @param ex 失败时的异常原因
	 */
	@SuppressWarnings("rawtypes")
	void notify(EventType type, boolean isSuccess, Serializable id, IEntity entity, RuntimeException ex);
}
