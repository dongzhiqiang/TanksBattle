package com.engine.common.event.sample;

import java.util.Date;

import com.engine.common.event.Event;

public class TestOneEvent extends TestEvent{
	public static String NAME = "Test:event";
	private int playerId;
	private Date time;
	private String operation;

	public static Event<TestOneEvent> valueOf(int playerId, Date time,
			String operation) {
		TestOneEvent result = new TestOneEvent();
		result.playerId = playerId;
		result.time = time;
		result.operation = operation;
		return Event.valueOf(NAME, result);
	}
	
	public void logicTest() {
		System.out.println(operation+"operation......");
	}

	public int getPlayerId() {
		return playerId;
	}

	public void setPlayerId(int playerId) {
		this.playerId = playerId;
	}

	public Date getTime() {
		return time;
	}

	public void setTime(Date time) {
		this.time = time;
	}

	public String getOperation() {
		return operation;
	}

	public void setOperation(String operation) {
		this.operation = operation;
	}

}
