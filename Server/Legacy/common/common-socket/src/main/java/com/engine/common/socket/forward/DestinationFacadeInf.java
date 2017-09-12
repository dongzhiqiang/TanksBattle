package com.engine.common.socket.forward;

import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.core.Command;

/**
 * 业务服务器对通信服务器的管理接口
 * 
 */
@SocketModule(-1)
public interface DestinationFacadeInf {

	/**
	 * 获取换发的命令数组
	 * @return
	 */
	@SocketCommand(1)
	Command[] getForwardCommand();
}
