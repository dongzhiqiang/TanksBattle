package com.engine.common.resource.excel;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.resource.anno.Static;

/**
 * 多表单合并测试
 * 
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class MultiSheetTest {

	@Static("1")
	private Pet cat1;
	@Static("2")
	private Pet cat2;
	@Static("3")
	private Pet dog1;
	@Static("4")
	private Pet dog2;
	
	@Test
	public void test() {
		assertThat(cat1, notNullValue());
		assertThat(cat1.getId(), is(1));
		assertThat(cat1.getName(), is("咪咪"));
		
		assertThat(cat2, notNullValue());
		assertThat(cat2.getId(), is(2));
		assertThat(cat2.getName(), is("喵喵"));
		
		assertThat(dog1, notNullValue());
		assertThat(dog1.getId(), is(3));
		assertThat(dog1.getName(), is("旺旺"));

		assertThat(dog2, notNullValue());
		assertThat(dog2.getId(), is(4));
		assertThat(dog2.getName(), is("狗狗"));
	}
}
