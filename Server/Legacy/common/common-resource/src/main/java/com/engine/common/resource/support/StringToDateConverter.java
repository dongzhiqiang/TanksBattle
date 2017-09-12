package com.engine.common.resource.support;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;

import org.springframework.core.convert.converter.Converter;

/**
 * 字符串到日期对象的转换器
 * 
 * 
 */
public class StringToDateConverter implements Converter<String, Date> {

	@Override
	public Date convert(String source) {
		SimpleDateFormat df = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
		try {
			return df.parse(source);
		} catch (ParseException e) {
			throw new IllegalArgumentException("字符串[" + source + "]不符合格式要求[yyyy-MM-dd HH:mm:ss]", e);
		}
	}

}
