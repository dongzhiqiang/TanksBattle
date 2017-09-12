package com.engine.common.ramcache.service;

import java.io.Serializable;

import com.engine.common.ramcache.IEntity;

public interface EntityBuilder<PK extends Comparable<PK> & Serializable, T extends IEntity<PK>> {
	
	T newInstance(PK id);

}
