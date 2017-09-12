package com.game.gow.module.account.model;

import com.engine.common.protocol.annotation.Transable;

@Transable
public class AccountRequest {
    private String channelId;         //渠道Id
    private String userId;            //用户Id
    private String token;             //登录验证用的token
    private int    serverId;          //服务器Id
    private String clientVer;         //客户端版本
    private String deviceModel;       //设备型号
    private String osName;            //系统名称
    private int    root;              //是否越狱
    private String macAddr;           //硬件地址
    private String network;           //联网类型
    private int    screenWidth;       //屏幕宽
    private int    screenHeight;      //屏幕高	

	public String getChannelId() {
		return channelId;
	}
	public void setChannelId(String channelId) {
		this.channelId = channelId;
	}
	public String getUserId() {
		return userId;
	}
	public void setUserId(String userId) {
		this.userId = userId;
	}
	public String getToken() {
		return token;
	}
	public void setToken(String token) {
		this.token = token;
	}
	public int getServerId() {
		return serverId;
	}
	public void setServerId(int serverId) {
		this.serverId = serverId;
	}
	public String getClientVer() {
		return clientVer;
	}
	public void setClientVer(String clientVer) {
		this.clientVer = clientVer;
	}
	public String getDeviceModel() {
		return deviceModel;
	}
	public void setDeviceModel(String deviceModel) {
		this.deviceModel = deviceModel;
	}
	public String getOsName() {
		return osName;
	}
	public void setOsName(String osName) {
		this.osName = osName;
	}
	public int getRoot() {
		return root;
	}
	public void setRoot(int root) {
		this.root = root;
	}
	public String getMacAddr() {
		return macAddr;
	}
	public void setMacAddr(String macAddr) {
		this.macAddr = macAddr;
	}
	public String getNetwork() {
		return network;
	}
	public void setNetwork(String network) {
		this.network = network;
	}
	public int getScreenWidth() {
		return screenWidth;
	}
	public void setScreenWidth(int screenWidth) {
		this.screenWidth = screenWidth;
	}
	public int getScreenHeight() {
		return screenHeight;
	}
	public void setScreenHeight(int screenHeight) {
		this.screenHeight = screenHeight;
	}
}
