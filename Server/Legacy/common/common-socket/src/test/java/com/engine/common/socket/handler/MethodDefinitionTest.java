package com.engine.common.socket.handler;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.lang.reflect.Method;
import java.lang.reflect.Type;

import org.junit.Test;

import com.engine.common.socket.core.Response;
import com.engine.common.socket.handler.MethodDefinition;

public class MethodDefinitionTest {
	
	class ResponseTarget {
		@SuppressWarnings("rawtypes")
		public Response retResponse() {
			return null;
		}
		
		public Response<Integer> retResponseInteger() {
			return null;
		}
	}

	@Test
	public void test_response_raw() throws Exception {
		Method method = ResponseTarget.class.getMethod("retResponse");
		Type type = MethodDefinition.findResponseType(method);
		assertThat(type == Object.class, is(true));
	}
	
	@Test
	public void test_response_integer() throws Exception {
		Method method = ResponseTarget.class.getMethod("retResponseInteger");
		Type type = MethodDefinition.findResponseType(method);
		assertThat(type == Integer.class, is(true));
	}
}
