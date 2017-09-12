using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class PetSkill
{
    public string skillId;
    public int level;
    public static PetSkill Create(PetSkillVo vo)
    {
        PetSkill petSkill;
        petSkill = new PetSkill();
        petSkill.LoadFromVo(vo);
        return petSkill;
    }

    virtual public void LoadFromVo(PetSkillVo vo)
    {
        skillId = vo.skillId;
        level = vo.level;
    }
}