package com.game.gow.module.item.manager;

import java.util.Collection;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.annotation.PostConstruct;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.service.IndexValue;
import com.engine.common.ramcache.service.RegionCacheService;
import com.engine.common.resource.Storage;
import com.engine.common.resource.anno.Static;
import com.engine.common.utils.id.MultiServerIdGenerator;
import com.game.gow.module.item.model.ItemTypeConstant;
import com.game.gow.module.item.resource.ItemList;
import com.game.gow.module.item.service.ItemService;
import com.game.gow.module.system.MultiServerIdGeneratorHolder;
import com.game.gow.module.system.SystemConfig;

/**
 * 道具管理器
 */
@Component
public class ItemManager {

	@SuppressWarnings("unused")
	private Logger logger = LoggerFactory.getLogger(getClass());
	
	@Inject
	private RegionCacheService<Long, Item> itemCache;
	@Autowired
	private MultiServerIdGeneratorHolder generatorHolder;
	private MultiServerIdGenerator idGenerator;
	@Autowired
	private SystemConfig systemConfig;
	@Static
	private Storage<Integer, ItemList> itemListStorage;
	@Autowired
	private ItemService itemService;
	
	@PostConstruct
	protected void init() {
		generatorHolder.initialize(Item.class, Item.MAX_ID);
		idGenerator = generatorHolder.getGenerator(Item.class);
	}
	
	/**
	 * 按标识加载道具对象
	 * @param owner 玩家标识
	 * @param id 标识
	 * @return
	 */
	public Item load(long owner, long id) {
		return itemCache.get(IndexValue.valueOf("owner", owner), id);
	}
	
	/**
	 * 获取一个玩家的道具集合
	 * @param owner 玩家标识
	 * @return
	 */
	public Collection<Item> loadByOwner(long owner)
	{
		return itemCache.load(IndexValue.valueOf("owner", owner));
	}
	
	/**
	 * 创建道具
	 * @param item 道具
	 * @return
	 */
	public Item create(Item item) {
		short server = systemConfig.getServers().iterator().next();

		// 创建新实体
		final long id = idGenerator.getNext(server);
		item.setId(id);
		
		Item result = itemCache.create(item);
		
		return result;
	}
	
	/**
	 * 判断是否有足够道具可扣除
	 * @param owner 玩家标识
	 * @param costItems 道具列表
	 * @return
	 */
	public boolean canCostItem(long owner, Collection<Item> costItems)
	{
		HashMap<Integer,Integer> costItemNums = new HashMap<Integer,Integer>();
		for(Item item : costItems )
		{
			Integer oldNum = costItemNums.get(item.getBaseId());
			if(oldNum == null)oldNum = 0;
			costItemNums.put(item.getBaseId(), oldNum+item.getNum());
		}
		Collection<Item> items = loadByOwner(owner);
		for(Item item : items)
		{
			if( costItemNums.containsKey(item.getBaseId()))
			{
				if( item.getNum()>=costItemNums.get(item.getBaseId()))
				{
					costItemNums.remove(item.getBaseId());
				}
				else
				{
					costItemNums.put(item.getBaseId(), costItemNums.get(item.getBaseId())-item.getNum());
				}
			}
			if( costItemNums.isEmpty() )
			{
				return true;
			}
		}
		return false;
	}
	
