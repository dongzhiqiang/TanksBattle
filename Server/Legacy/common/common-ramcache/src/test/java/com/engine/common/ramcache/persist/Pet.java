package com.engine.common.ramcache.persist;

import javax.persistence.Entity;
import javax.persistence.Id;

import org.hibernate.annotations.Index;

import com.engine.common.ramcache.IEntity;

@Entity
public class Pet implements IEntity<Integer>{

	@Id
	private Integer id;
	
	@Index(name="person")
	private Integer personId;
	
	private String name;

	public Pet() {
	}
	
	public Pet(int id, int personId, String name) {
		this.id = id;
		this.personId = personId;
		this.name = name;
	}

	public Integer getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public Integer getPersonId() {
		return personId;
	}

	public void setPersonId(Integer personId) {
		this.personId = personId;
	}

}
