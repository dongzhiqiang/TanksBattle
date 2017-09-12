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
public class BdbPerformanceTest implements ApplicationContextAware {
	
	private TimingPersister queue;

	private int total = 100000;
	
	@Test
	public void test() throws InterruptedException {
		Accessor accessor = applicationContext.getBean(Accessor.class);
		queue = new TimingPersister();
		queue.initialize("test", accessor, "0 0 0 * * *");
		
		long start = System.currentTimeMillis();
		for (int i = 0; i < total; i++) {
			queue.put(Element.saveOf(new BdbPerson(i, "name:" + i)));
		}
		queue.flush();
		while (true) {
			if (queue.getConsumer().getState() == TimingConsumerState.RUNNING) {
				Thread.yield();
			} else {
				break;
			}
		}
		System.out.println("插入完成时间:" + (System.currentTimeMillis() - start));
		
		start = System.currentTimeMillis();
		for (int i = 0; i < total; i++) {
			queue.put(Element.updateOf(new BdbPerson(i, "new:name:" + i)));
		}
		queue.flush();
		while (true) {
			if (queue.getConsumer().getState() == TimingConsumerState.RUNNING) {
				Thread.yield();
			} else {
				break;
			}
		}
		System.out.println("更新完成时间:" + (System.currentTimeMillis() - start));

		start = System.currentTimeMillis();
		for (int i = 0; i < total; i++) {
			queue.put(Element.removeOf(i, BdbPerson.class));
		}
		queue.flush();
		while (true) {
			if (queue.getConsumer().getState() == TimingConsumerState.RUNNING) {
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
