package com.engine.common.resource.excel;

import org.hamcrest.CoreMatchers;
import org.junit.Assert;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.resource.anno.Static;

/**
 * {@link Static}注释测试
 * 
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class StaticTest {

	@Autowired
	private StaticTestTarget target;
	
	@Test
	public void test() {
		Assert.assertThat(target, CoreMatchers.notNullValue());
	}
}
