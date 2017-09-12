package com.game.gow.module.account.exception;

import com.game.gow.module.account.facade.AccountResult;

/**
 * 账号异常代码值表
 * 
 *@author wenkin
 */
public interface AccountExceptionCode {
	
	/** 账号已经存在 */
	int ACCOUNT_ALREADY_EXISTS = AccountResult.ACCOUNT_ALREADY_EXISTS;
	
	/** 角色已经存在 */
	int PLAYER_ALREADY_EXISTS = AccountResult.PLAYER_ALREADY_EXISTS;
	
	/** 非法的账号名 */
	int INVAILD_ACCOUNT_NAME = AccountResult.INVAILD_ACCOUNT_NAME;

	/** 初始化奖励数据错误 */
	int INIT_REWARD_ERROR = AccountResult.INIT_REWARD_ERROR;
}
