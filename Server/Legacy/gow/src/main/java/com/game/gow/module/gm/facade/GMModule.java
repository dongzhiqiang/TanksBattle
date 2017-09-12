package com.game.gow.module.gm.facade;

import com.engine.common.socket.anno.SocketDefine;

/**
 * gm命令指令定义
 */
@SocketDefine
public interface GMModule {
	/** 当前的模块标识:13 */
	byte MODULE = 13;
	
	/** 账号模块的通信模块定义:[13] */
	byte[] MODULES = { MODULE };
	
	// 指令值定义部分

	/** 执行指令 */
	int COMMAND_PROCESS_GM_CMD = 1;
}
