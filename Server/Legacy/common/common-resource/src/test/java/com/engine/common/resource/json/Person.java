package com.engine.common.resource.json;

import com.engine.common.resource.anno.Id;
import com.engine.common.resource.anno.Index;
import com.engine.common.resource.anno.Resource;

@Resource
public class Person {
	
	public static final String INDEX_NAME = "person_name";
	public static final String INDEX_AGE = "person_age";

	@Id
	private Integer id;
	@Index(name = INDEX_NAME, unique = true)
	private String name;
	@Index(name = INDEX_AGE)
	private int age;
	private boolean sex;

	public Integer getId() {
		return id;
	}

	public void setId(Integer id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public int getAge() {
		return age;
	}

	public void setAge(int age) {
		this.age = age;
	}

	public boolean isSex() {
		return sex;
	}

	public void setSex(boolean sex) {
		this.sex = sex;
	}

}
