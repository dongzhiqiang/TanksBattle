#region Header
/**
 * 名称：用于播放ui特效的面板
 
 * 日期：201x.x.x
 * 描述：新建继承自mono的类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


public class UITxtFloatPanel : UIPanel
{
    
    //播放ui特效
    public static void ShowFloatTexts(List<string> texts)
    {
        ShowFloatTexts(texts, Vector3.zero);
    }

    public static void ShowFloatTexts(List<string> texts, Vector3 pos) 
    {
        UIMgr.instance.Open<UITxtFloatPanel>();
        UIMgr.instance.Get<UITxtFloatPanel>().Show(texts, pos);
    }
    #region Fields
    public GameObject m_textPos;
    public GameObject m_textTemplate;
    #endregion

    #region Properties
    private List<GameObject> m_showingText = new List<GameObject>();
    private List<GameObject> m_textPool = new List<GameObject>();
    private GameObject m_lastText;
    private List<string> m_texts;
    private int m_textIndex;
    private bool m_toClose;
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        UnityEvent ev = new UnityEvent();
        ev.AddListener(m_textTemplate.GetComponent<FloatText>().OnEnd);
        m_textTemplate.GetComponent<SimpleHandle>().m_onEnd = ev;
        AddToPool(m_textTemplate);

    }

    


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {
        
    }
    #endregion

    #region Private Methods

    class EndHandler
    {
        GameObject text;
        EndHandler(GameObject text)
        {
            this.text = text;
        }
    }
    
    void AddToPool(GameObject text,bool removeShow=false)
    {
        if(removeShow)
        {
            m_showingText.Remove(text);
        }
        text.SetActive(false);
        m_textPool.Add(text);
        

    }

    GameObject GetNewText()
    {
        GameObject result = null;
        if(m_textPool.Count>0)
        {
            result = m_textPool[m_textPool.Count - 1];
            m_textPool.RemoveAt(m_textPool.Count - 1);
        }
        else
        {
            result = GameObject.Instantiate(m_textTemplate);
            
            result.transform.parent = m_textPos.transform;
            result.transform.localScale = Vector3.one;
            result.transform.localPosition = Vector3.zero;

            UnityEvent ev = new UnityEvent();
            ev.AddListener(result.GetComponent<FloatText>().OnEnd);
            result.GetComponent<SimpleHandle>().m_onEnd = ev;
        }
        result.SetActive(true);

        m_showingText.Add(result);
        return result;
    }

    void ShowNextText()
    {
        GameObject textObj = GetNewText();
        if (m_textIndex==m_texts.Count-1)
        {
            //m_lastText = textObj;
        }
        textObj.GetComponent<TextEx>().text = m_texts[m_textIndex];
        textObj.GetComponent<FloatText>().m_index = m_textIndex;
        textObj.GetComponent<SimpleHandle>().m_handle.Start();

        m_textIndex++;
        if(m_textIndex < m_texts.Count)
        {
            TimeMgr.instance.AddTimer(0.2f, ShowNextText);
        }
    }
    #endregion
    public void Show(List<string> texts, Vector3 pos)
    {
        m_textPos.transform.localPosition = pos;
        m_toClose = true;
        m_texts = texts;
        if (m_texts.Count == 0)
        {
            Close();
            return;
        }
        //m_lastText = null;
        if(m_showingText.Count >= 0 )
        {
            foreach(GameObject textObj in m_showingText)
            {
                textObj.GetComponent<SimpleHandle>().m_handle.Clear();
                AddToPool(textObj);
            }
            m_showingText.Clear();
        }
        m_textIndex = 0;
        ShowNextText();
    }

    public void StartShow()
    {
        m_toClose = false;
        if (m_showingText.Count >= 0)
        {
            foreach (GameObject textObj in m_showingText)
            {
                textObj.GetComponent<SimpleHandle>().m_handle.Clear();
                AddToPool(textObj);
            }
            m_showingText.Clear();
        }
        m_texts = new List<string>();

    }

    public void Show(string text, Vector3 pos)
    {
        m_textPos.transform.localPosition = pos;
        GameObject textObj = GetNewText();
        if (m_textIndex == m_texts.Count - 1)
        {
            //m_lastText = textObj;
        }
        textObj.GetComponent<TextEx>().text = text;
        textObj.GetComponent<FloatText>().m_index = m_texts.Count-1;
        textObj.GetComponent<SimpleHandle>().m_handle.Start();
    }

    public void EndShow()
    {
        m_toClose = true;
    }

    public void OnFloatTextEnd(FloatText floatText)
    {
        //Debug.Log("" + floatText.m_index);
        AddToPool(floatText.gameObject, true);
        if (!m_toClose)
        {
            return;
        }
        if (floatText.m_index == m_texts.Count-1)
        {
            Close();
        }
    }

}
