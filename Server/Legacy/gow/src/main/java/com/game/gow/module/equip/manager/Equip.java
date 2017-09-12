package com.game.gow.module.equip.manager;

/**
 * 装备，作为角色身上属性存在
 */
public class Equip{

	/** 配置表ID */
	private int equipId;
	
	/** 等级 */
	private int level = 0;

	public int getEquipId() {
		return equipId;
	}

	public void setEquipId(int equipId) {
		this.equipId = equipId;
	}

	public int getLevel() {
		return level;
	}

	public void setLevel(int level) {
		this.level = level;
	}
	
}
