package com.game.auth.utils;

import java.io.Serializable;
import org.springframework.orm.hibernate3.support.HibernateDaoSupport;



public class HibernateAccessorEx extends HibernateDaoSupport {


	public <PK extends Serializable, T> T load(Class<T> clz, PK id) {
		return getHibernateTemplate().get(clz, id);
	}

	@SuppressWarnings("unchecked")
	public <PK extends Serializable, T> PK save(T entity) {
		return (PK) getHibernateTemplate().save(entity);
	}

	public <T> void remove(T entity) {
		getHibernateTemplate().delete(entity);
	}

	public <T> T update(T entity) {
		getHibernateTemplate().update(entity);
		return entity;
	}


	
}
