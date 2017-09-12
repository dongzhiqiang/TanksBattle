package com.engine.common.resource.other;

/**
 * 格式信息定义
 * 
 * 
 */
public class FormatDefinition {

	private final String location;
	private final String type;
	private final String suffix;

	public FormatDefinition(String location, String type, String suffix) {
		this.location = location;
		this.type = type;
		this.suffix = suffix;
	}

	// Getter and Setter ...

	public String getLocation() {
		return location;
	}

	public String getType() {
		return type;
	}

	public String getSuffix() {
		return suffix;
	}

}
