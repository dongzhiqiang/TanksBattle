using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UIPetTalentInfo : UIPanel
{
    #region SerializeFields
    public Text m_petTalentName;
    public ImageEx m_icon;
    public Text m_description;
    public Text m_petTalentLevel;
    public Text m_type;
    
    #endregion
    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        Talent talent = (Talent)param;
        TalentCfg talentCfg = TalentCfg.m_cfgs[talent.talentId];
        m_petTalentName.text = talentCfg.name;
        m_icon.Set(talentCfg.icon);
       
        m_petTalentLevel.text = "Lv." + talent.level;
        //m_type.text = "类型：" + TalentTypeCfg.m_cfgs[talentCfg.type].name;
        if(m_type!=null)
        {
            m_type.gameObject.SetActive(false);
        }
        string description = "本级属性：";
        BuffCfg buffCfg = BuffCfg.Get(talentCfg.stateId);
        description += LvValue.ParseText(buffCfg.desc, talent.level);
        /*
        PetTalentLvCfg talentLvCfg = PetTalentLvCfg.m_cfgs[talentCfg.upgradeId + talent.level - 1];
        for (enProp i = enProp.minFightProp + 1; i < enProp.maxFightProp; i++)
        {
            if (talentLvCfg.props.GetFloat(i) > Mathf.Epsilon)
            {
                PropTypeCfg propTypeCfg = PropTypeCfg.m_cfgs[(int)i];
                description += propTypeCfg.name + "+";
                if (propTypeCfg.format == enPropFormat.FloatRate)
                {
                    description += String.Format("{0:F}", talentLvCfg.props.GetFloat(i) * 100) + "%";
                }
                else
                {
                    description += Mathf.RoundToInt(talentLvCfg.props.GetFloat(i));
                }
            }
        }
         */
        if(talent.level < talentCfg.maxLevel)
        {
            description += "\n\n下级属性：";
            description += LvValue.ParseText(buffCfg.desc, talent.level+1);
            /*
            PetTalentLvCfg talentLvCfgNext = PetTalentLvCfg.m_cfgs[talentCfg.upgradeId + talent.level];
            for (enProp i = enProp.minFightProp + 1; i < enProp.maxFightProp; i++)
            {
                if (talentLvCfgNext.props.GetFloat(i) > Mathf.Epsilon)
                {
                    PropTypeCfg propTypeCfg = PropTypeCfg.m_cfgs[(int)i];
                    description += propTypeCfg.name + "+";
                    if (propTypeCfg.format == enPropFormat.FloatRate)
                    {
                        description += String.Format("{0:F}", talentLvCfgNext.props.GetFloat(i) * 100) + "%";
                    }
                    else
                    {
                        description += Mathf.RoundToInt(talentLvCfgNext.props.GetFloat(i));
                    }
                }
            }
             */
        }
        m_description.text = description;
    }

}