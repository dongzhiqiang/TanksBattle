package com.game.gow.module.system.facade;

import static com.engine.common.socket.filter.ManagementFilter.MANAGEMENT;
import static com.engine.common.socket.filter.session.SessionManager.IDENTITY;
import static com.game.gow.module.system.facade.SystemModule.COMMAND_SYSTEM_TIME;
import static com.game.gow.module.system.facade.SystemModule.MD5_DESCRIPTION;
import static com.game.gow.module.system.facade.SystemModule.MODULE;
import static com.game.gow.module.system.facade.SystemModule.REQUEST_DESCRIPTION;

import java.util.Date;

import com.engine.common.socket.anno.InSession;
import com.engine.common.socket.anno.Raw;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;

/**
 * 系统服务模块
 * 
 * @author wenkin
 */
@SocketModule(MODULE)
public interface SystemFacade {

	/**
	 * 获取传输对象定义MD5串, 用于验证客户端本地存储的传输对象定义是否有效
	 */
	@SocketCommand(MD5_DESCRIPTION)
	String md5Description();

	/**
	 * 获取传输对象定义, 只能管理后台调用
	 */
	@SocketCommand(value = REQUEST_DESCRIPTION, raw = @Raw(response = true))
	byte[] requestDescription(@InSession(MANAGEMENT) String mis);

	/**
	 * 获取当前的系统时间
	 * @param playerId 玩家标识
	 * @return
	 */
	@SocketCommand(COMMAND_SYSTEM_TIME)
	Date getSystemTime(@InSession(IDENTITY) long playerId);
}
