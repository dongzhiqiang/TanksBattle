package com.engine.common.ramcache.persist.check;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.ServiceManager;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class QueueCheckTest {
	
	@Autowired
	private ServiceManager serviceManager;

	private int total = 300000;
	
	@SuppressWarnings("unchecked")
	@Test
	public void test() throws Exception {
		EntityCacheService<Integer, QueueCheckEntity> cacheService = serviceManager.getEntityService(QueueCheckEntity.class);
		for (int i = 0; i < total; i++) {
			QueueCheckEntity entity = cacheService.loadOrCreate(i, new EntityBuilder<Integer, QueueCheckEntity>() {
				@Override
				public QueueCheckEntity newInstance(Integer id) {
					QueueCheckEntity result = new QueueCheckEntity();
					result.setId(id);
					return result;
				}
			});
			
			switch (i % 2) {
			case 0:
				cacheService.remove(i);
				break;
			case 1:
				entity.setContent(String.valueOf(i));
				break;
			default:
				break;
			}
		}
	}
}
