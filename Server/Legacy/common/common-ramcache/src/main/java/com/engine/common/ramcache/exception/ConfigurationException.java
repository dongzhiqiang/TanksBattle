package com.engine.common.ramcache.exception;

/**
 * 配置异常
 * 
 */
public class ConfigurationException extends CacheException {

	private static final long serialVersionUID = -8339968384335603091L;

	public ConfigurationException() {
		super();
	}

	public ConfigurationException(String message, Throwable cause) {
		super(message, cause);
	}

	public ConfigurationException(String message) {
		super(message);
	}

	public ConfigurationException(Throwable cause) {
		super(cause);
	}
	
}
