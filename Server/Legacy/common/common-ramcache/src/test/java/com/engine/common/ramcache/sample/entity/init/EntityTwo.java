package com.engine.common.ramcache.sample.entity.init;

import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.NamedQueries;
import javax.persistence.NamedQuery;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.InitialConfig;
import com.engine.common.ramcache.anno.InitialType;
import com.engine.common.ramcache.enhance.Enhance;

/**
 * 测试实体一
 * 
 */
@Entity
@NamedQueries(
	@NamedQuery(name = "EntityTwo.init", query = "From EntityTwo where id < 50")
)
@Cached(size = "default")
@InitialConfig(type = InitialType.QUERY, query = "EntityTwo.init")
public class EntityTwo implements IEntity<Integer> {

	/** 标识 */
	@Id
	private Integer id;
	/** 用户名 */
	private String name;
	
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

	@Enhance
	public void setName(String name) {
		this.name = name;
	}

}
