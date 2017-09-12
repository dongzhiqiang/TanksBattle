package com.game.gow.module.equip.facade;

import com.engine.common.protocol.annotation.Constant;
import com.engine.common.utils.model.ResultCode;

/**
 * 账号服务状态码声明
 */
@Constant
public interface EquipResult extends ResultCode {

	/** 装备不存在 */
	int EQUIP_DO_NOT_EXISTS = -1;
	
	/** 装备等级超过玩家等级 */
	int EQUIP_PLAYER_LEVEL_LIMIT = -2;
	
	/** 装备等级超过配置等级 */
	int EQUIP_LEVEL_LIMIT = -3;
	
	/** 所需材料不足 */
	int EQUIP_NO_ENOUGH_ITEM = -4;
	
	/** 装备等级未达到升品条件 */
	int EQUIP_NO_LEVEL = -5;
	
	/** 装备已在对应装备位 */
	int EQUIP_IS_IN_POS = -6;
	
	/** 装备位错误 */
	int EQUIP_IS_NOT_POS_TYPE = -7;
	
	/** 装备未装备上 */
	int EQUIP_NO_DRESSED = -8;
}
