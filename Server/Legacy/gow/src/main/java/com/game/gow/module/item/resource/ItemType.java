package com.game.gow.module.item.resource;

import com.engine.common.resource.anno.Id;
import com.engine.common.resource.anno.Resource;

@Resource(value="item")
public class ItemType {
	/** 子类型*/
	@Id
	private int subType;
	
	/** 类型*/
	private int type;
	
	private int itemId;

	public int getSubType() {
		return subType;
	}

	public void setSubType(int subType) {
		this.subType = subType;
	}

	public int getType() {
		return type;
	}

	public void setType(int type) {
		this.type = type;
	}

	public int getItemId() {
		return itemId;
	}

	public void setItemId(int itemId) {
		this.itemId = itemId;
	}
}
