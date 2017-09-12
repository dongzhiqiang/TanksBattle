package com.game.gow.module.system.facade;

import static com.game.gow.module.system.facade.SystemModule.MODULES;
import static com.game.gow.module.system.facade.SystemModule.PUSH_SYSTEM_TIME;

import com.engine.common.socket.core.Command;

/**
 * 系统模块指令表
 * 
 * @author wenkin
 */
public interface SystemCommand {

	/**
	 * 系统时间推送指令
	 */
	Command SYSTEM_TIME = Command.valueOf(PUSH_SYSTEM_TIME, MODULES);
	
}
