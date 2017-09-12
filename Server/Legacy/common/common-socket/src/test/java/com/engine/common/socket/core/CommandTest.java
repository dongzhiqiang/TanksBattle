package com.engine.common.socket.core;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.junit.Test;

import com.engine.common.socket.core.Command;

/**
 * {@link Command}的单元测试
 * 
 */
public class CommandTest {

	@Test
	public void test_valueOf() {
		Command command = Command.valueOf(10, 1,2,3);
		assertThat(command.getCommand(), is(10));
		assertThat(command.getModule().getId(), is(1));
		assertThat(command.getModule().getNext().getId(), is(2));
		assertThat(command.getModule().getNext().getNext().getId(), is(3));
		
		byte[] bytes = command.toBytes();
		Command target = Command.valueOf(bytes, 0, bytes.length);
		assertThat(target, is(command));
	}

	@Test
	public void test_toBytes() {
		Command command = Command.valueOf(10, 1,2,3);
		byte[] bytes = command.toBytes();
		assertThat(bytes.length, is(7));
		assertThat(bytes[0], is((byte) 0));
		assertThat(bytes[1], is((byte) 0));
		assertThat(bytes[2], is((byte) 0));
		assertThat(bytes[3], is((byte) 10));
		assertThat(bytes[4], is((byte) 1));
		assertThat(bytes[5], is((byte) 2));
		assertThat(bytes[6], is((byte) 3));
	}
}
