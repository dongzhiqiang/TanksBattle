using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIPetSkillItem : MonoBehaviour
{
    public ImageEx m_icon;
    public Text m_name;
    public Text m_level;
    public StateHandle m_button;
    public StateHandle m_state;
    public Text m_reason;
    private PetSkill m_petSkill = new PetSkill();
    private string m_petId;

    public void Init(PetInfo petInfo, string skillId, int skillLevel)
    {
        m_petSkill.skillId = skillId;
        m_petSkill.level = skillLevel;
        m_petId = petInfo.petId;
        RoleSystemSkillCfg roleSkillCfg = RoleSystemSkillCfg.Get(petInfo.petId, skillId);
        if (roleSkillCfg == null)
            return;

        SystemSkillCfg skillCfg = SystemSkillCfg.Get(petInfo.petId, skillId);
        
        m_icon.Set(skillCfg.icon);
        if(m_state!=null)
        {
            m_name.text = roleSkillCfg.name;

            if (petInfo.star < roleSkillCfg.needPetStar)
            {
                m_state.SetState(1);
                m_reason.text = "升星至<color=#d5aa64>" + roleSkillCfg.needPetStar + "星</color>开启";
            }
            else
            {
                m_state.SetState(0);
                m_level.text = " Lv." + skillLevel;
            }
        }
        else
        {
            m_level.text = " Lv." + skillLevel;
        }

        if (m_button != null)
            m_button.AddClick(OnClick);
    }

    void OnClick()
    {
        UIMgr.instance.Open<UIPetSkillInfo>(new object[] { m_petId, m_petSkill });
    }

}
