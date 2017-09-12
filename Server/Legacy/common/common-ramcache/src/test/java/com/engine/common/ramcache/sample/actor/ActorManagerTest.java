package com.engine.common.ramcache.sample.actor;

import java.util.Date;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.atomic.AtomicLong;

import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.orm.Querier;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class ActorManagerTest {
	@Inject
	EntityCacheService<Long, Actor> cache;

	@Test
	public void act() {
		long id=100;
		Actor actor=cache.loadOrCreate(id, new EntityBuilder<Long, Actor>() {
			
			@Override
			public Actor newInstance(Long id) {
				return Actor.valueOf(id);
			}
		});
		System.out.println("##########");
		AtomicInteger makeId=actor.getIdor();
        System.out.println("id:"+makeId.intValue());
        int after=makeId.incrementAndGet();
        System.out.println("after:"+after+"--"+makeId.intValue());
        actor.setMake(after);
        Actor actor2=cache.load(id);
        int v=actor2.getIdor().incrementAndGet();
        Date date=actor2.getDate();
        if(date==null){
        	System.out.println("date:"+null);
        }else{
        	System.out.println("date:"+date.getTime());
        }
        actor2.setMake(v);
	}
}
