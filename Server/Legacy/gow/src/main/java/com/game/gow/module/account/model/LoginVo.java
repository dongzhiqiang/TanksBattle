package com.game.gow.module.account.model;

import com.engine.common.protocol.annotation.Transable;
import com.game.gow.module.role.model.RoleVo;

/**
 * 登录返回信息VO(暂定)
 * 
 * @author wenkin
 */
@Transable
public class LoginVo {

	/** 用户的帐号信息 */
	private AccountVo account;
	/** 用户的角色信息 */
	private RoleVo role;

	
//	/** 用户所在地图标识 */
//	private String mapId;
//	/** 当前的系统时间 */
//	private Date systemTime = new Date();
//	/** 体力值 */
//	private ActionPointVo actionPoint;
//  在登录时，其它各个系统(玩法)需要返回的信息


	// Getter and Setter ...

	public AccountVo getAccount() {
		return account;
	}



	public void setAccount(AccountVo account) {
		this.account = account;
	}

	public RoleVo getRole() {
		return role;
	}



	public void setRole(RoleVo role) {
		this.role = role;
	}

	// Static Method's ...

	/**
	 * 构造方法
	 * @param account
	 * @param role
	 */
	public static LoginVo valueOf(AccountVo account, RoleVo role) {
		LoginVo result = new LoginVo();
		result.account = account;
		result.role = role;
		return result;
	}







}
