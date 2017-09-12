package com.game.gow.module.equip.facade;

import com.engine.common.socket.anno.SocketDefine;

/**
 * 账号模块指令定义
 */
@SocketDefine
public interface EquipModule {
	/** 当前的模块标识:12 */
	byte MODULE = 12;
	
	/** 账号模块的通信模块定义:[12] */
	byte[] MODULES = { MODULE };
	
	// 指令值定义部分

	/** 升级装备 */
	int COMMAND_UPGRADE = 1;
	
	/** 进阶装备 */
	int COMMAND_ADVANCE = 2;
	
	/** 觉醒装备 */
	int COMMAND_ROUSE = 3;
	
	/** 更换武器 */
	int COMMAND_CHANGE_WEAPON = 4;
	
	/** 一键升级 */
	int COMMAND_UPGRADE_ONCE = 5;
}
