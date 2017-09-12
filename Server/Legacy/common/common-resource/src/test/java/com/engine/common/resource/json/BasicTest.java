package com.engine.common.resource.json;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.util.List;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.resource.Storage;
import com.engine.common.resource.StorageManager;
import com.engine.common.resource.anno.Static;

/**
 * 静态资源注入基本功能测试用例
 * 
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
@Component
public class BasicTest {
	
	@Autowired
	private StorageManager resourceManager;
	@Static
	private Storage<Integer, Person> storage;
	@Static(value = "1")
	private Person person1;
	@Static(value = "2")
	private Person person2;
	@Static(value = "1")
	private Person personOther;
	@Static(value = "-1", required = false)
	private Person personNotFound;
	
	/**
	 * 测试管理对象注入
	 */
	@Test
	public void test_get_manager() {
		assertThat(resourceManager, notNullValue());
	}

	/**
	 * 测试管理对象基本功能
	 */
	@Test
	public void test_basic() {
		Person p1 = resourceManager.getResource(1, Person.class);
		assertThat(p1.getId(), is(1));
		assertThat(p1.getName(), is("Frank"));
		Person same = resourceManager.getResource(1, Person.class);
		assertThat(p1, sameInstance(same));
	}
	
	/**
	 * 测试指定资源存储空间注入
	 */
	@Test
	public void test_inject_storage() {
		assertThat(storage, notNullValue());
		Person p1 = resourceManager.getResource(1, Person.class);
		assertThat(p1.getId(), is(1));
		assertThat(p1.getName(), is("Frank"));
		Person same = resourceManager.getResource(1, Person.class);
		assertThat(p1, sameInstance(same));
	}
	
	/**
	 * 测试唯一索引获取
	 */
	@Test
	public void test_index_unique() {
		Person p1 = storage.getUnique(Person.INDEX_NAME, "Frank");
		assertThat(p1, notNullValue());
		assertThat(p1.getId(), is(1));
		assertThat(p1.getName(), is("Frank"));
		assertThat(p1, sameInstance(person1));
	}
	
	/**
	 * 测试列表索引获取
	 */
	@Test
	public void test_index_list() {
		List<Person> list = storage.getIndex(Person.INDEX_AGE, 32);
		assertThat(list, notNullValue());
		assertThat(list.size(), is(2));
		
		Person p1 = list.get(0);
		assertThat(p1.getId(), is(1));
		assertThat(p1.getName(), is("Frank"));
		assertThat(p1, sameInstance(person1));
		
		Person p2 = list.get(1);
		assertThat(p2.getId(), is(3));
		assertThat(p2.getName(), is("Kyle"));
	}

	/**
	 * 测试资源实例注入
	 */
	@Test
	public void test_inject_instance() {
		assertThat(person1, notNullValue());
		assertThat(person2, notNullValue());
		assertThat(personOther, notNullValue());
		assertThat(personNotFound, nullValue());
		
		assertThat(person1.getId(), is(1));
		assertThat(person1.getName(), is("Frank"));

		assertThat(person2.getId(), is(2));
		assertThat(person2.getName(), is("May"));
		
		assertThat(person1, sameInstance(personOther));
	}
	
}
