using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ShowUpObject
{
    public GameObject m_obj;
    public float m_delay;
}

public class ShowUpController : MonoBehaviour
{
    public List<ShowUpObject> m_showObjs;
    int m_index;
    TimeMgr.Timer m_timer;

    public void Prepare()
    {
        for (int i = 0; i < m_showObjs.Count; i++)
        {
            m_showObjs[i].m_obj.SetActive(false);
        }

        if (m_timer != null)
        {
            TimeMgr.instance.RemoveTimer(m_timer);
        }
    }

    public void Start()
    {

        if(m_showObjs.Count == 0)
        {
            return;
        }

        m_index = 0;
        ShowNext();
    }

    void ShowNext()
    {
        if(m_index >=m_showObjs.Count)
        {
            m_timer = null;
            return;
        }
        if (m_showObjs[m_index].m_obj == null)
            return;

        m_showObjs[m_index].m_obj.SetActive(true);
        m_index++;
        m_timer = null;
        if(m_index < m_showObjs.Count)
        {
            m_timer = TimeMgr.instance.AddTimer(m_showObjs[m_index - 1].m_delay, ShowNext);
        }
    }
}