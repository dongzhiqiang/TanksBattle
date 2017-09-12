package com.game.gow.module.equip.model;


import com.engine.common.protocol.annotation.Transable;

/**
 * 装备VO
 */
@Transable
public class ChangeWeaponResultVo {
	/** 角色ID */
	private long roleId;
	/** 当前武器*/
	private int currentWeapon;




	
	/** 构造方法 */
	public static ChangeWeaponResultVo valueOf(long roleId, int currentWeapon)
	{
		ChangeWeaponResultVo result = new ChangeWeaponResultVo();
		result.roleId = roleId;
		result.currentWeapon = currentWeapon;

		return result;
	}





	public long getRoleId() {
		return roleId;
	}





	public void setRoleId(long roleId) {
		this.roleId = roleId;
	}





	public int getCurrentWeapon() {
		return currentWeapon;
	}





	public void setCurrentWeapon(int currentWeapon) {
		this.currentWeapon = currentWeapon;
	}


}
