package com.game.gow.module.account.facade;

import com.engine.common.protocol.annotation.Constant;
import com.engine.common.utils.model.ResultCode;

/**
 * 账号服务状态码声明
 *
 *@author wenkin
 */
@Constant
public interface AccountResult extends ResultCode {
	
	/** 账号已经存在 */
	int ACCOUNT_ALREADY_EXISTS = -1;

	/** 角色已经存在 */
	int PLAYER_ALREADY_EXISTS = -2;

	/** 角色名非法 */
	int PLAYER_NAME_ILLEGAL = -3;

	/** 登录密匙非法 */
	int LOGIN_KEY_ILLEGAL = -4;

	/** 账号不存在 */
	int ACCOUNT_NOT_FOUND = -5;

	/** 重登录失败 */
	int RELOGIN_FAIL = -6;

	/** 非法的账号名 */
	int INVAILD_ACCOUNT_NAME = -7;

	/** 初始化奖励内容错误 */
	int INIT_REWARD_ERROR = -8;

	/** 账号已经被封 */
	int ACCOUNT_IS_BLOCK = -9;

	/** 停止注册 */
	int UNREGISTABLE = -10;

	/** 账号已经被清理 */
	int ACCOUNT_IS_CLEAN = -11;
	
	/**设置职业错误*/
	int PROFESSION_SET_ERROR=-12;
	
	/**职业选择错误*/
	int PROFESSION_CHOOSE_ERROR=-13;

}
