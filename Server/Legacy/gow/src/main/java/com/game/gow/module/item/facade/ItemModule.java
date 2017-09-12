package com.game.gow.module.item.facade;

import com.engine.common.socket.anno.SocketDefine;

/**
 * 道具模块指令定义
 */
@SocketDefine
public interface ItemModule {
	/** 当前的模块标识:15 */
	byte MODULE = 15;
	
	/** 账号模块的通信模块定义:[15] */
	byte[] MODULES = { MODULE };
	
	
	// 推送部分的指令定义

	/** 道具发生修改 */
	int PUSH_ITEM_MODIFY = -1;
}
