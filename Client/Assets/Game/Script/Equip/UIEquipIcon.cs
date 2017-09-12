using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIEquipIcon : MonoBehaviour
{
    public ImageEx m_icon;
    public ImageEx m_background;
    public Text m_level;
    public RectTransform m_lock;
    public bool roundQualityBg = false;

    public void Init(int equipId, int level, int advLv)
    {
        if (equipId == 0)
        {
            if (m_lock)
                m_lock.gameObject.SetActive(true);
            m_icon.Set(null);
            m_background.Set(null);
            m_level.text = "";
        }
        else
        {
            if (m_lock)
                m_lock.gameObject.SetActive(false);
            EquipCfg equipCfg = EquipCfg.m_cfgs[equipId];
            m_icon.Set(equipCfg.icon);
            EquipAdvanceRateCfg advCfg = EquipAdvanceRateCfg.m_cfgs[advLv];
            QualityCfg qualityCfg = QualityCfg.m_cfgs[advCfg.quality];
            m_background.Set(roundQualityBg ? qualityCfg.backgroundRound : qualityCfg.backgroundSquare);
            m_level.text = "Lv." + level;
            m_level.color = QualityCfg.GetColor(advCfg.quality);
        }        
    }
}
