using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;



public class FloatText : MonoBehaviour
{
    public UITxtFloatPanel m_parent;
    public int m_index;
    
    public void OnEnd()
    {
        m_parent.OnFloatTextEnd(this);
    }
}

