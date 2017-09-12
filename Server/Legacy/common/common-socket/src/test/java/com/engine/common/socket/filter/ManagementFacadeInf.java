package com.engine.common.socket.filter;

import com.engine.common.socket.anno.InSession;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.filter.ManagementFilter;

/**
 * {@link ManagementFilterTest} 使用的通信接口
 * 
 */
@SocketModule(2)
public interface ManagementFacadeInf {

	@SocketCommand(1)
	boolean isManagement(@InSession(value = ManagementFilter.MANAGEMENT, required = false)String name);
	
	@SocketCommand(2)
	String getName(@InSession(ManagementFilter.MANAGEMENT) String name);

}
