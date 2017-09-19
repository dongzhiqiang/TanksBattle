#region Header
/**
 * 名称：UIMainCity类模板
 * 日期：2015.12.3
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;



public class UIMainCity : UIPanel
{
    #region SerializeFields
    public UIMainCityItem[] m_items;
    public UIArtFont m_power;
    public Image m_exp;
    public StateHandle m_toggle;
    public GameObject m_toggleTip;
    public UIMainCityChatTip m_chatTip;
    public StateGroup roleInfoGroup;
    #endregion

    #region Fields
    static Dictionary<enSystem,Action> s_click = new Dictionary<enSystem,Action>();
    static Action s_onOpen;
    GameObject[] roleGo = new GameObject[3];  
    bool m_isTopBefore = false;
    Role m_role;
    int m_observer;
    int m_observer2;
    int m_observer3;
    int m_observer4;
    int m_observer5;
    int m_observer6;
    int m_observer7;
    int m_observer8;
    Dictionary<enSystem, UIMainCityItem> m_itemsBySys = new Dictionary<enSystem, UIMainCityItem>();
    #endregion

    #region Properties
    public UIMainCityChatTip ChatTip { get { return m_chatTip; } }
    #endregion

    #region Static Methods
    public static void AddClick(enSystem sys,Action a)
    {
        if(s_click.ContainsKey(sys))
        {
            Debuger.LogError("主城界面{0}系统按钮被重复监听，只能有一个监听者",sys);
            return;
        }
        s_click[sys]=a;
    }
    public static void AddOpen(Action a)
    {
        if(s_onOpen== null)
            s_onOpen = a;
        else
            s_onOpen+=a;
    }
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        //建立索引，并设置点击回调     
        foreach(UIMainCityItem item in m_items){
            if(m_itemsBySys.ContainsKey(item.sys))
            {
                Debuger.LogError("主城界面有重复的系统图标，是不是复制黏贴新图标后没有修改系统枚举？{0}",item.sys);
                continue;
            }

            m_itemsBySys[item.sys] = item;

            item.btn.AddClickEx(OnClickItem);
            if (item.tip)
            {
                item.tip.SetActive(false);
            }
        }

        AddClick(enSystem.hero, () =>
        {
            UIMgr.instance.Open<UIEquip>();
        });
        AddClick(enSystem.mail, () =>
        {
            UIMgr.instance.Open<UIMail>();
        });
        //好友
        AddClick(enSystem.social, () =>
        {
            UIMgr.instance.Open<UIFriend>();
        });
        AddClick(enSystem.flame, () =>
        {
            UIMgr.instance.Open<UIFlame>();
        });
        AddClick(enSystem.eliteLevel, () =>
        {
            UIMgr.instance.Open<UIEliteLevel>();
        });
        AddClick(enSystem.treasure, () =>
        {
            UIMgr.instance.Open<UITreasure>();
        });
        AddClick(enSystem.corps, () =>
        {
            //有公会打开公会界面，否则打开公会列表
            if(RoleMgr.instance.Hero.GetInt(enProp.corpsId) > 0)  
                UIMgr.instance.Open<UICorps>();
            else
                UIMgr.instance.Open<UICorpsList>();
        });
        m_toggle.AddClick(() =>
        {
            FreshToggleTip();
        });
        AddClick(enSystem.stamina, BuyStamina);

        //系统激活监听
        SystemMgr.instance.AddActiveListener(OnSystemActive);
        //系统红点监听
        SystemMgr.instance.AddTipListener(OnSystemTip);

        //聊天提示先隐藏
        m_chatTip.gameObject.SetActive(false);
        m_chatTip.Init();
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_isTopBefore = PanelBase.IsTop;

        this.GetComponent<RectTransform>().sizeDelta =Vector2.zero;

        m_role = RoleMgr.instance.Hero;
        //战斗力
        m_observer = m_role.AddPropChange(enProp.powerTotal, FreshPower);
        FreshPower();

        //经验
        m_observer2 = m_role.AddPropChange(enProp.level, FreshExp);
        m_observer3 = m_role.AddPropChange(enProp.exp, FreshExp);
        FreshExp();

        //体力
        m_observer4 = m_role.AddPropChange(enProp.stamina, FreshStamina);
        InvokeRepeating("FreshStamina", 0, 5);

        //金币
        m_observer5 = m_role.AddPropChange(enProp.gold, FreshItem);
        //砖石
        m_observer6 = m_role.AddPropChange(enProp.diamond, FreshItem);
        FreshItem();

        m_observer7 = m_role.AddPropChange(enProp.vipLv, FreshVipLv);
        FreshVipLv();




        m_role.EquipsPart.InitCheckEquipTip();
        m_role.ActivityPart.InitCheckActivityTip();
        m_role.OpActivityPart.InitCheckOpActivityTip();
        m_role.TaskPart.InitCheckTaskTip();
        m_role.WeaponPart.InitCheckWeaponTip();
        m_role.TreasurePart.InitCheckTreasureTip();

        FreshVisibily();
        FreshTip();
        FreshToggleTip();

        //角色头顶信息
        m_observer8 = m_role.AddPropChange(enProp.level, FreshRolesInfo);
        FreshRolesInfo();
        if (s_onOpen!=null)
            s_onOpen();//打开主城界面的时候广播消息给外部
    }
       

    //刷新可见性
    void FreshVisibilyOne( enSystem systemId )
    {
        UIMainCityItem item;
        if(!m_itemsBySys.TryGetValue(systemId, out item))
        {
            return; //不在主界面，无视
        }
        string errMsg;
        if(SystemMgr.instance.IsVisible(systemId, out errMsg) && SystemMgr.instance.IsActive(systemId, out errMsg))
        {
            item.gameObject.SetActive(true);
        }
        else
        {
            item.gameObject.SetActive(false);
        }
    }
    void FreshVisibily()
    {
        foreach(enSystem systemId in m_itemsBySys.Keys)
        {
            FreshVisibilyOne(systemId);
        }
    }

    void FreshTipOne(enSystem systemId)
    {
        UIMainCityItem item;
        if (!m_itemsBySys.TryGetValue(systemId, out item))
        {
            return; //不在主界面，无视
        }
        if(!item.tip)
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
        FreshVisibilyOne((enSystem)systemId);
    }

    void OnSystemTip(object systemId)
    {
        FreshTipOne((enSystem)systemId);
        FreshToggleTip();
    }

    void FreshToggleTip()
    {
        string errMsg;
        m_toggleTip.SetActive(m_toggle.m_curState == 0 && (
            (SystemMgr.instance.IsTip(enSystem.hero) && SystemMgr.instance.IsEnabled(enSystem.hero, out errMsg))
            ||
            (SystemMgr.instance.IsTip(enSystem.weapon) && SystemMgr.instance.IsEnabled(enSystem.weapon, out errMsg))
            || 
            (SystemMgr.instance.IsTip(enSystem.flame) && SystemMgr.instance.IsEnabled(enSystem.flame, out errMsg))
            || 
            (SystemMgr.instance.IsTip(enSystem.treasure) && SystemMgr.instance.IsEnabled(enSystem.treasure, out errMsg))
            ));
    }

    bool HideWhenOpen(enSystem sys)
    {
        if (sys == enSystem.hero || sys == enSystem.activity || sys == enSystem.mail ||
            sys == enSystem.social || sys == enSystem.opActivity || sys == enSystem.dailyTask || sys == enSystem.weapon || sys == enSystem.treasure || sys == enSystem.flame)
        {
            return false;
        }
        return true;
    }

    //金币钻石
    void FreshItem()
    {
        GetItem(enSystem.gold).txt.text = m_role.ItemsPart.GetGold().ToString();
        GetItem(enSystem.diamond).txt.text = m_role.GetInt(enProp.diamond).ToString();
    }
    //战斗力
    void FreshPower() { m_power.SetNum(m_role.GetInt(enProp.powerTotal).ToString()); }

    void FreshVipLv() { GetItem(enSystem.vip).txt.text = m_role.GetInt(enProp.vipLv).ToString(); }

    //经验
    void FreshExp(){m_exp.fillAmount = Mathf.Clamp01(m_role.GetInt(enProp.exp) / (float)RoleLvExpCfg.GetNeedExp(m_role.GetInt(enProp.level)));}

    public void FreshRolesInfo()
    {
        
        Role hero = RoleMgr.instance.Hero;
        Role[] roles = new Role[3];
        roles[0] = hero;
        roleInfoGroup.SetCount(roleGo.Length);
        for (int i=0;i< roleInfoGroup.Count;++i)
        {
            UIMainCityRoleItem item = roleInfoGroup.Get<UIMainCityRoleItem>(i);
            item.Init(roles[i], roleGo[i]);
        }
    }

    void FreshRolesInfoPos()
    {
        for (int i = 0; i < roleInfoGroup.Count; ++i)
        {
            UIMainCityRoleItem item = roleInfoGroup.Get<UIMainCityRoleItem>(i);
            item.UpdatePos();
        }
    }


    void BuyStamina()
    {
        //string content = "是否花费钻石换取体力";
        //if (m_role.GetStaminaBuyNum() > 0)
        //{
        //    //content = string.Format("是否花费钻石换取体力 购买{0}点体力需 <quad class={1}><color=#f1d7ad>{2}</color>", );
        //    UIMessageBox.Open(content, OnBuyStamina, null, "确定购买", null, "购买体力");
        //}
        //else
        //{

        //}
        UIMessage.Show("该功能未实现，敬请期待!");
        return;
    }

    void OnBuyStamina()
    {

    }
    
    //体力
    void FreshStamina() {
        int stamina = m_role.GetStamina();
        int maxStamina = RoleLvExpCfg.Get(m_role.GetInt(enProp.level)).maxStamina;

        Text txt = GetItem(enSystem.stamina).txt;
        Text txt1 = txt.transform.Find("txt1").GetComponent<Text>();
        Text txt2 = txt.transform.Find("txt2").GetComponent<Text>();
        Text txt3 = txt.transform.Find("txt3").GetComponent<Text>();
        txt.GetComponent<Text>().enabled = false;
        if (txt1 == null || txt2 == null || txt3 == null)
        {
            Debuger.LogError("主界面体力显示txt控件不对");
            return;
        }
        txt1.gameObject.SetActive(true);
        txt2.gameObject.SetActive(true);
        txt3.text = string.Format("/{0}", maxStamina);
        if (stamina > maxStamina)
        {
            txt2.text = stamina.ToString();
            txt1.gameObject.SetActive(false);
        }
        else
        {
            txt2.gameObject.SetActive(false);
            txt1.text = stamina.ToString();
        }
    }
    
    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        //要缩回右边栏
        m_toggle.SetState(0);

        //界面关掉的时候要取消监听
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
        if (m_observer2 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer2); m_observer2 = EventMgr.Invalid_Id; }
        if (m_observer3 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer3); m_observer3 = EventMgr.Invalid_Id; }
        if (m_observer4 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer4); m_observer4 = EventMgr.Invalid_Id; }
        if (m_observer5 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer5); m_observer5 = EventMgr.Invalid_Id; }

        CancelInvoke();
        m_role = null;
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

        if (PanelBase.IsTop != m_isTopBefore)
        {
            m_isTopBefore = PanelBase.IsTop;
            if (m_isTopBefore)
            {
                EventMgr.FireAll(MSG.MSG_SYSTEM, MSG_SYSTEM.MAINCITY_UI_TOP);
                PanelBase.PlayAni("ui_ani_maincity_top", false);
            }
            else
            {
                EventMgr.FireAll(MSG.MSG_SYSTEM, MSG_SYSTEM.MAINCITY_UI_UNTOP);
                PanelBase.PlayAni("ui_ani_maincity_untop", false);
            }
        }
        FreshRolesInfoPos();
    }
    #endregion

    #region Private Methods
    void OnClickItem(StateHandle s)
    {
        UIMainCityItem item =s.Get<UIMainCityItem>();
        if(item == null)
        {
            Debuger.LogError("找不到UIMainCityItem");
            return;
        }

        if(HideWhenOpen(item.sys))
        {
            SystemMgr.instance.SetTip(item.sys, false);
        }

        Action a =s_click.Get(item.sys);
        if (a == null)
        {
            UIMessage.Show("该功能未实现，敬请期待!");
            return;
        }

        a();
    }

    #endregion

    public UIMainCityItem GetItem(enSystem sys)
    {
        return m_itemsBySys.Get(sys);
    }
        
    public void SetRoleMod(GameObject obj,int index)
    {
        roleGo[index] = obj;
        if (UIMgr.instance.Get<UIMainCity>().IsOpen)
        {
            FreshRolesInfo();
        }
    }
   

}
