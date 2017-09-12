using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class EliteLevel
{
    #region Fields
    public int levelId;
    public bool passed;
    public Dictionary<string, int> starsInfo = new Dictionary<string, int>();
    public int enterTime;
    public int count;
    public int resetCount;
    public bool firstRewarded;
    #endregion

    public int GetStarState(string starId)
    {
        int value;
        if (starsInfo.TryGetValue(starId, out value))
        {
            return value;
        }
        else
        {
            return 0;
        }
    }

    public void CheckDay()
    {
        if(!TimeMgr.instance.IsSameDay(TimeMgr.instance.GetTimestamp(), enterTime))
        {
            resetCount = 0;
            count = 0;
        }
    }

    public static EliteLevel Create(EliteLevelVo vo)
    {
        EliteLevel eliteLevel;
        eliteLevel = new EliteLevel();
        eliteLevel.LoadFromVo(vo);
        return eliteLevel;
    }

    virtual public void LoadFromVo(EliteLevelVo vo)
    {
        levelId = vo.levelId;
        passed = vo.passed;
        starsInfo = vo.starsInfo;
        enterTime = vo.enterTime;
        count = vo.count;
        resetCount = vo.resetCount;
        firstRewarded = vo.firstRewarded;
    }

    public static bool CanOpen(EliteLevelCfg eliteLevelCfg)
    {
        Role role = RoleMgr.instance.Hero;
        if (!string.IsNullOrEmpty(eliteLevelCfg.openPassLvl))
        {
            var level = role.LevelsPart.GetLevelInfoById(eliteLevelCfg.openPassLvl);
            if (level == null || !level.isWin)
            {
                return false;
            }
        }
        if (eliteLevelCfg.openPassEltLvl != 0)
        {
            var eliteLevel = role.EliteLevelsPart.GetEliteLevel(eliteLevelCfg.openPassEltLvl);
            if (eliteLevel == null || !eliteLevel.passed)
            {
                return false;
            }
        }
        return true;
    }

    public int GetStars()
    {
        int totalNum = 0;
        foreach (int num in starsInfo.Values)
        {
            totalNum += num;
        }
        return totalNum;
    }
}