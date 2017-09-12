using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class UIHeroMenu : UIPanel {
    public enum MenuType
    {
        view = 1,                    //查看信息
        addFriend = 2,           //添加好友
        deleteFriend = 3,       //删除好友
        kickout = 4,               //踢出公会
        appointElder = 5,       //任命长老
        chat = 6,                      //私聊
        appointPresident = 7,   //转让会长
        removeElder = 8,         //废除长老
    }
    
    public class Cx
    {
        public string btnName;
        public System.Func<string, bool> callback;
        public Cx(string name, System.Func<string, bool> cb)
        {
            btnName = name;
            callback = cb;
        }
    }

    public class MenuInfo
    {
        public int heroId;
        public string name;
        public int level;
        public int power;
    }

    //格子们
    public StateGroup m_GridGroup;
    //滚动区域
    public ScrollRect m_ScrollView;

    public TextEx m_name;
    public TextEx m_level;
    public RectTransform m_levelParent;
    public TextEx m_power;
    public RectTransform m_powerParent;

    public MenuInfo m_mInfo;

    /// <summary>
    ///  打开角色菜单面板
    /// </summary>
    /// <param name="name"></param> 
    /// <param name="heroId"></param> 
    /// <param name="power"></param> 
    /// <param name="level"></param> 
    public static void Show(string name, int heroId, int power = -1, int level = -1)
    {
        MenuInfo f = new MenuInfo();
        f.heroId = heroId;
        f.name = name;
        f.level = level;
        f.power = power;
        Open(f);
    }
    public static void Open(MenuInfo f)
    {
        UIMgr.instance.Open<UIHeroMenu>(f);
    }
    
    public override void OnInitPanel()
    {
    }

    public override void OnOpenPanel(object param)
    {
        m_mInfo = (MenuInfo)param;
        m_name.text = m_mInfo.name;

        if (m_mInfo.power < 0)
        {
            m_powerParent.gameObject.SetActive(false);
            m_power.text = "";
        }
        else
        {
            m_powerParent.gameObject.SetActive(true);
            m_power.text = m_mInfo.power.ToString();
        }

        if (m_mInfo.level < 0)
        {
            m_levelParent.gameObject.SetActive(false);
            m_level.text = "";
        }
        else
        {
            m_levelParent.gameObject.SetActive(true);
            m_level.text = m_mInfo.level.ToString();
        }

        List<RoleMenuConfig> btnNames = GetBtns();
        int count = btnNames.Count;
        m_GridGroup.SetCount(count);
        for (int i = 0; i < count; i++)
        {
            var sel = m_GridGroup.Get<UIHeroMenuItem>(i);
            sel.Init(btnNames[i], OnBtnClick);   //通过回调处理点击操作判断
        }

        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_ScrollView, 0); });
    }

    public override void OnClosePanel()
    {
    }

    #region PrivateMethod
    bool OnBtnClick(int menuId)
    {
        switch ((MenuType)menuId)
        {
            case MenuType.view:
                OnReqHeroInfo();
                break;
            case MenuType.addFriend:
                OnAddFriend();
                break;
            case MenuType.deleteFriend:
                OnDeleteFriend();
                break;
            case MenuType.kickout:
                OnTickOut();
                break;
            case MenuType.appointElder:
                OnSetElder();
                break;
            case MenuType.chat:
                OnPrivateChat();
                break;
            case MenuType.appointPresident:
                OnChangeLeader();
                break;
            case MenuType.removeElder:
                OnUnsetElder();
                break;
            default:
                break;
        }
        return true;   
    }

    void OnReqHeroInfo()
    {
        NetMgr.instance.RoleHandler.RequestHeroInfo(m_mInfo.heroId);
        Close();
    }

    void OnAddFriend()
    {
        NetMgr.instance.SocialHandler.AddFriend(m_mInfo.name);
        Close();
    }

    void OnDeleteFriend()
    {
        NetMgr.instance.SocialHandler.DeleteFriend(m_mInfo.heroId);
        Close();
    }

    void OnTickOut()
    {
        Role role = RoleMgr.instance.Hero;
        int corpsId = role.GetInt(enProp.corpsId);
        NetMgr.instance.CorpsHandler.HandleMember(corpsId, role.GetInt(enProp.heroId), m_mInfo.heroId, m_mInfo.name, 
            (int)HandlerMemberType.kickout, 0);
        Close();
    }

    void OnSetElder()
    {
        Role role = RoleMgr.instance.Hero;
        int corpsId = role.GetInt(enProp.corpsId);
        NetMgr.instance.CorpsHandler.HandleMember(corpsId, role.GetInt(enProp.heroId), m_mInfo.heroId, m_mInfo.name, 
            (int)HandlerMemberType.appoint, 2);
        Close();
    }

    void OnPrivateChat()
    {
        NetMgr.instance.ChatHandler.SendCreateWhisper(m_mInfo.heroId);
        Close();
    }

    void OnChangeLeader()
    {
        Role role = RoleMgr.instance.Hero;
        int corpsId = role.GetInt(enProp.corpsId);
        NetMgr.instance.CorpsHandler.HandleMember(corpsId, role.GetInt(enProp.heroId), m_mInfo.heroId, m_mInfo.name, 
            (int)HandlerMemberType.appoint, 1);
        Close();
    }

    void OnUnsetElder()
    {
        Role role = RoleMgr.instance.Hero;
        int corpsId = role.GetInt(enProp.corpsId);
        NetMgr.instance.CorpsHandler.HandleMember(corpsId, role.GetInt(enProp.heroId), m_mInfo.heroId, m_mInfo.name, 
            (int)HandlerMemberType.appoint, 3);
        Close();
    }

    //根据情况添加操作按钮
    List<RoleMenuConfig> GetBtns()
    {
        Role role = RoleMgr.instance.Hero;
        //初始化操作按钮
        List<RoleMenuConfig> btnNames = new List<RoleMenuConfig>();
        //查看信息
        btnNames.Add(RoleMenuCfg.Get((int)MenuType.view));
        //是否好友
        if (role.SocialPart.IsFriendById(m_mInfo.heroId))
            btnNames.Add(RoleMenuCfg.Get((int)MenuType.deleteFriend));
        else
            btnNames.Add(RoleMenuCfg.Get((int)MenuType.addFriend));
        //私聊
        btnNames.Add(RoleMenuCfg.Get((int)MenuType.chat));
        if(role.GetInt(enProp.corpsId) > 0)  //自己有公会
        {
            int ownPos = role.CorpsPart.personalInfo.pos; //自己的职位
            CorpsMember mem = role.CorpsPart.GetMemberInfo(m_mInfo.heroId);  //他人在自己公会里的会员信息
            if (mem != null)
            {
                //公会职位是否会长
                if (ownPos == (int)CorpsPosEnum.President)
                {
                    //对方是否长老
                    if (mem.pos == (int)CorpsPosEnum.Elder)
                        btnNames.Add(RoleMenuCfg.Get((int)MenuType.removeElder));
                    else
                        btnNames.Add(RoleMenuCfg.Get((int)MenuType.appointElder));
                    btnNames.Add(RoleMenuCfg.Get((int)MenuType.appointPresident));

                }
                //公会职位是否长老以上并且职位比自己低，才能踢人
                if (ownPos <= (int)CorpsPosEnum.Elder && ownPos < mem.pos)
                        btnNames.Add(RoleMenuCfg.Get((int)MenuType.kickout));
            }
         
        }
     
        return btnNames;
    } 
    #endregion

}
