package com.engine.common.ramcache.persist.check;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class BothCheckTest {
	
	@Inject
	private EntityCacheService<Integer, TimingCheckEntity> timingService;
	@Inject
	private EntityCacheService<Integer, QueueCheckEntity> queueService;

	private int total = 100000;
	
	@Test
	public void test() throws Exception {
		for (int i = 0; i < total; i++) {
			TimingCheckEntity entity1 = timingService.loadOrCreate(i, new EntityBuilder<Integer, TimingCheckEntity>() {
				@Override
				public TimingCheckEntity newInstance(Integer id) {
					TimingCheckEntity result = new TimingCheckEntity();
					result.setId(id);
					return result;
				}
			});
			QueueCheckEntity entity2 = queueService.loadOrCreate(i, new EntityBuilder<Integer, QueueCheckEntity>() {
				@Override
				public QueueCheckEntity newInstance(Integer id) {
					QueueCheckEntity result = new QueueCheckEntity();
					result.setId(id);
					return result;
				}
			});
			
			switch (i % 2) {
			case 0:
				timingService.remove(i);
				queueService.remove(i);
				break;
			case 1:
				entity1.setContent(String.valueOf(i));
				entity2.setContent(String.valueOf(i));
				break;
			default:
				break;
			}
		}
	}
}
