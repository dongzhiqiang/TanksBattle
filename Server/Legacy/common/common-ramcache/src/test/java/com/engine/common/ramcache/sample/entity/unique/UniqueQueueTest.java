package com.engine.common.ramcache.sample.entity.unique;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.exception.UniqueFieldException;
import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.ramcache.orm.Querier;
import com.engine.common.ramcache.persist.QueuePersister;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class UniqueQueueTest {

	@Inject
	private EntityCacheService<Integer, Player> playerService;
	
	@Autowired
	private Accessor accessor;
	@Autowired
	private Querier querier;
	
	@Test
	public void test_create() throws InterruptedException {
		assertThat(playerService.hasUniqueValue("name", "test_create"), is(false));
		Player player1 = playerService.loadOrCreate(1, new EntityBuilder<Integer, Player>() {
			@Override
			public Player newInstance(Integer id) {
				Player result = new Player();
				result.setId(id);
				result.setName("test_create");
				return result;
			}
		});
		assertThat(playerService.hasUniqueValue("name", "test_create"), is(true));
		
		Player player2 = playerService.unique("name", "test_create");
		assertThat(player2, sameInstance(player1));
		
		wait4queueEmpty();
		
		Player entity = accessor.load(Player.class, 1);
		assertThat(entity, notNullValue());
		assertThat(entity.getId(), is(1));
		assertThat(entity.getName(), is("test_create"));
		
		entity = querier.unique(Player.class, "Player.name", "test_create");
		assertThat(entity, notNullValue());
		assertThat(entity.getId(), is(1));
		assertThat(entity.getName(), is("test_create"));
	}
	
	@Test
	public void test_unique() {
		Player entity = playerService.unique("name", "test_unique");
		assertThat(entity, nullValue());
		
		entity = new Player();
		entity.setId(2);
		entity.setName("test_unique");
		accessor.save(Player.class, entity);
		
		entity = playerService.unique("name", "test_unique");
		assertThat(entity, notNullValue());
		assertThat(entity.getId(), is(2));
		
		Player other = playerService.unique("name", "test_unique");
		assertThat(other, sameInstance(entity));
		other = playerService.load(2);
		assertThat(other, sameInstance(entity));
	}
	
	@Test
	public void test_update() throws InterruptedException {
		Player player = playerService.load(1);
		try {
			player.setName("test_unique");
			fail();
		} catch (Exception e) {
			assertThat(e, instanceOf(UniqueFieldException.class));
		}
		
		player.setName("test_update");
		assertThat(player.getName(), is("test_update"));
		
		Player other = playerService.unique("name", "test_update");
		assertThat(other, sameInstance(player));
	}
	
	@Test
	public void test_remove() throws InterruptedException {
		final Player player = playerService.unique("name", "test_update");
		assertThat(player, notNullValue());
		
		playerService.remove(player.getId());
		Player entity = playerService.unique("name", "test_update");
		assertThat(entity, nullValue());
		wait4queueEmpty();
		entity = playerService.loadOrCreate(player.getId(), new EntityBuilder<Integer, Player>() {
			@Override
			public Player newInstance(Integer id) {
				Player result = new Player();
				result.setId(player.getId());
				result.setName("test_update");
				return result;
			}
		});
		assertThat(entity, not(sameInstance(player)));
		
		Player other = playerService.load(player.getId());
		assertThat(other, sameInstance(entity));
		other = playerService.unique("name", "test_update");
		assertThat(other, sameInstance(entity));
		
		wait4queueEmpty();
		
		entity = accessor.load(Player.class, player.getId());
		assertThat(entity, notNullValue());
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
