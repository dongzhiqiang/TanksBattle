using UnityEngine;
using System.Collections;

public class UIWeaponTalentRow : MonoBehaviour {
    public TextEx skillName;
    public StateGroup group;
    
    WeaponSkill skill=null;

    public void Init(WeaponSkill s)
    {
        this.skill = s;

        skillName.text = s.RoleSkillCfg.name;

        group.SetCount(skill.TalentCount);
        for(int i = 0;i< skill.TalentCount; ++i)
        {
            UIWeaponTalentItem item =group.Get<UIWeaponTalentItem>(i);
            WeaponSkillTalent talent = skill.GetTalent(i);
            item.Init(talent);
        }
        
    }
    
}
