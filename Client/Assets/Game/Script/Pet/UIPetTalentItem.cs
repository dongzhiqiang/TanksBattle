using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIPetTalentItem : MonoBehaviour
{
    public ImageEx m_icon;
    public Text m_name;
    public Text m_level;
    public StateHandle m_button;
    public StateHandle m_state;
    public Text m_reason;
    private Talent m_talent = new Talent();
    private bool m_eventAdded = false;

    public void Init(PetInfo petInfo, string talentId, int talentLevel, int pos)
    {
        m_talent.talentId = talentId;
        m_talent.level = talentLevel;
        m_talent.pos = pos;
        TalentCfg talentCfg = TalentCfg.m_cfgs[talentId];
        TalentPosCfg talentPosCfg = TalentPosCfg.m_cfgs[pos];
        
        m_icon.Set(talentCfg.icon);
        if(m_state != null)
        {
            m_name.text = talentCfg.name;

            if (petInfo.advLv < talentPosCfg.needAdvLv)
            {
                m_state.SetState(1);
                PetAdvLvPropRateCfg advLvCfg = PetAdvLvPropRateCfg.m_cfgs[talentPosCfg.needAdvLv];
                QualityCfg qualityCfg = QualityCfg.m_cfgs[advLvCfg.quality];
                m_reason.text = "进阶至" + "<color=#" + qualityCfg.color + ">" + qualityCfg.name + (advLvCfg.qualityLevel > 0 ? ("+" + advLvCfg.qualityLevel) : "") + "</color>" + "开启";
            }
            else
            {
                m_state.SetState(0);
                m_level.text = " Lv." + talentLevel;
            }
        }
        else
        {
            m_level.text = " Lv." + talentLevel;
        }


        if (m_button != null)
            m_button.AddClick(OnClick);
    }

    void OnClick()
    {
        UIMgr.instance.Open<UIPetTalentInfo>(m_talent);
    }
}
