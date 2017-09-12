package com.engine.common.socket.anno.impl;

import java.lang.reflect.Method;

import javassist.NotFoundException;

import org.hamcrest.CoreMatchers;
import org.junit.Assert;
import org.junit.Test;

import com.engine.common.socket.anno.InBody;
import com.engine.common.socket.anno.Parameter;
import com.engine.common.socket.anno.ParameterBuilder;
import com.engine.common.socket.anno.impl.InBodyParameter;
//import com.engine.common.socket.exception.ParameterException;

public class InBodyParameterTest {

	@Test
	public void test_InBodyDefault_on_class() throws SecurityException, NoSuchMethodException, NotFoundException {
		Class<TestTargetImpl> clazz = TestTargetImpl.class;
		Method method = clazz.getDeclaredMethod("testInBodyDefault", String.class, String.class);
		
		InBody annotation = (InBody) method.getParameterAnnotations()[0][0];
		InBodyParameter parameter = InBodyParameter.valueOf(annotation, method, 0, 0, null);
		Assert.assertThat(parameter.getValue(), CoreMatchers.is("name"));
		
		annotation = (InBody) method.getParameterAnnotations()[1][0];
		parameter = InBodyParameter.valueOf(annotation, method, 1, 0, null);
		Assert.assertThat(parameter.getValue(), CoreMatchers.is("password"));
	}
	
	@Test//(expected = ParameterException.class)
	public void test_InBodyDefault_on_interface() throws SecurityException, NoSuchMethodException, NotFoundException {
		Class<TestTargetInf> clazz = TestTargetInf.class;
		Method method = clazz.getDeclaredMethod("testInBodyDefault", String.class, String.class);
		
		InBody annotation = (InBody) method.getParameterAnnotations()[0][0];
		InBodyParameter parameter = InBodyParameter.valueOf(annotation, method, 0, 0, null);
		Assert.assertThat(parameter.getValue(), CoreMatchers.is("name"));
		
		annotation = (InBody) method.getParameterAnnotations()[1][0];
		parameter = InBodyParameter.valueOf(annotation, method, 1, 0, null);
		Assert.assertThat(parameter.getValue(), CoreMatchers.is("password"));
	}

	@Test
	public void test_InBodyIndex_on_interface() throws SecurityException, NoSuchMethodException, NotFoundException {
		Class<TestTargetInf> clazz = TestTargetInf.class;
		Method method = clazz.getDeclaredMethod("testInBodyIndex", String.class, String.class, String.class);
		ParameterBuilder builder = new ParameterBuilder();
		Parameter[] parameters = builder.buildParameters(method);
		Assert.assertEquals(parameters.length, 3);
		Assert.assertThat(((InBodyParameter)parameters[1]).getValue(), CoreMatchers.is("0"));
		Assert.assertThat(((InBodyParameter)parameters[2]).getValue(), CoreMatchers.is("1"));
	}
}
