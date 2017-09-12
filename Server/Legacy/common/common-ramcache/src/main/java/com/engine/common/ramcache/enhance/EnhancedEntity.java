package com.engine.common.ramcache.enhance;

import com.engine.common.ramcache.IEntity;

/**
 * 被增强过的实体必须实现该接口
 * 
 */
public interface EnhancedEntity {

	/**
	 * 获取被增强前的实体对象
	 * @return
	 */
	@SuppressWarnings("rawtypes")
	IEntity getEntity();
}
