using UnityEngine;
using System.Collections;
using System;
using LitJson;
using System.Collections.Generic;

public class UIRankItemPetPower : UIScrollWrapItem
{
    public UIArtFont m_rank;
    public string m_rankPrefix1;
    public string m_rankPrefix2;
    public TextEx m_petName;
    public TextEx m_heroName;
    public TextEx m_power;
    public StateHandle m_like;
    public TextEx m_likeNum;
    public StateHandle m_bgHandle;
    public ImageEx m_redBg;    

    private int m_heroId;
    private string m_petGuid;

    public override void OnInitData(object data)
    {
        if (!(data is RankItemWrapper))
            return;

        var dataWrapper = (RankItemWrapper)data;
        var myRankData = (PetPowerRankItem)dataWrapper.data;
        var basicCfg = RankBasicConfig.Get();
        var hero = RoleMgr.instance.Hero;

        m_rank.m_prefix = dataWrapper.rank < 3 ? m_rankPrefix1 : m_rankPrefix2;
        m_rank.SetNum((dataWrapper.rank + 1).ToString());
        m_petName.text = myRankData.petName;
        m_heroName.text = myRankData.heroName.ToString();
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
                if (rankLikeLogObj != null && rankLikeLogObj.TryGetValue(RankMgr.RANK_TYPE_PET_POWER, out rankKeys))
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

        m_petGuid = myRankData.key;
        m_heroId = myRankData.heroId;        

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
        NetMgr.instance.RankHandler.SendReqDoLikeRankItem(RankMgr.RANK_TYPE_PET_POWER, m_petGuid);
    }

    private void OnClickBg()
    {
        var hero = RoleMgr.instance.Hero;
        var myHeroId = hero.GetInt(enProp.heroId);
        if (myHeroId == m_heroId)
        {            
            Role pet = hero.PetsPart.GetPet(m_petGuid);
            if (pet != null)
                UIMgr.instance.Open<UIPet>(pet);
            return;
        }

        NetMgr.instance.RoleHandler.RequestPetInfo(m_heroId, m_petGuid);
    }
}
