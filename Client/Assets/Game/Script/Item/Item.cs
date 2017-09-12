using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class Item
{
    #region Fields
    int m_itemId;
    int m_num;
    #endregion

    #region Properties
    public int ItemId { get { return m_itemId; } set { m_itemId = value; } }
    public int Num { get { return m_num; } set { m_num = value; } }
    #endregion

    public static Item Create(ItemVo vo)
    {
        Item item;
        item = new Item();
        item.LoadFromVo(vo);
        return item;
    }

    virtual public void LoadFromVo(ItemVo vo)
    {
        m_itemId = vo.itemId;
        m_num = vo.num;
    }
}