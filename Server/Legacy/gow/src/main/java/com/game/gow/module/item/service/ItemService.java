package com.game.gow.module.item.service;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.engine.common.event.EventBus;
import com.engine.common.resource.Storage;
import com.engine.common.resource.anno.Static;
import com.game.gow.module.item.event.ItemModifyEvent;
import com.game.gow.module.item.manager.Item;
import com.game.gow.module.item.manager.ItemManager;
import com.game.gow.module.item.model.ItemModifyVo;
import com.game.gow.module.item.model.ItemVo;
import com.game.gow.module.item.resource.ItemType;
import com.game.gow.module.item.resource.ItemList;

/**
 * 道具服务
 */
@Service
public class ItemService {

	@Static
	private Storage<Integer, ItemList> itemListStorage;
	
	@Static
	private Storage<Integer, ItemType> itemTypeStorage;
	
	@Autowired
	private ItemManager itemManager;
	
	@Autowired
	private EventBus eventBus;
	
	/**
	 * 获取道具子类型
	 * @param baseId 道具配置ID
	 * @return 道具子类型
	 */
	public int getItemSubType( int baseId )
	{
		ItemList itemList = itemListStorage.get(baseId, true);
		
		return itemList.getSubType();
	}
	
	/**
	 * 获取道具类型
	 * @param baseId 道具配置ID
	 * @return 道具类型
	 */
	public int getItemType( int baseId )
	{
		int itemSubType = getItemSubType( baseId );
		com.game.gow.module.item.resource.ItemType itemType = itemTypeStorage.get(itemSubType, true);
		
		return itemType.getType();
	}
	
	/**
	 * 创造物品实例
	 * @param baseId 道具配置ID
	 * @param num 数量
	 * @return 道具类型
	 */
	public Item itemOf( int baseId, int num )
	{
		String content = "";

		return Item.valueOf(0, 0, baseId, num, content);
	}
	
	/**
	 * 获取道具列表
	 * @param accountId 玩家标识
	 * @return
	 */
	public List<ItemVo> getItemList( long accountId )
	{
		Collection<Item> items = itemManager.loadByOwner(accountId);
		ArrayList<ItemVo> result = new ArrayList<ItemVo>();
		for( Item item : items )
		{
			result.add(ItemVo.valueOf(item, getItemType(item.getBaseId())));
		}
		return result;
	}
	
	/**
	 * 增加道具
	 * @param owner 玩家标识
	 * @param addItems 道具列表
	 * @return
	 */
	public void addItem(long accountId, Collection<Item> addItems)
	{
		List<Item> modifiedItems = new ArrayList<Item>();
		itemManager.addItem(accountId, addItems, modifiedItems);
		List<ItemVo> modifiedItemVos = new ArrayList<ItemVo>();
		for(Item item : modifiedItems)
		{
			modifiedItemVos.add(ItemVo.valueOf(item, getItemType(item.getBaseId())));
		}
		ItemModifyVo itemModifyVo = ItemModifyVo.valueOf(modifiedItemVos, null);
		eventBus.post(ItemModifyEvent.valueOf(accountId, itemModifyVo));
	}
	
	/**
	 * 扣除道具
	 * @param owner 玩家标识
	 * @param addItems 道具列表
	 * @return
	 */
	public void costItem(long accountId, Collection<Item> costItems)
	{
		List<Item> modifiedItems = new ArrayList<Item>();
		List<Long> removedItems = new ArrayList<Long>();
		itemManager.costItem(accountId, costItems, modifiedItems, removedItems);
		List<ItemVo> modifiedItemVos = new ArrayList<ItemVo>();
		for(Item item : modifiedItems)
		{
			modifiedItemVos.add(ItemVo.valueOf(item, getItemType(item.getBaseId())));
		}
		ItemModifyVo itemModifyVo = ItemModifyVo.valueOf(modifiedItemVos, removedItems);
		eventBus.post(ItemModifyEvent.valueOf(accountId, itemModifyVo));
	}
}
