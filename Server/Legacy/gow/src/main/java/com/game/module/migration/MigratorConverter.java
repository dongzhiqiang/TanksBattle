package com.game.module.migration;

import java.io.Serializable;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.orm.impl.HibernateAccessor;

/**
 * 迁移转换器,
 * 
 *
 */
public interface MigratorConverter {

	/**
	 * 转换
	 * 将旧数据转换为新数据
	 * @param context 转换过程的上下文对象
	 * @param newObject 新数据
	 * @param oldObject 旧数据
	 * @return 迁移则返回true,忽略则返回false;
	 * @throws Exception 
	 */
	public void convert(MigratorContext context, HibernateAccessor oldRawStore, HibernateAccessor newRawStore, IEntity<Serializable> entity) throws Exception;
	
}
