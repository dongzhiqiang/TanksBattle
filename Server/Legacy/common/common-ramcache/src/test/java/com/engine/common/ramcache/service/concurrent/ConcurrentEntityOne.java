package com.engine.common.ramcache.service.concurrent;

import javax.persistence.Entity;
import javax.persistence.Id;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.enhance.Enhance;

/**
 * 并发测试实体
 * 
 */
@Entity
@Cached(size = "default")
public class ConcurrentEntityOne implements IEntity<Integer> {

	public static ConcurrentEntityOne valueOf(Integer id) {
		ConcurrentEntityOne result = new ConcurrentEntityOne();
		result.id = id;
		return result;
	}

	@Id
	private Integer id;
	private int count;

	public Integer getId() {
		return id;
	}

	protected void setId(Integer id) {
		this.id = id;
	}

	public int getCount() {
		return count;
	}

	@Enhance
	public void setCount(int count) {
		this.count = count;
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
		ConcurrentEntityOne other = (ConcurrentEntityOne) obj;
		if (id == null) {
			if (other.id != null)
				return false;
		} else if (!id.equals(other.id))
			return false;
		return true;
	}
}
