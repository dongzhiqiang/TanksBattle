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

public class AttrPair
{
    public string attrName;
    public string attrValue;
    public bool plus;
}

public class UIPowerUp : UIPanel
{
    public static void SaveOldProp(Role role)
    {
        PropertyTable.Copy(role.PropPart.Props, m_oldProp);
        //Debuger.Log("old hp:" + m_oldProp.GetFloat(enProp.hpMax));
        m_oldPower = RoleMgr.instance.Hero.GetInt(enProp.powerTotal);
    }

    public static void SaveNewProp(Role role)
    {
        PropertyTable.Copy(role.PropPart.Props, m_newProp);
        //Debuger.Log("new hp:" + m_newProp.GetFloat(enProp.hpMax));
        m_newPower = RoleMgr.instance.Hero.GetInt(enProp.powerTotal);
    }

    public static bool ShowPowerUp(bool showAttr)
    {
        List<AttrPair> attrs = new List<AttrPair>();
        if (showAttr)
        {
            for (enProp i = enProp.minFightProp + 1; i < enProp.maxFightProp; i++)
            {
                float addValue = m_newProp.GetFloat(i)-m_oldProp.GetFloat(i);
                if (Mathf.Abs(addValue) <= Mathf.Epsilon)
                {
                    continue;
                }
                PropTypeCfg propTypeCfg = PropTypeCfg.m_cfgs[(int)i];
                AttrPair attrItem = new AttrPair();
                attrItem.attrName = propTypeCfg.name;
                attrItem.plus = true;
                if(addValue<0)
                {
                    attrItem.plus = false;
                    addValue = -addValue;
                }
                if (propTypeCfg.format == enPropFormat.FloatRate)
                {
                    attrItem.attrValue = String.Format("{0:F}", addValue * 100) + "%";
                }
                else
                {
                    int intAdd = Mathf.RoundToInt(m_newProp.GetFloat(i)) - Mathf.RoundToInt(m_oldProp.GetFloat(i));
                    if (intAdd==0)
                    {
                        continue;
                    }
                    if (!attrItem.plus) intAdd = -intAdd;
                    attrItem.attrValue = intAdd.ToString(); ;
                }
                attrs.Add(attrItem);
            }
            
        }
        return ShowPowerUp(attrs, m_oldPower, m_newPower);
    }

    public static bool ShowPowerUp(List<AttrPair> attrs, int oldPower, int newPower) 
    {
        UIMgr.instance.Open<UIPowerUp>();
        return UIMgr.instance.Get<UIPowerUp>().Show(attrs, oldPower, newPower);
    }
    #region Fields
    public TextEx m_power;
    public GameObject m_aniPowerEnter;
    public GameObject m_aniPowerExit;
    public GameObject m_aniGo;
    public StateGroup m_attrs;
    public float m_duration = 1.5f;
    #endregion



    #region Properties
    private List<GameObject> m_showingText = new List<GameObject>();
    private List<GameObject> m_textPool = new List<GameObject>();
    private List<AttrPair> m_attrPairs;
    private int m_playIndex;
    private int m_exitIndex;
    private int m_playOldPower;
    private int m_playNewPower;
    private bool m_playing = false;
    private float m_startTime;
    private static PropertyTable m_oldProp = new PropertyTable();
    private static PropertyTable m_newProp = new PropertyTable();
    private static int m_oldPower;
    private static int m_newPower;
    private TimeMgr.Timer m_timerStart;
    private TimeMgr.Timer m_timerExit;
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_aniPowerExit.SetActive(false);
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
    void ShowNextAttr()
    {
        if (m_playIndex >= m_attrPairs.Count)
        {
            m_timerStart = null;
            return;
        }
        AttrPair attrPair = m_attrPairs[m_playIndex];
        m_playIndex++;
        m_attrs.SetCount(m_playIndex);
        m_attrs.Get<UIPowerUpAttributeItem>(m_playIndex - 1).Init(attrPair.attrName, attrPair.attrValue, attrPair.plus);

        if (m_playIndex < m_attrPairs.Count)
        {
            m_timerStart = TimeMgr.instance.AddTimer(0.2f, ShowNextAttr);
        }
        else
        {
            m_timerStart = null;
        }
    }

    void Update()
    {
        if(m_playing)
        {
            float rate = (TimeMgr.instance.time - m_startTime) / m_duration;
            if (rate > 1) rate = 1;
            int power = Mathf.FloorToInt(Mathf.Lerp(m_playOldPower, m_playNewPower, rate));
            m_power.text = power.ToString();
        }
    }

    void StartExit()
    {
        m_aniPowerEnter.SetActive(false);
        m_aniPowerExit.SetActive(true);
        m_timerExit = TimeMgr.instance.AddTimer(0.2f, ExitNextAttr);
    }

    void ExitNextAttr()
    {
        if (m_exitIndex >= m_attrPairs.Count)
        {
            m_playing = false;
            Close();
            m_timerExit = null;
            return;
        }

        if (m_attrs.Count <= m_exitIndex)
        {
            m_playing = false;
            Close();
            m_timerExit = null;
            return;
        }

        m_attrs.Get<UIPowerUpAttributeItem>(m_exitIndex).PlayExit();
        m_exitIndex++;

        m_timerExit = TimeMgr.instance.AddTimer(0.2f, ExitNextAttr);
    }
    #endregion
    void Reset()
    {
        m_aniPowerEnter.SetActive(false);
        m_aniPowerExit.SetActive(false);
        m_aniPowerEnter.SetActive(false);
        m_aniGo.SetActive(false);
        m_aniGo.SetActive(true);
        m_attrs.SetCount(0);
        if(m_timerStart != null)
        {
            TimeMgr.instance.RemoveTimer(m_timerStart);
            m_timerStart = null;
        }
        if (m_timerExit != null)
        {
            TimeMgr.instance.RemoveTimer(m_timerExit);
            m_timerExit = null;
        }
    }

    public bool Show(List<AttrPair> attrs, int oldPower, int newPower)
    {
        if(m_playing)
        {
            Debuger.Log("战斗力变化播放中时请求了播放");
            //return false;
            Reset();
        }
        m_aniPowerEnter.SetActive(false);
        m_aniPowerExit.SetActive(false);
        m_aniPowerEnter.SetActive(true);
        m_attrPairs = attrs;
        m_playNewPower = newPower;
        m_playOldPower = oldPower;
        if (m_attrPairs.Count == 0 && m_playOldPower==m_playNewPower)
        {
            Close();
            return false;
        }

        m_playing = true;

        m_power.transform.parent.gameObject.SetActive(m_playOldPower != m_playNewPower);
        m_power.text = oldPower.ToString();
        m_startTime = TimeMgr.instance.time;

        m_timerExit = TimeMgr.instance.AddTimer(m_duration+0.5f, StartExit);

        m_attrs.SetCount(0);
        if (m_attrPairs.Count>0)
        {
            m_playIndex = 0;
            m_exitIndex = 0;
            ShowNextAttr();
        }
        return true;
    }





}
