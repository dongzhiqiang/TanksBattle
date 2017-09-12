package com.game.auth.module.auth.constant;

public interface ResultCode {
	
	/**
	 * 该玩家UserId已存在
	 */
	int USERID_EXIST = -1;
	
	/**
	 * 该玩家插入失败
	 */
	int INSERT_ERROR = -2;
	
	/**
	 * 该玩家不存在
	 */
	int USER_NOT_EXIST = -3;
	
	/**
	 * 获取玩家信息出错
	 */
	int GET_USER_INFO_ERROR = -4;
	
	/**
	 * 更新玩家信息出错
	 */
	int UPDATE_USER_INFO_ERROR = -5;
	
	/**
	 * 版本信息出错
	 */
	int VERSION_ERROR = -6;
	
	/**
	 * sdk验证数据已过期
	 */
	int CHECK_DATA_TIMEOUT = -7;
	
	/**
	 * 账号验证失败
	 */
	int USER_CHECK_ERROR = -828;
	/**
	 * 未知错误
	 */
	int UNKOWN_ERROR = -255;

}
