package com.engine.common.ramcache.orm;

import java.io.Serializable;
import java.util.Collection;
import java.util.Iterator;
import java.util.Map;
import java.util.Map.Entry;

import org.hibernate.Criteria;
import org.hibernate.Session;
import org.hibernate.criterion.Criterion;
import org.hibernate.criterion.Projections;
import org.hibernate.criterion.Restrictions;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.orm.exception.DataException;
import com.engine.common.ramcache.orm.exception.EntityExistsException;
import com.engine.common.ramcache.orm.exception.EntityNotFoundException;

/**
 * 物理存储层数据访问器接口<br/>
 * 用于给不同的ORM底层实现
 * 
 */
public interface Accessor {
	
	// CRUD 方法部分

	/**
	 * 从存储层加载指定的实体对象实例
	 * @param clz 实体类型
	 * @param id 实体主键
	 * @return 实体实例,不存在应该返回null
	 */
	<PK extends Serializable, T extends IEntity<PK>> T load(Class<T> clazz, PK id);

	/**
	 * 持久化指定的实体实例,并返回实体的主键值对象
	 * @param clz 实体类型
	 * @param entity 被持久化的实体实例(当持久化成功时,该实体的主键必须被设置为正确的主键值)
	 * @return 持久化实体的主键值对象
	 * @throws EntityExistsException 实体已经存在时抛出
	 * @throws DataException 实体数据不合法时抛出
	 */
	<PK extends Serializable, T extends IEntity<PK>> PK save(Class<T> clazz, T entity);

	/**
	 * 从存储层移除指定实体
	 * @param clz 实体类型
	 * @param id 实体主键
	 */
	<PK extends Serializable, T extends IEntity<PK>> void remove(Class<T> clazz, PK id);

	/**
	 * 从存储层移除指定实体
	 * @param clz 实体类型
	 * @param id 实体主键
	 */
	<PK extends Serializable, T extends IEntity<PK>> void remove(Class<T> clazz, T entity);

	
	/**
	 * 更新存储层的实体数据(不允许更新实体的主键值)
	 * @param entity 被更新实体对象实例
	 * @param clz 实体类型
	 * @throws EntityNotFoundException 被更新实体在存储层不存在时抛出
	 */
	<PK extends Serializable, T extends IEntity<PK>> T update(Class<T> clazz, T entity);
	
	/**
	 * 查询存储层的实体数据
	 * @param <PK>
	 * @param <T>
	 * @param clz 实体类型
	 * @param entities 填充的数据集合
	 * @param offset 查询偏移量
	 * @param size 查询数量
	 * @return 实体总数
	 */
	<PK extends Serializable, T extends IEntity<PK>> void listAll(Class<T> clazz, Collection<T> entities, Integer offset, Integer size);
	
	/**
	 * 按照指定条件交集(与查询)的方式查询存储层的实体数据
	 * @param <PK>
	 * @param <T>
	 * @param clz 实体类型
	 * @param entities 填充的数据集合
	 * @param keyValue 查询条件
	 * @param offset 查询偏移量
	 * @param size 总数大小
	 * @return 实体总数
	 */
	<PK extends Serializable, T extends IEntity<PK>> void listIntersection(Class<T> clazz, Collection<T> entities, Map<String, Object> keyValue, Integer offset, Integer size);
	
	/**
	 * 按照指定条件并集(或查询)的方式查询存储层的实体数据
	 * @param <PK>
	 * @param <T>
	 * @param clz 实体类型
	 * @param entities 填充的数据集合
	 * @param keyValue 查询条件
	 * @param offset 查询偏移量
	 * @param size 总数大小
	 * @return
	 */
	<PK extends Serializable, T extends IEntity<PK>> void listUnion(Class<T> clazz, Collection<T> entities, Map<String, Object> keyValue, Integer offset, Integer size);

	
	/**
	 * 获取实体记录总数(具体实现扩展方法)
	 * @param clz
	 * @return
	 */
	public <PK extends Serializable, T extends IEntity<PK>> long countAll(Class<T> clz);

	/**
	 * 获取实体记录交集总数
	 * @param clz
	 * @return
	 */
	public <PK extends Serializable, T extends IEntity<PK>> long countIntersection(
			Class<T> clz, Map<String, Object> keyValue);

	/**
	 * 获取实体记录并集总数
	 * @param clz
	 * @return
	 */
	public <PK extends Serializable, T extends IEntity<PK>> long countUnion(
			Class<T> clz, Map<String, Object> keyValue);
	
	/**
	 * 元信息方法
	 * @return
	 */
	Collection<EntityMetadata> getAllMetadata();
}
