package com.game.auth.module.auth.model;

/**
 * 系统信息cpu 和 内存使用量
 * @author LJJ
 *
 */
public class SystemInfo {
	
	private String ip;
	private int rValue = 0;
	private int bValue = 0;
	private int freeValue = 0;
	private int inValue = 0;
	private int csValue = 0;
	private int usValue = 0;
	
	/**
	 * 链接数
	 */
	private int connectNums = 0;
	/**
	 * 调用方法的数量
	 */
	private long dueNums;
	
	
	
	public SystemInfo(String ip,int rValue, int bValue, int freeValue, int inValue,
			int csValue, int usValue,long dueNums) {
		this.rValue = rValue;
		this.bValue = bValue;
		this.freeValue = freeValue;
		this.inValue = inValue;
		this.csValue = csValue;
		this.usValue = usValue;
		this.dueNums = dueNums;
		this.setIp(ip);
	}
	public SystemInfo() {
	}
	public int getrValue() {
		return rValue;
	}
	public void setrValue(int rValue) {
		this.rValue = rValue;
	}
	public int getbValue() {
		return bValue;
	}
	public void setbValue(int bValue) {
		this.bValue = bValue;
	}
	public int getFreeValue() {
		return freeValue;
	}
	public void setFreeValue(int freeValue) {
		this.freeValue = freeValue;
	}
	public int getInValue() {
		return inValue;
	}
	public void setInValue(int inValue) {
		this.inValue = inValue;
	}
	public int getCsValue() {
		return csValue;
	}
	public void setCsValue(int csValue) {
		this.csValue = csValue;
	}
	public int getUsValue() {
		return usValue;
	}
	public void setUsValue(int usValue) {
		this.usValue = usValue;
	}

	public int getConnectNums() {
		return connectNums;
	}
	public void setConnectNums(int connectNums) {
		this.connectNums = connectNums;
	}
	public long getDueNums() {
		return dueNums;
	}
	public void setDueNums(long dueNums) {
		this.dueNums = dueNums;
	}
	public String getIp() {
		return ip;
	}
	public void setIp(String ip) {
		this.ip = ip;
	}
	
	
	
	
	
}
