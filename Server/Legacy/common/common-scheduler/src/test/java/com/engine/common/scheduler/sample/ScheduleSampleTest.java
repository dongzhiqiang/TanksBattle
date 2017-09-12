package com.engine.common.scheduler.sample;

import java.util.Date;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.stereotype.Component;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.scheduler.Scheduled;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
@Component
public class ScheduleSampleTest {
	
	@Test
	public void test(){
		System.err.println("start a test......");
		try {
			Thread.sleep(1000*60*60);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
	}
	@Scheduled(name = "test:clean", value = "0 0 0 * * *")
	public void cleanPerSecond() {
		Date date=new Date();
		System.err.println("-----------"+date.getSeconds()+": "+date.getMinutes()+": "+date.getHours());
	}
}
