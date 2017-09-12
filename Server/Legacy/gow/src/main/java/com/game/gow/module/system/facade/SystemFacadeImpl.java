package com.game.gow.module.system.facade;

import java.util.Date;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.engine.common.protocol.Transfer;

@Service
public class SystemFacadeImpl implements SystemFacade {

	private static final Logger logger = LoggerFactory.getLogger(SystemFacadeImpl.class);

	@Autowired
	private Transfer transfer;

	@Override
	public String md5Description() {
		if (transfer == null) {
			return null;
		}

		String md5 = transfer.getMD5Description();
		if (logger.isDebugEnabled()) {
			logger.debug("返回传输对象定义MD5[{}]", md5);
		}
		return md5;
	}

	@Override
	public byte[] requestDescription(String mis) {
		if (transfer == null) {
			return null;
		}
		byte[] description = transfer.getDescription();
		if (logger.isDebugEnabled()) {
			logger.debug("返回传输对象定义  - byte[{}]", description.length);
		}
		return description;
	}

	@Override
	public Date getSystemTime(long playerId) {
		return new Date();
	}

}
