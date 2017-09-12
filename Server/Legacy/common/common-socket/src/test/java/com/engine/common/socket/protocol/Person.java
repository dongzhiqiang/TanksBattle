package com.engine.common.socket.protocol;

import com.engine.common.protocol.annotation.Ignore;
import com.engine.common.protocol.annotation.Transable;

@Transable
public class Person {
	
	private Integer id;
	private String name;
	private Generic<Person, Person> o;

	public static Person valueOf(Integer id, String name) {
		Person result = new Person();
		result.id = id;
		result.name = name;
		return result;
	}
	
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
	
	@Ignore
	public Generic<Person, Person> getO() {
		return o;
	}

	public void setO(Generic<Person, Person> o) {
		this.o = o;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((id == null) ? 0 : id.hashCode());
		result = prime * result + ((name == null) ? 0 : name.hashCode());
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
		Person other = (Person) obj;
		if (id == null) {
			if (other.id != null)
				return false;
		} else if (!id.equals(other.id))
			return false;
		if (name == null) {
			if (other.name != null)
				return false;
		} else if (!name.equals(other.name))
			return false;
		return true;
	}

}
