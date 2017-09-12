package com.game.gow.module.item.model;

import java.util.List;

import com.engine.common.protocol.annotation.Transable;

@Transable
public class ItemModifyVo {
	private List<ItemVo> modifiedItems;
	private List<Long> removedItems;
	
	public List<ItemVo> getModifiedItems() {
		return modifiedItems;
	}
	public void setModifiedItems(List<ItemVo> modifiedItems) {
		this.modifiedItems = modifiedItems;
	}
	public List<Long> getRemovedItems() {
		return removedItems;
	}
	public void setRemovedItems(List<Long> removedItems) {
		this.removedItems = removedItems;
	}
	
	/** 构造方法 */
	public static ItemModifyVo valueOf(List<ItemVo> modifiedItems, List<Long> removedItems)
	{
		ItemModifyVo result = new ItemModifyVo();
		result.modifiedItems = modifiedItems;
		result.removedItems = removedItems;
		return result;
	}
}
