package com.engine.common.ramcache.persist;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.BeansException;
import org.springframework.context.ApplicationContext;
import org.springframework.context.ApplicationContextAware;
import org.springframework.context.ConfigurableApplicationContext;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.ramcache.persist.Element;
import com.engine.common.ramcache.persist.TimingConsumerState;
import com.engine.common.ramcache.persist.TimingPersister;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class TimingPerformanceTest implements ApplicationContextAware {
	
	private TimingPersister persister;

	private int total = 10000;
	
	@Test
	public void test() throws InterruptedException {
		Accessor accessor = applicationContext.getBean(Accessor.class);
		persister = new TimingPersister();
		persister.initialize("test", accessor, "0 0 0 * * *");
		
		for (int i = 0; i < total; i++) {
			persister.put(Element.saveOf(new Person(i, "name:" + i)));
		}
		persister.flush();
		long start = System.currentTimeMillis();
		while (true) {
			if (persister.getConsumer().getState() == TimingConsumerState.RUNNING) {
				Thread.yield();
			} else {
				break;
			}
		}
		System.out.println("插入完成时间:" + (System.currentTimeMillis() - start));
		
		for (int i = 0; i < total; i++) {
			persister.put(Element.updateOf(new Person(i, "new:name:" + i)));
		}
		persister.flush();
		start = System.currentTimeMillis();
		while (true) {
			if (persister.getConsumer().getState() == TimingConsumerState.RUNNING) {
				Thread.yield();
			} else {
				break;
			}
		}

		System.out.println("更新完成时间:" + (System.currentTimeMillis() - start));

		for (int i = 0; i < total; i++) {
			persister.put(Element.removeOf(i, Person.class));
		}
		persister.flush();
		start = System.currentTimeMillis();
		while (true) {
			if (persister.getConsumer().getState() == TimingConsumerState.RUNNING) {
				Thread.yield();
			} else {
				break;
			}
		}
		System.out.println("删除完成时间:" + (System.currentTimeMillis() - start));
	}
	
	private ConfigurableApplicationContext applicationContext;
	
	@Override
	public void setApplicationContext(ApplicationContext applicationContext) throws BeansException {
		this.applicationContext = (ConfigurableApplicationContext) applicationContext;
	}
}
