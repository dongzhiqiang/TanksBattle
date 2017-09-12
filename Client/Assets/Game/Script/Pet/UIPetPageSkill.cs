
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class UIPetPageSkill : MonoBehaviour
{
    #region SerializeFields
    public StateGroup m_skills;
    #endregion

    #region Fields
    UIPet m_parent;
    Role m_pet;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化
    public void OnInitPage(UIPet parent)
    {
        m_parent = parent;
    }

    //显示
    public void OnOpenPage(Role pet)
    {
        m_pet = pet;

        List<PetSkill> petSkills = new List<PetSkill>();

        PetSkill petSkill = new PetSkill();
        petSkills.Add(pet.PetSkillsPart.GetPetSkill(pet.Cfg.atkUpSkill));
        foreach(string skillId in pet.Cfg.skills)
        {
            petSkills.Add(pet.PetSkillsPart.GetPetSkill(skillId));
        }
        m_skills.SetCount(petSkills.Count);
        for (int i = 0; i < petSkills.Count; i++)
        {
            m_skills.Get<UIPetSkill>(i).Init(pet, petSkills[i]);
        }
    }

    public void StartFx(string skillId)
    {
        foreach(UIPetSkill uiPetSkill in m_skills.GetComponentsInChildren<UIPetSkill>())
        {
            if(uiPetSkill.IsSkillId(skillId))
            {
                uiPetSkill.StartFx();
                break;
            }
        }
    }

    #endregion

    #region Private Methods


    #endregion

}
