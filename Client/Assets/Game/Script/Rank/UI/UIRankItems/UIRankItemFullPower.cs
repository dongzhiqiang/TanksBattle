using UnityEngine;
using System.Collections;
using LitJson;
using System.Collections.Generic;
using System;

public class UIRankItemFullPower : UIScrollWrapItem
{
    public UIArtFont m_rank;
    public string m_rankPrefix1;
    public string m_rankPrefix2;
    public TextEx m_name;
    public TextEx m_level;
    public TextEx m_power;
    public StateHandle m_like;
    public TextEx m_likeNum;
    public StateHandle m_bgHandle;
    public ImageEx m_redBg;    

    private int m_heroId;
    private string m_heroName;

    public override void OnInitData(object data)
    {
        if (!(data is RankItemWrapper))
            return;

        var dataWrapper = (RankItemWrapper)data;
        var myRankData = (FullPowerRankItem)dataWrapper.data;
        var basicCfg = RankBasicConfig.Get();
        var hero = RoleMgr.instance.Hero;

        m_rank.m_prefix = dataWrapper.rank < 3 ? m_rankPrefix1 : m_rankPrefix2;
        m_rank.SetNum((dataWrapper.rank + 1).ToString());
        m_name.text = myRankData.name;
        m_level.text = myRankData.level.ToString();
        m_power.text = myRankData.power.ToString();

        var lastLikeTime = hero.GetLong(enProp.lastRankLike);
        var rankLikeLogStr = hero.GetString(enProp.rankLikeLog);
        var rankLikeLogObj = (Dictionary<string, List<string>>)null;
        var thisAlreadyLike = false;
        var allAlreadyLike = false;

        try
        {
            if (TimeMgr.instance.IsToday(lastLikeTime))
            {
                rankLikeLogObj = JsonMapper.ToObject<Dictionary<string, List<string>>>(rankLikeLogStr);
                var rankKeys = (List<string>)null;
                if (rankLikeLogObj != null && rankLikeLogObj.TryGetValue(RankMgr.RANK_TYPE_FULL_POWER, out rankKeys))
                {
                    if (rankKeys.IndexOf(myRankData.key.ToString()) >= 0)
                        thisAlreadyLike = true;
                    if (rankKeys.Count >= basicCfg.likeCntLimit)
                        allAlreadyLike = true;
                }
            }
        }
        catch (Exception ex)
        {
        }

        if (dataWrapper.rank >= basicCfg.likeRankLimit)
        {
            if (m_like.gameObject.activeSelf)
                m_like.gameObject.SetActive(false);
            m_likeNum.text = "";            
        }
        else
        {
            if (!m_like.gameObject.activeSelf)
                m_like.gameObject.SetActive(true);
            m_like.enabled = !thisAlreadyLike && !allAlreadyLike;
            m_like.GetComponent<ImageEx>().SetGrey(!m_like.enabled);
            m_likeNum.text = myRankData.like > 999 ? "999+" : myRankData.like.ToString();
            m_like.AddClick(OnClickLike);
        }

        m_heroId = myRankData.key;
        m_heroName = myRankData.name;

        if (m_bgHandle)
            m_bgHandle.AddClick(OnClickBg);

        var myHeroId = hero.GetInt(enProp.heroId);
        if (myHeroId == m_heroId)
        {
            if (!m_redBg.gameObject.activeSelf)
                m_redBg.gameObject.SetActive(true);
        }
        else
        {
            if (m_redBg.gameObject.activeSelf)
                m_redBg.gameObject.SetActive(false);
        }
    }

    private void OnClickLike()
    {
        NetMgr.instance.RankHandler.SendReqDoLikeRankItem(RankMgr.RANK_TYPE_FULL_POWER, m_heroId.ToString());
    }

    private void OnClickBg()
    {
        var hero = RoleMgr.instance.Hero;
        var myHeroId = hero.GetInt(enProp.heroId);
        if (myHeroId == m_heroId)
            return;

        UIHeroMenu.Show(m_heroName, m_heroId);
        //NetMgr.instance.RoleHandler.RequestHeroInfo(m_heroId);
    }
}