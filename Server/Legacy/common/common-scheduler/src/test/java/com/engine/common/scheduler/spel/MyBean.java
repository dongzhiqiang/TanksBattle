package com.engine.common.scheduler.spel;

import org.springframework.stereotype.Component;

@Component("myBean")
public class MyBean {
	
	private int count;
	
	public String getCron() {
		return "*/2 * * * * *";
	}
	
	public void increase() {
		count++;
	}

	public int getCount() {
		return count;
	}
}
