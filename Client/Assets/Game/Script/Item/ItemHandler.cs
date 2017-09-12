using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[NetModule(MODULE.MODULE_ITEM)]
public class ItemHandler
{
    #region Fields
    
    #endregion


    #region Properties
    #endregion


    #region Net
    [NetHandler(MODULE_ITEM.PUSH_ADD_OR_UPDATE_ITEM)]
    public void OnAddOrUpdateItem(AddOrUpdateItemVo info)
    {
        Role role = RoleMgr.instance.Hero;
        ItemsPart itemsPart = role.ItemsPart;
        itemsPart.AddOrUpdateItem(info.item);        
    }

    [NetHandler(MODULE_ITEM.PUSH_ADD_OR_UPDATE_ITEMS)]
    public void OnAddOrUpdateItem(AddOrUpdateItemsVo info)
    {
        Role role = RoleMgr.instance.Hero;
        ItemsPart itemsPart = role.ItemsPart;
        foreach (var item in info.items)
            itemsPart.AddOrUpdateItem(item);
    }

    [NetHandler(MODULE_ITEM.PUSH_REMOVE_ITEM)]
    public void OnRemoveItem(RemoveItemVo info)
    {
        Role role = RoleMgr.instance.Hero;
        ItemsPart itemsPart = role.ItemsPart;
        itemsPart.RemoveItem(info.itemId);        
    }

    [NetHandler(MODULE_ITEM.PUSH_REMOVE_ITEMS)]
    public void OnRemoveItems(RemoveItemsVo info)
    {
        Role role = RoleMgr.instance.Hero;
        ItemsPart itemsPart = role.ItemsPart;
        foreach (var itemId in info.itemIds)
            itemsPart.RemoveItem(itemId);        
    }
    #endregion
}
