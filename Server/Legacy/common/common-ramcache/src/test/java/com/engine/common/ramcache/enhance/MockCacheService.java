package com.engine.common.ramcache.enhance;

import java.util.HashMap;
import java.util.Map;

import com.engine.common.ramcache.anno.CachedEntityConfig;
import com.engine.common.ramcache.service.EntityEnhanceService;

public class MockCacheService implements EntityEnhanceService<Integer, Player> {

	private Integer id;
	private Player entity;
	private Map<String, Integer> constants = new HashMap<String, Integer>();
	private CachedEntityConfig config;
	
	public MockCacheService() {
		constants.put("default", 100);
		config = CachedEntityConfig.valueOf(Player.class, constants);
	}

	@Override
	public void writeBack(Integer id, Player entity) {
		this.id = id;
		this.entity = entity;
	}

	public Integer getId() {
		return id;
	}

	public Player getEntity() {
		return entity;
	}
	
	public void clear() {
		id = null;
		entity = null;
	}

	@Override
	public CachedEntityConfig getEntityConfig() {
		return config;
	}

	@Override
	public boolean hasUniqueValue(String name, Object value) {
		return false;
	}

	@Override
	public void replaceUniqueValue(Integer id, String name, Object value) {
	}

}
