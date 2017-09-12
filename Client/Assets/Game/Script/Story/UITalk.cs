using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UITalk : UIPanel
{

    #region Fields
    public StateHandle btnNext;
    public StateHandle btnSkip;

    public TextEx leftName;
    public TextEx rightName;

    public ImageEx leftIcon;
    public ImageEx rightIcon;

    public TextEx content;

    public GameObject leftNpc;
    public GameObject rightNpc;

    List<StoryCfg> m_storyList;
    StoryTalkCfg m_curTalk;

    float m_startTime;
    float m_openTime;
    bool m_isEnd;

    public static string HeroName = "[主角]";
    public static float SpeedDelayTime = 0.8f;

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        btnNext.AddClick(OnClickNext);
        btnSkip.AddClick(OnSkip);
        leftNpc.gameObject.SetActive(false);
        rightNpc.gameObject.SetActive(false);
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_curTalk = param as StoryTalkCfg;
        if (m_curTalk == null)
        {
            Debug.LogError("打开剧情对话窗口传参错误");
            UIMgr.instance.Close<UITalk>();
            return;
        }

        m_isEnd = false;
        UpdateTalk(m_curTalk);
        m_openTime = TimeMgr.instance.realTime;
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        leftNpc.gameObject.SetActive(false);
        rightNpc.gameObject.SetActive(false);
    }

    //显示动画播放完,保证在初始化之后
    public override void OnOpenPanelEnd()
    {

    }

    //关闭动画播放完，保证在初始化之后
    public override void OnClosePanelEnd()
    {

    }


    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {
        if (m_curTalk == null)
        {
            m_isEnd = true;
            return;
        }

        int time = (int)(TimeMgr.instance.realTime - m_startTime);
        if (time >= m_curTalk.time)
        {
            OnNext();
        }

        if (Input.GetKeyUp(KeyCode.Keypad5))
        {
            OnNext();
        }
    }
    #endregion

    #region Private

    public void OnSkip()
    {
        m_isEnd = true;
        if (ActionCamera.IsActionNow && ActionCamera.cameraHandle != null)
        {
            ActionCamera.cameraHandle.Release();
            ActionCamera.IsActionNow = false;
            RoleMgr.instance.Hero.CanMove = true;
        }
        StoryMgr.instance.SkipStory();
    }

    void OnClickNext()
    {
        if (TimeMgr.instance.realTime - m_openTime < SpeedDelayTime)
            return;

        if (StoryMgr.instance.CanSpeed)
            OnNext();
    }
    void OnNext()
    {
        StoryCfg cfg = StoryMgr.instance.GetNext();

        if (cfg == null || cfg.type != StoryType.STORY_TALK)
        {
            m_isEnd = true;
            this.Close();
            if (ActionCamera.IsActionNow && ActionCamera.cameraHandle != null)
            {
                ActionCamera.cameraHandle.Release();
                ActionCamera.IsActionNow = false;
                RoleMgr.instance.Hero.CanMove = true;
            }
            return;
        }

        UpdateTalk((StoryTalkCfg)cfg);
    }

    void OnShow()
    {
        if (m_curTalk == null)
            return;

        leftIcon.color = new Color(0.6F, 0.6F, 0.6F, 1F);
        rightIcon.color = new Color(0.6F, 0.6F, 0.6F, 1F);

        content.text = m_curTalk.content;
        if (RoleMgr.instance.Hero != null)
            content.text = m_curTalk.content.Replace(HeroName, RoleMgr.instance.Hero.GetString(enProp.name));
        Vector3 pos = content.transform.localPosition;
        StoryRoleCfg cfg = StoryRoleCfg.Get(m_curTalk.roleId);
        if (m_curTalk.localIdx == 0)  //左边显示
        {
            leftIcon.color = new Color(1F, 1F, 1F, 1F);

            leftNpc.gameObject.SetActive(true);
            leftName.text = cfg.name;
            if (RoleMgr.instance.Hero != null)
                leftName.text = cfg.name.Replace(HeroName, RoleMgr.instance.Hero.GetString(enProp.name));
            leftIcon.Set(cfg.bust);
            content.transform.localPosition = new Vector3(-80, pos.y, pos.z);
        }
        else if (m_curTalk.localIdx == 1) //右边显示
        {
            rightIcon.color = new Color(1F, 1F, 1F, 1F);

            rightNpc.gameObject.SetActive(true);
            rightName.text = cfg.name;
            if (RoleMgr.instance.Hero != null)
                rightName.text = cfg.name.Replace(HeroName, RoleMgr.instance.Hero.GetString(enProp.name));
            rightIcon.Set(cfg.bust);
            content.transform.localPosition = new Vector3(80, pos.y, pos.z);
        }
    }
    #endregion

    public bool IsEnd()
    {
        return m_isEnd;
    }
    public void UpdateTalk(StoryTalkCfg talkCfg)
    {
        if (StoryMgr.instance.CanSpeed)
            btnSkip.gameObject.SetActive(true);
        else
            btnSkip.gameObject.SetActive(false);
        m_curTalk = talkCfg;
        if (m_curTalk == null)
        {
            m_isEnd = true;
            this.Close();
            return;
        }

        m_startTime = TimeMgr.instance.realTime;

        OnShow();
    }

}
