package com.engine.common.ramcache.persist;

public class PersisterConfig {

	private final PersisterType type;
	private final String value;

	public PersisterConfig(PersisterType type, String value) {
		this.type = type;
		this.value = value;
	}

	public PersisterType getType() {
		return type;
	}

	public String getValue() {
		return value;
	}

}
