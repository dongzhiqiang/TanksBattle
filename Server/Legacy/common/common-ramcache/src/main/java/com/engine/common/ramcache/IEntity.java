package com.engine.common.ramcache;

import java.io.Serializable;

/**
 * 实体标识接口，用于告知锁创建器具体类实例是实体对象
 * 
 */
public interface IEntity<PK extends Serializable> {

	/**
	 * 获取实体标识
	 * @return
	 */
	PK getId();
	
}
