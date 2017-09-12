package com.engine.common.socket.anno.body;

import java.util.HashMap;
import java.util.Map;

import org.hamcrest.CoreMatchers;
import org.junit.Assert;
import org.springframework.stereotype.Component;

import com.engine.common.socket.model.Person;

@Component
public class InBodyFacade implements InBodyFacadeInf {

	@Override
	public Integer personId(int id) {
		return id;
	}

	@Override
	public String personName(String name) {
		return name;
	}

	@Override
	public Double arrayDouble(double number) {
		return number;
	}

	@Override
	public String arrayString(String string) {
		return string;
	}

	@Override
	public Person arrayObject(Person p1, String string) {
		return p1;
	}

	@Override
	public Map<String, Object> mapPrimitive(Long one, String two) {
		Map<String, Object> result = new HashMap<String, Object>();
		result.put("one", two);
		result.put("two", one);
		return result;
	}

	@Override
	public Person mapPerson(Person person, String id) {
		Assert.assertThat(id, CoreMatchers.is("uuid"));
		return person;
	}

	@Override
	public int required(Person person, String id) {
		if (person == null && id == null) {
			return 3;
		} else if (person == null) {
			return 1;
		} else if (id == null) {
			return 2;
		}
		return 0;
	}

}
