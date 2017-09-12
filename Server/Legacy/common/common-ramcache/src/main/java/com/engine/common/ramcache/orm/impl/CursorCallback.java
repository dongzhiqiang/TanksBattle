package com.engine.common.ramcache.orm.impl;

public interface CursorCallback<T> {

	public void call(T entity);
	
}
