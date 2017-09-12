package com.engine.common.ramcache.enhance.sample;

import static org.junit.Assert.*;

import java.util.Iterator;

import org.apache.commons.collections.bidimap.DualHashBidiMap;
import org.junit.BeforeClass;
import org.junit.Test;

import com.engine.common.ramcache.enhance.EnhancedEntity;
import com.engine.common.ramcache.enhance.JavassistEntityEnhancer;
import com.engine.common.ramcache.enhance.MockCacheService;
import com.googlecode.concurrentlinkedhashmap.ConcurrentLinkedHashMap;
import com.googlecode.concurrentlinkedhashmap.EvictionListener;
import com.googlecode.concurrentlinkedhashmap.ConcurrentLinkedHashMap.Builder;

public class EntityEnhancerTest {
	private static MockCacheService cacheService = new MockCacheService();
	static JavassistEntityEnhancer encher;
	
	@BeforeClass
	public static  void init(){
	    encher=new JavassistEntityEnhancer();
	    encher.initialize(cacheService);
	}
	@Test
	public void test() {
		Actor actor=new Actor();
		actor.setId(100);
		actor.setName("wenkin");
		actor.setIndexValue(new IndexValue(28, "xiang"));
		Actor afterActor=encher.transform(actor);
	    if(afterActor instanceof EnhancedEntity){
	       EnhancedEntity tempEnhancedEntity=(EnhancedEntity) afterActor;
	       Actor tempActor=(Actor) tempEnhancedEntity.getEntity();
	       assertTrue(tempActor==actor);
	       assertTrue(tempActor.getId()==actor.getId());
	       assertTrue(tempActor.getName()==actor.getName());
	       assertTrue(tempActor.getIndexValue()==actor.getIndexValue());
	    }
	    
	    assertTrue(afterActor instanceof EnhancedEntity);
		assertTrue(actor.getClass()!=afterActor.getClass());
		assertTrue(actor.getId()==afterActor.getId());
		assertTrue(actor.getName()==afterActor.getName());
		assertTrue(actor.getIndexValue().getIndex()==afterActor.getIndexValue().getIndex());
		assertTrue(actor.getIndexValue().getValue()==afterActor.getIndexValue().getValue());
		assertTrue(actor.getIndexValue()==afterActor.getIndexValue());
		assertTrue(actor.getIndexValue().equals(afterActor.getIndexValue()));
		System.out.println(afterActor.getId()+" "+afterActor.getName()+" --"+afterActor.getIndexValue().getIndex()+":"+afterActor.getIndexValue().getValue());
		afterActor.setId(1000);
		afterActor.setName("wenjun");
		afterActor.setIndexValue(new IndexValue(128, "wenjun"));
		assertTrue(actor.getClass()!=afterActor.getClass());
		assertTrue(actor.getId()==afterActor.getId());
		assertTrue(actor.getName()==afterActor.getName());
		assertTrue(actor.getIndexValue().getIndex()==afterActor.getIndexValue().getIndex());
		assertTrue(actor.getIndexValue().getValue()==afterActor.getIndexValue().getValue());
		assertTrue(actor.getIndexValue()==afterActor.getIndexValue());
		assertTrue(actor.getIndexValue().equals(afterActor.getIndexValue()));
		System.out.println(actor.getId()+" "+actor.getName()+" --"+actor.getIndexValue().getIndex()+":"+actor.getIndexValue().getValue());	
	}
	@Test
	public void lrumap(){
		Builder<Integer,String> builder=new Builder<Integer, String>().initialCapacity(5).maximumWeightedCapacity(10);
		builder.listener(new EvictionListener<Integer, String>() {
			
			@Override
			public void onEviction(Integer key, String value) {
			      System.out.println("key:"+key+" value:"+value);
				
			}
		});
		ConcurrentLinkedHashMap<Integer, String> store=builder.build();
		for(int i=1;i<=10;i++){
			store.put(i, i+"");
		}
		assertTrue(store.get(1)!=null);
		store.put(11, 11+"");
        assertTrue(store.get(2)==null);
    	store.put(12, 12+"");
    	assertTrue(store.get(3)==null);
    	store.get(4);
    	store.put(13, 13+"");
    	assertTrue(store.get(5)==null);
    	assertTrue(store.get(6)!=null);
    	for(int i=7;i<=10;i++){
    		assertTrue(store.get(i)!=null);
    	}
	}

}
