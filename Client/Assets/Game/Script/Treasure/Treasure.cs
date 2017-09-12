using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class Treasure
{
    #region Fields
    public int treasureId;
    public int level;
    #endregion



    private static PropertyTable m_temp = new PropertyTable();
    private static PropertyTable m_temp2 = new PropertyTable();

    public static Treasure Create(TreasureVo vo)
    {
        Treasure treasure;
        treasure = new Treasure();
        treasure.LoadFromVo(vo);
        return treasure;
    }

    virtual public void LoadFromVo(TreasureVo vo)
    {
        treasureId = vo.treasureId;
        level = vo.level;
    }


    public static bool CanUpgrade(int treasureId)
    {
        TreasureCfg treasureCfg = TreasureCfg.m_cfgs[treasureId];
        Role hero = RoleMgr.instance.Hero;
        Treasure treasure = hero.TreasurePart.GetTreasure(treasureId);
        int treasureLevel = 0;

        if (treasure != null)
        {
            treasureLevel = treasure.level;

        }
        TreasureLevelCfg levelCfg = TreasureLevelCfg.Get(treasureId, treasureLevel+1);
        if (levelCfg==null)
        {
            return false;
        }
        if (hero.ItemsPart.GetItemNum(treasureCfg.pieceId) >= levelCfg.pieceNum)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}