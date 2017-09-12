package com.engine.common.ramcache.sample.entity.basic;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.ramcache.persist.QueuePersister;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class BasicQueueTest {

	@Inject
	private EntityCacheService<Integer, Player> playerService;
	
	@Autowired
	private Accessor accessor;
	
	@Test
	public void test_create() throws InterruptedException {
		final String initName = "test_create";
		Player player1 = playerService.loadOrCreate(1, new EntityBuilder<Integer, Player>() {
			@Override
			public Player newInstance(Integer id) {
				return Player.valueOf(id, initName, 0);
			}
		});
		Player player2 = playerService.load(1);
		assertThat(player2, sameInstance(player1));
		
		wait4queueEmpty();
		
		Player entity = accessor.load(Player.class, 1);
		assertThat(entity, notNullValue());
		assertThat(entity.getId(), is(1));
		assertThat(entity.getName(), is("test_create"));
		assertThat(entity, not(sameInstance(player1)));
		assertThat(entity, not(sameInstance(player2)));
	}
	
	@Test
	public void test_update() throws InterruptedException {
		Player player = playerService.load(1);
		assertThat(player, notNullValue());
		try {
			player.charge(-10);
			fail();
		} catch (ChargeFailException e) {
		} finally {
			Player entity = accessor.load(Player.class, 1);
			assertThat(entity.getGold(), is(0));
		}
		
		boolean result = player.increaseGold(-10);
		assertThat(result, is(false));
		Player entity = accessor.load(Player.class, 1);
		assertThat(entity.getGold(), is(0));
		
		result = player.increaseGold(10);
		assertThat(result, is(true));
		
		wait4queueEmpty();
		
		entity = accessor.load(Player.class, 1);
		assertThat(entity.getGold(), is(10));
	}

	@Test
	public void test_remove() throws InterruptedException {
		playerService.remove(1);
		Player player = playerService.load(1);
		assertThat(player, nullValue());
		
		wait4queueEmpty();
		
		Player entity = accessor.load(Player.class, 1);
		assertThat(entity, nullValue());
	}

	/** 等待更新队列清空 */
	private void wait4queueEmpty() throws InterruptedException {
		QueuePersister persister = (QueuePersister) playerService.getPersister();
		while (persister.size() > 0) {
			Thread.yield();
		}
		Thread.sleep(100);
	}
}
