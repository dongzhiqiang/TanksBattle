using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIActivity : UIPanel
{
    #region SerializeFields
    public UIActivityItem[] m_items;
    public GameObject m_group;
    #endregion

    #region Fields
    static Dictionary<enSystem, Action> s_click = new Dictionary<enSystem, Action>();
    Dictionary<enSystem, UIActivityItem> m_itemsBySys = new Dictionary<enSystem, UIActivityItem>();
    bool m_reordered = false;
    int m_ob;
    #endregion

    #region Static Methods
    public static void AddClick(enSystem sys, Action a)
    {
        if (s_click.ContainsKey(sys))
        {
            Debuger.LogError("活动界面{0}活动按钮被重复监听，只能有一个监听者", sys);
            return;
        }
        s_click[sys] = a;
    }
    #endregion

    public override void OnInitPanel()
    {
        foreach (UIActivityItem item in m_items)
        {
            if (m_itemsBySys.ContainsKey(item.sys))
            {
                Debuger.LogError("活动界面有重复的活动图标，是不是复制黏贴新图标后没有修改系统枚举？{0}", item.sys);
                continue;
            }

            m_itemsBySys[item.sys] = item;

            item.btn.AddClickEx(OnClickItem);
            if (item.tip)
            {
                item.tip.SetActive(false);
            }
        }

        AddClick(enSystem.goldLevel, () => {
            UIMgr.instance.Open<UIGoldLevel>();
        });
        AddClick(enSystem.hadesLevel, () =>
        {
            UIMgr.instance.Open<UIHadesLevel>();
        });
        AddClick(enSystem.venusLevel, () =>
        {
            UIMgr.instance.Open<UIVenusLevel>();
        });
        AddClick(enSystem.guardLevel, () =>
        {
            UIMgr.instance.Open<UIGuardLevel>();
        });
        AddClick(enSystem.warriorTried, () =>
        {
            UIMgr.instance.Open<UIWarriorsTried>();
        });
        AddClick(enSystem.prophetTower, () =>
        {
            UIMgr.instance.Open<UIProphetTowerLevel>();
        });
        AddClick(enSystem.treasureRob, () =>
        {
            UIMgr.instance.Open<UITreasureRob>();
        });

        //系统激活监听
        SystemMgr.instance.AddActiveListener(OnSystemActive);
        //系统红点监听
        SystemMgr.instance.AddTipListener(OnSystemTip);


    }



    public override void OnOpenPanel(object param)
    {
        m_ob = RoleMgr.instance.Hero.Add(MSG_ROLE.NET_ACT_PROP_SYNC, FreshComplete);

        if(!m_reordered)
        {
            m_reordered = true;
            List<UIActivityItem> items = new List<UIActivityItem>();
            foreach (UIActivityItem item in m_items)
            {
                items.Add(item);
                item.gameObject.transform.SetParent(null);
            }
            items.Sort((UIActivityItem item1, UIActivityItem item2) =>
            {
                ActivityCfg cfg1 = ActivityCfg.m_cfgs[(int)item1.sys];
                ActivityCfg cfg2 = ActivityCfg.m_cfgs[(int)item2.sys];
                return cfg1.order - cfg2.order;
            });
            foreach (UIActivityItem item in items)
            {
                item.gameObject.transform.SetParent(m_group.transform);
            }
        }


        FreshSystemState();
        FreshTip();
        RoleMgr.instance.Hero.ActivityPart.CheckActivityTip();
        FreshComplete();
    }

    //刷新系统状态
    void FreshSystemStateOne(enSystem systemId)
    {
        UIActivityItem item;
        if (!m_itemsBySys.TryGetValue(systemId, out item))
        {
            return; //不在活动界面，无视
        }
        string errMsg = "";
        if (SystemMgr.instance.IsVisible(systemId, out errMsg) )
        {
            item.gameObject.SetActive(true);
        }
        else
        {
            item.gameObject.SetActive(false);
        }

        if (!SystemMgr.instance.IsActive(systemId, out errMsg))
        {
            foreach (ImageEx image in item.btn.GetComponentsInChildren<ImageEx>())
            {
                image.SetGrey(true);
            }
        }
        else
        {
            foreach (ImageEx image in item.btn.GetComponentsInChildren<ImageEx>())
            {
                image.SetGrey(false);
            }
        }
       
    }
    void FreshSystemState()
    {
        foreach (enSystem systemId in m_itemsBySys.Keys)
        {
            FreshSystemStateOne(systemId);
        }
    }

    void FreshComplete()
    {
        ActivityPart activityPart = RoleMgr.instance.Hero.ActivityPart;
        foreach (enSystem systemId in m_itemsBySys.Keys)
        {
            UIActivityItem item;
            if (!m_itemsBySys.TryGetValue(systemId, out item))
            {
                continue; //不在主界面，无视
            }
            item.complete.SetActive(activityPart.IsComplete(systemId));
        }
    }


    void FreshTipOne(enSystem systemId)
    {
        UIActivityItem item;
        if (!m_itemsBySys.TryGetValue(systemId, out item))
        {
            return; //不在主界面，无视
        }
        if (!item.tip)
        {
            return;
        }

        if (SystemMgr.instance.IsTip(systemId))
        {
            item.tip.SetActive(true);
        }
        else
        {
            item.tip.SetActive(false);
        }
    }
    void FreshTip()
    {
        foreach (enSystem systemId in m_itemsBySys.Keys)
        {
            FreshTipOne(systemId);
        }
    }

    void OnSystemActive(object systemId)
    {
        FreshSystemStateOne((enSystem)systemId);
    }

    void OnSystemTip(object systemId)
    {
        FreshTipOne((enSystem)systemId);
    }

    public override void OnClosePanel()
    {
        if (m_ob != EventMgr.Invalid_Id) { EventMgr.Remove(m_ob); m_ob = EventMgr.Invalid_Id; }
    }

    public override void OnUpdatePanel()
    {
    }


    #region Private Methods
    void OnClickItem(StateHandle s)
    {
        UIActivityItem item = s.GetComponentInParent<UIActivityItem>();
        if (item == null)
        {
            Debuger.LogError("找不到UIActivityItem");
            return;
        }

        Action a = s_click.Get(item.sys);
        if (a == null)
        {
            UIMessage.Show("该活动未实现，敬请期待!");
            return;
        }

        string errMsg = "";
        if (SystemMgr.instance.IsGrey(item.sys, out errMsg))
        {
            UIMessage.Show(errMsg);
            return;
        }

        a();
    }

    #endregion

    public UIActivityItem GetItem(enSystem sys)
    {
        return m_itemsBySys.Get(sys);
    }
}