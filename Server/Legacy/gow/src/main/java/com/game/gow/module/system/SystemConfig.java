package com.game.gow.module.system;

import java.util.Collections;
import java.util.HashSet;
import java.util.Set;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Qualifier;

/**
 * 系统配置信息对象
 * 
 * @author wenkin
 */
public class SystemConfig {

	/** 分隔符 */
	public static final String SPLIT = ",";
	/** 服务器标识 */
	private Set<Short> servers;
	/** 服务器密匙 */
	private String key;
	/** 礼包服连接 */
	private String giftUrl;
	/** 礼包通讯密钥 */
	private String giftkey;
	/** 运营商标识 */
	private String agent;
	/** 游戏名称 */
	private String gameName;
	/** 运营商标识(主键生成) */
	private short operator;
	/** 是否忽略当前的主键值 */
	private boolean ignoreCurrentId;
	/** 是否帐号大小写 */
	private boolean ignoreCase = true;
	/** 是否允许注册帐号 */
	private boolean registable = true;

	/** 是否合服 */
	private boolean compose = false;

	/**
	 * 获取服务器标识集合
	 * @return 返回集合不可修改
	 */
	public Set<Short> getServers() {
		return servers;
	}

	/**
	 * 获取服务器密匙
	 * @return
	 */
	public String getKey() {
		return key;
	}

	// Setter ...

	@Autowired
	@Qualifier("server.config.servers")
	protected void setServers(String servers) {
		HashSet<Short> temp = new HashSet<Short>();
		for (String s : servers.split(SPLIT)) {
			temp.add(Short.valueOf(s));
		}
		this.servers = Collections.unmodifiableSet(temp);
	}

	public void setKey(String key) {
		this.key = key;
	}

	public String getGiftUrl() {
		return giftUrl;
	}

	public void setGiftUrl(String giftUrl) {
		this.giftUrl = giftUrl;
	}

	public String getAgent() {
		return agent;
	}

	public void setAgent(String agent) {
		this.agent = agent;
	}

	public String getGameName() {
		return gameName;
	}

	public void setGameName(String gameName) {
		this.gameName = gameName;
	}

	public short getOperator() {
		return operator;
	}

	public void setOperator(short operator) {
		this.operator = operator;
	}

	public boolean isIgnoreCurrentId() {
		return ignoreCurrentId;
	}

	public void setIgnoreCurrentId(boolean ignoreCurrentId) {
		this.ignoreCurrentId = ignoreCurrentId;
	}

	public boolean isIgnoreCase() {
		return ignoreCase;
	}

	public void setIgnoreCase(boolean ignoreCase) {
		this.ignoreCase = ignoreCase;
	}

	public boolean isRegistable() {
		return registable;
	}

	public void setRegistable(boolean registable) {
		this.registable = registable;
	}


	public String getGiftkey() {
		return giftkey;
	}

	public void setGiftkey(String giftkey) {
		this.giftkey = giftkey;
	}

	public boolean isCompose() {
		return compose;
	}

	public void setCompose(boolean compose) {
		this.compose = compose;
	}

	public boolean isMergeServer() {
		// 不是混服， 服务器数量 > 1
		return !isCompose() && servers.size() > 1;
	}

}
