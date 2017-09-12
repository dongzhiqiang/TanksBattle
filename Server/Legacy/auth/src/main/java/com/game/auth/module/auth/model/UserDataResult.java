package com.game.auth.module.auth.model;



public class UserDataResult extends ResultBase{

	
	/**
	 * 玩家拥有的角色列表
	 */
	private UserData ud;
	
	
	public static UserDataResult valueOf(UserData ud)
	{
		UserDataResult result = new UserDataResult();
		result.setUd(ud);
		return result;
	}


	public UserData getUd()
	{
		return ud;
	}

	public void setUd(UserData ud)
	{
		this.ud = ud;
	}
	

	
}
