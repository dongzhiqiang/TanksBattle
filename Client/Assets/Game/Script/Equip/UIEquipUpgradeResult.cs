using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class UIEquipUpgradeResult : UIPanel
{
    #region SerializeFields
    public StateGroup m_items;
    public float m_interval = 0.2f;
    #endregion SerializeFields

    private List<GrowEquipVo> m_grows;
    private List<GrowEquipVo> m_showGrows;
    private int m_growsIndex;

    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_grows = (List<GrowEquipVo>)param;
        m_showGrows = new List<GrowEquipVo>();
        m_growsIndex = 0;
        
    }

    public override void OnOpenPanelEnd()
    {
        ShowNextGrow();
        UIFxPanel.ShowFx("fx_ui_zhuangbei_shengjichengong", new Vector3(0, 200, 0));
    }

    void ShowNextGrow()
    {
        m_showGrows.Add(m_grows[m_growsIndex]);
        if(m_showGrows.Count>3)
        {
            m_showGrows.RemoveAt(0);
        }
        m_items.SetCount(m_showGrows.Count);
        for(int i=0;i<m_showGrows.Count;i++)
        {
            m_items.Get<UIEquipGrowItem>(i).Init(m_showGrows[i]);
        }
        m_growsIndex++;
        if(m_growsIndex<m_grows.Count)
        {
            TimeMgr.instance.AddTimer(m_interval, ShowNextGrow);
        }
    }
}