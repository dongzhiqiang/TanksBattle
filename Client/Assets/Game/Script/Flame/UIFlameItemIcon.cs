using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIFlameItemIcon : MonoBehaviour
{
    public ImageEx m_icon;
    public TextEx m_num;
    public RepeatPressListener m_button;
    public RepeatPressListener m_minus;
    public ImageEx m_background;
    public GameObject m_qualityObj;
    public Text m_quality;
    public GameObject m_fx;
    private int m_itemId;
    private int m_itemNum;
    private bool m_eventAdded = false;
    private UIFlameMaterial m_parent;
    private TimeMgr.Timer m_fxTimer;
    private bool m_pressLock = false;

    public void Init(UIFlameMaterial parent, int itemId, int itemNum)
    {
        m_parent = parent;
        m_itemId = itemId;
        m_itemNum = itemNum;
        ItemCfg itemCfg = ItemCfg.m_cfgs[itemId];
        QualityCfg qualityCfg = QualityCfg.m_cfgs[itemCfg.quality];

        if (itemNum > 0)
        {
            m_num.text = string.Format("{0}/{1}", itemNum, RoleMgr.instance.Hero.ItemsPart.GetItemNum(itemId));
        }
        else
        {
            m_num.text = RoleMgr.instance.Hero.ItemsPart.GetItemNum(itemId).ToString();
        }
            
        
        m_icon.Set(itemCfg.icon);
        
        m_background.Set(qualityCfg.backgroundSquare);

        if (!m_eventAdded)
        {
            m_button.m_onRepeatPress = OnPress;
            m_button.m_onPressEnd = OnPressEnd;
            m_minus.m_onRepeatPress = OnPressMinus;

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

        if(m_itemNum>0)
        {
            m_minus.gameObject.SetActive(true);
            m_minus.GetComponent<StateHandle>().SetState(0);
        }
        else
        {
            m_minus.gameObject.SetActive(false);
        }
        if(m_itemNum>=RoleMgr.instance.Hero.ItemsPart.GetItemNum(itemId))
        {
            m_icon.SetGrey(true);
            m_background.SetGrey(true);
        }
        else
        {
            m_icon.SetGrey(false);
            m_background.SetGrey(false);
        }
    }

    void OnPress(bool firstInvoke)
    {
        if(firstInvoke)
        {
            m_pressLock = false;
            m_button.m_interval = 0.2f;
        }
        else
        {
            if(m_button.m_interval>=0.04)m_button.m_interval -= 0.02f;
        }
        if (m_pressLock) return;
        int itemNum = RoleMgr.instance.Hero.ItemsPart.GetItemNum(m_itemId);
        if(m_itemNum>=itemNum)
        {
            return;
        }
        if(!m_parent.CanIncreaseItem())
        {
            UIMessage.Show("已达到满级，无法继续添加材料");
            m_pressLock = true;
            if(!firstInvoke)
            {
                StopIncrease();
            }
            return;
        }
        if (firstInvoke)
        {
            m_parent.StartIncreaseItem();
            m_parent.FlyIcon(m_background, m_icon);

            ShowFx();
            GetComponentInParent<UIParticleMask>().UpdateLimitRect();
        }
        m_parent.IncreaseItem(m_itemId);

    }

    void ShowFx()
    {
        m_fx.SetActive(false);
        m_fx.SetActive(true);
        m_fxTimer = TimeMgr.instance.AddTimer(0.8f, ShowFx);
    }

    void StopIncrease()
    {
        m_parent.EndIncreaseItem();
        if (m_fxTimer != null)
        {
            TimeMgr.instance.RemoveTimer(m_fxTimer);
            m_fxTimer = null;
        }
    }

    void OnPressEnd()
    {
        if (m_pressLock)
        {
            return;
        }
        StopIncrease();
    }

    void OnPressMinus(bool firstInvoke)
    {
        if (firstInvoke)
        {
            m_minus.m_interval = 0.2f;
        }
        else
        {
            if (m_minus.m_interval >= 0.04) m_minus.m_interval -= 0.02f;
        }
        if (m_itemNum <= 0)
        {
            return;
        }
        m_parent.DecreaseItem(m_itemId);
    }

}
