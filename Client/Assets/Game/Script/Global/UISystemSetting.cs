#region Header
/**
 * 名称：UISystemSetting
 
 * 日期：2016.8.10
 * 描述：系统设置界面
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


public class UISystemSetting : UIPanel
{

    #region Fields
    public Slider m_bgm;
    public Slider m_sound;
    public StateGroup m_frameRate;
    public StateGroup m_quality;
    public StateGroup m_bloom;
    public StateGroup m_shadow;
    public StateGroup m_aniEffect;
    public StateHandle m_btnRestore;

    bool m_isIniting = false;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        UIMainCity.AddClick(enSystem.setting,()=>UIMgr.instance.Open<UISystemSetting>());

        //恢复默认
        m_btnRestore.AddClick(()=>
        {
            QualityMgr.instance.SetBgmVolumn(1);
            QualityMgr.instance.SetSoundVolumn(1);
            QualityMgr.instance.MatchQuality();
            FreshAll();
        });

        //音乐
        m_bgm.onValueChanged.AddListener((float v) => {
            if (this.m_isIniting) return;
            QualityMgr.instance.SetBgmVolumn(v);
        });

        //音效
        m_sound.onValueChanged.AddListener((float v) =>
        {
            if (this.m_isIniting) return;
            QualityMgr.instance.SetSoundVolumn(v);
        });

        //帧率
        m_frameRate.AddSel((handle,idx)=> {
            if (this.m_isIniting) return;
            QualityMgr.instance.SetFrameRate( (QualityMgr.enFrameRate)idx);
        });

        //品质
        m_quality.AddSel((handle, idx) =>
        {
            if (this.m_isIniting) return;
            QualityMgr.instance.SetQuality((QualityMgr.enQuality)idx);
            FreshCustom();
        });

        //泛光和雾效
        m_bloom.AddSel((handle, idx) =>
        {
            if (this.m_isIniting) return;
            QualityMgr.instance.SetBloomEffect(idx == 1);
            QualityMgr.instance.SetFogEffect(idx == 1);
        });
        //角色阴影
        m_shadow.AddSel((handle, idx) =>
        {
            if (this.m_isIniting) return;
            QualityMgr.instance.SetShadowEffect(idx == 1);
        });
        
        //动作特效、场景特效和角色高光
        m_aniEffect.AddSel((handle, idx) =>
        {
            if (this.m_isIniting) return;
            QualityMgr.instance.SetAniEffect(idx == 1);
            QualityMgr.instance.SetSceneEffect(idx == 1);
            QualityMgr.instance.SetRoleffect(idx == 1);
        });
       
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        FreshAll();
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        PlayerPrefs.Save();
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {
        
    }
    #endregion

    #region Private Methods
   

    #endregion
    void FreshAll()
    {
        m_isIniting = true;
        m_bgm.value = QualityMgr.instance.GetBgmVolumn();
        m_sound.value = QualityMgr.instance.GetSoundVolumn();
        m_frameRate.SetSel((int)QualityMgr.instance.GetFrameRate());
        m_quality.SetSel((int)QualityMgr.instance.GetQuality());
        m_isIniting = false;
        FreshCustom();
    }

    void FreshCustom()
    {
        m_isIniting = true;
        m_bloom.SetSel(QualityMgr.instance.GetBloomEffect() ? 1 : 0);
        m_shadow.SetSel(QualityMgr.instance.GetShadowEffect() ? 1 : 0);
        m_aniEffect.SetSel(QualityMgr.instance.GetAniEffect() ? 1 : 0);
        m_isIniting = false;
    }
    
}
