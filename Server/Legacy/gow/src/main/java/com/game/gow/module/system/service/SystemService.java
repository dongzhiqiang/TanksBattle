package com.game.gow.module.system.service;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.engine.common.socket.core.Request;
import com.engine.common.socket.filter.session.SessionManager;
import com.game.gow.module.system.facade.SystemCommand;
import com.game.gow.module.system.manager.GlobalServerManager;

/**
 * 系统服务对象
 * 
 * @author wenkin
 */
@Service
public class SystemService {

	@Autowired
	private SessionManager sessionManager;
	
	@Autowired
	private GlobalServerManager globalServerManager;

//	@Scheduled(name = "定时系统时间推送", value = "push_system_time", type = ValueType.BEANNAME)
	protected void time() {
		long currentTime = System.currentTimeMillis();
		Request<Long> request = Request.valueOf(SystemCommand.SYSTEM_TIME, currentTime);
		sessionManager.sendAllIdentified(request);
	}

	public GlobalServerManager getGlobalServerManager() {
		return globalServerManager;
	}
}
