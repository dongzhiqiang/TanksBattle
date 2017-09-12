package com.engine.common.ramcache.orm.impl;

import java.io.Serializable;
import java.util.Collection;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.Map;
import java.util.Map.Entry;

import javax.annotation.PostConstruct;

import org.hibernate.Criteria;
import org.hibernate.Query;
import org.hibernate.ScrollMode;
import org.hibernate.ScrollableResults;
import org.hibernate.Session;
import org.hibernate.criterion.Criterion;
import org.hibernate.criterion.Projections;
import org.hibernate.criterion.Restrictions;
import org.hibernate.metadata.ClassMetadata;
import org.springframework.orm.hibernate3.support.HibernateDaoSupport;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.ramcache.orm.EntityMetadata;
import com.engine.common.ramcache.orm.model.HibernateMetadata;

/**
 * {@link Accessor} 的 Hibernate 实现
 * 
 */
public class HibernateAccessor extends HibernateDaoSupport implements Accessor {

	private Map<String, EntityMetadata> entityMetadataMap = new HashMap<String, EntityMetadata>();


	@PostConstruct
	public void postConstruct() {
		Map<String, ClassMetadata> classMetadataMap = this.getSessionFactory().getAllClassMetadata();
		for (ClassMetadata classMetadata : classMetadataMap.values()) {
			entityMetadataMap.put(classMetadata.getEntityName(), new HibernateMetadata(classMetadata));
		}
	}

	@Override
	public <PK extends Serializable, T extends IEntity<PK>> T load(Class<T> clz, PK id) {
		return getHibernateTemplate().get(clz, id);
	}

	@SuppressWarnings("unchecked")
	@Override
	public <PK extends Serializable, T extends IEntity<PK>> PK save(Class<T> clz, T entity) {
		return (PK) getHibernateTemplate().save(entity);
	}

	@Override
	public <PK extends Serializable, T extends IEntity<PK>> void remove(Class<T> clz, PK id) {
		final Session session = this.getSession();
		try {
			final StringBuilder stringBuilder = new StringBuilder();
			final String name = clz.getSimpleName();
			final String primary = this.entityMetadataMap.get(clz.getName()).getPrimaryKey();
			
			stringBuilder.append("DELETE ").append(name).append(" ").append(name.charAt(0));
			stringBuilder.append(" WHERE ") ;
			stringBuilder.append(name.charAt(0)).append(".").append(primary).append("=:").append(primary);
			
			final Query query = session.createQuery(stringBuilder.toString());
			query.setParameter(primary, id);
			query.executeUpdate();
		} finally {
			session.close();
		}
	}

	@Override
	public <PK extends Serializable, T extends IEntity<PK>> void remove(Class<T> clz, T entity) {
		getHibernateTemplate().delete(entity);
	}

	@Override
	public <PK extends Serializable, T extends IEntity<PK>> T update(Class<T> clz, T entity) {
		getHibernateTemplate().update(entity);
		return entity;
	}

	
	@SuppressWarnings("unchecked")
	@Override
	public <PK extends Serializable, T extends IEntity<PK>> void listAll(
			Class<T> clz, Collection<T> entities, Integer offset, Integer size) {
		final Session session = this.getSession();
		try {
			final Criteria entityCriteria = session.createCriteria(clz);
			if(offset!=null && size!=null) {
				entityCriteria.setFirstResult(offset);
				entityCriteria.setMaxResults(size);
			}
			entities.addAll(entityCriteria.list());
		} finally {
			session.close();
		}
		
	}

	@SuppressWarnings("unchecked")
	@Override
	public <PK extends Serializable, T extends IEntity<PK>> void listIntersection(
			Class<T> clz, Collection<T> entities, Map<String, Object> keyValue,
			Integer offset, Integer size) {
		final Session session = this.getSession();
		try {
			final Criteria entityCriteria = session.createCriteria(clz);
			final Iterator<Entry<String, Object>> iterator = keyValue.entrySet().iterator();
			Criterion left = null, right = null;
			if (iterator.hasNext()) {
				Entry<String, Object> entry = iterator.next();
				left = Restrictions.eq(entry.getKey(), entry.getValue());
			}
			while (iterator.hasNext()) {
				Entry<String, Object> entry = iterator.next();
				right = Restrictions.eq(entry.getKey(), entry.getValue());
				left = Restrictions.and(left, right);
			};
			if (left != null) {
				entityCriteria.add(left);
			}
			if(offset!=null && size!=null) {
				entityCriteria.setFirstResult(offset);
				entityCriteria.setMaxResults(size);
			}
			entities.addAll(entityCriteria.list());
		} finally {
			session.close();
		}
	}

	@SuppressWarnings("unchecked")
	@Override
	public <PK extends Serializable, T extends IEntity<PK>> void listUnion(
			Class<T> clz, Collection<T> entities, Map<String, Object> keyValue,
			Integer offset, Integer size) {
		final Session session = this.getSession();
		try {
			final Criteria entityCriteria = session.createCriteria(clz);
			final Iterator<Entry<String, Object>> iterator = keyValue.entrySet().iterator();
			Criterion left = null, right = null;
			if (iterator.hasNext()) {
				Entry<String, Object> entry = iterator.next();
				left = Restrictions.eq(entry.getKey(), entry.getValue());
			}
			while (iterator.hasNext()) {
				Entry<String, Object> entry = iterator.next();
				right = Restrictions.eq(entry.getKey(), entry.getValue());
				left = Restrictions.or(left, right);
			};
			if (left != null) {
				entityCriteria.add(left);
			}
			if(offset!=null && size!=null) {
				entityCriteria.setFirstResult(offset);
				entityCriteria.setMaxResults(size);
			}
			entities.addAll(entityCriteria.list());
		} finally {
			session.close();
		}
	}

