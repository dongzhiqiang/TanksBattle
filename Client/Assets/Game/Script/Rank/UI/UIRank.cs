using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIRank : UIPanel
{
    #region Const
    private const int PAGE_SIZE = 10;

    private static string[] m_rankTypes = new string[]
    {
        RankMgr.RANK_TYPE_FULL_POWER,
        RankMgr.RANK_TYPE_ALL_PET_POWER,
        RankMgr.RANK_TYPE_PET_POWER,
        RankMgr.RANK_TYPE_CORPS,
        RankMgr.RANK_TYPE_LEVEL_STAR,
        RankMgr.RANK_TYPE_ARENA,
        RankMgr.RANK_TYPE_PREDICTOR,
    };
    #endregion

    #region Fields
    public StateGroup m_category;
    public UIScrollWrap[] m_tables;

    public TextEx m_fullPowerMyRank;
    public TextEx m_fullPowerMyPower;
    public TextEx m_fullPowerMyLeftLike;

    public TextEx m_allPetPowerMyRank;
    public TextEx m_allPetPowerMyPower;

    public TextEx m_petPowerMyRank;
    public TextEx m_petPowerMyPower;
    public TextEx m_petPowerMyLeftLike;

    public TextEx m_arenaMyRank;
    public TextEx m_arenaMyGrade;
    public TextEx m_arenaMyScore;

    public TextEx m_levelStarMyRank;
    public TextEx m_levelStarMyStars;

    public TextEx m_corpsMyRank;
    public TextEx m_corpsMyCorpsLv;

    public TextEx m_predictorMyRank;
    public TextEx m_predictorMyMax;

    private HashSet<string> m_openFlags = new HashSet<string>();
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_category.AddSel(OnCategorySel);

        foreach (var table in m_tables)
        {
            table.OnUIItemsReachStart += OnScrollReachStart;
            table.OnUIItemsReachEnd += OnScrollReachEnd;
        }        
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        SwitchToRankType(param as string);
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        m_openFlags.Clear();
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods
    private int GetUIIndexFromRankType(string type)
    {
        return Array.IndexOf(m_rankTypes, type);
    }

    private string GetRankTypeFromUIIndex(int idx)
    {
        return idx < 0 || idx >= m_rankTypes.Length ? null : m_rankTypes[idx];
    }

    private void OnCategorySel(StateHandle handle, int idx)
    {
        var type = GetRankTypeFromUIIndex(idx);
        RefreshUI(type);

        NetMgr.instance.RankHandler.SendRequestRankData(type, 0, PAGE_SIZE, true);
    }

    private void RefreshUI(string type)
    {
        var uiIndex = GetUIIndexFromRankType(type);
        var dataList = RankMgr.instance.GetRankList(type);
        m_tables[uiIndex].SetDataList(dataList, !m_openFlags.Contains(type));

        m_openFlags.Add(type);

        RefreshMyInfo(type);
    }

    private void RefreshMyInfo(string type)
    {
        var rankVal = RankMgr.instance.GetRankDataRankVal(type);
        var rankStr = rankVal >= 0 ? (rankVal + 1).ToString() : "--";
        var hero = RoleMgr.instance.Hero;
        var basicCfg = RankBasicConfig.Get();

        var lastLikeTime = hero.GetLong(enProp.lastRankLike);
        var rankLikeLogStr = hero.GetString(enProp.rankLikeLog);
        var rankLikeLogObj = (Dictionary<string, List<string>>)null;

        try
        {
            if (TimeMgr.instance.IsToday(lastLikeTime))
                rankLikeLogObj = JsonMapper.ToObject<Dictionary<string, List<string>>>(rankLikeLogStr);
        }
        catch (Exception ex)
        {
        }

        switch (type)
        {
            case RankMgr.RANK_TYPE_FULL_POWER:
                {
                    var lastLikeChance = basicCfg.likeCntLimit;
                    var rankKeys = (List<string>)null;
                    if (rankLikeLogObj != null && rankLikeLogObj.TryGetValue(RankMgr.RANK_TYPE_FULL_POWER, out rankKeys))
                        lastLikeChance = Math.Max(0, lastLikeChance - rankKeys.Count);

                    var fullPower = hero.GetInt(enProp.power) + hero.GetInt(enProp.powerPets);
                    m_fullPowerMyRank.text = rankStr;
                    m_fullPowerMyPower.text = fullPower.ToString();
                    m_fullPowerMyLeftLike.text = lastLikeChance.ToString();
                }                
                break;
            case RankMgr.RANK_TYPE_ALL_PET_POWER:
                {
                    m_allPetPowerMyRank.text = rankStr;
                    m_allPetPowerMyPower.text = hero.GetInt(enProp.powerPets).ToString();
                }                
                break;
            case RankMgr.RANK_TYPE_PET_POWER:
                {
                    var lastLikeChance = basicCfg.likeCntLimit;
                    var rankKeys = (List<string>)null;
                    if (rankLikeLogObj != null && rankLikeLogObj.TryGetValue(RankMgr.RANK_TYPE_PET_POWER, out rankKeys))
                        lastLikeChance = Math.Max(0, lastLikeChance - rankKeys.Count);

                    var maxPowerPet = hero.PetsPart.GetPet(hero.GetString(enProp.maxPowerPet));
                    m_petPowerMyRank.text = rankStr;
                    m_petPowerMyPower.text = maxPowerPet == null ? "--" : maxPowerPet.GetInt(enProp.power).ToString();
                    m_petPowerMyLeftLike.text = lastLikeChance.ToString();
                }                
                break;
            case RankMgr.RANK_TYPE_ARENA:
                {
                    ActivityPart part = hero.ActivityPart;
                    var score = part.GetInt(enActProp.arenaScore);
                    var grade = ArenaGradeCfg.GetGrade(score);
                    var gradeCfg = ArenaGradeCfg.Get(grade);
                    m_arenaMyRank.text = rankStr;
                    m_arenaMyGrade.text = gradeCfg == null ? "--" : gradeCfg.gradeName;
                    m_arenaMyScore.text = score.ToString();
                }
                break;
            case RankMgr.RANK_TYPE_LEVEL_STAR:
                {
                    m_levelStarMyRank.text = rankStr;
                    m_levelStarMyStars.text = hero.LevelsPart.GetAllStars().ToString();
                }
                break;
            case RankMgr.RANK_TYPE_CORPS:
                {
                    m_corpsMyRank.text = rankStr;
                    m_corpsMyCorpsLv.text = hero.CorpsPart.corpsInfo.props.level.ToString();
                }
                break;
            case RankMgr.RANK_TYPE_PREDICTOR:
                {
                    m_predictorMyRank.text = rankStr;
                    m_predictorMyMax.text = hero.GetInt(enProp.towerLevel).ToString();
                }
                break;
        }
    }

    private void OnScrollReachStart(UIScrollWrap ui)
    {
        var idx = Array.IndexOf(m_tables, ui);
        var type = m_rankTypes[idx];
        NetMgr.instance.RankHandler.SendRequestRankData(type, 0, PAGE_SIZE, true);
    }

    private void OnScrollReachEnd(UIScrollWrap ui)
    {
        var idx = Array.IndexOf(m_tables, ui);
        var type = m_rankTypes[idx];
        NetMgr.instance.RankHandler.SendRequestRankData(type, ui.GetDataCount(), PAGE_SIZE, true);
    }
    #endregion

    public void OnUpdateData(string type)
    {
        RefreshUI(type);
    }

    public void SwitchToRankType(string rankType, bool openIfNot = false)
    {
        if (openIfNot && !IsOpen)
        {
            Open(rankType);
        }
        else
        {
            var rankIdx = string.IsNullOrEmpty(rankType) ? 0 : Mathf.Clamp(GetUIIndexFromRankType(rankType), 0, m_rankTypes.Length - 1);
            m_category.SetSel(rankIdx);
        }        
    }
}