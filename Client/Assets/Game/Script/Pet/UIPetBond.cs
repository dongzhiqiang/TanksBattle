using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIPetBond : UIPanel
{
    #region SerializeFields
    public StateGroup m_bonds;
    
    #endregion
    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {

        string petId = (string)param;
        RoleCfg roleCfg = RoleCfg.Get(petId);
        m_bonds.SetCount(roleCfg.petBonds.Count);
        for (int i = 0; i < roleCfg.petBonds.Count; i++)
        {
            m_bonds.Get<UIPetBondDetail>(i).Init(roleCfg.petBonds[i]);
        }
    }

}