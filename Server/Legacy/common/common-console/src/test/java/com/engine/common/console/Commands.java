package com.engine.common.console;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.console.ConsoleBean;
import com.engine.common.console.ConsoleCommand;

@Component
@ConsoleBean
public class Commands {

	@Autowired
	private TestBean testBean;
	
	@ConsoleCommand(name = "test", description = "无参方法测试")
	public void test() {
		testBean.doSomething();
	}
	
	@ConsoleCommand(name = "int", description = "int测试")
	public void testInt(int value) {
		testBean.testInt(value);
	}

	@ConsoleCommand(name = "integer", description = "Integer测试")
	public void testInteger(Integer value) {
		testBean.testInteger(value);
	}

	@ConsoleCommand(name = "string", description = "string测试")
	public void testString(String value) {
		testBean.testString(value);
	}

	@ConsoleCommand(name = "boolean", description = "boolean测试")
	public void testBoolean(boolean value) {
		testBean.testBoolean(value);
	}

	@ConsoleCommand(name = "add", description = "加法测试")
	public void add(int a, int b) {
		System.out.println(a + b);
	}
}
