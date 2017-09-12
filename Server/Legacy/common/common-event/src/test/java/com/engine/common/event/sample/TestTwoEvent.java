package com.engine.common.event.sample;

import com.engine.common.event.Event;

public class TestTwoEvent extends TestEvent{
	public static String NAME = "test:two";
    
	byte type;
	public static Event<TestTwoEvent> valueOf(byte type) {
       TestTwoEvent result=new TestTwoEvent();
       result.type=type;
       return Event.valueOf(NAME, result);
	}
	public byte getType() {
		return type;
	}
	public void setType(byte type) {
		this.type = type;
	}
	@Override
	public void logicTest() {
		System.out.println("event:type:"+type);
		
	}
	
	
	
	
}
