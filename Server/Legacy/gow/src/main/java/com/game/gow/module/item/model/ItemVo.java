package com.game.gow.module.item.model;

import com.engine.common.protocol.annotation.Transable;
import com.game.gow.module.item.manager.Item;

/**
 * 物品VO
 */
@Transable
public class ItemVo {
	/** 唯一编号 */
	private Long id;
	/** 配置表ID */
	private int baseId;
	/** 道具数量 */
	private int num;
	
	public Long getId() {
		return id;
	}
	public void setId(Long id) {
		this.id = id;
	}
	public int getBaseId() {
		return baseId;
	}
	public void setBaseId(int baseId) {
		this.baseId = baseId;
	}
	public int getNum() {
		return num;
	}
	public void setNum(int num) {
		this.num = num;
	}
	/** 构造方法 */
	public static ItemVo valueOf(Item item, int itemType)
	{
		ItemVo result = new ItemVo();
		result.id = item.getId();
		result.baseId = item.getBaseId();
		result.num = item.getNum();

		return result;
	}

}
