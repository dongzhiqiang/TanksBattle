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



public class UILevelAreaBlood : UILevelArea
{
    public enum enCampBloodType{
        friend1,//友军、小怪血条
        friend2,//友军、精英血条
        friend3,//友军、建筑怪血条
        enemy1,//敌军、小怪血条
        enemy2,//敌军、精英血条
        enemy3,//敌军、建筑怪血条
        npc,//npc血条
        arenaEnemyHero,//竞技场敌方主角血条
        max,
    }
    
    #region Fields
    public float m_hideDelay = 2f;
    public List<SimplePool> m_bloodPools;

    Dictionary<int, UIBlood> m_bloods = new Dictionary<int, UIBlood>();//所有显示中的血条
    List<int> m_removes = new List<int>();
    int m_observer;
    RectTransform m_rt;
    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.blood; } }
    public override bool IsOpenOnStart { get{return true;} }
    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {
        m_rt = this.GetComponent<RectTransform>();
    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {
        //隐藏所有血条
        foreach (SimplePool pool in m_bloodPools)
        {
            pool.Clear();
        }
    }

    protected override void OnUpdateArea()
    {
        float time = TimeMgr.instance.logicTime;
        Camera ca= CameraMgr.instance.CurCamera;
        Camera caUI = UIMgr.instance.UICameraHight;
        if (ca == null || caUI==null) return;

        Role r;
        UIBlood blood;
        foreach (KeyValuePair<int, UIBlood> pair in m_bloods)
        {
            blood = pair.Value;
            //间隔一定时间删除
            if (time - blood.m_lastBehitTime > m_hideDelay)
            {
                blood.gameObject.SetActive(false);
                m_removes.Add(pair.Key);
                continue;
            }

            r = blood.m_role;           
            //角色死了或者被销毁的话删除
            if (r == null || r.IsDestroy(blood.m_roleId) || r.State != global::Role.enState.alive)
            {
                blood.gameObject.SetActive(false);
                m_removes.Add(pair.Key);
                continue;
            }

            //设置下位置
            Vector2 pos2D;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rt,ca.WorldToScreenPoint(r.RoleModel.Title.position),caUI, out pos2D)){
                blood.m_rt.anchoredPosition = pos2D;
            }
            else
                Debuger.LogError("UILevelAreaBlood计算不出2d位置");

        }

        if (m_removes.Count != 0)
        {
            for (int i = 0; i < m_removes.Count; ++i)
            {
                m_bloods.Remove(m_removes[i]);
            }
                
            m_removes.Clear();
        }
    }

    //关闭
    protected override void OnCloseArea()
    {
        //隐藏所有血条
        foreach (SimplePool pool in m_bloodPools)
            pool.Clear();    

        //清空当前血条列表
        m_bloods.Clear();

        //取消角色攻击监听
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
    }

    protected override void OnRoleBorn()
    {
        //监听主角打别人
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
        m_observer = Role.Add(MSG_ROLE.HIT, OnHit);
        
    }

    #endregion

    #region Private Methods

    void OnHit(object p)
    {
        Role hero = RoleMgr.instance.Hero;
        if (hero.State != global::Role.enState.alive)
            return;
        Role target = (Role)p;
        if (target.GetFlag(GlobalConst.FLAG_SHOW_FRIENDBLOOD) > 0)
            return;
        if (m_parent.Get<UILevelAreaBossHead>().IsOpen && m_parent.Get<UILevelAreaBossHead>().IsInMoniter(target))
            return;
        if (m_parent.Get<UILevelAreaHead>().IsOpen && m_parent.Get<UILevelAreaHead>().IsInMoniter(target))
            return;
        if (m_parent.Get<UILevelAreaArena>().IsOpen && m_parent.Get<UILevelAreaArena>().IsInMoniter(target))
            return;
        int cutHp = target.HatePart.LastBeHitHp;
        if (cutHp == -1)
        {
            Debuger.LogError("逻辑错误，记录不到被扣多少血");
            return;
        }

        //判断血条类型，可能不需要显示血条
        enCampBloodType bloodType = enCampBloodType.max;
        bool isEnemy = RoleMgr.instance.IsEnemy(hero, target);


        switch (target.Cfg.TitleBloodType)
        {
            case enBloodType.none: break;
            case enBloodType.small: bloodType = isEnemy ? enCampBloodType.enemy1 : enCampBloodType.friend1; break;
            case enBloodType.big: bloodType = isEnemy ? enCampBloodType.enemy2 : enCampBloodType.friend2; break;
            case enBloodType.building: bloodType = isEnemy ? enCampBloodType.enemy3 : enCampBloodType.friend3; break;
            case enBloodType.npc: bloodType = enCampBloodType.npc; break;
        }


        if (bloodType == enCampBloodType.max)
            return;

        UIBlood blood = m_bloods.Get(target.Id);

        //血条之前没有的话，新建
        if (blood == null)
        {
            //从对象池取出来，初始化
            blood = m_bloodPools[(int)bloodType].Get().GetComponent<UIBlood>();
            blood.m_roleId = target.Id;
            blood.m_role = target;
            m_bloods[target.Id] = blood;
            if (blood.m_name != null)
                blood.m_name.text = target.GetString(enProp.name);
            //设置下位置
            Camera ca = CameraMgr.instance.CurCamera;
            Camera caUI = UIMgr.instance.UICameraHight;
            if (ca != null && caUI != null)
            {
                Vector2 pos2D;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rt, ca.WorldToScreenPoint(target.RoleModel.Title.position), caUI, out pos2D))
                {
                    blood.m_rt.anchoredPosition = pos2D;
                }
                else
                    Debuger.LogError("UILevelAreaBlood计算不出2d位置2");
            }

            //血量，直接设置
            blood.m_progress.SetProgress((target.GetInt(enProp.hp) + cutHp) / target.GetFloat(enProp.hpMax), true);

            //打击属性
            if (blood.m_showHitPropDef != null)
            {
                if (LevelMgr.instance.CurLevel.roomCfg.hitProp != 0 && !string.IsNullOrEmpty(target.Cfg.HitDefBloodIcon))
                {
                    blood.m_showHitPropDef.SetActive(true);
                    blood.m_hitPropDefIcon.Set(target.Cfg.HitDefBloodIcon);
                }
                else
                    blood.m_showHitPropDef.SetActive(false);
            }

        }

        //血量，渐变
        float f = target.GetInt(enProp.hp) / target.GetFloat(enProp.hpMax);
        blood.m_lastBehitTime = TimeMgr.instance.logicTime;
        blood.m_progress.SetProgress(f, false);


    }

    #endregion

    
}
