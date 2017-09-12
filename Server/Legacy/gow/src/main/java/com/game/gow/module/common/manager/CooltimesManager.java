package com.game.gow.module.common.manager;

import java.util.Date;

import org.springframework.stereotype.Component;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;
import com.game.gow.module.common.model.CooltimeKey;

/**
 * 冷却时间集合实体的管理器
 * 
 */
@Component
public class CooltimesManager {

	@Inject
	private EntityCacheService<Long, Cooltimes> cooltimesCache;

	/**
	 * 加载或创建某一用户的冷却时间集合对象
	 * @param owner 用户标识
	 * @return
	 */
	public Cooltimes loadOrCreate(long owner) {
		return cooltimesCache.loadOrCreate(owner, new EntityBuilder<Long, Cooltimes>() {
			@Override
			public Cooltimes newInstance(Long id) {
				Cooltimes times = new Cooltimes();
				times.setId(id);
				return times;
			}
		});
	}

	/**
	 * 更新某一玩家的指定冷却时间值
	 * @param owner 玩家标识
	 * @param key 冷却时间键
	 * @param time 冷却时间值
	 */
	public void update(long owner, CooltimeKey key, Date time) {
		Cooltimes cooltimes = loadOrCreate(owner);
		cooltimes.update(key, time);
	}

	/**
	 * 移除某一玩家的指定冷却时间值
	 * @param owner 玩家标识
	 * @param keys 冷却时间键
	 */
	public void remove(long owner, CooltimeKey... keys) {
		Cooltimes cooltimes = loadOrCreate(owner);
		cooltimes.remove(keys);
	}

}
