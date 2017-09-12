package com.engine.common.ramcache.service;

import java.io.Serializable;
import java.util.Collection;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.CachedEntityConfig;
import com.engine.common.ramcache.exception.InvaildEntityException;
import com.engine.common.ramcache.exception.UniqueFieldException;
import com.engine.common.ramcache.persist.Persister;

/**
 * 区域缓存服务接口
 * 
 *
 * @param <PK>
 * @param <T>
 */
public interface RegionCacheService<PK extends Comparable<PK> & Serializable, T extends IEntity<PK>> {

	/**
	 * 加载指定区域的实体集合
	 * @param idx 索引值
	 * @return 不可修改的列表
	 */
	Collection<T> load(IndexValue idx);
	
	/**
	 * 加载指定主键的实体(半异步)
	 * @param idx 索引值
	 * @param id 主键
	 * @param builder 实体不存在时的实体创建器，允许为null
	 * @return 不会返回null
	 * @throws InvaildEntityException 无法创建合法的实体时抛出
	 * @throws UniqueFieldException 实体的唯一属性域值重复时抛出
	 */
	T getOrCreate(IndexValue idx, PK id, EntityBuilder<PK, T> builder);

	/**
	 * 获取某一主键的实体
	 * @param idx
	 * @param id
	 * @return 实体不存在会返回null
	 */
	T get(IndexValue idx, PK id);
	
	/**
	 * 创建新的实体
	 * @param entity
	 * @return 被增强过的实体实例
	 */
	T create(T entity);
	
	/**
	 * 移除指定实体(异步)
	 * @param entity
	 */
	void remove(T entity);
	
	/**
	 * 清除指定的区域缓存
	 * @param idx
	 */
	void clear(IndexValue idx);
	
	/**
	 * 清理全部缓存数据
	 */
	void truncate();
	
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

	/**
	 * 获取对应的持久化处理器
	 * @return
	 */
	Persister getPersister();

}
