package com.game.module.migration;

import java.io.Serializable;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.orm.impl.HibernateAccessor;


public class IgnoreConverter implements MigratorConverter {

	@Override
	public void convert(MigratorContext context, HibernateAccessor oldRawStore,
			HibernateAccessor newRawStore, IEntity<Serializable> entityt)
			throws Exception {
		throw new IgnoreMigrateException();
	}

	

}
