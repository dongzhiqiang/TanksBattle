using UnityEngine;
using System.Collections;
using System;

public class UIPetIcon2 : MonoBehaviour
{
    public ImageEx m_icon;
    public TextEx m_level;
    public StateGroup m_stars;
    public StateHandle m_battleState;
    public TextEx m_isOut;
    public TextEx m_name;
    public TextEx m_type;
    public StateHandle m_button;

    private string m_guid;
    private Action<string> m_viewPetFunc;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="level"></param>
    /// <param name="stars"></param>
    /// <param name="battleState">0 不出战，1 主战，2 助战</param>
    /// <param name="name"></param>
    public void Init(string roleId, int level, int stars, int battleState, string name = null, bool have = true, Action<string> viewFunc = null, string guid = null)
    {
        var cfg = string.IsNullOrEmpty(roleId) ? null : RoleCfg.Get(roleId);

        m_icon.Set(cfg == null ? null : cfg.icon);
        m_icon.SetGrey(!have);

        m_viewPetFunc = viewFunc;
        m_guid = guid;

        if (m_button != null)
            m_button.AddClick(OnButton);

        if (cfg == null)
        {
            if (m_level != null)
                m_level.text = "";
            if (m_stars != null)
                m_stars.SetCount(0);
            if (m_battleState != null)
                m_battleState.SetState(0);
            if (m_isOut != null)
                m_isOut.gameObject.SetActive(false);
            if (m_name != null)
                m_name.text = "";
            if (m_type != null)
                m_type.text = "";
            
        }
        else
        {
            if (m_level != null)
                m_level.text = "Lv." + level.ToString();
            if (m_stars != null)
                m_stars.SetCount(stars);
            if (m_battleState != null)
                m_battleState.SetState(battleState);
            if (m_isOut != null)
                m_isOut.gameObject.SetActive(battleState == 1);
            if (m_name != null)
                m_name.text = string.IsNullOrEmpty(name) ? cfg.name : name;
            if (m_type != null)
                m_type.text = PetTypeCfg.m_cfgs[cfg.subType].name;
        }        
    }

    public void OnButton()
    {
        if (m_viewPetFunc != null && !string.IsNullOrEmpty(m_guid))
            m_viewPetFunc(m_guid);
    }
}
