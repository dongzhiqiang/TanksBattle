package com.engine.common.ramcache.enhance;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.CachedEntityConfig;
import com.engine.common.ramcache.enhance.EnhancedEntity;
import com.engine.common.ramcache.service.RegionEnhanceService;

@SuppressWarnings("rawtypes")
public class ItemEnhance extends Item implements EnhancedEntity {

	private final RegionEnhanceService service;
	private final Item entity;

	public ItemEnhance(RegionEnhanceService service, Item entity) {
		this.service = service;
		this.entity = entity;
	}

	@Override
	public IEntity getEntity() {
		return entity;
	}

	// Enhanced Methods

	@SuppressWarnings("unchecked")
	public void setOwner(int owner) {
		CachedEntityConfig config = service.getEntityConfig();
		Object prev = config.getIndexValue("owner", entity);
		entity.setOwner(owner);
		service.changeIndexValue("owner", this, prev);
		service.writeBack(entity.getId(), entity);
	}

	// Delegate's ...

	public boolean equals(Object obj) {
		return entity.equals(obj);
	}

	public Integer getId() {
		return entity.getId();
	}

	public int getOwner() {
		return entity.getOwner();
	}

	public int hashCode() {
		return entity.hashCode();
	}

	public String toString() {
		return entity.toString();
	}

}
