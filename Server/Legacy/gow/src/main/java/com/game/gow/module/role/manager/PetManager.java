package com.game.gow.module.role.manager;

import java.util.Collection;

import javax.annotation.PostConstruct;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.service.IndexValue;
import com.engine.common.ramcache.service.RegionCacheService;
import com.engine.common.utils.id.MultiServerIdGenerator;
import com.game.gow.module.system.MultiServerIdGeneratorHolder;
import com.game.gow.module.system.SystemConfig;

/**
 * 宠物管理器
 */
@Component
public class PetManager {
	@SuppressWarnings("unused")
	private Logger logger = LoggerFactory.getLogger(getClass());
	
	@Inject
	private RegionCacheService<Long, Pet> petCache;
	@Autowired
	private MultiServerIdGeneratorHolder generatorHolder;
	private MultiServerIdGenerator idGenerator;
	@Autowired
	private SystemConfig systemConfig;
	
	@PostConstruct
	protected void init() {
		generatorHolder.initialize(Pet.class, Pet.MAX_ID);
		idGenerator = generatorHolder.getGenerator(Pet.class);
	}
	
	/**
	 * 按标识加载宠物对象
	 * @param owner 玩家标识
	 * @param id 标识
	 * @return
	 */
	public Pet load(long owner, long id) {
		return petCache.get(IndexValue.valueOf("owner", owner), id);
	}
	
	/**
	 * 获取一个玩家的宠物集合
	 * @param owner 玩家标识
	 * @return
	 */
	public Collection<Pet> loadByOwner(long owner)
	{
		return petCache.load(IndexValue.valueOf("owner", owner));
	}
	
	/**
	 * 删除宠物
	 * @param pet 宠物
	 * @return
	 */
	public void remove(Pet pet)
	{
		petCache.remove(pet);
	}
	
	/**
	 * 创建宠物
	 * @param pet 宠物
	 * @return
	 */
	public Pet create(Pet pet) {
		short server = systemConfig.getServers().iterator().next();

		// 创建新实体
		final long id = idGenerator.getNext(server);
		pet.setId(id);
		
		Pet result = petCache.create(pet);
		
		return result;
	}
}
