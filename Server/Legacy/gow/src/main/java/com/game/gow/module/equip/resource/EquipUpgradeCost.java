package com.game.gow.module.equip.resource;

import com.engine.common.resource.anno.Id;
import com.engine.common.resource.anno.Resource;

@Resource(value="equip")
public class EquipUpgradeCost {
	/** id*/
	@Id
	private String id;
	
	/** 消耗*/
	private String cost;

	public String getId() {
		return id;
	}

	public void setId(String id) {
		this.id = id;
	}

	public String getCost() {
		return cost;
	}

	public void setCost(String cost) {
		this.cost = cost;
	}


}
