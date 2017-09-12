using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIEquipCostItem : MonoBehaviour
{
    public ImageEx m_icon;
    public Text m_num;
    public StateHandle m_button;
    public ImageEx m_background;
    public GameObject m_qualityObj;
    public Text m_quality;
    private int m_itemId;
    private Color m_normal = QualityCfg.ToColor("C4B8AA");
    private Color m_red = QualityCfg.ToColor("F72D2D");//QualityCfg.ToColor("CE3535");

    public void Init(CostItem costItem)
    {
        int itemNum = RoleMgr.instance.Hero.ItemsPart.GetItemNum(costItem.itemId);
        
        if(itemNum >= costItem.num)
        {
            //m_num.color = m_normal;
            m_num.text = itemNum + "/" + costItem.num;
        }
        else
        {
            //m_num.color = m_red;
            m_num.text = "<color=#F72D2D>" + itemNum + "</color>/" + costItem.num;
        }
        ItemCfg itemCfg = ItemCfg.m_cfgs[costItem.itemId];
        m_icon.Set(itemCfg.icon);
        QualityCfg qualityCfg = QualityCfg.m_cfgs[itemCfg.quality];
        m_background.Set(qualityCfg.backgroundSquare);
        if (itemNum==0)
        {
            m_icon.SetGrey(true);
            m_background.SetGrey(true);
        }
        else
        {
            m_icon.SetGrey(false);
            m_background.SetGrey(false);
        }
        m_itemId = costItem.itemId;
        m_button.AddClick(OnClick);
       
        if(m_qualityObj!=null)
        {
            if(itemCfg.qualityLevel==0)
            {
                m_qualityObj.SetActive(false);
            }
            else
            {
                m_qualityObj.SetActive(true);
                m_quality.text = "" + itemCfg.qualityLevel;
            }
        }
    }

    void OnClick()
    {
        UIMgr.instance.Open<UIItemInfo>(m_itemId);
    }

}
