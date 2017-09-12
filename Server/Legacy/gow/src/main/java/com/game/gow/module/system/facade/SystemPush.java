package com.game.gow.module.system.facade;

import static com.game.gow.module.system.facade.SystemModule.MODULE;
import static com.game.gow.module.system.facade.SystemModule.PUSH_SYSTEM_TIME;

import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.anno.SocketPush;

/**
 * 系统服务模块的推送接口
 * 
 * @author wenkin
 */
@SocketPush
@SocketModule(MODULE)
public interface SystemPush {

	/**
	 * 推送系统的当前时间
	 * @param time
	 */
	@SocketCommand(PUSH_SYSTEM_TIME)
	void systemTime(long time);
}
