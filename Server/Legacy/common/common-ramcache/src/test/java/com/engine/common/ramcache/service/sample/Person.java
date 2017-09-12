package com.engine.common.ramcache.service.sample;

import javax.persistence.Entity;
import javax.persistence.Id;

import com.engine.common.ramcache.IEntity;

@Entity
public class Person implements IEntity<Integer> {

	@Id
	private Integer id;
	private String name;

	public Person() {
	}
	
	public Person(int id, String name) {
		this.id = id;
		this.name = name;
	}

	@Override
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

}
