package com.game.gow.module.common;

/**
 * 持久化策略名定义
 * 
 */
public interface Persisters {

	/** 每分钟持久化 */
	String PRE_MINUTE = "pre_minute";
	
	/** 每5分钟持久化 */
	String PRE_5_MINUTE = "pre_5_minute";
	
	/** 每10分钟持久化 */
	String PRE_10_MINUTE = "pre_10_minute";
	
	/** 每半小时持久化 */
	String PRE_HALF_HOUR = "pre_half_hour";
	
	/** 每小时持久化 */
	String PRE_HOUR = "pre_hour";
}
