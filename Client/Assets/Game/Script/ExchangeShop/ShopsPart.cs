using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class ShopsPart : RolePart
{
    #region Fields   
    List<Shop> m_shops = new List<Shop>();
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.shop; } }
    public List<Shop> shops { get { return m_shops; } }
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
        List<Shop> netShops = vo.shops;
        if (netShops == null)
            return;
        else
        {
            m_shops = netShops;
            for (int i = 0; i < m_shops.Count; ++i)
            {
                m_shops[i] = SortShop(m_shops[i]);
            }
        }
    }

    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {


    }

    public override void OnClear()
    {
        m_shops.Clear();
    }
    #endregion


    #region Private Methods
    Shop SortShop(Shop shop)
    {
        for (int i = 0; i < shop.wares.Count - 1; i++)
        {
            for (int j = 0; j < shop.wares.Count - 1 - i; j++)
            {
                Ware temp =new Ware();
                if (shop.wares[j].wareId > shop.wares[j + 1].wareId)
                {
                    temp = shop.wares[j + 1];
                    shop.wares[j + 1] = shop.wares[j];
                    shop.wares[j] = temp;
                }
            }
        }     
        return shop;
    }
    #endregion

    public void UpdateShop(Shop shop)
    {
        for(int i=0;i<m_shops.Count;++i)
        {
            if(m_shops[i].shopId ==shop.shopId)
            {
                m_shops[i] = SortShop(shop);
                return;
            }
        }
    }

    public void UpdateWare(int shopId,Ware ware)
    {
        for (int i = 0; i < m_shops.Count; ++i)
        {
            if (m_shops[i].shopId == shopId)
            {
                for (int j = 0; j < m_shops[i].wares.Count; ++j)
                {
                    if (m_shops[i].wares[j].wareIndex == ware.wareIndex)
                    {
                        m_shops[i].wares[j] = ware;
                        return;
                    }
                }
            }
        }
    }

    
}
