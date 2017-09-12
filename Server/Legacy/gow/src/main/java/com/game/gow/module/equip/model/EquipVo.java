package com.game.gow.module.equip.model;


import com.engine.common.protocol.annotation.Transable;
import com.game.gow.module.equip.manager.Equip;

/**
 * 装备VO
 */
@Transable
public class EquipVo {
	/** 配置表ID */
	private int equipId;
	/** 装备等级 */
	private int level = 0;
	/** 持有者id，当作为结果时才使用*/
	private long roleId = 0;

	public int getLevel() {
		return level;
	}
	public void setLevel(int level) {
		this.level = level;
	}

	
	/** 构造方法 */
	public static EquipVo valueOf(Equip equip)
	{
		if(equip == null)return null;
		EquipVo result = new EquipVo();
		result.equipId = equip.getEquipId();
		result.level = equip.getLevel();
		result.level = equip.getLevel();

		return result;
	}

	public int getEquipId() {
		return equipId;
	}
	public void setEquipId(int equipId) {
		this.equipId = equipId;
	}
	public long getRoleId() {
		return roleId;
	}
	public void setRoleId(long roleId) {
		this.roleId = roleId;
	}
}
