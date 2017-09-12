using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIFlameIcon : MonoBehaviour
{
    public ImageEx m_icon;
    public Text m_level;
    public StateHandle m_button;
    public GameObject m_lock;
    public GameObject m_select;
    private UIFlame m_parent;
    private int m_flameId;
    private bool m_canSelect = false;
    private bool m_eventAdded = false;

    public void UpdateSelect(int selectFrameId)
    {
        if(m_flameId==selectFrameId)
        {
            m_select.SetActive(true);
        }
        else
        {
            m_select.SetActive(false);
        }
    }

    public void Init(UIFlame parent, int flameId)
    {
        m_parent = parent;
        m_flameId = flameId;
        Flame flame = RoleMgr.instance.Hero.FlamesPart.GetFlame(flameId);
        FlameCfg flameCfg = FlameCfg.m_cfgs[flameId];
        if(flame == null)
        {
            m_icon.SetGrey(true);
            if(flameCfg.needLevel<=RoleMgr.instance.Hero.GetInt(enProp.level))
            {
                m_lock.SetActive(false);
                m_level.gameObject.SetActive(true);
                m_level.text = "Lv." + 0;
                m_canSelect = true;
            }
            else
            {
                m_lock.SetActive(true);
                m_level.gameObject.SetActive(false);
                m_canSelect = false;
            }
        }
        else
        {
            m_icon.SetGrey(false);
            m_lock.SetActive(false);
            m_level.gameObject.SetActive(true);
            m_level.text = "Lv." + flame.Level;
            m_canSelect = true;
        }

        if(!m_eventAdded)
        {
            m_button.AddClick(OnClick);
            m_eventAdded = true;
        }
    }

    void OnClick()
    {
        if(!m_canSelect)
        {
            FlameCfg flameCfg = FlameCfg.m_cfgs[m_flameId];
            UIMessage.Show("角色等级达到" + flameCfg.needLevel + "解锁");
            return;
        }
        m_parent.SelectFlame(m_flameId);
    }

}
