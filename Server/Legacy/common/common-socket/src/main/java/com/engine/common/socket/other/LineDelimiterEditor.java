package com.engine.common.socket.other;

import java.beans.PropertyEditor;
import java.beans.PropertyEditorSupport;

import org.apache.mina.filter.codec.textline.LineDelimiter;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

/**
 * {@link LineDelimiter}的{@link PropertyEditor}
 * 
 */
public class LineDelimiterEditor extends PropertyEditorSupport {
	
	private static final Logger logger = LoggerFactory.getLogger(LineDelimiterEditor.class);

	private LineDelimiter value;

	@Override
	public void setAsText(String text) throws IllegalArgumentException {
		if (text == null) {
			this.value = LineDelimiter.DEFAULT;
			return;
		}
		text = text.toUpperCase();
		if ("AUTO".equals(text)) {
			value = LineDelimiter.AUTO;
		} else if ("CRLF".equals(text)) {
			value = LineDelimiter.CRLF;
		} else if ("MAC".equals(text)) {
			value = LineDelimiter.MAC;
		} else if ("DEFAULT".equals(text)) {
			value = LineDelimiter.DEFAULT;
		} else if ("NUL".equals(text)) {
			value = LineDelimiter.NUL;
		} else if ("UNIX".equals(text)) {
			value = LineDelimiter.UNIX;
		} else if ("WINDOWS".equals(text)) {
			value = LineDelimiter.WINDOWS;
		} else {
			FormattingTuple message = MessageFormatter.format("无法识别的 LineDelimiter 类型[{}]", text);
			logger.error(message.getMessage());
			throw new IllegalArgumentException(message.getMessage());
		}
	}

	@Override
	public Object getValue() {
		return value;
	}

}
