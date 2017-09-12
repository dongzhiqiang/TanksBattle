package com.engine.common.ramcache.lock;

import java.io.Serializable;

import com.engine.common.ramcache.IEntity;

/**
 * 测试用的抽象实体类
 * 
 * 
 */
@SuppressWarnings("rawtypes")
public abstract class AbstractEntity<T extends Comparable<T> & Serializable> implements IEntity<T> {

	private T id;

	public AbstractEntity(T id) {
		this.id = id;
	}

	@Override
	public T getId() {
		return id;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((id == null) ? 0 : id.hashCode());
		return result;
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		AbstractEntity other = (AbstractEntity) obj;
		if (id == null || other.id == null) {
			return false;
		}
		if (!id.equals(other.id)) {
			return false;
		}
		return true;
	}

}
