package com.engine.common.ramcache.service;

import java.io.Serializable;
import java.util.Comparator;
import java.util.List;
import java.util.Set;

import com.engine.common.ramcache.IEntity;

/**
 * 缓存内容的查找器
 * 
 *
 * @param <PK>
 * @param <T>
 */
public interface CacheFinder<PK extends Comparable<PK> & Serializable, T extends IEntity<PK>> {

	/**
	 * 获取符合条件的缓存实体集合
	 * @param filter 实体过滤器
	 * @return 不会返回null
	 */
	Set<T> find(Filter<T> filterter);
	
	/**
	 * 将全部实体按指定排序规则排序并返回
	 * @param comparator 排序器
	 * @return 不会返回null
	 */
	List<T> sort(Comparator<T> comparator);
	
	/**
	 * 获取符合条件的缓存实体的有序集合
	 * @param filter 实体过滤器
	 * @param comparator 排序器
	 * @return 不会返回null
	 */
	List<T> find(Filter<T> filter, Comparator<T> comparator);
	
	/**
	 * 获取缓存中的全部实体集合
	 * @return
	 */
	Set<T> all();
}
