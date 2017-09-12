/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package com.game.auth.module.auth.model;


public class BasePassportData {

	/**
	 * 查询用的数据条件
	 */
	private String userId;
	
	private String  serverId;
	
	/**
	 *计费服设计的通行证ID
	 */
	private String account_id;
	
	/**
	 * ipv4
	 */
	private String ip;
	
	private String sdk_version;
	/**
	 * 设备唯一标示符
	 */
	private String udid;
	
	private String devId;
	
	/**
	 * 运营渠道
	 */
	private String app_channel;
	
	

	/**
	 * token
	 */
	private String token;
	

	private String aid ;
	
	private String channel;
	
	private String username;
	
	private String platform;
	
	
	
	public String getSdk_version() {
		return sdk_version;
	}

	public void setSdk_version(String sdk_version) {
		this.sdk_version = sdk_version;
	}
	
	
	public String getServerId() {
		return serverId;
	}

	public void setServerId(String serverId) {
		this.serverId = serverId;
	}


	

	public String getIp() {
		return ip;
	}

	public void setIp(String ip) {
		this.ip = ip;
	}




	public String getUdid() {
		return udid;
	}

	public void setUdid(String udid) {
		this.udid = udid;
	}

	public String getApp_channel() {
		return app_channel;
	}

	public void setApp_channel(String app_channel) {
		this.app_channel = app_channel;
	}


	public String getToken() {
		return token;
	}

	public void setToken(String token) {
		this.token = token;
	}






	public String getAid() {
		return aid;
	}

	public void setAid(String aid) {
		this.aid = aid;
	}

	public String getDevId() {
		return devId;
	}

	public void setDevId(String devId) {
		this.devId = devId;
	}

	public String getChannel() {
		return channel;
	}

	public void setChannel(String channel) {
		this.channel = channel;
	}

	public String getUsername() {
		return username;
	}

	public void setUsername(String username) {
		this.username = username;
	}

	public String getPlatform() {
		return platform;
	}

	public void setPlatform(String platform) {
		this.platform = platform;
	}



	public String getAccount_id()
	{
		return account_id;
	}

	public void setAccount_id(String account_id)
	{
		this.account_id = account_id;
	}

	public String getUserId()
	{
		return userId;
	}

	public void setUserId(String userId)
	{
		this.userId = userId;
	}

	
	
}
