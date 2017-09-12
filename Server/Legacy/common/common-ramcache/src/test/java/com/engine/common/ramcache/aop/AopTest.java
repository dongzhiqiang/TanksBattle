package com.engine.common.ramcache.aop;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
@SuppressWarnings({"rawtypes", "unchecked"})
public class AopTest {

	@Autowired
	private TestTarget target;
	
	@Test
	public void test_object() {
		System.out.println("[test_object]");
		target.methodObject(1, 2);
		target.methodObject(1, null);
		target.methodObject(null, null);
	}
	
	@Test
	public void test_list() {
		System.out.println("[test_list]");
		
		List list = new ArrayList();
		Collections.addAll(list, 1, 2);
		target.methodList(list);
		
		list.set(0, null);
		target.methodList(list);
		
		list.clear();
		target.methodList(list);
		
		target.methodList(null);
	}
	
	@Test
	public void test_array_one() {
		System.out.println("[test_array_one]");
		
		target.methodArrayOne(1, 2, 3);
		target.methodArrayOne(1, null, 3);
		target.methodArrayOne();
		target.methodArrayOne(new Object[0]);
	}
	
	@Test
	public void test_array_two() {
		System.out.println("[test_array_two]");
		
		target.methodArrayTwo(new Object[]{1, 2, 3});
		target.methodArrayTwo(new Object[]{1, null, 3});
		target.methodArrayTwo(null);
		target.methodArrayTwo(new Object[0]);
	}
	
	@Test
	public void test_map() {
		System.out.println("[test_map]");
		
		target.methodMap(null);
		Map map = new HashMap();
		target.methodMap(map);
		map.put(1, 1);
		map.put(2, 2);
		target.methodMap(map);
		map.put(1, null);
		target.methodMap(map);
	}

}
