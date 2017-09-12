package com.engine.common.ramcache.service;

import java.io.Serializable;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.CachedEntityConfig;

/**
 * 增强服务接口
 * 
 *
 * @param <PK>
 * @param <T>
 */
public interface EnhanceService<PK extends Comparable<PK> & Serializable, T extends IEntity<PK>> {

	/**
	 * 将缓存中的指定实体回写到存储层(异步)
	 * @param id 主键
	 * @param T 回写实体实例
	 */
	void writeBack(PK id, T entity);

	/**
	 * 获取实体缓存配置信息
	 * @return
	 */
	CachedEntityConfig getEntityConfig();

}
