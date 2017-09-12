package com.engine.common.resource.spring;

import org.hamcrest.CoreMatchers;
import org.junit.Assert;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.stereotype.Component;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.resource.anno.Static;

/**
 * 注入测试
 * 
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
@Component
public class InjectTest {

	@Static("1")
	private Config config_one;
	@Static("2")
	private Config config_two;
	
	@Test
	public void test() {
		Assert.assertThat(config_one.getConversionService(), CoreMatchers.notNullValue());
		Assert.assertThat(config_two.getConversionService(), CoreMatchers.notNullValue());
		Assert.assertThat(config_one.getInjectObject(), CoreMatchers.notNullValue());
		Assert.assertThat(config_two.getInjectObject(), CoreMatchers.notNullValue());
		Assert.assertThat(config_one.getInjectObject(), CoreMatchers.sameInstance(config_two.getInjectObject()));
	}
}
