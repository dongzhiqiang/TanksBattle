package com.engine.common.ramcache.service.test;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

import com.googlecode.concurrentlinkedhashmap.ConcurrentLinkedHashMap.Builder;

public class LruTest {

	public static void main(String[] args) {
		ConcurrentHashMap<String, ConcurrentMap<Object, String>> cache = new ConcurrentHashMap<String, ConcurrentMap<Object, String>>();
		Builder<Object, String> builder = 
				new Builder<Object, String>()
				.initialCapacity(16) 			// 设置调整大小因子
				.maximumWeightedCapacity(10) 	// 设置最大元素数量(可能会临时超出该数值)
				.concurrencyLevel(16); 		// 设置并发更新线程数预计值
		cache.put("owner", builder.build());
		
		Map<Object, String> target = cache.get("owner");
		for (int i = 0; i < 10000; i++) {
			target.put(i, String.valueOf(i));
		}
		System.out.println(target.size());
		System.out.println(target.values());
	}

}
