using UnityEngine;
using System.Collections;
using System;

public class UIPetBattleIcon2 : MonoBehaviour
{
    public ImageEx m_icon;
    public StateGroup m_stars;
    public TextEx m_limitText;
    public StateHandle m_button;

    private string m_guid;
    private Action<string> m_viewPetFunc;

    public void Init(string roleId, int star, enPetPos pos, int heroLv, Action<string> viewFunc = null, string guid = null)
    {
        m_icon.Set(string.IsNullOrEmpty(roleId) ? null : RoleCfg.GetHeadIcon(roleId));
        m_viewPetFunc = viewFunc;
        m_guid = guid;

        if (m_stars != null)
            m_stars.SetCount(string.IsNullOrEmpty(roleId) ? 0 : star);

        if (m_limitText != null)
        {
            PetPosCfg cfg = PetPosCfg.m_cfgs[(int)pos];
            if (heroLv >= cfg.level)
            {
                m_limitText.gameObject.SetActive(false);
            }
            else
            {
                m_limitText.gameObject.SetActive(true);
                m_limitText.text = "Lv." + cfg.level + "解锁";
            }
        }

        if (m_button != null)
            m_button.AddClick(OnButton);
    }

    public void OnButton()
    {
        if (m_viewPetFunc != null && !string.IsNullOrEmpty(m_guid))
            m_viewPetFunc(m_guid);
    }
}
