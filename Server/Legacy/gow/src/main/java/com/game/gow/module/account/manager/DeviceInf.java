package com.game.gow.module.account.manager;

import javax.persistence.Embeddable;


/**
 * 设备信息
 * 
 * @author wenkin
 */
@Embeddable
public class DeviceInf {   
	/**设备型号*/
	private String deviceModel;
	/**系统名称*/
	private String osName;
	/**是否越狱*/
	private Integer root;
	/**硬件地址*/
	private String macAddr;
	/**使用网络(wifi/2g/3g/4g)*/	
	private String network;
	/**屏幕宽度*/
	private int screenWidth;
	/**屏幕高度*/
	private int screenHeight;

	public static DeviceInf valueOf(String deviceModel, String osName, Integer root, String macAddr, String network, int screenWidth, int screenHeight) {
		DeviceInf result=new DeviceInf();
		result.deviceModel = deviceModel;
		result.osName = osName;
		result.root = root;
		result.macAddr = macAddr;
		result.network = network;
		result.screenWidth = screenWidth;
		result.screenHeight = screenHeight;
		return result;
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

	public Integer getRoot() {
		return root;
	}

	public void setRoot(Integer root) {
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
