
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class UIPetPageTalent : MonoBehaviour
{
    #region SerializeFields
    public StateGroup m_talents;
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


        List<Talent> talents = new List<Talent>();
        Talent talent = new Talent();
        foreach (string talentId in pet.Cfg.talents)
        {
            talents.Add(pet.TalentsPart.GetTalent(talentId));
        }

        m_talents.SetCount(talents.Count);
        for (int i = 0; i < talents.Count; i++)
        {
            m_talents.Get<UIPetTalent>(i).Init(pet, talents[i]);
        }
    }

    public void StartFx(string talentId)
    {
        foreach (UIPetTalent uiPetTalent in m_talents.GetComponentsInChildren<UIPetTalent>())
        {
            if (uiPetTalent.IsTalentId(talentId))
            {
                uiPetTalent.StartFx();
                break;
            }
        }
    }

    #endregion

    #region Private Methods


    #endregion

}
