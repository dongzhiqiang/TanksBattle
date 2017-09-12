package com.game.gow.module.common.service;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.game.gow.module.common.manager.GlobalInfo;
import com.game.gow.module.common.manager.GlobalInfoManager;
import com.game.gow.module.common.manager.GlobalKey;

/**
 * 全局信息服务对象
 * 
 */
@Service
public class GlobalInfoService {
	
	@Autowired
	private GlobalInfoManager globalInfoManager;

	/**
	 * 获取全局信息
	 * @param key
	 * @return
	 */
	public GlobalInfo getInfo(GlobalKey key) {
		GlobalInfo entity = globalInfoManager.loadOrCreate(key, null);
		return entity;
	}

	/**
	 * 获取全局信息
	 * @param key
	 * @param clz
	 * @return
	 */
	public <T> T getInfo(GlobalKey key, Class<T> clz) {
		GlobalInfo entity = globalInfoManager.loadOrCreate(key, null);
		return entity.getValue(clz);
	}
	
	/**
	 * 获取全局信息
	 * @param key
	 * @param clz
	 * @param defaultValue
	 * @return
	 */
	public <T> T getInfo(GlobalKey key, Class<T> clz, T defaultValue) {
		GlobalInfo entity = globalInfoManager.loadOrCreate(key, defaultValue);
		return entity.getValue(clz);
	}
	
	/**
	 * 更新全局信息
	 * @param key
	 * @param value
	 */
	public void update(GlobalKey key, Object value) {
		GlobalInfo entity = globalInfoManager.loadOrCreate(key, null);
		globalInfoManager.update(entity, value);
	}
}
