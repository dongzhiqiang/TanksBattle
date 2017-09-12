package com.game.gow.module.common.manager;

import org.springframework.stereotype.Component;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;

/**
 * 全局信息管理器
 * 
 */
@Component
public class GlobalInfoManager {

	@Inject
	private EntityCacheService<GlobalKey, GlobalInfo> cacheService;

	/**
	 * 加载或创建指定的全局信息实体
	 * @param key
	 * @param defaultValue
	 * @return
	 */
	public GlobalInfo loadOrCreate(GlobalKey key, final Object defaultValue) {
		return cacheService.loadOrCreate(key, new EntityBuilder<GlobalKey, GlobalInfo>() {
			@Override
			public GlobalInfo newInstance(GlobalKey id) {
				return GlobalInfo.valueOf(id, defaultValue);
			}
		});
	}
	
	/**
	 * 更新指定的全局信息
	 * @param entity
	 * @param value
	 */
	public void update(GlobalInfo entity, Object value) {
		entity.update(value);
	}
}
