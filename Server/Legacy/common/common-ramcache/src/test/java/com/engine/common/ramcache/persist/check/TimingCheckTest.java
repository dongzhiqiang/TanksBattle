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
public class TimingCheckTest {

	@Inject
	private EntityCacheService<Integer, TimingCheckEntity> cacheService;

	private int total = 300000;
	
	@Test
	public void test() throws Exception {
		for (int i = 0; i < total; i++) {
			TimingCheckEntity entity = cacheService.loadOrCreate(i, new EntityBuilder<Integer, TimingCheckEntity>() {
				@Override
				public TimingCheckEntity newInstance(Integer id) {
					TimingCheckEntity result = new TimingCheckEntity();
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
