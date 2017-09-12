package com.game.gow.module.equip.exception;

import com.game.gow.module.equip.facade.EquipResult;

/**
 * 装备异常代码值表
 */
public interface EquipExceptionCode {
	
	/** 装备不存在 */
	int EQUIP_DO_NOT_EXISTS = EquipResult.EQUIP_DO_NOT_EXISTS;
	
	/** 装备等级超过玩家等级 */
	int EQUIP_PLAYER_LEVEL_LIMIT = EquipResult.EQUIP_PLAYER_LEVEL_LIMIT;
	
	/** 装备等级超过配置等级 */
	int EQUIP_LEVEL_LIMIT = EquipResult.EQUIP_LEVEL_LIMIT;
	
	/** 所需材料不足 */
	int EQUIP_NO_ENOUGH_ITEM = EquipResult.EQUIP_NO_ENOUGH_ITEM;
	
	/** 装备等级未达到升品条件 */
	int EQUIP_NO_LEVEL = EquipResult.EQUIP_NO_LEVEL;
	
	/** 装备已在对应装备位 */
	int EQUIP_IS_IN_POS = EquipResult.EQUIP_IS_IN_POS;
	
	/** 装备位错误 */
	int EQUIP_IS_NOT_POS_TYPE = EquipResult.EQUIP_IS_NOT_POS_TYPE;
	
	/** 装备未装备上 */
	int EQUIP_NO_DRESSED = EquipResult.EQUIP_NO_DRESSED;
}
