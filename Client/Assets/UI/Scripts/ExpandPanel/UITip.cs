using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class UITip : UIPanel
{
    public enum enCloseType{
        none,
        pointerUp,
        pointerDown,
    }

    public TextEx m_text;
    public RectTransform m_tran;
    public string m_testText;
    public UIPanel m_testPanel;
   

    Coroutine m_curCo=null;
    enCloseType m_closeType= enCloseType.none;
    public static void Show(string msg, RectTransform t, enCloseType closeType= enCloseType.pointerDown)
    {
        UIMgr.instance.Get<UITip>().ShowContent(msg, t, closeType);
    }

    void Awake()
    {
        if (m_testPanel != null)
        {
            StateHandle[] btns = m_testPanel.GetComponentsInChildren<StateHandle>(true);
            foreach (var b in btns)
            {
                var b2 = b;
                b2.AddPressHold((StateHandle) =>
                {
                    RectTransform rt = b2.GetComponent<RectTransform>();
                   
                    ShowContent(b2.name + m_testText, rt, enCloseType.pointerUp);
                });
            }
        }
    }
    
    //初始化时调用
    public override void OnInitPanel()
    {
        
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        CloseContent();
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }

    public void ShowContent(string msg, RectTransform t, enCloseType closeType)
    {
        CloseContent();
        m_closeType = closeType;
        if (m_closeType == enCloseType.pointerDown)
            InputMgr.instance.PointerDownEvent += CloseContent;
        else if (m_closeType == enCloseType.pointerUp)
            InputMgr.instance.PointerUpEvent += CloseContent;
        m_curCo =StartCoroutine(CoShowContent(msg,t));
    }

    IEnumerator CoShowContent(string msg, RectTransform t)
    {
        m_text.text = msg;

        m_tran.localScale = Vector3.one;
        yield return new WaitForEndOfFrame();
        m_tran.gameObject.SetActive(true);
        MathUtil.CalcAlignPos(this.GetComponent<RectTransform>(), t, m_tran,Vector2.zero);
        m_curCo = null;
    }

    public void CloseContent()
    {
        m_tran.gameObject.SetActive(false);
        if (m_closeType == enCloseType.pointerDown)
            InputMgr.instance.PointerDownEvent -= CloseContent;
        else if (m_closeType == enCloseType.pointerUp)
            InputMgr.instance.PointerUpEvent -= CloseContent;
        m_closeType = enCloseType.none;

        if (m_curCo != null)
        {
            StopCoroutine(m_curCo);
            m_curCo = null;
        }
    }
   



}
