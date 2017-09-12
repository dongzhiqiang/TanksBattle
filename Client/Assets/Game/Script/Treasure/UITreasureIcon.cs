using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UITreasureIcon : MonoBehaviour
{
    public ImageEx m_icon;
    public Text m_name;
    public StateHandle m_button;
    public StateHandle m_battlePos;
    public GameObject m_tip;
    public Text m_pieceNum;
    public ImageEx m_progress;
    private int m_treasureId;

    public void Init(int treasureId)
    {
        m_treasureId = treasureId;

        Role hero = RoleMgr.instance.Hero;

        TreasureCfg treasureCfg = TreasureCfg.m_cfgs[treasureId];
        Treasure treasure = hero.TreasurePart.GetTreasure(treasureId);
        int treasureLevel = 0;
        bool enabled = false;
        if(treasure!=null)
        {
            treasureLevel = treasure.level;
            enabled = true;

        }
        TreasureLevelCfg levelCfg = TreasureLevelCfg.Get(treasureId, treasureLevel+1);
        if(treasure == null)
        {
            if (hero.ItemsPart.GetItemNum(treasureCfg.pieceId) >= levelCfg.pieceNum)
            {
                enabled = true;
            }

        }

        if (m_pieceNum != null && m_progress != null)
        {
            if (levelCfg == null)
            {
                m_progress.fillAmount = 1.0f;
                m_pieceNum.text = "最高";
            }
            else
            {
                m_pieceNum.text = string.Format("{0}/{1}", hero.ItemsPart.GetItemNum(treasureCfg.pieceId), levelCfg.pieceNum);
                float rate = (float)hero.ItemsPart.GetItemNum(treasureCfg.pieceId) / levelCfg.pieceNum;
                if (rate > 1f) rate = 1f;
                m_progress.fillAmount = rate;
            }
        }        

        if (m_tip != null)
        {
            if (Treasure.CanUpgrade(treasureId))
            {
                m_tip.SetActive(true);
            }
            else
            {
                m_tip.SetActive(false);
            }
        }        

        if(enabled)
        {
            foreach (ImageEx image in GetComponentsInChildren<ImageEx>())
            {
                image.SetGrey(false);
            }
        }
        else
        {
            foreach (ImageEx image in GetComponentsInChildren<ImageEx>())
            {
                image.SetGrey(true);
            }
        }

        m_icon.Set(treasureCfg.icon);

        if (m_name != null)
            m_name.text = treasureCfg.name + " Lv." + treasureLevel;

        if (m_battlePos != null)
        {
            int pos = hero.TreasurePart.BattleTreasures.IndexOf(treasureId);
            if (pos == -1)
            {
                m_battlePos.SetState(3);
            }
            else
            {
                m_battlePos.SetState(pos);
            }
        }        

        if (m_button != null)
            m_button.AddClick(OnClick);
    }

    void OnClick()
    {
        UIMgr.instance.Open<UITreasureInfo>(m_treasureId);
    }

}
