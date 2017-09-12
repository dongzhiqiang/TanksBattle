package com.engine.common.scheduler.sample;

import com.engine.common.scheduler.Scheduled;

public class TestSchedule {
    
	@Scheduled(name="test:clean",value="3 * * * * *")
	public void cleanPerSecond(){
		System.out.println("runnable per three second");
	}
}
