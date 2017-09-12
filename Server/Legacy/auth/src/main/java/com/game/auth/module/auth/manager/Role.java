package com.game.auth.module.auth.manager;


public class Role {

	
	
	/**
	 * 服务器id
	 */
	private int serverId;
	/**
	 * 玩家等级
	 */
	private int level = 1;

	/**
	 * 玩家名字
	 */
	private String name = "";
	

	public int getLevel() {
		return level;
	}

	public void setLevel(int level) {
		this.level = level;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public int getServerId() {
		return serverId;
	}

	public void setServerId(int serverId) {
		this.serverId = serverId;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + serverId;
		return result;
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		Role other = (Role) obj;
		if (serverId != other.serverId)
			return false;
		return true;
	}

	
	
	
}
