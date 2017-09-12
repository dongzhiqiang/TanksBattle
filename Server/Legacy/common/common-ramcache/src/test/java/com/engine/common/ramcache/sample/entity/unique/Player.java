package com.engine.common.ramcache.sample.entity.unique;

import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.NamedQueries;
import javax.persistence.NamedQuery;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.ChkUnique;
import com.engine.common.ramcache.anno.Unique;
import com.engine.common.ramcache.enhance.Enhance;

/**
 * 用户对象实体
 * 
 */
@Entity
@Cached(size = "default")
@NamedQueries({
	@NamedQuery(name = "Player.name", query = "from Player where name = ?")
})
public class Player implements IEntity<Integer> {

	/** 用户标识 */
	@Id
	private Integer id;
	/** 用户名 */
	@Unique(query = "Player.name")
	private String name;

	@Enhance
	public void setName(@ChkUnique("name") String name) {
		this.name = name;
	}

	// Getter and Setter ...

	public Integer getId() {
		return id;
	}

	protected void setId(Integer id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

}
