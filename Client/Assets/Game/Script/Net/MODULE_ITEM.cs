using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class MODULE_ITEM
{
    public const int PUSH_ADD_OR_UPDATE_ITEM = -1;   //添加或更新物品
    public const int PUSH_ADD_OR_UPDATE_ITEMS = -2;   //批量添加或更新物品
    public const int PUSH_REMOVE_ITEM = -3;   //删除物品
    public const int PUSH_REMOVE_ITEMS = -4;   //批量删除物品
}


public class AddOrUpdateItemVo
{
    public bool isAdd;
    public ItemVo item;
}

public class AddOrUpdateItemsVo
{
    public bool isAdd;
    public List<ItemVo> items;
}

public class RemoveItemVo
{
    public int itemId;
}

public class RemoveItemsVo
{
    public List<int> itemIds;
}

public class ItemVo
{
    public int itemId;  //物品配置ID
    public int num;     //物品数量
}

