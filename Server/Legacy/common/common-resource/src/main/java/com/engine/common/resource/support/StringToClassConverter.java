package com.engine.common.resource.support;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.beans.BeansException;
import org.springframework.context.ApplicationContext;
import org.springframework.context.ApplicationContextAware;
import org.springframework.core.convert.converter.Converter;

/**
 * {@link String} 到 {@link Class} 的 {@link Converter}
 * 
 */
@SuppressWarnings("rawtypes")
public class StringToClassConverter implements Converter<String, Class>, ApplicationContextAware {
	
	private final static Logger logger = LoggerFactory.getLogger(StringToClassConverter.class);
	
	@Override
	public Class convert(String source) {
		if (!StringUtils.contains(source, ".") && !source.startsWith("[")) {
			source = "java.lang." + source;
		}
		ClassLoader loader = applicationContext.getClassLoader();
		try {
			return Class.forName(source, true, loader);
		} catch (ClassNotFoundException e) {
			FormattingTuple message = MessageFormatter.format("无法将字符串[{}]转换为 Class", source);
			logger.error(message.getMessage());
			throw new IllegalArgumentException(message.getMessage());
		}
	}
	
	private ApplicationContext applicationContext;

	@Override
	public void setApplicationContext(ApplicationContext applicationContext) throws BeansException {
		this.applicationContext = applicationContext;
	}

}