using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIPetIcon : MonoBehaviour
{
    public ImageEx m_icon;
    public StateGroup m_stars;
    public TextEx m_name;
    public StateHandle m_button;

    private string m_petId;
    private PetInfo m_petInfo;

    public void Init(string petId, bool grey, int star)
    {
        var cfg = string.IsNullOrEmpty(petId) ? null : RoleCfg.Get(petId);
        m_petId = petId;
        m_icon.Set(cfg == null ? null : cfg.icon);
        m_icon.SetGrey(grey);
        if (m_stars != null)
            m_stars.SetCount(star);
        if (m_name != null)
            m_name.text = cfg == null ? "" : cfg.name;
        if (m_button != null)
            m_button.AddClick(OnClick);
    }

    void OnClick()
    {
        Role pet = RoleMgr.instance.Hero.PetsPart.GetPetByRoleId(m_petId);
        if (pet != null)
        {
            UIMgr.instance.Open<UIPet>(pet);
            //UIMgr.instance.Open<UIPetInfo>(pet);
        }
        else
        {
            if (m_petInfo==null)
            {
                m_petInfo = new PetInfo();
            }
            m_petInfo.petId = m_petId;
            m_petInfo.star = m_petInfo.Cfg.initStar;
            UIMgr.instance.Open<UIPetInfo>(m_petInfo);
        }
    }


}
