package com.engine.common.socket.other;

import java.beans.PropertyEditor;
import java.beans.PropertyEditorSupport;
import java.nio.charset.Charset;

/**
 * {@link Charset}的{@link PropertyEditor}
 * 
 * 
 */
public class CharsetEditor extends PropertyEditorSupport {
	
	private final static Charset DEFAULT_CHARSET = Charset.forName("utf-8");

	private Charset value;

	@Override
	public void setAsText(String text) throws IllegalArgumentException {
		if (text == null) {
			this.value = DEFAULT_CHARSET;
			return;
		}
		this.value = Charset.forName(text);
	}

	@Override
	public Object getValue() {
		return value;
	}
}
