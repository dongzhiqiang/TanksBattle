#region Header
/**
 * 名称：UILevelAreaHead
 
 * 日期：2016.1.13
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



public class UILevelAreaBossHead : UILevelArea
{

    #region Fields
    public GameObject m_offset;
    public UISmoothProgress m_progress;
    public ImageEx m_enemy;
    public ImageEx m_friend;
    public GameObject m_showHitPropDef;
    public ImageEx m_hitPropDefIcon;


    int m_bossId;
    Role m_boss;
    int m_observer;
    int m_observer2;
    int m_observer3;
    int m_observer4;

    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.bossHead; } }
    public override bool IsOpenOnStart { get { return true; } }
    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {

    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {
        FindBoss();

        //监听有标志的怪物的创建
        m_observer4 = EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.FLAG_CHANGE, OnFlagChange);

    }

    protected override void OnUpdateArea()
    {

    }

    //关闭
    protected override void OnCloseArea()
    {
        ClearBoss();
        if (m_observer4 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer4); m_observer4 = EventMgr.Invalid_Id; }


    }

    protected override void OnRoleBorn()
    {

    }
    #endregion

    #region Private Methods
    void FindBoss(Role except = null)
    {
        Role r = RoleMgr.instance.GetRoleByFlag(GlobalConst.FLAG_SHOW_BLOOD,-1, except);
        SetBoss(r);
    }
    static string[] s_bloods = new string[]{
        "ui_tongyong_jindu_29",
        "ui_tongyong_jindu_23",
        "ui_tongyong_jindu_25",
        "ui_tongyong_jindu_27",
        "ui_tongyong_jindu_32",
        "ui_tongyong_jindu_29",
        "ui_tongyong_jindu_23",
        "ui_tongyong_jindu_25",
        "ui_tongyong_jindu_27",
    };
    void SetBoss(Role boss)
    {
        if (m_boss != null && m_boss == boss) return;
        ClearBoss();
        if (Role == null || Role.State != global::Role.enState.alive||
         boss == null || boss.State != Role.enState.alive) return;

        m_boss=boss;
        m_bossId = m_boss.Id;

        //血条条数设置
        bool isEnemy = RoleMgr.instance.IsEnemy(Role, m_boss);
        if (!isEnemy)
        {
            m_progress.SetNum(new string[] { "ui_tongyong_jindu_23" });  
            m_enemy.gameObject.SetActive(false);
            m_friend.gameObject.SetActive(true); 
        }
        else
        {
            int num = m_boss.Cfg.headBloodNum-1;
            List<string> ss = new List<string>();
            ss.Add("ui_tongyong_jindu_32");//最底的颜色一定是红的
            for(int i = 0;i<num;++i){
                ss.Add(s_bloods[i% s_bloods.Length]);
            }
            m_progress.SetNum(ss.ToArray());

            m_enemy.gameObject.SetActive(true);
            m_friend.gameObject.SetActive(false); 
        }

        //刷新血条
        float f = m_boss.GetInt(enProp.hp) / m_boss.GetFloat(enProp.hpMax);
        m_progress.SetProgress(f,true);

        //监听血量变化和死亡
        m_observer = m_boss.AddPropChange(enProp.hp, OnHp);
        m_observer2 = m_boss.AddPropChange(enProp.hpMax, OnHp);
        m_observer3 = m_boss.Add(MSG_ROLE.DEAD, OnDead);

        //打击属性
        if (LevelMgr.instance.CurLevel.roomCfg.hitProp !=0&&!string.IsNullOrEmpty(boss.Cfg.HitDefBloodIcon))
        {
            m_showHitPropDef.SetActive(true);
            m_hitPropDefIcon.Set(boss.Cfg.HitDefBloodIcon);
        }
        else
            m_showHitPropDef.SetActive(false);

        m_offset.SetActive(true);
    }

    void ClearBoss()
    {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
        if (m_observer2 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer2); m_observer2 = EventMgr.Invalid_Id; }
        if (m_observer3 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer3); m_observer3 = EventMgr.Invalid_Id; }

        m_boss = null;
        m_offset.SetActive(false);
        
    }

    //当前没有boss的时候有boss被设置了显示血条标记，那么这个boss的血条显示
    void OnFlagChange(object p, object p2, object p3, EventObserver ob)
    {
        string flag = (string)p;
        if (flag != GlobalConst.FLAG_SHOW_BLOOD)
            return ;

        if (m_boss != null && !m_boss.IsDestroy(m_bossId) && m_boss.State== global::Role.enState.alive)
            return ;

        Role r =ob.GetParent<Role>();
        if (r.GetFlag(GlobalConst.FLAG_SHOW_BLOOD)==0)//检查是不是有这个标记
            return ;

        SetBoss(r);
        return ;
    }
    void OnHp()
    {
        float f = m_boss.GetInt(enProp.hp) / m_boss.GetFloat(enProp.hpMax);
        m_progress.SetProgress(f, false);
        
    }

    void OnDead()
    {
        Role boss = m_boss;
        ClearBoss();
        FindBoss(boss);//找到新的，需要显示血条的怪
    }
    #endregion

    public bool IsInMoniter(Role role)
    {
        return m_boss == role;
    }
}
