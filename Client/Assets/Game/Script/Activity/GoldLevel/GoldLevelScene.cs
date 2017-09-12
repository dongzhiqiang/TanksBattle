using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GoldLevelScene : LevelBase
{
    private const int CHECK_STATE_INV = 50; //检查逻辑时间间隔，单位毫秒

    private int m_npcHPMax;
    private UILevel m_uiLevel;
    private UILevelAreaTime m_uiTime;
    private UILevelAreaReward m_uiReward;
    private GoldLevelModeCfg m_cfg;
    private DateTime m_lastRefresh = DateTime.Now;    
    private string m_lastRate = "";
    private bool hasSendEndMsg = false;
    private float lastDmgHPRatio = 0.0f;
    private int lastGold = 0;
    private int flyCountrer = 0; //用于飘物统计，如果这个值为0，HP为0，就可以结算了

    private void RefreshUI()
    {
        if (m_npcHPMax > 0)
        {
            List<Role> npcs = GetRoleById(m_cfg.monsterId);
            Role monster = npcs.Count > 0 ? npcs[0] : null;
            float npcHP = monster != null ? monster.GetFloat(enProp.hp) : 0;

            if (npcHP <= 0)
                OnFinishLevel(true);

            float leftHPRatio = npcHP / m_npcHPMax;
            float dmgHPRatio = 1 - leftHPRatio;
            float flyGoldGap = GoldLevelBasicCfg.Get().flyGoldGap;
            int curIntRatio = Mathf.FloorToInt(dmgHPRatio * 100 / flyGoldGap);
            int lastIntRatio = Mathf.FloorToInt(lastDmgHPRatio * 100 / flyGoldGap);

            if (lastGold == 0 || curIntRatio != lastIntRatio)
            {
                var gold = (int)Mathf.Floor(m_cfg.basicGold + m_cfg.maxGold * (1 - Mathf.Pow(leftHPRatio, m_cfg.goldFactor)));
                if (gold - lastGold > 0 && curIntRatio > 0)
                {
                    ++flyCountrer;
                    m_uiReward.PlayGoldFly(monster, gold - lastGold, GoldLevelBasicCfg.Get().flyGoldShowNum, () => {
                        if (--flyCountrer <= 0 && npcHP <= 0)
                            SendEndMsg();
                    });
                }                    
                else
                    m_uiReward.SetGoldVal(gold);
                lastGold = gold;
            }
            lastDmgHPRatio = dmgHPRatio;

            //怪存在？那就根据血量加动作状态
            var rate = GoldLevelBasicCfg.GetRate((int)npcHP, m_npcHPMax);
            if (rate != m_lastRate)
            {
                var newRewardNum = GoldLevelModeCfg.GetAccRewardNum(m_cfg.mode, rate);
                var oldRewardNum = GoldLevelModeCfg.GetAccRewardNum(m_cfg.mode, m_lastRate);
                var addRewardNum = newRewardNum - oldRewardNum;

                m_lastRate = rate;

                if (addRewardNum > 0)
                {
                    ++flyCountrer;
                    m_uiReward.PlayItemFly(monster, addRewardNum, () =>
                    {
                        if (--flyCountrer <= 0 && npcHP <= 0)
                            SendEndMsg();
                    });
                }

                if (monster != null)
                {
                    var bufId = GoldLevelBasicCfg.GetBufId(rate);
                    if (bufId > 0)
                        monster.BuffPart.AddBuff(bufId);
                }
            }
        }
    }

    public override IEnumerator OnLoad()
    {
        var cfg = GoldLevelBasicCfg.Get();
        BuffCfg.ProLoad(cfg.rateCBuf);
        BuffCfg.ProLoad(cfg.rateBBuf);
        BuffCfg.ProLoad(cfg.rateABuf);
        BuffCfg.ProLoad(cfg.rateSBuf);
        BuffCfg.ProLoad(cfg.rateSSBuf);
        BuffCfg.ProLoad(cfg.rateSSSBuf);
        yield return 0;
    }

    public override void OnLoadFinish()
    {
        hasSendEndMsg = false;

        m_cfg = (GoldLevelModeCfg)mParam;
        m_uiLevel = UIMgr.instance.Open<UILevel>();
        m_uiTime = m_uiLevel.Open<UILevelAreaTime>();
        m_uiTime.SetTime(m_cfg.limitTime);
        m_uiReward = m_uiLevel.Open<UILevelAreaReward>();
        m_uiReward.ResetUI();
    }

  
    public override void OnHeroEnter(Role hero)
    {
    }

    public override void OnRoleEnter(Role role)
    {
        if (role.GetString(enProp.roleId) == m_cfg.monsterId)
        {
            m_npcHPMax = (int)role.GetFloat(enProp.hpMax);
            role.Add(MSG_ROLE.DEAD, () => { RefreshUI(); });
            RefreshUI();
        }
    }

    //倒计时结束回调
    public override void OnTimeout(int time)
    {
        OnFinishLevel(false);
    }

    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
        DateTime curTime = DateTime.Now;
        if ((curTime - m_lastRefresh).TotalMilliseconds >= CHECK_STATE_INV)
        {
            m_lastRefresh = curTime;
            RefreshUI();
        }
    }

    public void OnFinishLevel(bool isWin)
    {
        if (isWin)
        {
            //停止计时
            m_uiTime.OnPause(true);
            //保险起见，还有一个定时器来保证一定传发消息
            //注意，如果提前打死怪时，SendEndMsg一般会调用两次，因为有判断是否已发送，所以没事
            TimeMgr.instance.AddTimer(6, SendEndMsg);
        }            
        else
        {
            //强制停止
            TimeMgr.instance.AddPause();
            SendEndMsg();
        }            
    }

    public void SendEndMsg()
    {
        if (hasSendEndMsg)
            return;
        hasSendEndMsg = true;

        List<Role> npcs = GetRoleById(m_cfg.monsterId);
        Role monster = npcs.Count > 0 ? npcs[0] : null;
        int npcHP = monster != null ? (int)monster.GetFloat(enProp.hp) : 0;
        NetMgr.instance.ActivityHandler.SendEndGoldLevel(npcHP, m_npcHPMax);
        //if (monster != null)
        //{
        //    monster.DeadPart.Handle(true);
        //}
    }    
}
