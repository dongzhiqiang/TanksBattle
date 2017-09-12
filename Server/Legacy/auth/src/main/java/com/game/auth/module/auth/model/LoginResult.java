package com.game.auth.module.auth.model;

import java.util.List;

import com.game.auth.module.auth.manager.LoginUser;


public class LoginResult extends ResultBase{

	/**
	 * 服务器列表
	 */
	private List<ServerHost> serverList;
	
	/**
	 * 玩家拥有的角色列表
	 */
	private LoginUser userRoles;
	
	
	private boolean flag;
	
	private String md5;
	
	private String address;
	
	private String userId;
	
	private String key;
	
	private Long timestamp;
	
	
	public static LoginResult valueOf(List<ServerHost> serverList,LoginUser userRoles,boolean flag,String serverIp,String userId,Long timestamp,String key)
	{
		LoginResult result = new LoginResult();
		result.serverList = serverList;
		result.userRoles = userRoles;
		result.flag = flag;
		result.setAddress(serverIp);
		result.userId = userId;
		result.key = key;
		result.timestamp =timestamp;
		return result;
	}

	public List<ServerHost> getServerList() {
		return serverList;
	}

	public void setServerList(List<ServerHost> serverList) {
		this.serverList = serverList;
	}

	public LoginUser getUserRoles() {
		return userRoles;
	}

	public void setUserRoles(LoginUser userRoles) {
		this.userRoles = userRoles;
	}

	public boolean isFlag() {
		return flag;
	}

	public void setFlag(boolean flag) {
		this.flag = flag;
	}

	public String getMd5() {
		return md5;
	}

	public void setMd5(String md5) {
		this.md5 = md5;
	}

	public String getAddress()
	{
		return address;
	}

	public void setAddress(String address)
	{
		this.address = address;
	}

	public String getUserId()
	{
		return userId;
	}

	public void setUserId(String userId)
	{
		this.userId = userId;
	}

	public String getKey() {
		return key;
	}

	public void setKey(String key) {
		this.key = key;
	}
	public Long getTimestamp() {
		return timestamp;
	}

	public void setTimestamp(Long timestamp) {
		this.timestamp = timestamp;
	}


	
}
