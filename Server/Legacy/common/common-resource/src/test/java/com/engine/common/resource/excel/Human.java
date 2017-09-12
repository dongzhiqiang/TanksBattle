package com.engine.common.resource.excel;

import java.util.Comparator;

import com.engine.common.resource.Validate;
import com.engine.common.resource.anno.Id;
import com.engine.common.resource.anno.Index;
import com.engine.common.resource.anno.Resource;

@Resource
public class Human implements Validate {
	
	public static final String INDEX_NAME = "human_name";
	public static final String INDEX_AGE = "human_age";
	
	public static final class HumanComparator implements Comparator<Human> {
		@Override
		public int compare(Human o1, Human o2) {
			return -o1.id.compareTo(o2.id);
		}
	};

	@Id
	private Integer id;
	@Index(name = INDEX_NAME, unique = true)
	private String name;
	@Index(name = INDEX_AGE, comparatorClz = HumanComparator.class)
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

	@Override
	public boolean isValid() {
		return true;
	}

}
