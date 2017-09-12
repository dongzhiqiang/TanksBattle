package com.engine.common.socket.filter;

import org.apache.mina.core.filterchain.IoFilterAdapter;
import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * 策略文件过滤器
 * 
 * 
 */
public class PolicyFilter extends IoFilterAdapter {

	private final static Logger logger = LoggerFactory.getLogger(PolicyFilter.class);

	private final static byte[] policyResponse = "<?xml version=\"1.0\"?><cross-domain-policy><site-control permitted-cross-domain-policies=\"all\"/><allow-access-from domain=\"*\" to-ports=\"*\"/></cross-domain-policy>\0"
			.getBytes();

	@Override
	public void sessionOpened(NextFilter nextFilter, IoSession session) throws Exception {
		session.write(policyResponse);
		if (logger.isDebugEnabled()) {
			logger.debug("向会话[{}]返回策略文件", session.getId());
		}
		super.sessionOpened(nextFilter, session);
	}
}
