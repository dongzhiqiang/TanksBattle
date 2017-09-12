package com.engine.common.ramcache.sample.region.basic;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.util.Collection;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.exception.InvaildEntityException;
import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.ramcache.persist.QueuePersister;
import com.engine.common.ramcache.service.IndexValue;
import com.engine.common.ramcache.service.RegionCacheService;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class BasicQueueTest {

	@Inject
	private RegionCacheService<Integer, BasicItem> itemService;
	
	@Autowired
	private Accessor accessor;
	
	private IndexValue idx = IndexValue.valueOf("owner", 1);

	@Test
	public void test_create() throws InterruptedException {
		System.out.println("[test_create]:重复创建");
		try {
			itemService.create(BasicItem.valueOf(0, idx.getValue(int.class), 10));
			fail();
		} catch (InvaildEntityException e) {
		}
		
		System.out.println("[test_create]:正常创建");
		BasicItem item1 = itemService.create(BasicItem.valueOf(1, idx.getValue(int.class), 10));
		try {
			itemService.create(BasicItem.valueOf(1, idx.getValue(int.class), 10));
			fail();
		} catch (InvaildEntityException e) {
		}
		BasicItem item2 = itemService.create(BasicItem.valueOf(2, idx.getValue(int.class), 10));
		Collection<BasicItem> items = itemService.load(idx);
		assertThat(items.size(), is(2));
		assertThat(items.contains(item1), is(true));
		assertThat(items.contains(item2), is(true));
		
		System.out.println("[test_create]:缓存加载");
		BasicItem other = itemService.get(idx, 1);
		assertThat(other, sameInstance(item1));
		other = itemService.get(idx, 2);
		assertThat(other, sameInstance(item2));
		
		wait4queueEmpty();
		System.out.println("[test_create]:数据库加载");
		BasicItem entity = accessor.load(BasicItem.class, 1);
		assertThat(entity, notNullValue());
		entity = accessor.load(BasicItem.class, 2);
		assertThat(entity, notNullValue());
	}
	
	@Test
	public void test_update() throws InterruptedException {
		System.out.println("[test_update]:缓存加载");
		BasicItem item = itemService.get(idx, 1);
		System.out.println("[test_update]:更新数据");
		item.setAmount(1);
		BasicItem other = itemService.get(idx, 1);
		assertThat(other, sameInstance(item));
		assertThat(other.getAmount(), is(1));
		
		wait4queueEmpty();
		System.out.println("[test_update]:数据库加载");
		BasicItem entity = accessor.load(BasicItem.class, 1);
		assertThat(entity.getAmount(), is(1));
	}

	@Test
	public void test_delete() throws InterruptedException {
		System.out.println("[test_delete]:缓存加载");
		BasicItem item = itemService.get(idx, 1);
		System.out.println("[test_delete]:删除实体");
		itemService.remove(item);
		BasicItem other = itemService.get(idx, 1);
		assertThat(other, nullValue());
		
		BasicItem item2 = itemService.get(idx, 2);
		Collection<BasicItem> items = itemService.load(idx);
		assertThat(items.size(), is(1));
		assertThat(items.contains(item), is(false));
		assertThat(items.contains(item2), is(true));
		
		wait4queueEmpty();
		System.out.println("[test_delete]:数据库加载");
		BasicItem entity = accessor.load(BasicItem.class, 1);
		assertThat(entity, nullValue());
	}
	
	@Test
	public void test_enhance() throws InterruptedException {
		System.out.println("[test_enhance]:缓存加载");
		BasicItem item = itemService.get(idx, 2);
		
		item.increaseAmount(10);
		assertThat(item.increaseAmount(-1), is(false));
		assertThat(item.getAmount(), is(20));
		
		try {
			item.decreaseAmount(10);
		} catch (AmountNotEnoughException e) {
			fail();
		}
		assertThat(item.getAmount(), is(10));
		try {
			item.decreaseAmount(100);
			fail();
		} catch (AmountNotEnoughException e) {
			assertThat(e.getValue(), is(90));
		}
		assertThat(item.getAmount(), is(0));
		
		wait4queueEmpty();
		System.out.println("[test_enhance]:数据库加载");
		BasicItem entity = accessor.load(BasicItem.class, 2);
		assertThat(entity.getAmount(), is(0));
	}

	/** 等待更新队列清空 */
	private void wait4queueEmpty() throws InterruptedException {
		QueuePersister persister = (QueuePersister) itemService.getPersister();
		while (persister.size() > 0) {
			Thread.yield();
		}
		Thread.sleep(100);
	}
}
