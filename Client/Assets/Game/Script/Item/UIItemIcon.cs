using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIItemIcon : MonoBehaviour
{
    public ImageEx m_icon;
    public TextEx m_name;
    public TextEx m_num;
    public StateHandle m_button;
    public ImageEx m_background;
    public GameObject m_qualityObj;
    public Text m_quality;
    public bool isSimpleTip = false;
    public bool isSmallIcon = false;

    private int m_itemId;


    //showSelf，表明显示身上有的
    public void Init(int itemId, int itemNum,bool showSelf=false)
    {
        m_itemId = itemId;
        ItemCfg itemCfg = ItemCfg.m_cfgs[itemId];
        QualityCfg qualityCfg = QualityCfg.m_cfgs[itemCfg.quality];

        if (m_num)
        {
            if(!showSelf)
                m_num.text = itemNum > 1 ? StringUtil.LimitNumLength(itemNum) : "";
            else
            {
                int selfNum = RoleMgr.instance.Hero.ItemsPart.GetItemNum(itemId);
                if(selfNum>= itemNum)
                    m_num.text = string.Format("{0}/{1}", StringUtil.LimitNumLength(selfNum), StringUtil.LimitNumLength(itemNum));
                else
                    m_num.text = string.Format("<color=red>{0}</color>/{1}", StringUtil.LimitNumLength(selfNum), StringUtil.LimitNumLength(itemNum));
                    
            }
        }

        if(m_icon)
        {
            if(isSmallIcon)
            {
                m_icon.Set(itemCfg.iconSmall);
            }
            else
            {
                m_icon.Set(itemCfg.icon);
            }
            
        }
            

        if (m_name)
            m_name.text = itemCfg.name;

        if (m_background)
            m_background.Set(qualityCfg.backgroundSquare);

        if (m_button)        
            m_button.AddClick(OnClick);
           
        if (m_qualityObj != null)
        {
            if (itemCfg.qualityLevel == 0)
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
        if (isSimpleTip)
        {
            UIMgr.instance.Open<UIItemTip>(m_itemId);
        }
        else
        {            
            UIMgr.instance.Open<UIItemInfo>(m_itemId);            
        }
    }

}
