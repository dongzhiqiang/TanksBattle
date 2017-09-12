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
import com.engine.common.ramcache.persist.QueuePersister;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class QueuePerformanceTest implements ApplicationContextAware {
	
	private QueuePersister persister;

	private int total = 10000;
	
	@Test
	public void test() throws Exception {
		Accessor accessor = applicationContext.getBean(Accessor.class);
		persister = new QueuePersister();
		persister.initialize("test", accessor, "-1:true");
		
		long start = System.currentTimeMillis();
		for (int i = 0; i < total; i++) {
			persister.put(Element.saveOf(new Person(i, "name:" + i)));
		}
		while (true) {
			if (persister.size() == 0) {
				break;
			}
			Thread.sleep(1);
		}
		System.out.println("插入完成时间:" + (System.currentTimeMillis() - start));
		
		start = System.currentTimeMillis();
		for (int i = 0; i < total; i++) {
			persister.put(Element.updateOf(new Person(i, "new:name:" + i)));
		}
		while (true) {
			if (persister.size() == 0) {
				break;
			}
			Thread.sleep(1);
		}
		System.out.println("更新完成时间:" + (System.currentTimeMillis() - start));

		start = System.currentTimeMillis();
		for (int i = 0; i < total; i++) {
			accessor.load(Person.class, i);
		}
		System.out.println("查询完成时间:" + (System.currentTimeMillis() - start));

		start = System.currentTimeMillis();
		for (int i = 0; i < total; i++) {
			persister.put(Element.removeOf(i, Person.class));
		}
		while (true) {
			if (persister.size() == 0) {
				break;
			}
			Thread.sleep(1);
		}
		System.out.println("删除完成时间:" + (System.currentTimeMillis() - start));
	}

	
	private ConfigurableApplicationContext applicationContext;
	
	@Override
	public void setApplicationContext(ApplicationContext applicationContext) throws BeansException {
		this.applicationContext = (ConfigurableApplicationContext) applicationContext;
	}
}
