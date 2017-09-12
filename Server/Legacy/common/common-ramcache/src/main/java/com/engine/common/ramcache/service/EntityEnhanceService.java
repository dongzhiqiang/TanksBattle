package com.engine.common.ramcache.service;

import java.io.Serializable;

import com.engine.common.ramcache.IEntity;

/**
 * 实体增强服务接口
 * 
 *
 * @param <PK>
 * @param <T>
 */
public interface EntityEnhanceService<PK extends Comparable<PK> & Serializable, T extends IEntity<PK>> extends EnhanceService<PK, T> {

	/**
	 * 检查指定的唯一属性域值是否存在
	 * @param name 属性域名
	 * @param value 属性值
	 * @return
	 */
	boolean hasUniqueValue(String name, Object value);

	/**
	 * 替换指定实体标识对应的唯一属性缓存值
	 * @param id 主键
	 * @param name 属性域名
	 * @param value 最新值
	 */
	void replaceUniqueValue(PK id, String name, Object value);

}
