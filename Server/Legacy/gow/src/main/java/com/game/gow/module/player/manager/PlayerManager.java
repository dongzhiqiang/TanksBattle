package com.game.gow.module.player.manager;

import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.exception.UniqueFieldException;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;
import com.engine.common.utils.model.ResultCode;
import com.game.gow.module.player.exception.PlayerException;


/**
 * 玩家角色实体管理
 * @author wenkin
 *
 */
@Component
public class PlayerManager {
	@SuppressWarnings("unused")
	private Logger logger=LoggerFactory.getLogger(getClass());
	
    @Inject
	private EntityCacheService<Long, Player> playerCache;
    
	/** 角色名对应ID的映射 */
	private ConcurrentMap<String, Long> name2Id = new ConcurrentHashMap<String, Long>();
	
	/**
	 * 获取制定的角色对象
	 * @param id 角色Id
	 * @return
	 */
	public Player load(long id){
		return playerCache.load(id);
	}
	
	/**
	 * 创建玩家角色
	 * @param accountId   账号Id
	 * @param accountName 账号名称
	 * @param playerName  角色名称
	 * @param profession  职业(阵营)
	 * @return
	 */
	public Player create(final long id,final String playerName){
		// 再次检测账号是否存在
		Long oldId=putName2Id(playerName, id);
		if (oldId != null && !oldId.equals(id)) {
			// 账号已被占用
			throw new UniqueFieldException("角色名[" + playerName + "]已经存在");
		}
		try{
			return playerCache.loadOrCreate(id, new EntityBuilder<Long, Player>() {

				@Override
				public Player newInstance(Long id) {
					return Player.valueOf(id, playerName);
				}
			});
		}catch(Exception e){
			// 移除占用
			removeName2Id(playerName, id);
			throw new PlayerException(ResultCode.UNKNOWN_ERROR, e);
		}
	}
	
	public void setNameAndPro(Player player, String name){
		rename(player,name);
	}
	
	/**
	 * 玩家修改角色名
	 * @param name
	 * @return
	 * @throws UniqueFieldException 角色名重复时抛出
	 */
	public void rename(Player player, String name) {
		// 再次检测账号是否存在
		long id = player.getId();
		Long oldId = this.putName2Id(name, id);
		if (oldId != null && !oldId.equals(id)) {
			// 账号已被占用
			throw new UniqueFieldException("角色名[" + name + "]已经存在");
		}
		String oldName = player.getName();
		try {
			player.setName(name);
			this.removeName2Id(oldName, id);
		} catch (Exception e) {
			// 移除占用
			this.removeName2Id(name, id);
			throw new UniqueFieldException("角色名[" + name + "]已经存在");
		}
	}
	
	/**
	 * 按角色名转换ID
	 * @param name 角色名
	 * @return
	 */
	public Long getName2Id(String name) {
		Long id = name2Id.get(name.toLowerCase());
		return id;
	}

	/**
	 * 移除角色名和ID对应关系
	 * @param name
	 * @param id
	 * @return
	 */
	private boolean removeName2Id(String name, Long id) {
		return name2Id.remove(name.toLowerCase(), id);
	}

	/**
	 * 设置角色名和ID对应关系
	 * @param name
	 * @param id
	 * @return
	 */
	private Long putName2Id(String name, Long id) {
		return name2Id.putIfAbsent(name.toLowerCase(), id);
	}
}
