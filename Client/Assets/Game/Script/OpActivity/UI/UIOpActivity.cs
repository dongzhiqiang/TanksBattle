using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIOpActivity : UIPanel
{
    #region SerializeFields
    public UICheckIn UICheckIn;
    public UILevelReward UILevelReward;
    public StateGroup m_opActivityGroup;
    public GameObject kuang;
    private List<GameObject> opActivities = new List<GameObject>();
    //public StateHandle m_btnQuit;
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_opActivityGroup.AddSel(OnSelectOpActivityItem);
        opActivities.Add(UICheckIn.gameObject);
        opActivities.Add(UILevelReward.gameObject);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        kuang.SetActive(true);
        //初始化左侧图标栏
        m_opActivityGroup.SetCount(OpActivitiySortCfg.m_cfgs.Count);

        for (int i = 0; i < m_opActivityGroup.Count; i++)
        {
            UIOpActivityItem item = m_opActivityGroup.Get<UIOpActivityItem>(i);          
            OpActivitiySortCfg opActivitySortCfg = OpActivitiySortCfg.m_cfgs[OpActivitiySortCfg.GetIdByLocation(i + 1)];
            item.init(opActivitySortCfg);

        }
        m_opActivityGroup.SetSel(0);

        

    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        //确保没有残影
        TimeMgr.instance.AddTimer(0.1f, () => { kuang.SetActive(false); });
        Role hero = RoleMgr.instance.Hero;
        OpActivityPart opActivityPart = hero.OpActivityPart;
        opActivityPart.CheckTip();
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods
    void OnBtnQuit()
    {
        this.Close();
        //LevelMgr.Instance.GotoMaincity();
    }

    void OnBtnContinue()
    {
        this.Close();
    }

    void OnSelectOpActivityItem(StateHandle s, int idx)
    {
        UIOpActivityItem item = s.GetComponent<UIOpActivityItem>();

        switch (item.id)
        {
            case (0):
                for (int i = 0; i < opActivities.Count; i++)
                {
                    if (opActivities[i] == UICheckIn.gameObject)
                    {
                        opActivities[i].SetActive(true);
                    }
                    else
                    {
                        opActivities[i].SetActive(false);
                    }
                }
                UICheckIn.GetComponent<UICheckIn>().Init();
                break;
            case (1):
                for (int i = 0; i < opActivities.Count; i++)
                {
                    if (opActivities[i] == UILevelReward.gameObject)
                    {
                        opActivities[i].SetActive(true);
                    }
                    else
                    {
                        opActivities[i].SetActive(false);
                    }
                }
                UILevelReward.GetComponent<UILevelReward>().Init();
                break;
        }       
    }
    #endregion

    public void SelectOpActivity(int id)
    {
        OpActivitiySortCfg opActivitySortCfg = OpActivitiySortCfg.m_cfgs[id];
        m_opActivityGroup.SetSel(opActivitySortCfg.location - 1);
    }
}



