package com.game.gow.module.common;

/**
 * 缓存常量值定义
 * 
 */
public interface CachedSizes {

	/** 默认缓存大小(当实体与在线玩家数量对应时使用该值) */
	String DEFAULT = "default";

	/** 双倍缓存大小(在线用户数的双倍) */
	String DOUBLE = "double";

	/** 三倍缓存大小 (在线用户数的三倍) */
	String TRIPLE = "triple";

	/** 最大的缓存大小 (用于放置最多的Player对象缓存, 简化操作) */
	String MAXIMUM = "maximum";
}
