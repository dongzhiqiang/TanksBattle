package com.engine.common.socket.handler;

import java.io.IOException;
import java.util.Properties;

import javax.annotation.PostConstruct;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.beans.BeansException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationContext;
import org.springframework.context.ApplicationContextAware;
import org.springframework.core.convert.ConversionService;
import org.springframework.core.io.Resource;

/**
 * 配置支持对象
 * 
 */
public abstract class ConfigSupport implements ApplicationContextAware {
	
	/** 资源属性名 */
	public static final String PROP_NAME_LOCATION = "location";
	
	private static final Logger logger = LoggerFactory.getLogger(ConfigSupport.class);
	
	/** 配置文件位置 */
	private String location;
	/** 配置文件 */
	private Properties properties;

	@Autowired
	protected ConversionService conversionService;
	protected ApplicationContext applicationContext;
	
	/**
	 * 获取必要的配置键数组
	 * @return
	 */
	public abstract String[] getPropertyKeys();
	
	/**
	 * 执行初始化
	 */
	protected abstract void doInitialize();

	@PostConstruct
	public void initialize() {
		// 加载配置文件
		Resource resource = this.applicationContext.getResource(location);
		properties = new Properties();
		try {
			properties.load(resource.getInputStream());
		} catch (IOException e) {
			FormattingTuple message = MessageFormatter.format("资源[{}]加载失败", location);
			logger.error(message.getMessage(), e);
			throw new RuntimeException(message.getMessage(), e);
		}
		
		// 检查配置是否完整
		for (String key : getPropertyKeys()) {
			if (!properties.containsKey(key)) {
				FormattingTuple message = MessageFormatter.format("配置缺失，配置键[{}]", key);
				logger.error(message.getMessage());
				throw new RuntimeException(message.getMessage());
			}
		}
		
		// 执行初始化
		doInitialize();
	}

	/**
	 * 获取配置值
	 * @param key 配置键
	 * @return
	 */
	public String getProperty(String key) {
		return properties.getProperty(key);
	}
	
	/**
	 * 获取配置值
	 * @param <T>
	 * @param key
	 * @param clz
	 * @return
	 */
	public <T> T getProperty(String key, Class<T> clz) {
		return conversionService.convert(getProperty(key), clz);
	}
	
	// Getter and Setter ...
	
	public String getLocation() {
		return location;
	}
	
	public void setLocation(String location) {
		this.location = location;
	}
	
	// 实现接口的方法

	@Override
	public void setApplicationContext(ApplicationContext applicationContext) throws BeansException {
		this.applicationContext = applicationContext;
	}

}
