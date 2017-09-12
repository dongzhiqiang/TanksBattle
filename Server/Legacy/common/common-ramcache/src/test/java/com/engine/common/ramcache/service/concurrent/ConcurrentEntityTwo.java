package com.engine.common.ramcache.service.concurrent;

import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.NamedQueries;
import javax.persistence.NamedQuery;
import javax.persistence.Table;
import javax.persistence.UniqueConstraint;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.ChkUnique;
import com.engine.common.ramcache.anno.Unique;
import com.engine.common.ramcache.enhance.Enhance;

/**
 * 并发测试实体
 * 
 */
@Entity
@Cached(size = "default")
@NamedQueries({
	@NamedQuery(name = "ConcurrentEntityTwo.name", query = "from ConcurrentEntityTwo c where c.name = ?")
})
@Table(uniqueConstraints = @UniqueConstraint(name = "ConcurrentEntityTwo.name", columnNames = "name"))
public class ConcurrentEntityTwo implements IEntity<Integer> {

	public static ConcurrentEntityTwo valueOf(Integer id, String name) {
		ConcurrentEntityTwo result = new ConcurrentEntityTwo();
		result.id = id;
		result.name = name;
		return result;
	}

	@Id
	private Integer id;
	@Unique(query = "ConcurrentEntityTwo.name")
	private String name;

	@Enhance
	public void setName(@ChkUnique("name") String name) {
		this.name = name;
	}

	public Integer getId() {
		return id;
	}

	protected void setId(Integer id) {
		this.id = id;
	}

	public String getName() {
		return name;
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
		ConcurrentEntityTwo other = (ConcurrentEntityTwo) obj;
		if (id == null) {
			if (other.id != null)
				return false;
		} else if (!id.equals(other.id))
			return false;
		return true;
	}

	@Override
	public String toString() {
		return "ConcurrentEntityTwo [id=" + id + ", name=" + name + "]";
	}
}
