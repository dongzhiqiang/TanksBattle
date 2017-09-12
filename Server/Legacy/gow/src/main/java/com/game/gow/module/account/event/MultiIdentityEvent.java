package com.game.gow.module.account.event;

/**
 * 多个身份标识事件接口,
 * 常用于在组队等多人系统中
 * 
 * @author wenkin
 */
public interface MultiIdentityEvent extends NamedEvent {

	/**
	 * 获取发生事件的用户身份标识
	 * @return
	 */
	long[] getOwners();

}
