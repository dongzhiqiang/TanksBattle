package com.game.gow.module.account.facade;

import static com.game.gow.module.account.facade.AccountModule.*;

import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.anno.SocketPush;
import com.engine.common.socket.core.Command;

/**
 * 账号服务推送接口
 * 
 *@author wenkin
 */
@SocketPush
@SocketModule(MODULE)
public interface AccountPush {

	/** 用户强制退出 */
	public static final Command ENFORCE_LOGOUT = Command.valueOf(PUSH_ENFORCE_LOGOUT, MODULES);

	/**
	 * 用户强制退出(发生在同一个账号有重复登录时)
	 */
	@SocketCommand(PUSH_ENFORCE_LOGOUT)
	void enforceLogout();

}
