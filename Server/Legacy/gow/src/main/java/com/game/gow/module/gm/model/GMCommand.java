package com.game.gow.module.gm.model;

public interface GMCommand {
	/**
	 * 增加道具
	 * @param msg[1] 道具id
	 * @param msg[2] 道具数量
	 */
	String CMD_ADD_ITEM = "addItem";
	
	/**
	 * 设置等级(测试装备用，没加推送包，设完要重登
	 * @param msg[1] 
	 */
	String CMD_SET_LEVEL = "setLevel";
}
