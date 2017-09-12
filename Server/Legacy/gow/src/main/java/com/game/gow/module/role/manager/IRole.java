package com.game.gow.module.role.manager;

import java.util.List;

import com.game.gow.module.equip.manager.Equip;


/**
 *  主角和宠物通用的属性放在此接口里 
 *  */
public interface IRole 
{	
	List<Equip> getEquips();
	void setEquips(List<Equip> equips);
	void setCurrentWeapon(int currentWeapon);
	int getLevel();
}
