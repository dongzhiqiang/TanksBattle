package com.game.gow;

import java.util.Date;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.context.support.ClassPathXmlApplicationContext;

import com.engine.common.utils.time.DateUtils;

/**
 * 启动服务器的指令
 * 
 */
public class Start {

	private static final Logger logger = LoggerFactory.getLogger(Start.class);

	/** 默认的上下文配置名 */
	private static final String DEFAULT_APPLICATION_CONTEXT = "applicationContext.xml";

	public static void main(String[] args) {
		ClassPathXmlApplicationContext applicationContext = null;
		try {
			applicationContext = new ClassPathXmlApplicationContext(DEFAULT_APPLICATION_CONTEXT);
		} catch (Exception e) {
			logger.error("初始化服务器应用上下文出错:{}", e.getMessage(), e);
			Runtime.getRuntime().exit(-1);
		}
		applicationContext.registerShutdownHook();
		applicationContext.start();
		
		logger.error("服务器已经启动 - [{}]", DateUtils.date2String(new Date(), DateUtils.PATTERN_DATE_TIME));
		while (applicationContext.isActive()) {
			try {
				Thread.sleep(1000);
			} catch (InterruptedException e) {
				if (logger.isDebugEnabled()) {
					logger.debug("服务器主线程被非法打断", e);
				}
			}
		}
		
		while(applicationContext.isRunning()) {
			Thread.yield();
		}
		logger.error("服务器已经关闭 - [{}]", DateUtils.date2String(new Date(), DateUtils.PATTERN_DATE_TIME));
		Runtime.getRuntime().exit(0);
	}

}
