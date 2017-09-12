using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UITreasureBattleIcon : MonoBehaviour
{
    public ImageEx m_icon;
    public Text m_name;
    public StateHandle m_button;
    public StateHandle m_battlePos;
    private int m_index;
    private Treasure m_treasure;
    private bool m_eventAdded = false;

    public void Init(int index)
    {
        m_index = index;
        Role hero = RoleMgr.instance.Hero;
        m_treasure = hero.TreasurePart.GetTreasure(hero.TreasurePart.BattleTreasures[index]);

        TreasureCfg treasureCfg = TreasureCfg.m_cfgs[m_treasure.treasureId];

        m_icon.Set(treasureCfg.icon);
        m_name.text = treasureCfg.name + " Lv." + m_treasure.level;

        m_battlePos.SetState(index);

        if(!m_eventAdded)
        {
            m_button.AddClick(OnClick);
            m_eventAdded = true;
        }
    }

    void OnClick()
    {
        Role hero = RoleMgr.instance.Hero;
        hero.TreasurePart.CancelBattle(m_treasure.treasureId);
    }

}
