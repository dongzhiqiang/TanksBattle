package com.game.gow.module.player.service;

import java.util.ArrayList;
import java.util.List;

import org.apache.commons.lang3.StringUtils;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.engine.common.event.EventBus;
import com.game.gow.module.equip.service.EquipService;
import com.game.gow.module.item.manager.Item;
import com.game.gow.module.item.manager.ItemManager;
import com.game.gow.module.player.event.PlayerNameChangeEvent;
import com.game.gow.module.player.manager.Player;
import com.game.gow.module.player.manager.PlayerManager;
import com.game.gow.module.role.service.PetService;

/**
 * 玩家角色服务
 * @author wenkin
 *
 */
@Service
public class PlayerService {
	
	@Autowired
    private PlayerManager playerManager;
	@Autowired
    private EquipService equipService;
	@Autowired
    private ItemManager itemManager;
	@Autowired
	private PetService petService;
	
	@Autowired
	private EventBus eventBus;
	
	public Player load(long id){
	  return playerManager.load(id);	
	}
	
	/**
	 * 
	 * @param id 账号Id
	 * @param playerName 账号名称
	 * @param userId 用户Id
	 * @param name 角色名称
	 * @param profession 角色职业
	 * @return
	 */
	public Player createPlayer(long accountId,String playerName){
		if (StringUtils.isBlank(playerName)) {
			playerName = "@" + accountId;
		}
		
		Player player = playerManager.create(accountId,playerName);
		
		// 初始装备
		player.setEquips(equipService.getInitEquips("kratos"));
		
		// 金钱
		List<Item> addItems = new ArrayList<Item>();
		addItems.add(Item.valueOf(0, accountId, 30000, 100000, ""));
		List<Item> modifiedItems = new ArrayList<Item>();
		itemManager.addItem(accountId, addItems, modifiedItems);
		
		// 测试宠物
		petService.createTestPets(accountId);
		
		return player;
	}
	
	/**
	 * 设置角色名称
	 * @param playerId
	 * @param name
	 */
	public boolean setNameAndPro(Player player,String name){
		playerManager.setNameAndPro(player, name);
		// 发出改名事件
		eventBus.post(PlayerNameChangeEvent.valueOf(player.getId(), name));
		return true;
	}
	
	/**
	 * 重命名角色名称
	 * @param player
	 * @param name
	 * @return
	 */
	public boolean rename(Player player,String name){
		playerManager.rename(player, name);
		// 发出改名事件
		eventBus.post(PlayerNameChangeEvent.valueOf(player.getId(), name));
		return true;
	}
	
	
}
