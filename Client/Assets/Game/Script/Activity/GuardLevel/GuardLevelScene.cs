using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GuardLevelScene : LevelBase
{
    private UILevel m_uiLevel;
    private UILevelAreaTime m_uiTime;
    private UILevelAreaWave m_uiWave;
    private UILevelAreaReward m_uiReward;
    private UILevelSkillItem m_uiLevelSkillItem;
    private GuardLevelModeCfg m_cfg;
    private int m_observer;
    private int m_obScene;
    private int m_wave = 0;
    private List<Role> m_npcs = new List<Role>();
    private float m_npcPercent = 1f;
    private bool m_hpOk = true;
    private int m_killCount = 0;
    private PropertyTable tempProps = new PropertyTable();

    public override void OnLoadFinish()
    {
        m_cfg = (GuardLevelModeCfg)mParam;
        m_uiLevel = UIMgr.instance.Open<UILevel>();
        m_uiLevel.Open<UILevelAreaGizmos>();
        m_uiReward = m_uiLevel.Open<UILevelAreaReward>();
        m_uiReward.ResetUI();
        //m_uiTime = m_uiLevel.Open<UILevelAreaTime>();
        //m_uiTime.SetTime(GuardLevelBasicCfg.Get().limitTime);
        m_uiLevelSkillItem = m_uiLevel.Get<UILevelAreaJoystick>().m_qte;

        m_uiLevel.Close<UILevelAreaBossHead>();
        m_uiLevel.Open<UILevelAreaFamilyHead>();
        
        //m_uiWave = m_uiLevel.Open<UILevelAreaWave>();
        //m_uiWave.SetWave(0, GuardLevelBasicCfg.Get().maxWave);
        //m_uiWave.SetMonsterCount(0);
        m_npcPercent = 1f;
        
        m_wave = 0;
        m_killCount = 0;
        m_npcs.Clear();

        m_hpOk = true;
         
        //m_monsterCount = 0;
        m_observer = EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.DEAD, OnRoleDead);

        m_obScene = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.EVENTMSG, OnSceneEvent);
    }

    
    public override void OnHeroEnter(Role hero)
    {
    }

    public override void OnRoleEnter(Role role)
    {
        TimeMgr.instance.AddTimer(0, () =>
        {
            if(role.GetFlag("NPC")>0)
            {
                m_npcs.Add(role);
            }
            else if(role.IsMonster)
            {
                int wave = m_uiLevel.Get<UILevelAreaWave>().CurWave;
                GuardLevelWaveCfg waveCfg = GuardLevelWaveCfg.m_cfgs[wave];
                if (waveCfg!=null && !string.IsNullOrEmpty(waveCfg.propRate))
                {
                    PropertyTable.Add(1f, PropRateCfg.Get(waveCfg.propRate).props, tempProps);
                    PropertyTable.Mul(tempProps, role.PropPart.Values, role.PropPart.Values);
                }
                role.PropPart.SetBaseProps(role.PropPart.Values, role.PropPart.Rates);
            }
        });
         
    }

    public void OnRoleDead(object p, object p2, object p3, EventObserver ob)
    {
        Role role = ob.GetParent<Role>();

        // 计算百分比?
        if (role.GetFlag("NPC") > 0)
        {
            m_npcPercent = 0f;
            m_npcs.Clear();
        }
        else
        {
            int wave = role.GetFlag(GlobalConst.FLAG_REFLESH_WAVE);
            if(m_wave != wave)
            {
                m_killCount = 0;
                m_wave = wave;
            }
            m_killCount++;
            //Debuger.Log("kill" + m_killCount);
        }
    }

    void OnSceneEvent(object p, object p2, object p3, EventObserver ob)
    {
        string id = (string)p;
        Debuger.Log("scene event msg:" + id);
        if( id == "playWallAni")
        {
            string ani = (string)p2;
            Debuger.Log("playWallAni:" + ani);
            PlayWallAni(ani);
        }
    }


    //倒计时结束回调
    public override void OnTimeout(int time)
    {
        //SendResult(true);
        SendResult(false);
    }

    public override void OnExit()
    {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
        if (m_obScene != EventMgr.Invalid_Id) { EventMgr.Remove(m_obScene); m_obScene = EventMgr.Invalid_Id; }
    }

    public override void OnUpdate()
    {
        Role hero = RoleMgr.instance.Hero;
        if (hero.State != Role.enState.alive)//如果角色不在生存态，下面获取距离会报错
            return;

        bool inRange = false;
        bool lowHp = false;
        for(int i=0; i<m_npcs.Count; i++)
        {
            Role npc = m_npcs[i];
            float npcPercent = npc.GetFloat(enProp.hp) / npc.GetFloat(enProp.hpMax) * 100;
            if(npcPercent<50)
            {
                lowHp = true;
            }
            if (npc.Distance(hero) <= GuardLevelBasicCfg.Get().skillRange) 
            {
                inRange = true;
            }
        }
        if (lowHp)
        {
            if(m_hpOk)
            {
                m_hpOk = false;
                UIMessage.Show("血量下降，快回到家人身边");
            }
        }
        else
        {
            m_hpOk = true;
        }
        if( inRange )
        {
            if (m_uiLevelSkillItem.RuntimeSkill == null)
            {
                m_uiLevelSkillItem.RuntimeSkill = hero.CombatPart.GetSkill(GuardLevelBasicCfg.Get().skillId);
            }
        }
        else
        {
            if (m_uiLevelSkillItem.RuntimeSkill != null)
            {
                m_uiLevelSkillItem.RuntimeSkill = null;
            }
        }
    }

    public override void SendResult(bool isWin)
    {
        //m_uiTime.OnPause(true);
        Role hero = RoleMgr.instance.Hero;
        //float selfPercent = hero.GetFloat(enProp.hp)/hero.GetFloat(enProp.hpMax)*100;
        /*
        if(!isWin)
        {
            // lose
            NetMgr.instance.ActivityHandler.SendEndGuardLevel(m_uiLevel.Get<UILevelAreaWave>().CurWave, selfPercent, m_npcPercent);
            
            return;
        }*/
        int wave = m_uiLevel.Get<UILevelAreaWave>().CurWave;
        int point = 0;
        for (int i = 1; i < wave; i++)
        {
            GuardLevelWaveCfg waveCfg = GuardLevelWaveCfg.m_cfgs[i];
            point += waveCfg.point;
        }
        if(isWin)
        {
            point += GuardLevelWaveCfg.m_cfgs[wave].point;
        }
        else
        {
            float rate = (float)m_killCount / GuardLevelWaveCfg.m_cfgs[wave].monsterNum;
            if(rate > 1.0001)
            {
                Debuger.LogError("挚爱守护怪物数量填写错误 wave:" + wave + " 杀死:" + m_killCount + " 总数:" + GuardLevelWaveCfg.m_cfgs[wave].monsterNum);
                rate = 1;
            }
            //Debuger.Log(m_killCount + "/" + GuardLevelWaveCfg.m_cfgs[wave].monsterNum + " rate:" + rate);
            point += Mathf.FloorToInt(GuardLevelWaveCfg.m_cfgs[wave].point*rate);
            //Debuger.Log("all point:" + GuardLevelWaveCfg.m_cfgs[wave].point);
        }
        //Debuger.Log("point:" + point);

        /*
        if (m_npcPercent != 0)
        {
            float hp = 0;
            float maxHp = 0;
            foreach (Role role in m_npcs)
            {
                hp += role.GetFloat(enProp.hp);
                maxHp += role.GetFloat(enProp.hpMax);
            }
            if (maxHp == 0)
            {
                m_npcPercent = 0;
            }
            else
            {
                m_npcPercent = hp / maxHp * 100;
            }
        }
         */

        if(!isWin)
        {
            TimeMgr.instance.AddPause();
        }

        TimeMgr.instance.AddTimer(3, () =>
        {
            //NetMgr.instance.ActivityHandler.SendEndGuardLevel(m_wave, 100f, 100f);
            NetMgr.instance.ActivityHandler.SendEndGuardLevel(wave, point);
        });
        

    }

    void PlayWallAni(string aniName)
    {
        GameObject wall = GameObject.Find("shouhujiaren_donghua_001");
        Animation ani = wall.GetComponent<Animation>();
        ani.clip = ani.GetClip(aniName);
        ani.Play();
    }
}
