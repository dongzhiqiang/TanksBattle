package com.game.gow.module.account.model;

import com.engine.common.protocol.annotation.Transable;

/**
 * 账号状态
 * 
 *  @author wenkin
 */
@Transable
public enum AccountState {

	/** 正常 */
	NORMAL,
	/** 锁定 */
	BLOCK,
	/** 清理 */
	CLEAN;
}
