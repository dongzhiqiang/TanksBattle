package com.engine.common.console;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.context.support.AbstractApplicationContext;
import org.springframework.context.support.ClassPathXmlApplicationContext;

import com.engine.common.console.Console;

/**
 * 服务的启动方法
 * 
 */
public class Main {
	
	private static final Logger logger = LoggerFactory.getLogger(Main.class);
	
	/** 默认的上下文配置名 */
	private static final String DEFAULT_APPLICATION_CONTEXT = "applicationContext.xml";

	public static void main(String[] args) {
		String[] contexts = null;
		if (args.length == 0) {
			logger.warn("启动加载的环境配置文件未指定，将使用默认配置文件[{}]替代", DEFAULT_APPLICATION_CONTEXT);
			contexts = new String[]{DEFAULT_APPLICATION_CONTEXT};
		} else {
			contexts = args;
		}
		
		AbstractApplicationContext applicationContext = new ClassPathXmlApplicationContext(contexts);
		try {
			logger.warn("容器已启动完成，开启服务器控制台");
			Console console = new Console(applicationContext);
			console.start();
		} catch (Exception e) {
			e.printStackTrace();
			if (applicationContext.isRunning()) {
				applicationContext.destroy();
			}
		}
	}

}
