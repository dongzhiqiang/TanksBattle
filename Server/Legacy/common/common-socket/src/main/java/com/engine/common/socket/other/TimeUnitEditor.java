package com.engine.common.socket.other;

import java.beans.PropertyEditor;
import java.beans.PropertyEditorSupport;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.TimeUnit;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

/**
 * {@link TimeUnit}的{@link PropertyEditor}
 * 
 */
public class TimeUnitEditor extends PropertyEditorSupport {

	private static final Logger logger = LoggerFactory.getLogger(LineDelimiterEditor.class);

	private final static Map<String, TimeUnit> UNITS = new HashMap<String, TimeUnit>();
	
	static {
		for (TimeUnit u : TimeUnit.values()) {
			UNITS.put(u.name(), u);
		}
	}

	private TimeUnit value;

	@Override
	public void setAsText(String text) throws IllegalArgumentException {
		value = UNITS.get(text.toUpperCase());
		if (value == null) {
			FormattingTuple message = MessageFormatter.format("无效的 TimeUnit 值[{}]", text);
			logger.error(message.getMessage());
			throw new IllegalArgumentException(message.getMessage());
		}
	}

	@Override
	public Object getValue() {
		return value;
	}
}
