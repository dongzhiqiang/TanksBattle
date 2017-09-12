#region Header
/**
 * 名称：UILevelAreaSetting
 
 * 日期：2016.1.13
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



public class UILevelAreaSetting : UILevelArea
{
    
    #region Fields
    public StateHandle m_btnBack;
    //public StateHandle m_toggleFov;
    public StateHandle m_btnGuaji;
    public ImageEx m_guaji;
    public ImageEx m_pause;
    public TextEx m_guajiText;
    public ImageEx m_lock;
    int m_observer4;
    
    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.setting; } }
    public override bool IsOpenOnStart { get{return true;} }
    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {
        //m_toggleFov.AddClick(OnToggleFov);
        m_btnBack.AddClick(OnClickPause);
        m_btnGuaji.AddClick(OnClickGuaji);
        m_pause.Set("ui_guanqia_anniu_00");
    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {
        OnRoleBorn();//关掉再打开的时候要重新初始化下

        if (!reopen)
        {
            //挂机按钮有可能被关闭，重新打开一个
            SetGuaJiButtonVisible(true);
        }        
    }

    protected override void OnUpdateArea()
    {
        //摇杆控制自动战斗
        if (Input.GetKeyDown(KeyCode.Joystick1Button10))
        {
            OnClickGuaji();
        }
    }
   
    //关闭
    protected override void OnCloseArea()
    {
        if (m_observer4 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer4); m_observer4 = EventMgr.Invalid_Id; }
    }

    protected override void OnRoleBorn()
    {
        if (Role == null|| Role.AIPart==null)
            return;

        m_guajiText.text = "自动战斗";
        m_guaji.Set(CanGuaji() ? "ui_guanqia_anniu_03" : "ui_guanqia_anniu_04");

        if (m_observer4 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer4); m_observer4 = EventMgr.Invalid_Id; }
        m_observer4 = Role.Add(MSG_ROLE.AI_CHANGE, OnAIChange);
        OnAIChange();

    }

    #endregion

    #region Private Methods
    void OnAIChange()
    {
        string text = Role.AIPart.IsPlaying ? "取消自动" : "自动战斗";
        if (CanGuaji())
        {
            m_guaji.Set(Role.AIPart.IsPlaying ? "ui_guanqia_anniu_03" : "ui_guanqia_anniu_02");
        }
        else
        {
            text = "自动战斗";
        }

        m_guajiText.text = text;
        UpdateGuaJiState();
    }

    

    void OnClickPause()
    {
        if (UIMgr.instance.Get<UILevelPause>().IsOpen)
            return;

        UIMgr.instance.Open<UILevelPause>();
    }

    void OnClickGuaji()
    {
        if (!CanGuaji())
        {
            int openLevel = ConfigValue.GetInt("openGuajiLevelId");
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_LEVEL, RESULT_CODE_LEVEL.LEVEL_CANNOT_USE_AI));
            return;
        }

        LevelBattleType battleType = LevelMgr.instance.levelBattleType;
        if (battleType == LevelBattleType.Cant_AutoBattle)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_LEVEL, RESULT_CODE_LEVEL.LEVEL_CANNT_AUTOBATTLE));
            return;
        }

        if (battleType == LevelBattleType.Must_AutoBattle)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_LEVEL, RESULT_CODE_LEVEL.LEVEL_MUST_AUTOBATTLE));
            return;
        }

        if (Role == null || Role.State != Role.enState.alive) return;
        if (!Role.AIPart.IsPlaying)
        {
            Role.AIPart.Play(AIPart.HeroAI);
            PlayerPrefs.SetInt(LevelMgr.AutoBattleFlag, 1);
        }
        else
        {
            Role.AIPart.Stop();
            PlayerPrefs.SetInt(LevelMgr.AutoBattleFlag, 0);
        }
    }

    bool CanGuaji()
    {
        int openLevel = ConfigValue.GetInt("openGuajiLevelId");
        int curLevelId = int.Parse(Role.LevelsPart.CurLevelId);
        if (curLevelId > openLevel)
        {
            return true;
        }
        return false;
    }

    //void OnToggleFov()
    //{
    //    CameraMgr.instance.DisScale = m_toggleFov.CurStateIdx == 0?1:m_disScale;
    //    CameraMgr.instance.VScale = m_toggleFov.CurStateIdx == 0 ? 1 :m_verticalAngleScale;
    //    CameraHandle handle = CameraMgr.instance.Add(CameraMgr.instance.CurHandle.m_info, false);//优先级要提高点
    //    handle.CalcLookPos();//看的点的类型可能会改变，这个时候要重新计算下
    //    handle.Reset();
    //}
    #endregion

    public void SetGuaJiButtonVisible(bool visible)
    {
        m_btnGuaji.gameObject.SetActive(visible);
    }

    public void UpdateGuaJiState()
    {
        if (!m_btnGuaji.gameObject.activeSelf)
            return;

        LevelBattleType battleType = LevelMgr.instance.levelBattleType;
        LevelAutoBattleState battleState = LevelMgr.instance.levelBattleState;
        m_lock.gameObject.SetActive(false);
        if (battleType == LevelBattleType.Cant_AutoBattle || battleType == LevelBattleType.Must_AutoBattle)
        {
            m_lock.gameObject.SetActive(true);
        }
    }
}
