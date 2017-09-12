package com.engine.common.ramcache.lock;

import java.lang.ref.WeakReference;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;
import java.util.WeakHashMap;

import org.hamcrest.CoreMatchers;
import org.junit.Assert;
import org.junit.Test;

/**
 * WeakHashMap 使用测试
 * 
 */
public class WeakHashMapTest {
	
	class MyObject {
		final String name;
		public MyObject(String name) {
			this.name = name;
		}
		@Override
		public int hashCode() {
			final int prime = 31;
			int result = 1;
			result = prime * result + ((name == null) ? 0 : name.hashCode());
			return result;
		}
		@Override
		public boolean equals(Object obj) {
			if (this == obj)
				return true;
			if (obj == null)
				return false;
			if (getClass() != obj.getClass())
				return false;
			MyObject other = (MyObject) obj;
			if (name == null) {
				if (other.name != null)
					return false;
			} else if (!name.equals(other.name))
				return false;
			return true;
		}
	}
	
	class MyObjectProxy extends MyObject {
		private final MyObject obj;
		public MyObjectProxy(MyObject obj) {
			super(null);
			this.obj = obj;
		}
		public String getName() {
			return obj.name;
		}
		@Override
		public int hashCode() {
			return obj.hashCode();
		}
		@Override
		public boolean equals(Object obj) {
			return this.obj.equals(obj);
		}
	}
	
	@Test
	public void test_object() throws InterruptedException {
		Map<Integer, String> map = new WeakHashMap<Integer, String>();
		
		map.put(new Integer(1), "Value One");
		Assert.assertThat(map.size(), CoreMatchers.is(1));
		Assert.assertThat(map.get(new Integer(1)), CoreMatchers.is("Value One"));
		
		System.gc();
		Thread.sleep(100);
		Assert.assertThat(map.get(new Integer(1)), CoreMatchers.nullValue());
		Assert.assertThat(map.size(), CoreMatchers.is(0));
		
		map.put(2, "Value Two");
		Assert.assertThat(map.size(), CoreMatchers.is(1));
		Assert.assertThat(map.get(2), CoreMatchers.is("Value Two"));
		Assert.assertThat(map.get(new Integer(2)), CoreMatchers.is("Value Two"));
		
		System.gc();
		Thread.sleep(100);
		Assert.assertThat(map.size(), CoreMatchers.is(1));
		Assert.assertThat(map.get(2), CoreMatchers.is("Value Two"));
		Assert.assertThat(map.get(new Integer(2)), CoreMatchers.is("Value Two"));
	}
	
	private Set<MyObject> holder = new HashSet<MyObject>();
	
	@Test
	public void test_custom_object() throws InterruptedException {
		Map<MyObject, String> map = new WeakHashMap<MyObject, String>();
		map.put(new MyObject("Key One"), "Value One");
		
		Assert.assertThat(map.size(), CoreMatchers.is(1));
		Assert.assertThat(map.get(new MyObject("Key One")), CoreMatchers.is("Value One"));
		
		System.gc();
		Thread.sleep(100);
		Assert.assertThat(map.get(new MyObject("Key One")), CoreMatchers.nullValue());
		Assert.assertThat(map.size(), CoreMatchers.is(0));
		
		holder.add(new MyObject("Key One"));
		map.put(holder.iterator().next(), "Value One");
		Assert.assertThat(map.size(), CoreMatchers.is(1));
		Assert.assertThat(map.get(new MyObject("Key One")), CoreMatchers.is("Value One"));
		System.gc();
		Thread.sleep(100);
		Assert.assertThat(map.size(), CoreMatchers.is(1));
		Assert.assertThat(map.get(new MyObject("Key One")), CoreMatchers.is("Value One"));
	}
	
	@Test
	public void test_key_value_one() throws InterruptedException {
		Map<MyObject, MyObject> map = new WeakHashMap<MyObject, MyObject>();
		MyObject obj = new MyObject("TEST");
		map.put(obj, obj);
		Assert.assertThat(map.size(), CoreMatchers.is(1));
		
		obj = null;
		System.gc();
		Thread.sleep(100);
		Assert.assertThat(map.size(), CoreMatchers.is(1));
	}

	@Test
	public void test_key_value_two() throws InterruptedException {
		Map<MyObject, WeakReference<MyObject>> map = new WeakHashMap<MyObject, WeakReference<MyObject>>();
		MyObject obj = new MyObject("TEST");
		map.put(obj, new WeakReference<MyObject>(obj));
		Assert.assertThat(map.size(), CoreMatchers.is(1));
		obj = new MyObject("TEST");
		Assert.assertThat(map.get(obj), CoreMatchers.notNullValue());
		
		obj = null;
		System.gc();
		Thread.sleep(100);
		Assert.assertThat(map.size(), CoreMatchers.is(0));
	}

	@Test
	public void test_key_value_three() throws InterruptedException {
		Map<MyObject, WeakReference<MyObjectProxy>> map = new WeakHashMap<MyObject, WeakReference<MyObjectProxy>>();
		MyObject obj = new MyObject("TEST");
		MyObjectProxy proxy = new MyObjectProxy(obj); 
		map.put(obj, new WeakReference<MyObjectProxy>(proxy));
		Assert.assertThat(map.size(), CoreMatchers.is(1));
		obj = new MyObject("TEST");
		Assert.assertThat(map.get(obj), CoreMatchers.notNullValue());
		Assert.assertThat(map.get(obj).get(), CoreMatchers.notNullValue());

		obj = null;
		System.gc();
		Thread.sleep(100);
		Assert.assertThat(map.size(), CoreMatchers.is(1));
		obj = new MyObject("TEST");
		Assert.assertThat(map.get(obj), CoreMatchers.notNullValue());
		Assert.assertThat(map.get(obj).get(), CoreMatchers.notNullValue());
		
		obj = null;
		proxy = null;
		System.gc();
		Thread.sleep(100);
		Assert.assertThat(map.size(), CoreMatchers.is(0));
	}
}
