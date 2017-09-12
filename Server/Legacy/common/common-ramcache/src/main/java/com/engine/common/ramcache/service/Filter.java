package com.engine.common.ramcache.service;

import com.engine.common.ramcache.IEntity;

/**
 * 查询结果过滤器
 * 
 *
 * @param <T>
 */
@SuppressWarnings("rawtypes")
public interface Filter<T extends IEntity> {

	/**
	 * 检查是否排除该实体
	 * @param entity 被检查的实体，不会为null
	 * @return true:排除被检查的实体(不会出现在返回结果中),false:不排除被检查的实体(会出现在返回结果中)
	 */
	boolean isExclude(T entity);
}
