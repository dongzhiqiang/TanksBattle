package com.engine.common.scheduler;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.stereotype.Component;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
@Component
public class ErrorTest {

//	@Scheduled(name = "异常测试定时任务", value = "*/5 * * * * *")
	public void throwException() {
		throw new RuntimeException("刻意抛出的异常");
	}
	
//	@Test
	public void test() throws InterruptedException {
		Thread.sleep(30000);
	}
}
