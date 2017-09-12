using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class BowListItem : MonoBehaviour
{
    public ImageEx m_mask;
    private Action<object> m_setDataAction;
    private Action<bool> m_setSelectedAction;
    private object m_data;

    public void SetSetDataAction(Action<object> setDataAction)
    {
        m_setDataAction = setDataAction;
    }

    public void SetSetSelectedAction(Action<bool> setSelectedAction)
    {
        m_setSelectedAction = setSelectedAction;
    }

    public void SetData(object data)
    {
        m_data = data;
        if(m_setDataAction!=null)
        {
            m_setDataAction.Invoke(data);
        }
    }

    public void Reflesh()
    {
        if (m_setDataAction != null)
        {
            m_setDataAction.Invoke(m_data);
        }
    }

    public void SetSelected(bool isSel)
    {
        if(m_setSelectedAction!=null)
        {
            m_setSelectedAction.Invoke(isSel);
        }
    }
}