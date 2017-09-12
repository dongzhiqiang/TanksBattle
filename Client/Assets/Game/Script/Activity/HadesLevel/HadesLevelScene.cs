using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class HadesLevelScene : LevelBase
{
    private UILevel m_uiLevel;
    private UILevelAreaTime m_uiTime;
    private UILevelAreaWave m_uiWave;
    private UILevelAreaMonster m_uiMonster;
    private UILevelAreaReward m_uiReward;
    private UIHades m_hades;
    private HadesLevelModeCfg m_cfg;
    private int m_observer;
    private int m_observerBorn;
    private int m_wave = 0;
    private int m_monsterCount = 0;
    private List<Role> m_bosses = new List<Role>();
    private List<Role> m_monsteres = new List<Role>();

    public override void OnLoadFinish()
    {
        m_cfg = (HadesLevelModeCfg)mParam;
        m_uiLevel = UIMgr.instance.Open<UILevel>();
        m_uiReward = m_uiLevel.Open<UILevelAreaReward>();
        m_uiReward.ResetUI();
        m_uiTime = m_uiLevel.Open<UILevelAreaTime>();
        m_uiTime.SetTime(HadesLevelBasicCfg.Get().limitTime);
        m_uiWave = m_uiLevel.Open<UILevelAreaWave>();
        m_uiWave.SetDesc("怪物波次：");
        m_uiWave.SetWave(0, HadesLevelBasicCfg.Get().maxWave);
        m_uiMonster = m_uiLevel.Open<UILevelAreaMonster>();
        m_uiMonster.SetMonsterCount(0);
        m_hades = UIMgr.instance.Open<UIHades>();
        //UIMgr.instance.CheckOpenTopAndAutoOrder(m_uiLevel.GetComponent<UIPanelBase>());
        m_wave = 0;
        m_monsterCount = 0;
        m_bosses.Clear();
        m_monsteres.Clear();
        m_observerBorn = EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.BORN, OnRoleBorn);
        m_observer = EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.DEAD, OnRoleDead);
    }

    
    public override void OnHeroEnter(Role hero)
    {
    }



    public override void OnRoleEnter(Role role)
    {
        /*
        if(role.GetString(enProp.roleId)==m_cfg.bossId)
        {
            m_monsterCount++;
            m_uiMonster.SetMonsterCount(m_monsterCount);
        }
        
        TimeMgr.instance.AddTimer(0, () =>
        {
            if(role.GetFlag(m_cfg.waveFlag)>0)
            {
                m_wave++;
                m_uiWave.SetWave(m_wave, HadesLevelBasicCfg.Get().maxWave);
            }
        });
         */
    }

    public void OnRoleBorn(object p, object p2, object p3, EventObserver ob)
    {
        Role role = ob.GetParent<Role>();
        if (role.GetString(enProp.roleId) == m_cfg.bossId)
        {
            m_bosses.Add(role);
        }
        else if(role.GetString(enProp.roleId) != "hd_hadisi_xueqiu" && role.Cfg.RolePropType == enRolePropType.monster)
        {
            m_monsteres.Add(role);
        }
        if(m_wave != m_uiWave.CurWave)
        {
            m_wave = m_uiWave.CurWave;
            UIMessage.Show(string.Format("第{0}波怪物已经出现，请注意拦截",m_wave));
        }
    }

    public void OnRoleDead(object p, object p2, object p3, EventObserver ob)
    {
        /*
        Role role = ob.GetParent<Role>();
        if (role.GetString(enProp.roleId) == m_cfg.bossId)
        {
            m_monsterCount--;
            m_uiMonster.SetMonsterCount(m_monsterCount);
            //if (m_monsterCount == 0 && m_wave == HadesLevelBasicCfg.Get().maxWave)
            {
                //SendResult(true);
            }
        }
        */
        Role role = ob.GetParent<Role>();
        if (role.GetString(enProp.roleId) == m_cfg.bossId)
        {
            m_bosses.Remove(role);
        }
        else if (role.GetString(enProp.roleId) != "hd_hadisi_xueqiu" && role.Cfg.RolePropType == enRolePropType.monster)
        {
            m_monsteres.Remove(role);
        }
        if(m_bosses.Count == 0 && m_monsteres.Count == 0 && m_uiWave.CurWave >= HadesLevelBasicCfg.Get().maxWave)
        {
            SendResult(true);
        }
    }

    //倒计时结束回调
    public override void OnTimeout(int time)
    {
        SendResult(true);
    }

    public override void OnExit()
    {
        if (m_observerBorn != EventMgr.Invalid_Id) { EventMgr.Remove(m_observerBorn); m_observerBorn = EventMgr.Invalid_Id; }
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
    }

    public override void OnUpdate()
    {
        m_monsterCount = m_bosses.Count;
        m_uiMonster.SetMonsterCount(m_monsterCount);
        m_hades.FreshRole(RoleMgr.instance.Hero, /*GetRoleById("hd_hadisi_xiaogou")*/m_monsteres, m_bosses, GetRoleById("hd_hadisi_xueqiu"));
    }

    public override void SendResult(bool isWin)
    {
        m_uiTime.OnPause(true);
        if(RoleMgr.instance.Hero.GetFloat(enProp.hp) <= 0)
        {
            isWin = false;
        }
        if(!isWin)
        {
            TimeMgr.instance.AddPause();

            UIMgr.instance.Open<UILevelFail>();
            return;
        }

        OnUpdate();

        TimeMgr.instance.AddPause();

        TimeMgr.instance.AddTimer(3, () =>
        {
            NetMgr.instance.ActivityHandler.SendEndHadesLevel(m_uiWave.CurWave, m_monsterCount);
        });
        

    }
}
