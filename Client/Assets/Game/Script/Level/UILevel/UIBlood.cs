#region Header
/**
 * 名称：进度条
 
 * 日期：2016.2.29
 * 描述：支持多条,支持渐变
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIBlood : MonoBehaviour
{
   public RectTransform m_rt;
   public UISmoothProgress m_progress;
    public GameObject m_showHitPropDef;
    public ImageEx m_hitPropDefIcon;
    public TextEx m_name;
    [HideInInspector]
   public float m_lastBehitTime;
   [HideInInspector]
   public int m_roleId;
   [HideInInspector]
   public Role m_role;
   
}
