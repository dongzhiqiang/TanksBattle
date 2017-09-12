using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UILevelPause : UIPanel
{
    #region SerializeFields
    public StateHandle m_btnContinue;
    public StateHandle m_btnQuit;
    public TextEx m_title;
    public TextEx m_title2;
    public TextEx m_target;
    public List<ImageEx> mStars;
    public List<ImageEx> mFails;
    public List<TextEx> mTips;

    public GameObject bg1;
    public GameObject bg2;

    TimeMgr.TimeScaleHandle m_timeHandle;

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {        
        m_btnContinue.AddClick(OnBtnContinue);
        m_btnQuit.AddClick(OnBtnQuit);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        SetTips();

        m_title.text = Room.instance.roomCfg.roomName;
        m_title2.text = Room.instance.roomCfg.roomName;

        bg1.gameObject.SetActive(false);
        bg2.gameObject.SetActive(false);

        RoomCfg cfg = LevelMgr.instance.CurLevel.roomCfg;
        if (!string.IsNullOrEmpty(cfg.targetDesc))
        {
            bg2.gameObject.SetActive(true);
            m_target.text = cfg.targetDesc;
        }
        else
        {
            bg1.gameObject.SetActive(true);
        }

        //清空飘血数字
        UIMgr.instance.Get<UILevel>().Get<UILevelAreaNum>().ClearNum();

        if (m_timeHandle != null && !m_timeHandle.IsOver)
            return;

        m_timeHandle = TimeMgr.instance.AddTimeScale(0, -1, 100);

    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        UILevelAreaSetting areaSet = UIMgr.instance.Get<UILevel>().Get<UILevelAreaSetting>();
        if (areaSet == null)
            return;

        //areaSet.m_pause.Set("ui_guanqia_anniu_01");

        if (m_timeHandle != null)
        {
            m_timeHandle.Release();
            m_timeHandle = null;
        }
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

     #region Private Methods
    void OnBtnQuit()
    {
        //UIMgr.instance.Get<UILevel>().Get<UILevelAreaSetting>().m_pause.Set("ui_guanqia_anniu_01");
        this.Close();
        LevelMgr.instance.GotoMaincity();
    }

    void OnBtnContinue()
    {
        this.Close();
    }

    void SetTips()
    {
        List<SceneTrigger> triList = SceneEventMgr.instance.conditionTriggerList;
        if (triList != null && triList.Count > 0)
        {
            for (int i = 0; i < triList.Count; i++)
            {
                mTips[i].gameObject.SetActive(true);
                mStars[i].gameObject.SetActive(true);
                mFails[i].gameObject.SetActive(true);
                mTips[i].text = triList[i].GetDesc();
                mStars[i].gameObject.SetActive(triList[i].bReach());
            }
        }
        else
        {
            for(int i=0;i<mTips.Count;i++)
            {
                mTips[i].gameObject.SetActive(false);
                mStars[i].gameObject.SetActive(false);
                mFails[i].gameObject.SetActive(false);
            }
        }
     
    }
    
    #endregion

    #region Public Methods
   
    #endregion
}

