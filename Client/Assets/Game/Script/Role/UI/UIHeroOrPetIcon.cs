using UnityEngine;
using System.Collections;

public class UIHeroOrPetIcon : MonoBehaviour
{
    public ImageEx m_heroBg;
    public ImageEx m_petBg;
    public ImageEx m_icon;
    public StateGroup m_petStars;

    public void Init(Role role)
    {
        string roleId = "";
        bool isPet = true;
        int star = 0;

        if (role != null)
        {
            if (role.GetInt(enProp.heroId) != 0)
            {
                roleId = role.GetString(enProp.roleId);
                isPet = false;
                star = 0;
            }
            else
            {
                roleId = role.GetString(enProp.roleId);
                isPet = true;
                star = role.GetInt(enProp.star);
            }
        }

        Init(roleId, isPet, star);
    }

    public void Init(string roleId, bool isPet, int star = 0)
    {
        var cfg = string.IsNullOrEmpty(roleId) ? null : RoleCfg.Get(roleId);
        m_icon.Set(cfg == null ? null : cfg.icon);

        if (isPet)
        {
            m_heroBg.gameObject.SetActive(false);
            m_petBg.gameObject.SetActive(true);
            if (m_petStars != null)
            {
                m_petStars.gameObject.SetActive(true);
                m_petStars.SetCount(star);
            }
        }
        else
        {
            m_heroBg.gameObject.SetActive(true);
            m_petBg.gameObject.SetActive(false);            
            if (m_petStars != null)
            {
                m_petStars.gameObject.SetActive(false);
                m_petStars.SetCount(0);
            }
        }
    }
}
