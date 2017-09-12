package com.engine.common.ramcache.persist;

//import com.engine.common.bdb.annotation.Store;
import com.engine.common.ramcache.IEntity;

//@Entity
//@Store("test")
public class BdbPerson implements IEntity<Integer>{

	//@PrimaryKey
	private Integer id;
	private String name;

	public BdbPerson() {
	}
	
	public BdbPerson(int id, String name) {
		this.id = id;
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

}
