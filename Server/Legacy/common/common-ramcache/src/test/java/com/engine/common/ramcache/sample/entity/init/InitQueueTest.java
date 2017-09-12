package com.engine.common.ramcache.sample.entity.init;

import java.util.Set;

import org.hamcrest.CoreMatchers;
import org.junit.Assert;
import org.junit.Ignore;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.persist.QueuePersister;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;
import com.engine.common.ramcache.service.Filter;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class InitQueueTest {

	@Inject
	private EntityCacheService<Integer, EntityOne> entityOneService;
	@Inject
	private EntityCacheService<Integer, EntityTwo> entityTwoService;
	
	@Test
	@Ignore
	public void test_init_data() throws Exception {
		for (int i = 0; i < 100; i++) {
			entityOneService.loadOrCreate(i, new EntityBuilder<Integer, EntityOne>() {
				@Override
				public EntityOne newInstance(Integer id) {
					EntityOne result = new EntityOne();
					result.setId(id);
					result.setName("name:" + id);
					return result;
				}
			});
			entityTwoService.loadOrCreate(i, new EntityBuilder<Integer, EntityTwo>() {
				@Override
				public EntityTwo newInstance(Integer id) {
					EntityTwo result = new EntityTwo();
					result.setId(id);
					result.setName("name:" + id);
					return result;
				}
			});
		}
		wait4queueEmpty(entityOneService);
	}
	
	@Test
	public void test_init_one() {
		Set<EntityOne> result = entityOneService.getFinder().find(new Filter<EntityOne>() {
			@Override
			public boolean isExclude(EntityOne entity) {
				return false;
			}
		});
		Assert.assertThat(result.size(), CoreMatchers.is(100));
	}
	
	@Test
	public void test_init_two() {
		Set<EntityTwo> result = entityTwoService.getFinder().find(new Filter<EntityTwo>() {
			@Override
			public boolean isExclude(EntityTwo entity) {
				return false;
			}
		});
		Assert.assertThat(result.size(), CoreMatchers.is(50));
		
		result = entityTwoService.getFinder().find(new Filter<EntityTwo>() {
			@Override
			public boolean isExclude(EntityTwo entity) {
				if (entity.getId() < 50) {
					return true;
				}
				return false;
			}
		});
		Assert.assertThat(result.size(), CoreMatchers.is(0));
	}
	
	/** 等待更新队列清空 */
	@SuppressWarnings("rawtypes")
	private void wait4queueEmpty(EntityCacheService service) throws InterruptedException {
		QueuePersister persister = (QueuePersister) service.getPersister();
		while (persister.size() > 0) {
			Thread.yield();
		}
		Thread.sleep(100);
	}
}
