package com.game.gow.module.system.facade;

import com.engine.common.socket.anno.SocketDefine;

/**
 * 系统服务模块指令定义
 * 
 * @author wenkin
 */
@SocketDefine
public interface SystemModule {

	/** 当前的模块标识:0 */
	byte MODULE = 0;

	/** 频道模块的通信模块定义:[0] */
	byte[] MODULES = { MODULE };

	// 指令值定义部分

	/** 获取传输对象定义 */
	int REQUEST_DESCRIPTION = 1;

	/** 获取系统时间 */
	int COMMAND_SYSTEM_TIME = 2;

	/** 获取传输对象定义MD5特征码 */
	int MD5_DESCRIPTION = 3;

	// 推送指令定义部分

	/** 推送系统时间 */
	int PUSH_SYSTEM_TIME = -1;

}
