package com.game.gow.module.equip.service;

import java.util.ArrayList;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.engine.common.resource.Storage;
import com.engine.common.resource.anno.Static;
import com.game.gow.module.equip.exception.EquipException;
import com.game.gow.module.equip.exception.EquipExceptionCode;
import com.game.gow.module.equip.manager.Equip;
import com.game.gow.module.equip.model.ChangeWeaponResultVo;
import com.game.gow.module.equip.model.EquipPosConstant;
import com.game.gow.module.equip.model.EquipVo;
import com.game.gow.module.equip.resource.EquipAdvanceCost;
import com.game.gow.module.equip.resource.EquipInitList;
import com.game.gow.module.equip.resource.EquipList;
import com.game.gow.module.equip.resource.EquipRouseCost;
import com.game.gow.module.equip.resource.EquipUpgradeCost;
import com.game.gow.module.item.manager.Item;
import com.game.gow.module.item.manager.ItemManager;
import com.game.gow.module.item.service.ItemService;
import com.game.gow.module.player.manager.Player;
import com.game.gow.module.player.manager.PlayerManager;
import com.game.gow.module.role.manager.IRole;
import com.game.gow.module.role.manager.PetManager;
import com.game.gow.module.role.resource.RoleList;

/**
 * 装备服务
 */
@Service
public class EquipService {
	private Logger logger=LoggerFactory.getLogger(getClass());
	@Autowired
	private ItemManager itemManager;
	@Autowired
	private PlayerManager playerManager;
	@Autowired
	private PetManager petManager;
	@Autowired
	private ItemService itemService;
	@Static
	private Storage<Integer, EquipList> equipListStorage;
	@Static
	private Storage<String, EquipUpgradeCost> equipUpgradeCostStorage;
	@Static
	private Storage<String, EquipAdvanceCost> equipAdvanceCostStorage;
	@Static
	private Storage<String, EquipRouseCost> equipRouseCostStorage;
	@Static
	private Storage<String, EquipInitList> equipInitListStorage;
	@Static
	private Storage<String, RoleList> roleListStorage;
	
	/**
	 * 获取配置表里的消耗道具
	 * @param costItemString 消耗道具字符串
	 * @return 道具列表
	 */
	public List<Item> getCostItems( String costItemString )
	{
		ArrayList<Item> result = new ArrayList<Item>();
		String[] itemStrs = costItemString.split(",");
		for( String itemStr : itemStrs )
		{
			if(!StringUtils.isBlank(itemStr))
			{
				String[] itemOneStr = itemStr.split("\\|");
				Integer itemId = Integer.valueOf(itemOneStr[0]);
				Integer itemNum = Integer.valueOf(itemOneStr[1]);
				result.add(Item.valueOf(0, 0, itemId, itemNum, ""));
			}
		}
		return result;
	}
	
	/**
	 * 升级装备
	 * @param accountId
	 * @param roleId 角色id
	 * @param equipPosIndex 装备位
	 * @return {@link EquipVo}
	 */
	public EquipVo upgrade(long accountId, long roleId, int equipPosIndex)
	{
		IRole role = getRole( accountId, roleId );

		List<Equip> equips = role.getEquips();
		Equip equip = equips.get(equipPosIndex);
		
		// 检查玩家等级
		if( equip.getLevel() >= role.getLevel() ) {
			throw new EquipException(EquipExceptionCode.EQUIP_PLAYER_LEVEL_LIMIT);
		}
		
		EquipList equipList = equipListStorage.get(equip.getEquipId(), true);
		if( equip.getLevel() >= equipList.getMaxLevel() )
		{
			throw new EquipException(EquipExceptionCode.EQUIP_LEVEL_LIMIT);
		}
		
		EquipUpgradeCost cost = equipUpgradeCostStorage.get(equipList.getPosIndex()+"_"+(equip.getLevel()+1), true);
		List<Item> costItems = getCostItems(cost.getCost());
		if( !itemManager.canCostItem(accountId, costItems))
		{
			throw new EquipException(EquipExceptionCode.EQUIP_NO_ENOUGH_ITEM);
		}
		itemService.costItem(accountId, costItems);

		equip.setLevel(equip.getLevel()+1);
		
		// TODO 重新计算战斗力
		
		// TODO 推送事件和日志
		
		role.setEquips(equips);
		
		EquipVo result = EquipVo.valueOf(equip);
		result.setRoleId(roleId);
		return result;
	}
	
	
	/**
	 * 升品装备
	 * @param accountId
	 * @param roleId 角色id
	 * @param equipPosIndex 装备位
	 * @return {@link EquipVo}
	 */
	public EquipVo advance(long accountId, long roleId, int equipPosIndex)
	{
		IRole role = getRole( accountId, roleId );

		List<Equip> equips = role.getEquips();
		Equip equip = equips.get(equipPosIndex);
		
		EquipList equipList = equipListStorage.get(equip.getEquipId(), true);
		if( equip.getLevel() < equipList.getMaxLevel() )
		{
			throw new EquipException(EquipExceptionCode.EQUIP_NO_LEVEL);
		}
		
		// TODO 检查玩家等级
		
		EquipAdvanceCost cost = equipAdvanceCostStorage.get(equipList.getAdvanceCostId(), true);
		List<Item> costItems = getCostItems(cost.getCost());
		if( !itemManager.canCostItem(accountId, costItems))
		{
			throw new EquipException(EquipExceptionCode.EQUIP_NO_ENOUGH_ITEM);
		}
		itemService.costItem(accountId, costItems);
		
		equip.setEquipId(equipList.getAdvanceEquipId());
		
		role.setEquips(equips);
		
		EquipVo result = EquipVo.valueOf(equip);
		result.setRoleId(roleId);
		return result;
	}
	