	/**
	 * 扣除道具
	 * @param owner 玩家标识
	 * @param costItems 道具列表
	 * @return
	 */
	public void costItem(long owner, Collection<Item> costItems, List<Item> modifiedItems, List<Long> removedItems)
	{
		HashMap<Long,Item> modifiedItemMap = new HashMap<Long,Item>();
		HashMap<Integer,Integer> costItemNums = new HashMap<Integer,Integer>();
		for(Item item : costItems )
		{
			Integer oldNum = costItemNums.get(item.getBaseId());
			if(oldNum == null)oldNum = 0;
			costItemNums.put(item.getBaseId(), oldNum+item.getNum());
		}
		Collection<Item> items = loadByOwner(owner);
		for(Item item : items)
		{
			if( costItemNums.containsKey(item.getBaseId()))
			{
				if( item.getNum()>costItemNums.get(item.getBaseId()))
				{
					item.setNum(item.getNum()-costItemNums.get(item.getBaseId()));
					costItemNums.remove(item.getBaseId());
					modifiedItemMap.put(item.getId(), item);
				}
				else
				{
					costItemNums.put(item.getBaseId(), costItemNums.get(item.getBaseId())-item.getNum());
					if( costItemNums.get(item.getBaseId())==0 )
					{
						costItemNums.remove(item.getBaseId());
					}
					if( itemService.getItemType(item.getBaseId()) != ItemTypeConstant.ITEM_TYPE_ABSTRACT )
					{
						itemCache.remove(item);
						modifiedItemMap.remove(item.getId());
						removedItems.add(item.getId());
						
					}
					else
					{
						item.setNum(0);
						modifiedItemMap.put(item.getId(), item);
					}
				}
			}
			if( costItemNums.isEmpty() )
			{
				break;
			}
		}
		for(Item item : modifiedItemMap.values())
		{
			modifiedItems.add(item);
		}
	}
	
	/**
	 * 增加道具
	 * @param owner 玩家标识
	 * @param addItems 道具列表
	 * @return
	 */
	public void addItem(long owner, Collection<Item> addItems, List<Item> modifiedItems)
	{
		HashMap<Long,Item> modifiedItemMap = new HashMap<Long,Item>();
		HashMap<Integer,Integer> addItemNums = new HashMap<Integer,Integer>();
		for(Item item : addItems )
		{
			ItemList itemList = itemListStorage.get(item.getBaseId(), true);
			if(itemList.getPackNum()<2)
			{
				item.setOwner(owner);
				create(item);
				modifiedItemMap.put(item.getId(), item);
				continue;
			}
			Integer oldNum = addItemNums.get(item.getBaseId());
			if(oldNum == null)oldNum = 0;
			addItemNums.put(item.getBaseId(), oldNum+item.getNum());
		}
		Collection<Item> items = loadByOwner(owner);
		for(Item item : items)
		{
			if( addItemNums.containsKey(item.getBaseId()))
			{
				if( itemService.getItemType(item.getBaseId()) == ItemTypeConstant.ITEM_TYPE_ABSTRACT )
				{
					item.setNum(item.getNum()+addItemNums.get(item.getBaseId()));
					addItemNums.remove(item.getBaseId());
					modifiedItemMap.put(item.getId(), item);
					continue;
				}
				ItemList itemList = itemListStorage.get(item.getBaseId(), true);
				if( item.getNum() >= itemList.getPackNum() )
				{
					continue;
				}
				if( item.getNum()+addItemNums.get(item.getBaseId()) <= itemList.getPackNum())
				{
					item.setNum(item.getNum()+addItemNums.get(item.getBaseId()));
					addItemNums.remove(item.getBaseId());
				}
				else
				{
					addItemNums.put(item.getBaseId(), addItemNums.get(item.getBaseId())-(itemList.getPackNum()-item.getNum()));
					item.setNum(itemList.getPackNum());
				}
				modifiedItemMap.put(item.getId(), item);
			}
			if( addItemNums.isEmpty() )
			{
				break;
			}
		}
		for(Map.Entry<Integer,Integer> itemEntry : addItemNums.entrySet())
		{
			int baseId = itemEntry.getKey();
			int itemNum = itemEntry.getValue();
			if( itemService.getItemType(baseId) == ItemTypeConstant.ITEM_TYPE_ABSTRACT )
			{
				Item item = create(Item.valueOf(0, owner, baseId, itemNum, ""));
				modifiedItemMap.put(item.getId(), item);
				continue;
			}
			ItemList itemList = itemListStorage.get(baseId, true);

			while( itemNum > 0 )
			{
				if( itemNum > itemList.getPackNum())
				{
					Item item = create(Item.valueOf(0, owner, baseId, itemList.getPackNum(), ""));
					itemNum -= itemList.getPackNum();
					modifiedItemMap.put(item.getId(), item);
				}
				else
				{
					Item item = create(Item.valueOf(0, owner, baseId, itemNum, ""));
					itemNum = 0;
					modifiedItemMap.put(item.getId(), item);
				}
			}
		}
		for(Item item : modifiedItemMap.values())
		{
			modifiedItems.add(item);
		}
	}
}
