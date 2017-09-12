using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(UIPanelBase))]
public class UIPanel : MonoBehaviour
{
     UIPanelBase m_base;

    public UIPanelBase PanelBase { get { return m_base; } }
    public bool IsOpen { get { return m_base.IsOpen; } }
    public bool IsOpenEx { get { return m_base.IsOpenEx; } }
    public bool IsTop { get { return m_base.IsTop; } }
    //开启关闭效果播放中
    public bool IsAniPlaying { get { return m_base.IsAniPlaying; } }

    public float BeginOpenTime { get { return m_base.BeginOpenTime; } }
    public float BeginCloseTime { get { return m_base.BeginCloseTime; } }

    public float BeginOpenDuration { get { return Time.time - m_base.BeginOpenTime; } }
    public float BeginCloseDuration { get { return Time.time - m_base.BeginCloseTime; } }


    public void Init()  {
        m_base = this.GetComponent<UIPanelBase>();
        m_base.Init();
    }

    public void Open(object param, bool immediate = false)   {m_base.Open(param, immediate); }

    public void Fresh(){m_base.Fresh();}

    public void Close(bool immediate) {m_base.Close(immediate);}

    public void Close(){m_base.Close(false);}

    //初始化时调用
    public virtual void OnInitPanel() { }

    //显示,保证在初始化之后
    public virtual void OnOpenPanel(object param) { }

    //关闭，保证在初始化之后
    public virtual void OnClosePanel() { }

    //显示动画播放完,保证在初始化之后
    public virtual void OnOpenPanelEnd() { }

    //关闭动画播放完，保证在初始化之后
    public virtual void OnClosePanelEnd() { }

    //更新，保证在初始化之后
    public virtual void OnUpdatePanel() { }

}
