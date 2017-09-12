package com.engine.common.ramcache.orm.impl;

import java.sql.SQLException;
import java.util.Collection;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;

import org.hibernate.HibernateException;
import org.hibernate.Query;
import org.hibernate.Session;
import org.springframework.orm.hibernate3.HibernateCallback;
import org.springframework.orm.hibernate3.support.HibernateDaoSupport;

import com.engine.common.ramcache.orm.Paging;
import com.engine.common.ramcache.orm.Querier;

/**
 * {@link Querier} 的 Hibernate 实现
 * 
 */
@SuppressWarnings({ "unchecked", "rawtypes" })
public class HibernateQuerier extends HibernateDaoSupport implements Querier {

	@Override
	public <T> List<T> all(Class<T> clz) {
		return getHibernateTemplate().loadAll(clz);
	}

	@Override
	public <T> List<T> list(Class<T> clz, String queryname, Object... params) {
		return getHibernateTemplate().findByNamedQuery(queryname, params);
	}

	@Override
	public <E> List<E> list(Class entityClz, Class<E> retClz, final String queryname, final Object... params) {
		return getHibernateTemplate().findByNamedQuery(queryname, params);
	}

	@Override
	public <T> T unique(Class<T> clz, final String queryname, final Object... params) {
		return getHibernateTemplate().executeWithNativeSession(new HibernateCallback<T>() {
			@Override
			public T doInHibernate(Session session) throws HibernateException, SQLException {
				Query query = session.getNamedQuery(queryname);
				for (int i = 0; i < params.length; i++) {
					query.setParameter(i, params[i]);
				}
				return (T) query.uniqueResult();
			}
		});
	}

	@Override
	public <E> E unique(Class entityClz, Class<E> retClz, final String queryname, final Object... params) {
		return getHibernateTemplate().executeWithNativeSession(new HibernateCallback<E>() {
			@Override
			public E doInHibernate(Session session) throws HibernateException, SQLException {
				Query query = session.getNamedQuery(queryname);
				for (int i = 0; i < params.length; i++) {
					query.setParameter(i, params[i]);
				}
				return (E) query.uniqueResult();
			}
		});
	}

	@Override
	public <T> List<T> paging(Class<T> clz, final String queryname, final Paging paging, final Object... params) {
		return getHibernateTemplate().executeWithNativeSession(new HibernateCallback<List<T>>() {
			@Override
			public List<T> doInHibernate(Session session) throws HibernateException, SQLException {
				Query query = session.getNamedQuery(queryname);
				for (int i = 0; i < params.length; i++) {
					query.setParameter(i, params[i]);
				}
				query.setFirstResult(paging.getFirst());
				query.setMaxResults(paging.getSize());
				return query.list();
			}
		});
	}

	@Override
	public <E> List<E> paging(Class entityClz, Class<E> retClz, final String queryname, final Paging paging,
			final Object... params) {
		return getHibernateTemplate().executeWithNativeSession(new HibernateCallback<List<E>>() {
			@Override
			public List<E> doInHibernate(Session session) throws HibernateException, SQLException {
				Query query = session.getNamedQuery(queryname);
				for (int i = 0; i < params.length; i++) {
					query.setParameter(i, params[i]);
				}
				query.setFirstResult(paging.getFirst());
				query.setMaxResults(paging.getSize());
				return query.list();
			}
		});
	}

	@Override
	public int execute(Class entityClz, final String queryname, final Object... params) {
		return getHibernateTemplate().executeWithNativeSession(new HibernateCallback<Integer>() {
			@Override
			public Integer doInHibernate(Session session) throws HibernateException, SQLException {
				Query query = session.getNamedQuery(queryname);
				String[] names = query.getNamedParameters();
				if (names == null || names.length == 0) {
					for (int i = 0; i < params.length; i++) {
						query.setParameter(i, params[i]);
					}
				} else {
					Map<String, Object> kv = (Map<String, Object>) params[0];
					for (Entry<String, Object> entry : kv.entrySet()) {
						String name = entry.getKey();
						Object value = entry.getValue();
						if (value instanceof Collection) {
							query.setParameterList(name, (Collection) value);
						} else {
							query.setParameter(name, value);
						}
					}
				}
				return query.executeUpdate();
			}
		});
	}
}
