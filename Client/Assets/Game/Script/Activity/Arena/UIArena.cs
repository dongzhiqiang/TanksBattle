using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIArena : UIPanel
{
    public ScrollRect m_scroll;
    public StateGroup m_grid;
    public StateHandle m_rule;
    public TextEx m_myScore;
    public ImageEx m_gradeBigIcon;
    public ImageEx m_gradeNameImg;
    public TextEx m_arenaCoin;
    public TextEx m_leftCnt;
    public StateHandle m_leftCntPlus;
    //public StateHandle m_rewardState;
    public StateHandle m_switchDayWeekReward;
    public StateGroup m_rewardGroup;
    public StateHandle m_pet1Btn;
    public ImageEx m_pet1None;
    public ImageEx m_pet1Icon;
    public StateHandle m_pet2Btn;
    public ImageEx m_pet2None;
    public ImageEx m_pet2Icon;
    public StateHandle m_exchangeShop;
    public GameObject fx1;
    public GameObject fx2;
    public bool isOnFirstUpgrade = false;

    private int m_myselfIndex = -1;
    private DateTime m_lastRefresh = DateTime.Now;
    private string m_teachFuncCmd = "";
    private bool m_inReqChallengers = false;
    private Regex m_regExDummy = new Regex(@"<dummy>.*?</dummy>\s*?(\r\n|\r|\n)?", RegexOptions.Singleline);
    

    public override void OnInitPanel()
    {
        m_rule.AddClick(OnRuleIntro);
        m_leftCntPlus.AddClick(BuyChance);
        m_pet1Btn.AddClick(() => { UIMgr.instance.Open<UIChoosePet>(); });
        m_pet2Btn.AddClick(() => { UIMgr.instance.Open<UIChoosePet>(); });
        //m_switchDayWeekReward.AddClick(() => { m_rewardState.SetState((m_rewardState.CurStateIdx + 1) % m_rewardState.m_states.Count); RefreshUI(); });
        m_exchangeShop.AddClick(() => { UIMgr.instance.Open<UIShop>(enShopType.arenaShop); });
        TimeMgr.instance.AddTimer(60, CheckTip, -1, -1);
        EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.HERO_CREATED, CheckTip);
    }

    private void BuyChance()
    {
        ArenaBasicCfg basicCfg = ArenaBasicCfg.Get();
        Role hero = RoleMgr.instance.Hero;
        ActivityPart part = hero.ActivityPart;
        long arenaBuyCntTime = part.GetLong(enActProp.arenaBuyCntTime);
        int arenaBuyCnt = part.GetInt(enActProp.arenaBuyCnt);
        int todayBuyCnt = TimeMgr.instance.IsToday(arenaBuyCntTime) ? arenaBuyCnt : 0;
        ArenaBuyCfg arenaBuyCfg = ArenaBuyCfg.Get(todayBuyCnt);
        if (hero.GetInt(enProp.diamond) < arenaBuyCfg.price)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE.DIAMOND_INSUFFICIENT));
            return;
        }
        VipCfg vipCfg = VipCfg.Get(hero.GetInt(enProp.vipLv));
        if (todayBuyCnt >= vipCfg.arenaBuyNum)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.BUY_CHANCE_MAX_COUNT));
            return;
        }

        UIMessageBox.Open(string.Format(LanguageCfg.Get("buy_challenge_num"), arenaBuyCfg.price), () =>
        {
            NetMgr.instance.ActivityHandler.SendBuyArenaChance();
        }, () =>
        {
        });
    }

    private void OnRuleIntro()
    {
        Role hero = RoleMgr.instance.Hero;
        ActivityPart part = hero.ActivityPart;
        int score = part.GetInt(enActProp.arenaScore);
        int grade = ArenaGradeCfg.GetGrade(score);
        int maxScore = part.GetInt(enActProp.arenaMaxScore);
        int maxGrade = ArenaGradeCfg.GetGrade(maxScore);

        ArenaBasicCfg basicCfg = ArenaBasicCfg.Get();
        ArenaGradeCfg gradeCfg = ArenaGradeCfg.Get(grade);
        ArenaGradeCfg maxGradeCfg = ArenaGradeCfg.Get(maxGrade);
        maxGradeCfg = maxGradeCfg == null ? gradeCfg : maxGradeCfg;
        ArenaGradeCfg nextGradeCfg = ArenaGradeCfg.Get(grade + 1);

        string ruleInfo = basicCfg.ruleIntro;
        var rankVal = ActivityMgr.instance.GetMyArenaRankVal();
        var rankValStr = rankVal >= 0 ? (rankVal + 1).ToString() : "暂无";

        ruleInfo = ruleInfo.Replace("{curScore}", score.ToString());
        ruleInfo = ruleInfo.Replace("{curGrade}", gradeCfg.gradeName);

        ruleInfo = ruleInfo.Replace("{curRank}", rankValStr);
        ruleInfo = ruleInfo.Replace("{maxScore}", maxScore.ToString());
        ruleInfo = ruleInfo.Replace("{maxGrade}", maxGradeCfg.gradeName);

        List<RewardItem> curGradeRewards = RewardCfg.GetRewardsDefinite(gradeCfg.dayRewardId);
        string curGradeRewardsStr = "";
        for (int i = 0; i < curGradeRewards.Count; ++i)
        {
            curGradeRewardsStr += string.Format("<quad class=\"{0}\">{1} ", curGradeRewards[i].itemId, curGradeRewards[i].itemNum);
        }

        ruleInfo = ruleInfo.Replace("{1}", curGradeRewardsStr);
        if (nextGradeCfg == null)
        {
            ruleInfo = m_regExDummy.Replace(ruleInfo, "\n");
        }
        else
        {
            ruleInfo = ruleInfo.Replace("{nextGrade}", nextGradeCfg.gradeName);
            List<RewardItem> nextGradeRewards = RewardCfg.GetRewardsDefinite(nextGradeCfg.dayRewardId);
            List<RewardItem> upgradeRewards = RewardCfg.GetRewardsDefinite(nextGradeCfg.upgradeRewardId);
            string nextGradeRewardsStr = "";
            for (int i = 0; i < nextGradeRewards.Count; ++i)
            {
                nextGradeRewardsStr += string.Format("<quad class=\"{0}\">{1} ", nextGradeRewards[i].itemId, nextGradeRewards[i].itemNum);
            }
            ruleInfo = ruleInfo.Replace("{2}", nextGradeRewardsStr);

            string upgradeRewardsStr = "";
            for (int i = 0; i < upgradeRewards.Count; ++i)
            {
                upgradeRewardsStr += string.Format("<quad class=\"{0}\">{1} ", upgradeRewards[i].itemId, upgradeRewards[i].itemNum);
            }
            ruleInfo = ruleInfo.Replace("{3}", upgradeRewardsStr);
        }





        UIMgr.instance.Open<UIRuleIntro>().SetContent(ruleInfo);
    }

    private void RefreshUI()
    {
        Role hero = RoleMgr.instance.Hero;
        ActivityPart part = hero.ActivityPart;
        int score = part.GetInt(enActProp.arenaScore);
        int grade = ArenaGradeCfg.GetGrade(score);

        long curTime = TimeMgr.instance.GetTimestamp();
        long arenaTime = part.GetLong(enActProp.arenaTime);
        int arenaCnt = part.GetInt(enActProp.arenaCnt);
        long arenaBuyCntTime = part.GetLong(enActProp.arenaBuyCntTime);
        int arenaBuyCnt = part.GetInt(enActProp.arenaBuyCnt);

        ArenaBasicCfg basicCfg = ArenaBasicCfg.Get();
        ArenaGradeCfg gradeCfg = ArenaGradeCfg.Get(grade);
        ArenaGradeCfg nextGradeCfg = ArenaGradeCfg.Get(grade + 1);

        m_myScore.text = score.ToString();
        if (!isOnFirstUpgrade)
        {
            m_gradeNameImg.Set(gradeCfg.nameImg);
            m_gradeBigIcon.Set(gradeCfg.iconName);
        }
        m_arenaCoin.text = hero.GetInt(enProp.arenaCoin).ToString();

        long timePass = curTime >= arenaTime ? curTime - arenaTime : arenaTime - curTime;

        VipCfg vipCfg = VipCfg.Get(hero.GetInt(enProp.vipLv));
        if (timePass < vipCfg.arenaFreezeTime)
        {
            long coolDownLeft = vipCfg.arenaFreezeTime - timePass;
            m_leftCnt.text = "冷却时间：" + StringUtil.FormatTimeSpan(coolDownLeft);
            m_leftCntPlus.gameObject.SetActive(false);
        }
        else
        {
            int leftCnt = Math.Max(0, basicCfg.freeChance + (TimeMgr.instance.IsToday(arenaBuyCntTime) ? arenaBuyCnt : 0) - (TimeMgr.instance.IsToday(arenaTime) ? arenaCnt : 0));
            m_leftCnt.text = "剩余挑战次数：" + leftCnt;
            m_leftCntPlus.gameObject.SetActive(leftCnt <= 0);
        }

        /* if (m_rewardState.CurStateIdx == 1)
         {
             m_rewardGold.text = gradeCfg.weekRewardGold.ToString();
             m_rewardArenaCoin.text = gradeCfg.weekRewardArenaCoin.ToString();
             m_rewardDiamond.text = gradeCfg.weekRewardDiamond.ToString();
         }
         else
         {*/
        List<RewardItem> items = RewardCfg.GetRewardsDefinite(gradeCfg.dayRewardId);
        m_rewardGroup.SetCount(items.Count);
        for (int i = 0; i < m_rewardGroup.Count; ++i)
        {
            UITaskRewardItem item = m_rewardGroup.Get<UITaskRewardItem>(i);
            ItemCfg itemCfg = ItemCfg.m_cfgs[items[i].itemId];
            item.init(itemCfg.iconSmall, items[i].itemNum);
        }

        // }

        string pet1RoleId = null;

        PetFormation myPetFormation = hero.PetFormationsPart.GetCurPetFormation();
        Role pet1 = hero.PetsPart.GetPet(myPetFormation.GetPetGuid(enPetPos.pet1Main));


        if (pet1==null)
        {
            m_pet1None.gameObject.SetActive(true);
            m_pet1Icon.gameObject.SetActive(false);
            m_pet1Icon.Set(null);
        }
        else
        {
            m_pet1None.gameObject.SetActive(false);
            m_pet1Icon.gameObject.SetActive(true);
            m_pet1Icon.Set(pet1.Cfg.icon);
            pet1RoleId = pet1.GetString(enProp.roleId);
        }

        string pet2RoleId = null;
        Role pet2 = hero.PetsPart.GetPet(myPetFormation.GetPetGuid(enPetPos.pet2Main));
        if (pet2==null)
        {
            m_pet2None.gameObject.SetActive(true);
            m_pet2Icon.gameObject.SetActive(false);
            m_pet2Icon.Set(null);
        }
        else
        {
            m_pet2None.gameObject.SetActive(false);
            m_pet2Icon.gameObject.SetActive(true);
            m_pet2Icon.Set(pet2.Cfg.icon);
            pet2RoleId = pet2.GetString(enProp.roleId);
        }

        var uiItem = m_grid.Get<UIArenaItem>(m_myselfIndex);
        if (uiItem != null)
        {
            uiItem.UpdatePetIcon(pet1RoleId, pet2RoleId);
        }
    }


    public override void OnOpenPanel(object param)
    {     
        ArenaBasicCfg cfg = ArenaBasicCfg.Get();
        Role role = RoleMgr.instance.Hero;
        if (role.GetInt(enProp.level) < cfg.openLevel)
        {
            UIMessage.Show(string.Format("{0}级才可以开启这个功能", cfg.openLevel));
            this.Close(true);
            return;
        }

        m_inReqChallengers = true;
        NetMgr.instance.ActivityHandler.SendReqArenaChallengers();

        RefreshUI();

        GetComponent<ShowUpController>().Prepare();
    }

    public override void OnClosePanel()
    {
        fx1.SetActive(false);
        fx2.SetActive(false);
        isOnFirstUpgrade = false;
        CheckTip();
    }

    public override void OnUpdatePanel()
    {
        DateTime curTime = DateTime.Now;
        if ((curTime - m_lastRefresh).TotalSeconds >= 1)
        {
            m_lastRefresh = curTime;
            RefreshUI();
        }
    }

    public void OnUpdateChallengers(ReqArenaChallengersResultVo vo)
    {
        m_inReqChallengers = false;

        if (vo == null)
        {
            m_grid.SetCount(0);
            return;
        }

        Role myHero = RoleMgr.instance.Hero;
        int myHeroId = myHero.GetInt(enProp.heroId);
        string myHeroName = myHero.GetString(enProp.name);
        string myHeroRoleId = myHero.GetString(enProp.roleId);
        PetFormation petFormation = myHero.PetFormationsPart.GetPetFormation(enPetFormation.normal); // TODO 当可以配置arena阵型时请用enPetFormation.arena
        string myPet1Guid = petFormation.GetPetGuid(enPetPos.pet1Main);
        string myPet2Guid = petFormation.GetPetGuid(enPetPos.pet2Main);

        string myPet1RoleId = "";
        Role myPet1 = myHero.PetsPart.GetPet(myPet1Guid);
        if (myPet1 != null)
            myPet1RoleId = myPet1.GetString(enProp.roleId);
        string myPet2RoleId = "";
        Role myPet2 = myHero.PetsPart.GetPet(myPet2Guid);
        if (myPet2 != null)
            myPet2RoleId = myPet2.GetString(enProp.roleId);

        int myArenaScore = myHero.ActivityPart.GetInt(enActProp.arenaScore);

        m_grid.SetCount(vo.challengers.Count + 1);

        int myRank = vo.myRankVal + 1;

        m_myselfIndex = -1; //小于0表示未设置自己的项
        for (int i = 0, j = 0, len = vo.challengers.Count; i < len; ++i)
        {
            var item = vo.challengers[i];
            var rank = item.rank + 1;
            var info = item.info;
            var uiItem = m_grid.Get<UIArenaItem>(j++);
            if (myRank < rank && m_myselfIndex < 0)
            {
                uiItem.Init(myRank, myHeroName, myHeroId, myHeroRoleId, myPet1Guid, myPet1RoleId, myPet2Guid, myPet2RoleId, myArenaScore, true);
                m_myselfIndex = j - 1; //前面自加了，这里减一
                uiItem = m_grid.Get<UIArenaItem>(j++);
            }
            uiItem.Init(rank, info.name, info.key, info.roleId, info.pet1Guid, info.pet1RoleId, info.pet2Guid, info.pet2RoleId, info.score, false);
        }
        if (m_myselfIndex < 0)
        {
            var uiItem = m_grid.Get<UIArenaItem>(m_grid.Count - 1);
            uiItem.Init(myRank, myHeroName, myHeroId, myHeroRoleId, myPet1Guid, myPet1RoleId, myPet2Guid, myPet2RoleId, myArenaScore, true);
            m_myselfIndex = m_grid.Count - 1;
        }

        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_scroll, m_myselfIndex); });

        ProcessTeachUIObjParam();
    }

    void ProcessTeachUIObjParam()
    {
        if (!string.IsNullOrEmpty(m_teachFuncCmd))
        {
            var cmd = m_teachFuncCmd;
            m_teachFuncCmd = "";

            switch (cmd)
            {
                case "myCombatLog":
                    {
                        var item = m_grid.Get<UIArenaItem>(m_myselfIndex);
                        if (item == null)
                        {
                            Debuger.LogError("找不到自己主角的条目：" + cmd);
                            TeachMgr.instance.StartPlay(false);
                        }
                        else
                            TeachMgr.instance.PlayNextStepWithUIObjParam(item.btnCombatLog.transform as RectTransform);
                    }
                    break;
                case "findOpponent":
                    {
                        var item = m_grid.Get<UIArenaItem>(m_myselfIndex == 0 ? 1 : m_myselfIndex - 1);
                        if (item == null)
                        {
                            Debuger.LogError("找不到对手的条目：" + cmd);
                            TeachMgr.instance.StartPlay(false);
                        }
                        else
                            TeachMgr.instance.PlayNextStepWithUIObjParam(item.btnChallenge.transform as RectTransform);
                    }
                    break;
                default:
                    Debuger.LogError("无效TeachAction参数：" + cmd);
                    TeachMgr.instance.StartPlay(false);
                    break;
            }

        }
    }

    void OnTeachAction(string arg)
    {
        switch (arg)
        {
            case "myCombatLog":
            case "findOpponent":
                m_teachFuncCmd = arg;
                if (!m_inReqChallengers)
                {
                    m_inReqChallengers = true;
                    NetMgr.instance.ActivityHandler.SendReqArenaChallengers();
                }
                break;
            default:
                Debuger.LogError("无效TeachAction参数：" + arg);
                TeachMgr.instance.StartPlay(false);
                break;
        }
    }

    bool OnTeachCheck(string arg)
    {
        return true;
    }

    public void OnArenaUpGrade()
    {
        StartCoroutine(PlayFx());
    }

    IEnumerator PlayFx()
    {
        float delayTime = 0;
        List<ShowUpObject> obj = GetComponent<ShowUpController>().m_showObjs;
        for (int i = 0; i < obj.Count; ++i)
        {
            delayTime += obj[i].m_delay;
        }
        Role hero = RoleMgr.instance.Hero;
        ActivityPart part = hero.ActivityPart;
        int score = part.GetInt(enActProp.arenaScore);
        int grade = ArenaGradeCfg.GetGrade(score);

        ArenaGradeCfg gradeCfg = ArenaGradeCfg.Get(grade);
        ArenaGradeCfg lastGradeCfg = ArenaGradeCfg.Get(grade - 1);

        m_gradeNameImg.Set(lastGradeCfg.nameImg);
        m_gradeBigIcon.Set(lastGradeCfg.iconName);

        float curTime = Time.time;

        while(Time.time<curTime+ delayTime+1f)
        {
            yield return 0;
        }
        fx1.SetActive(true);

        while (Time.time < curTime + delayTime + 1.2f)
        {
            yield return 0;
        }
        m_gradeBigIcon.gameObject.SetActive(false);

        while (Time.time < curTime + delayTime + 2f)
        {
            yield return 0;
        }
        //fx1.SetActive(false);
        fx2.SetActive(true);

        while (Time.time < curTime + delayTime + 2.6f)
        {
            yield return 0;
        }
        m_gradeNameImg.Set(gradeCfg.nameImg);
        m_gradeBigIcon.Set(gradeCfg.iconName);
        m_gradeBigIcon.gameObject.SetActive(true);

        while (Time.time < curTime + delayTime + 3.5f)
        {
            yield return 0;
        }
        UIMgr.instance.Open<UIArenaUpgrade>();
        isOnFirstUpgrade=false;
    }

    public static bool CheckOpen()
    {
        if (LevelMgr.instance.PrevRoomId == ArenaBasicCfg.Get().roomId)
        {
            UIArenaUpgrade uiArenaUpgrade = UIMgr.instance.Get<UIArenaUpgrade>();
            if (uiArenaUpgrade.items.Count > 0)
            {
                UIMgr.instance.Get<UIArena>().isOnFirstUpgrade = true;
                UIMgr.instance.Open<UIArena>().OnArenaUpGrade();
            }
            else
            {
                UIMgr.instance.Open<UIArena>();
            }



            return true;
        }
        return false;
    }

    public void CheckTip()
    {
        Role hero = RoleMgr.instance.Hero;
        if (hero == null)
            return;

        ActivityPart part = hero.ActivityPart;
        long curTime = TimeMgr.instance.GetTimestamp();
        long arenaTime = part.GetLong(enActProp.arenaTime);
        int arenaCnt = part.GetInt(enActProp.arenaCnt);
        long arenaBuyCntTime = part.GetLong(enActProp.arenaBuyCntTime);
        int arenaBuyCnt = part.GetInt(enActProp.arenaBuyCnt);

        ArenaBasicCfg basicCfg = ArenaBasicCfg.Get();

        long timePass = curTime >= arenaTime ? curTime - arenaTime : arenaTime - curTime;
        VipCfg vipCfg = VipCfg.Get(hero.GetInt(enProp.vipLv));
        if (timePass < vipCfg.arenaFreezeTime)
        {
            //冷却中
            SystemMgr.instance.SetTip(enSystem.arena, false);
        }
        else
        {
            int leftCnt = Math.Max(0, basicCfg.freeChance + (TimeMgr.instance.IsToday(arenaBuyCntTime) ? arenaBuyCnt : 0) - (TimeMgr.instance.IsToday(arenaTime) ? arenaCnt : 0));
            SystemMgr.instance.SetTip(enSystem.arena, leftCnt > 0);
        }
    }

    override public void OnOpenPanelEnd()
    {
        GetComponent<ShowUpController>().Start();
    }
}