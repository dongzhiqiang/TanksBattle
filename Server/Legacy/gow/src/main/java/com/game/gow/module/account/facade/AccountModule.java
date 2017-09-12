package com.game.gow.module.account.facade;

import com.engine.common.socket.anno.SocketDefine;

/**
 * 账号模块指令定义
 * 
 *@author wenkin
 */
@SocketDefine
public interface AccountModule {

	/** 当前的模块标识:10 */
	byte MODULE = 10;

	/** 账号模块的通信模块定义:[10] */
	byte[] MODULES = { MODULE };

	// 指令值定义部分

	/** 账号登录 */
	int COMMAND_LOGIN = 1;

	/** 重登录(断线重连的登录方法) */
	int COMMAND_RELOGIN = 2;
	
	/** 获取账号信息 */
	int COMMAND_LOGIN_INFO = 3;

	/** 检查账号是否存在 */
	int COMMAND_CHECK_ACCOUNT = 4;
	
	/**选择职业*/
	int COMMAND_CHOOSE_PROFESSION=5;

	/** 修改角色名 */
	int COMMAND_RENAME = 6;

	/** 登录完成 */
	int COMMAND_LOGIN_COMPLETE = 7;
	
	// 推送部分的指令定义

	/** 用户强制退出(发生在同一个账号有重复登录时) */
	int PUSH_ENFORCE_LOGOUT = -1;

}
