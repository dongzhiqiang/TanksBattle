package com.game.gow.module.account.event;

/**
 * 身份标识事件接口
 * 
 */
public interface IdentityEvent extends NamedEvent {

	/**
	 * 获取发生事件的用户身份标识
	 * @return
	 */
	long getOwner();

}
