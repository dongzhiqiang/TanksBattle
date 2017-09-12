using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIPetSkillInfo : UIPanel
{
    #region SerializeFields
    public Text m_petSkillName;
    public ImageEx m_icon;
    public Text m_description;
    public Text m_petSkillLevel;
    public Text m_type;
    
    #endregion
    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        object[] pp = (object[])param;
        string petId = (string)pp[0];
        PetSkill petSkill = (PetSkill)pp[1];

        RoleSystemSkillCfg roleSkillCfg = RoleSystemSkillCfg.Get(petId, petSkill.skillId);
        SystemSkillCfg skillCfg = SystemSkillCfg.Get(petId, petSkill.skillId);

        m_petSkillName.text = roleSkillCfg.name;
        m_icon.Set(skillCfg.icon);
        m_description.text = roleSkillCfg.description;
        m_petSkillLevel.text = "Lv."+petSkill.level;
        if(m_type!=null)
        {
            m_type.gameObject.SetActive(false);
        }
        //m_type.text = "类型：" + roleSkillCfg.type;
    }

}