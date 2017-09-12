using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UITreasure : UIPanel
{
    #region SerializeFields
    public StateGroup m_treasures;
    public StateGroup m_battleTreasures;
    public Text m_power;
    #endregion

    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        UpdateTreasures();

        //GetComponent<ShowUpController>().Prepare();
    }

    void UpdateTreasures()
    {
        List<int> treasures = new List<int>();
        Role hero = RoleMgr.instance.Hero;
        foreach (Treasure treasure in hero.TreasurePart.Treasures.Values)
        {
            treasures.Add(treasure.treasureId);
        }
        treasures.Sort();
        List<int> treasures2 = new List<int>();
        List<int> treasures3 = new List<int>();
        foreach (TreasureCfg cfg in TreasureCfg.m_cfgs.Values)
        {
            if (treasures.IndexOf(cfg.id)==-1)
            {
                TreasureLevelCfg levelCfg = TreasureLevelCfg.Get(cfg.id, 1);
                if(hero.ItemsPart.GetItemNum(cfg.pieceId) >= levelCfg.pieceNum)
                {
                    treasures2.Add(cfg.id);
                }
                else
                {
                    treasures3.Add(cfg.id);
                }
                
            }
                
        }
        treasures2.Sort();
        treasures3.Sort();
        m_treasures.SetCount(treasures.Count + treasures2.Count+treasures3.Count);
        for (int i = 0; i < treasures.Count; i++)
        {
            m_treasures.Get<UITreasureIcon>(i).Init(treasures[i]);
        }
        for (int i = 0; i < treasures2.Count; i++)
        {
            m_treasures.Get<UITreasureIcon>(i+treasures.Count).Init(treasures2[i]);
        }
        for (int i = 0; i < treasures3.Count; i++)
        {
            m_treasures.Get<UITreasureIcon>(i + treasures.Count+treasures2.Count).Init(treasures3[i]);
        }

        m_battleTreasures.SetCount(hero.TreasurePart.BattleTreasures.Count);
        for(int i=0;i<hero.TreasurePart.BattleTreasures.Count; i++)
        {
            m_battleTreasures.Get<UITreasureBattleIcon>(i).Init(i);
        }

        m_power.text = hero.TreasurePart.GetTreasurePower().ToString();
    }

    public void Refresh()
    {
        UpdateTreasures();
    }


    public override void OnOpenPanelEnd()
    {
        //GetComponent<ShowUpController>().Start();
    }

}