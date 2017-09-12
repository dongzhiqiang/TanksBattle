using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIPetExpItem : MonoBehaviour
{
    public ImageEx m_icon;
    public Text m_num;
    public ImageEx m_background;
    public RepeatPressListener m_button;
    public Text m_exp;
    public GameObject m_qualityObj;
    public Text m_quality;
    private int m_itemId;
    private bool m_eventAdded = false;
    private bool m_pressLock = false;
    private bool m_repeatLock = false;
    private UIPetPageUpgrade m_parent;
    private int m_useNum=0;

    public void Init(int itemId, UIPetPageUpgrade parent, bool fleshAll)
    {
        
        m_repeatLock = false;
        m_parent = parent;
        if (fleshAll) { m_useNum = 0; m_pressLock = false; }
        int itemNum = RoleMgr.instance.Hero.ItemsPart.GetItemNum(itemId)-m_useNum;
        m_num.text = "" + itemNum;
        ItemCfg itemCfg = ItemCfg.m_cfgs[itemId];
        m_icon.Set(itemCfg.icon);
        QualityCfg qualityCfg = QualityCfg.m_cfgs[itemCfg.quality];
        m_background.Set(qualityCfg.backgroundSquare);
        if (itemNum == 0)
        {
            m_icon.SetGrey(true);
            m_background.SetGrey(true);
        }
        else
        {
            m_icon.SetGrey(false);
            m_background.SetGrey(false);
        }
        m_itemId = itemId;
        m_exp.text = "EXP+" + itemCfg.useValue1;
        if(!m_eventAdded)
        {
            m_button.m_onRepeatPress = OnPress;
            m_button.m_onPressEnd = OnPressEnd;

            m_eventAdded = true;
        }

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

    /*
    public int GetVirtualExp()
    {
        ItemCfg itemCfg = ItemCfg.m_cfgs[m_itemId];
        return int.Parse(itemCfg.useValue1) * m_useNum;
    }
     */

    void OnPress(bool firstInvoke)
    {
        if (m_pressLock)
        {
            return;
        }
        if( m_parent == null)
        {
            return;
        }
        int itemNum = RoleMgr.instance.Hero.ItemsPart.GetItemNum(m_itemId) - m_useNum;
        if (firstInvoke)
        {
            if (!m_parent.CanVirtualAddExp() )
            {
                //SubmitUse();
                m_repeatLock = true;
                return;
            }
            if (itemNum ==0)
            {
                m_repeatLock = true;
                return;
            }
            m_repeatLock = false;
            m_useNum = 1;
            m_button.m_interval = 0.2f;// / m_useNumMax;
        }
        else
        {

            if(m_repeatLock)
            {
                return;
            }
            if (!m_parent.CanVirtualAddExp())
            {
                //SubmitUse();
                m_repeatLock = true;
                return;
            }
            if (itemNum==0)
            {
                //SubmitUse();
                m_repeatLock = true;
                return;
            }
            m_useNum++;
            //m_button.m_interval = 0.5f / m_useNumMax;
            if (m_button.m_interval >= 0.04) m_button.m_interval -= 0.02f;

       

        
        }


        m_num.text = "" + itemNum;



        ItemCfg itemCfg = ItemCfg.m_cfgs[m_itemId];
        m_parent.VirtualAddExp(int.Parse(itemCfg.useValue1));




    }

    void SubmitUse()
    {
        if (m_useNum==0)
        {
            return;
        }
        m_parent.UpgradePet(m_itemId, m_useNum);
        m_repeatLock = true;
        m_pressLock = true;
        m_useNum = 0;
    }

    void OnPressEnd()
    {
        SubmitUse();
    }
}
