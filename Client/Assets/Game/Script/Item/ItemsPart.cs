#region Header
/**
 * 名称：物品部件
 
 * 日期：2015.9.21
 * 描述：背包和装备
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemsPart : RolePart
{
    #region Fields
    /// <summary>
    /// 虚拟物品的物品ID和对应的用户属性映射
    /// </summary>
    private static Dictionary<int, enProp> s_ItemId_RoleProp_Map = new Dictionary<int, enProp>() {
        {ITEM_ID.GOLD, enProp.gold},
        {ITEM_ID.EXP, enProp.exp},
        {ITEM_ID.DIAMOND, enProp.diamond},
        {ITEM_ID.STAMINA, enProp.stamina},
        {ITEM_ID.ARENA_COIN, enProp.arenaCoin},
    };

    /// <summary>
    /// 物品ID与物品对象的映射
    /// </summary>
    private Dictionary<int, Item> m_items;
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.items; } }
    public Dictionary<int, Item> Items { get { return m_items; } }
    #endregion


    #region Frame    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {
        //Debuger.Log("背包部件初始化");
        if( vo.items != null )
        {
            m_items = new Dictionary<int, Item>();
            foreach (ItemVo itemVo in vo.items)
            {
                //虚拟物品不能这里添加
                if (s_ItemId_RoleProp_Map.ContainsKey(itemVo.itemId))
                    return;

                Item item = Item.Create(itemVo);
                AddOrUpdateItem(item);
            }
        }
    }
    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
    }

    public override void OnClear()
    {
        if (m_items != null)
            m_items.Clear();
    }

    public bool IsAbstractItem(int itemId)
    {
        return s_ItemId_RoleProp_Map.ContainsKey(itemId);
    }

    public enProp GetAbstractItemEnum(int itemId)
    {
        enProp e;
        if (s_ItemId_RoleProp_Map.TryGetValue(itemId, out e))
            return e;
        else
            return enProp.max;
    }

    /// <summary>
    /// 注意costItems里itemId不能重复
    /// </summary>
    /// <param name="costItems"></param>
    /// <param name="needItemId"></param>
    /// <returns></returns>
    public bool CanCost(List<CostItem> costItems, out int needItemId)
    {
        foreach(CostItem costItem in costItems)
        {
            if (GetItemNum(costItem.itemId) < costItem.num)
            {
                needItemId = costItem.itemId;
                return false;
            }
        }
        needItemId = 0;
        return true;
    }

    public int GetGold()
    {
        return GetItemNum(ITEM_ID.GOLD);
    }

    public int GetItemNum(int itemId)
    {
        enProp propId;
        if (s_ItemId_RoleProp_Map.TryGetValue(itemId, out propId))
        {
            switch (propId)
            {
                case enProp.stamina:
                    return this.PropPart.GetStamina();
                default:
                    return this.PropPart.GetInt(propId);
            }            
        }

        if (m_items == null)
            return 0;

        int num = 0;
        Item item;
        if (m_items.TryGetValue(itemId, out item))
        {
            num = item.Num;
        }
        return num;
    }

    public void AddOrUpdateItem(Item item)
    {
        //虚拟物品不能这里修改
        if (s_ItemId_RoleProp_Map.ContainsKey(item.ItemId))
            return;

        if (m_items == null)
            return;

        m_items[item.ItemId] = item;
        m_parent.Fire(MSG_ROLE.ITEM_CHANGE, null);
    }

    public void AddOrUpdateItem(ItemVo itemVo)
    {
        //虚拟物品不能这里修改
        if (s_ItemId_RoleProp_Map.ContainsKey(itemVo.itemId))
            return;

        if (m_items == null)
            return;

        Item item;
        if (m_items.TryGetValue(itemVo.itemId, out item))
        {
            item.LoadFromVo(itemVo);
            m_parent.Fire(MSG_ROLE.ITEM_CHANGE, null);
        }
        else
        {
            item = Item.Create(itemVo);
            AddOrUpdateItem(item);
        }
    }

    public bool RemoveItem(Item item)
    {
        if (m_items == null)
            return false;
        return RemoveItem(item.ItemId);
    }

    public bool RemoveItem(int itemId)
    {
        if (m_items == null)
            return false;
        bool ok = m_items.Remove(itemId);
        if (ok)
            m_parent.Fire(MSG_ROLE.ITEM_CHANGE, null);
        return ok;
    }

    #endregion


    #region Private Methods
    
    #endregion
}