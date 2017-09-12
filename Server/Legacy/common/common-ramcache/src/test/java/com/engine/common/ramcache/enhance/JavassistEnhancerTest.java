package com.engine.common.ramcache.enhance;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.junit.BeforeClass;
import org.junit.Test;

import com.engine.common.ramcache.enhance.JavassistEntityEnhancer;

public class JavassistEnhancerTest {

	private static MockCacheService cacheService = new MockCacheService();
	private static JavassistEntityEnhancer enhancer;

	@BeforeClass
	public static void before() {
		enhancer = new JavassistEntityEnhancer();
		enhancer.initialize(cacheService);
	}

	@Test
	public void test() {
		Item item = new Item();
		Item itemEnhanced = enhancer.transform(item);
		assertThat(itemEnhanced, notNullValue());
		assertTrue(item.getClass() != itemEnhanced.getClass());
		assertTrue(item.getContent().getClass() == itemEnhanced.getContent().getClass());

		Player player = new Player();
		Player enhanced = enhancer.transform(player);
		assertThat(enhanced, notNullValue());

		enhanced.setId(10);
		assertThat(enhanced.getId(), is(10));
		assertThat(player.getId(), is(10));

		enhanced.setName("Frank");
		assertThat(enhanced.getName(), is("Frank"));
		assertThat(player.getName(), is("Frank"));
		assertThat(cacheService.getId(), is(10));
		assertThat(cacheService.getEntity(), sameInstance(player));
		cacheService.clear();

		assertThat(enhanced.increaseGold(-10), is(false));
		assertThat(player.getGold(), is(0));
		assertThat(cacheService.getId(), nullValue());

		assertThat(enhanced.increaseGold(5), is(true));
		assertThat(enhanced.getGold(), is(5));
		assertThat(player.getGold(), is(5));
		assertThat(cacheService.getId(), is(10));
		assertThat(cacheService.getEntity(), sameInstance(player));
		cacheService.clear();

		try {
			enhanced.charge(-10);
			fail();
		} catch (Exception e) {
			assertThat(e, instanceOf(IllegalArgumentException.class));
			assertThat(player.getGold(), is(5));
			assertThat(cacheService.getId(), nullValue());
		}

		try {
			enhanced.charge(2);
		} catch (Exception e) {
			fail();
		}
		assertThat(enhanced.getGold(), is(3));
		assertThat(player.getGold(), is(3));
		assertThat(cacheService.getId(), is(10));
		assertThat(cacheService.getEntity(), sameInstance(player));
	}
	
	@Test
	public void test_primitive_array() {
		Player player = new Player();
		Player enhanced = enhancer.transform(player);
		int[] result = enhanced.getPrimitiveArray();
		assertThat(result.length, is(3));
		assertThat(result[0], is(0));
		assertThat(result[1], is(1));
		assertThat(result[2], is(2));
	}

	@Test
	public void test_object_array() {
		Player player = new Player();
		Player enhanced = enhancer.transform(player);
		Player[] result = enhanced.getObjectArray();
		assertThat(result.length, is(1));
		assertThat(result[0], is(enhanced));
	}

	@Test
	public void test_hashCode() {
		Player player = new Player();
		Player enhanced = enhancer.transform(player);
		assertThat(enhanced.hashCode(), is(player.hashCode()));
	}

	@Test
	public void test_equals() {
		Player player = new Player();
		player.setId(1);
		Player enhanced = enhancer.transform(player);
		player = new Player();
		player.setId(1);
		assertThat(enhanced.equals(player), is(true));
		assertThat(player.equals(enhanced), is(true));
	}

}