	@Override
	public Collection<EntityMetadata> getAllMetadata() {
		return entityMetadataMap.values();
	}
	
	/**
	 * 获取实体记录总数(具体实现扩展方法)
	 * @param clz
	 * @return
	 */
	public <PK extends Serializable, T extends IEntity<PK>> long countAll(Class<T> clz) {
		final Session session = this.getSession();
		try {
			final Criteria countCriteria = session.createCriteria(clz);
			countCriteria.setProjection(Projections.rowCount());
			final Object object = countCriteria.uniqueResult();
			return Long.class.cast(object);
		} finally {
			session.close();
		}
	}

	/**
	 * 获取实体记录交集总数(具体实现扩展方法)
	 * @param clz
	 * @return
	 */
	public <PK extends Serializable, T extends IEntity<PK>> long countIntersection(
			Class<T> clz, Map<String, Object> keyValue) {
		final Session session = this.getSession();
		try {
			final Criteria countCriteria = session.createCriteria(clz);
			final Iterator<Entry<String, Object>> iterator = keyValue.entrySet().iterator();
			Criterion left = null, right = null;
			if (iterator.hasNext()) {
				Entry<String, Object> entry = iterator.next();
				left = Restrictions.eq(entry.getKey(), entry.getValue());
			}
			while (iterator.hasNext()) {
				Entry<String, Object> entry = iterator.next();
				right = Restrictions.eq(entry.getKey(), entry.getValue());
				left = Restrictions.and(left, right);
			};
			if (left != null) {
				countCriteria.add(left);
			}
			
			countCriteria.setProjection(Projections.rowCount());
			final Object object = countCriteria.uniqueResult();
			return Number.class.cast(object).longValue();
		} finally {
			session.close();
		}
	}

	/**
	 * 获取实体记录并集总数(具体实现扩展方法)
	 * @param clz
	 * @return
	 */
	public <PK extends Serializable, T extends IEntity<PK>> long countUnion(
			Class<T> clz, Map<String, Object> keyValue) {
		final Session session = this.getSession();
		try {
			final Criteria countCriteria = session.createCriteria(clz);
			final Iterator<Entry<String, Object>> iterator = keyValue.entrySet().iterator();
			Criterion left = null, right = null;
			if (iterator.hasNext()) {
				Entry<String, Object> entry = iterator.next();
				left = Restrictions.eq(entry.getKey(), entry.getValue());
			}
			while (iterator.hasNext()) {
				Entry<String, Object> entry = iterator.next();
				right = Restrictions.eq(entry.getKey(), entry.getValue());
				left = Restrictions.or(left, right);
			};
			if (left != null) {
				countCriteria.add(left);
			}

			countCriteria.setProjection(Projections.rowCount());
			final Object object = countCriteria.uniqueResult();
			return Long.class.cast(object);
		} finally {
			session.close();
		}
	}
	
	/**
	 * 按照指定条件遍历实体(具体实现扩展方法)
	 * @param clz
	 * @param criterion
	 * @param callback
	 */
	public <PK extends Serializable, T extends IEntity<PK>> void cursor(Class<T> clz, Criterion criterion, CursorCallback<T> callback) {
		final Session session = this.getSession();
		try {
			final Criteria entityCriteria = session.createCriteria(clz);
			if(criterion!=null) {
				entityCriteria.add(criterion);
			}
			final ScrollableResults scrollableResults = entityCriteria.scroll(ScrollMode.FORWARD_ONLY);
			try {
				while(scrollableResults.next()) {
					try {
						final T entity = clz.cast(scrollableResults.get(0));
						callback.call(entity);
					} catch(Throwable throwable) {
						throwable.printStackTrace();
					}
				}
			} finally {
				scrollableResults.close();
			}
		} finally {
			session.close();
		}
	}
	
	/**
	 * 删除指定实体记录并集(具体实现扩展方法)
	 * @param clz
	 * @return
	 */
	public <PK extends Serializable, T extends IEntity<PK>> Collection<T> removeUnion(
			Class<T> clz, Map<String, Object> keyValue) {
		final LinkedList<T> entities = new LinkedList<T>();
		this.listUnion(clz, entities, keyValue, null, null);
		getHibernateTemplate().deleteAll(entities);
		return entities;
	}
	
	/**
	 * 删除指定实体记录交集(具体实现扩展方法)
	 * @param clz
	 * @return
	 */
	public <PK extends Serializable, T extends IEntity<PK>> void removeIntersection(
			Class<T> clz, Map<String, Object> keyValue) {
//		final LinkedList<T> entities = new LinkedList<T>();
//		this.listIntersection(clz, entities, keyValue, null, null);
//		getHibernateTemplate() .deleteAll(entities);
//		return entities;
		final Session session = this.getSession();
		try {
			final StringBuilder stringBuilder = new StringBuilder();
			final String name = clz.getSimpleName();
			
			stringBuilder.append("DELETE ").append(name).append(" ").append(name.charAt(0));
			if(!keyValue.isEmpty()) {
				stringBuilder.append(" WHERE ") ;
				for(Entry<String, Object> entry : keyValue.entrySet()) {
					stringBuilder.append(name.charAt(0)).append(".").append(entry.getKey()).append("=:").append(entry.getKey());
					stringBuilder.append(" AND ");
				}
				stringBuilder.delete(stringBuilder.length() - 5, stringBuilder.length());
			}
			
			final Query query = session.createQuery(stringBuilder.toString());
			for(Entry<String, Object> entry : keyValue.entrySet()) {
				query.setParameter(entry.getKey(), entry.getValue());
			}
			query.executeUpdate();
		} finally {
			session.close();
		}
		
	}

}
