package com.engine.common.socket.core;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.junit.Test;

import com.engine.common.socket.core.Module;

/**
 * {@link Module}的单元测试
 * 
 */
public class ModuleTest {
	
	/** 基本功能测试 */
	@Test
	public void test_basic() {
		Module target = Module.valueOf(1, 2, 3);
		assertThat(target.getId(), is(1));
		assertThat(target.getNext().getId(), is(2));
		assertThat(target.getNext().getNext().getId(), is(3));
		
		assertThat(target.getDeep(), is(3));
		
		byte[] array = target.toBytes();
		assertThat(array.length, is(3));
		assertThat(array[0], is((byte) 1));
		assertThat(array[1], is((byte) 2));
		assertThat(array[2], is((byte) 3));
	}
	
	/** 非法构造测试1 */
	@Test(expected = IllegalArgumentException.class)
	public void test_error_1() {
		Module.valueOf(-129);
	}
	
	/** 非法构造测试2 */
	@Test(expected = IllegalArgumentException.class)
	public void test_error_2() {
		Module.valueOf(255);
	}

}