	/**
	 * 觉醒装备
	 * @param accountId
	 * @param roleId 角色id
	 * @param equipPosIndex 装备位
	 * @return {@link EquipVo}
	 */
	public EquipVo rouse(long accountId, long roleId, int equipPosIndex)
	{
		IRole role = getRole( accountId, roleId );

		List<Equip> equips = role.getEquips();
		Equip equip = equips.get(equipPosIndex);
		
		EquipList equipList = equipListStorage.get(equip.getEquipId(), true);
		
		EquipRouseCost cost = equipRouseCostStorage.get(equipList.getRouseCostId(), true);
		List<Item> costItems = getCostItems(cost.getCost());
		if( !itemManager.canCostItem(accountId, costItems))
		{
			throw new EquipException(EquipExceptionCode.EQUIP_NO_ENOUGH_ITEM);
		}
		itemService.costItem(accountId, costItems);
		
		equip.setEquipId(equipList.getRouseEquipId());
		
		role.setEquips(equips);
		
		EquipVo result = EquipVo.valueOf(equip);
		result.setRoleId(roleId);
		return result;
	}
	
	// 穿卸装备的一些辅助方法
	/**
	 * 根据id取得role
	 * @param accountId
	 * @param roleId 0(玩家)或者宠物id
	 * @return 
	 */
	private IRole getRole(long accountId, long roleId)
	{
		IRole role;
		if(roleId == 0)
		{
			role = playerManager.load(accountId);
		}
		else
		{
			role = petManager.load(accountId, roleId);
		}
		return role;
	}
	
	/**
	 * 更换使用武器
	 * @param accountId
	 * @param roleId 装备角色id
	 * @param posIndex 使用的武器装备孔
	 * @return 使用的武器装备孔
	 */
	public ChangeWeaponResultVo changeWeapon( long accountId, long roleId, int posIndex )
	{
		IRole role = getRole( accountId, roleId );
		if(posIndex<EquipPosConstant.MIN_EQUIP_POS_WEAPON || posIndex>EquipPosConstant.MAX_EQUIP_POS_WEAPON)
		{
			throw new EquipException(EquipExceptionCode.EQUIP_IS_NOT_POS_TYPE);
		}
		role.setCurrentWeapon( posIndex );
		
		return ChangeWeaponResultVo.valueOf(roleId, posIndex);
	}
	
	/**
	 * 获取初始装备
	 * @param roleCfgId 装备角色配置id
	 * @return 初始装备列表
	 */
	public List<Equip> getInitEquips(String roleCfgId)
	{
		RoleList roleList = roleListStorage.get(roleCfgId, true);
		EquipInitList equipInit = equipInitListStorage.get(roleList.getInitEquips(), true);
		List<Equip> result = new ArrayList<Equip>();
		ArrayList<Integer> equipsAry = equipInit.getEquipsAry();
		if(equipsAry.size() == 10)
		{
			for(Integer equipId : equipsAry)
			{
				Equip equip = new Equip();
				equip.setEquipId(equipId);
				result.add(equip);
			}
		}
		else if(equipInit.getEquipsAry().size() == 7)
		{
			for(Integer equipId : equipsAry)
			{
				Equip equip = new Equip();
				equip.setEquipId(equipId);
				result.add(equip);
				if(result.size() == 1)
				{
					result.add(null);result.add(null);result.add(null);
				}
			}
		}
		else
		{
			logger.error("wrong init equip size:"+roleCfgId);
		}
		return result;
	}
	
	/**
	 * 校验装备列表
	 * @param equipList
	 * @return 
	 */
	public boolean verifyEquipList(List<Equip> equipList)
	{
		if(equipList.size() != EquipPosConstant.EQUIP_POS_COUNT)
		{
			return false;
		}
		return true;
	}
	
	/**
	 * 一键升级装备
	 * @param accountId
	 * @param roleId 角色id
	 * @param equipPosIndex 装备位
	 * @return {@link EquipVo}
	 */
	public EquipVo upgradeOnce(long accountId, long roleId, int equipPosIndex)
	{
		IRole role = getRole( accountId, roleId );

		List<Equip> equips = role.getEquips();
		Equip equip = equips.get(equipPosIndex);
		
		

		
		EquipList equipList = equipListStorage.get(equip.getEquipId(), true);
		
		while(true)
		{
			// 检查玩家等级
			if( equip.getLevel() >= role.getLevel() ) {
				break;
			}
			
			if( equip.getLevel() >= equipList.getMaxLevel() )
			{
				break;
			}
			
			EquipUpgradeCost cost = equipUpgradeCostStorage.get(equipList.getPosIndex()+"_"+(equip.getLevel()+1), true);
			List<Item> costItems = getCostItems(cost.getCost());
			if( !itemManager.canCostItem(accountId, costItems))
			{
				break;
			}
			itemService.costItem(accountId, costItems);
	
			equip.setLevel(equip.getLevel()+1);
		}
		// TODO 重新计算战斗力
		
		// TODO 推送事件和日志
		
		role.setEquips(equips);
		
		EquipVo result = EquipVo.valueOf(equip);
		result.setRoleId(roleId);
		return result;
	}
}