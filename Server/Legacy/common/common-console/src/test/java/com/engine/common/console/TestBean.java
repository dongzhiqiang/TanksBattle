package com.engine.common.console;

import org.springframework.stereotype.Component;

@Component
public class TestBean {
	
	public void doSomething() {
		System.out.println("TestBean doing something.");
	}
	
	public void testInt(int value) {
		System.out.println("参数是:" + value);
	}

	public void testInteger(Integer value) {
		System.out.println("参数是:" + value);
	}

	public void testString(String value) {
		System.out.println("参数是:" + value);
	}

	public void testBoolean(boolean value) {
		System.out.println("参数是:" + value);
	}

}
