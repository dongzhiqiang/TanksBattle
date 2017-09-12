package com.game.auth.module.auth.model;

public class ServerHost {

	public static final String SERVER_CHENNAL = "Server_Chennal";
	/**
	 * 游戏大区
	 */
	private String game;
	/**
	 * 服务器名
	 */
	private String serverName;
	/**
	 * 端口
	 */
	private int port;
	/**
	 * 索引
	 */
	private int index;
	/**
	 * ip
	 */
	private String ip;

	private int serverId;
	
	/**
	 * 内网IP
	 */
	private String lanIp;
	
	/**
	 * 内网端口
	 */
	private int lanPort;
	
	
	/**
	 * 描述是否推荐等
	 */
	private String recommendState;
	private String channel;
	/**
	 * 在线人数
	 */
	private int onlineNums;

	public ServerHost() {

	}

	public String getServerName() {
		return serverName;
	}

	public void setServerName(String serverName) {
		this.serverName = serverName;
	}

	public int getPort() {
		return port;
	}

	public void setPort(int port) {
		this.port = port;
	}

	public int getIndex() {
		return index;
	}

	public void setIndex(int index) {
		this.index = index;
	}

	public String getIp() {
		return ip;
	}

	public void setIp(String ip) {
		this.ip = ip;
	}

	public String getGame() {
		return game;
	}

	public void setGame(String game) {
		this.game = game;
	}

	public String getRecommendState() {
		return recommendState;
	}

	public void setRecommendState(String recommendState) {
		this.recommendState = recommendState;
	}

	public int getServerState() {
		// 维护
		if (onlineNums < 0) {
			return -1;
		}
		// 流畅
		else if (onlineNums < 500) {
			return 0;
		}
		// 繁忙
		else if (onlineNums < 1100) {
			return 1;
		}

		// 拥挤

		else {
			return 2;
		}
	}

	public int getServerId() {
		return serverId;
	}

	public void setServerId(int serverId) {
		this.serverId = serverId;
	}

	public String getChannel() {
		return channel;
	}

	public void setChannel(String channel) {
		this.channel = channel;
	}

	public int getOnlineNums() {
		return onlineNums;
	}

	public void setOnlineNums(int onlineNums) {
		this.onlineNums = onlineNums;
	}

	@Override
	public String toString() {
		return "ServerHost [game=" + game + ", serverName=" + serverName
				+ ", port=" + port + ", index=" + index + ", ip=" + ip
				+ ", serverId=" + serverId + ", recommendState="
				+ recommendState + ", channel=" + channel + ", onlineNums="
				+ onlineNums + ", lanIp=" + lanIp + ", lanPort=" + lanPort +"]";
	}

	public String getLanIp() {
		return lanIp;
	}

	public void setLanIp(String lanIp) {
		this.lanIp = lanIp;
	}

	public int getLanPort() {
		return lanPort;
	}

	public void setLanPort(int lanPort) {
		this.lanPort = lanPort;
	}

}
