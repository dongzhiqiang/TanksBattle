using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIEliteLevelItem : MonoBehaviour
{

    //public ImageEx m_selImage;
    public ImageEx m_image;
    public Text m_name;
    public Text m_subname;
    public Text m_needLevel;
    public GameObject m_mask;
    public GameObject m_selection;
    public StateHandle m_button;

    int m_levelId;

    bool m_cache = false;
    bool m_isFirstSetData = true;
    void Awake()
    {
        Cache();
    }

    public void Cache()
    {
        if (m_cache) return;
        m_cache = true;

        BowListItem listItem = GetComponent<BowListItem>();
        listItem.SetSetDataAction(SetData);
        listItem.SetSetSelectedAction(SetSelected);
        m_button.AddClick(OnClick);
    }



    public void SetData(object data)
    {
        if(!(data is int))
        {
            return;
        }
        m_levelId = (int)data;
        if (m_levelId == 0) return;
        if(m_levelId == UIMgr.instance.Get<UIEliteLevel>().GetLevelId())
        {
            m_selection.SetActive(true);
            gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }
        else
        {
            m_selection.SetActive(false);
            gameObject.transform.localScale = Vector3.one;
        }
        EliteLevelCfg eliteLevelCfg = EliteLevelCfg.m_cfgs[m_levelId];
        m_image.Set(eliteLevelCfg.titleImage);
        m_name.text = eliteLevelCfg.name;
        m_subname.text = eliteLevelCfg.subname;
        if (!EliteLevel.CanOpen(eliteLevelCfg))
        {
            m_needLevel.gameObject.SetActive(false);
            //m_needLevel.text = eliteLevelCfg.messageNotOpen;
            m_mask.SetActive(true);
        }
        else
        {
            m_needLevel.gameObject.SetActive(false);
            m_mask.SetActive(false);
        }
       

        m_isFirstSetData = false;
    }

    public void SetSelected(bool selected)
    {
        //m_selImage.gameObject.SetActive(selected);
    }

    void OnClick()
    {
        UIMgr.instance.Get<UIEliteLevel>().SelectEliteLevel(m_levelId);
    }
}