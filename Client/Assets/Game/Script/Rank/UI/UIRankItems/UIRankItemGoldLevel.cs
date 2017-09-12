using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIRankItemGoldLevel : UIScrollWrapItem
{
    public string prefixBigNum;
    public string prefixSmallNum;
    public UIArtFont rank;
    public ImageEx invalidRank;
    public TextEx roleName;
    public TextEx level;
    public TextEx power;
    public TextEx score;
    public ImageEx myself;

    private int m_heroId;
    private string m_heroName;

    public override void OnInitData(object data)
    {
        if (!(data is RankItemWrapper))
            return;

        var dataWrapper = (RankItemWrapper)data;
        var myRankData = (GoldLevelRankItem)dataWrapper.data;

        if (!rank.gameObject.activeSelf)
            rank.gameObject.SetActive(true);
        rank.m_prefix = dataWrapper.rank < 3 ? prefixBigNum : prefixSmallNum;
        rank.SetNum((dataWrapper.rank + 1).ToString());
        if (invalidRank != null && invalidRank.gameObject.activeSelf)
            invalidRank.gameObject.SetActive(false);

        roleName.text = myRankData.name;
        level.text = myRankData.level.ToString();
        power.text = myRankData.power.ToString();
        score.text = myRankData.score.ToString();
        myself.gameObject.SetActive(myRankData.key == RoleMgr.instance.Hero.GetInt(enProp.heroId));

        m_heroId = myRankData.key;
        m_heroName = myRankData.name;

        var stateHandle = GetComponent<StateHandle>();
        if (stateHandle != null)
        {
            stateHandle.AddClick(OnBtnClick);
        }
    }

    void OnBtnClick()
    {
        var hero = RoleMgr.instance.Hero;
        var myHeroId = hero.GetInt(enProp.heroId);
        if (myHeroId == m_heroId)
            return;

        UIHeroMenu.Show(m_heroName, m_heroId);
        //NetMgr.instance.RoleHandler.RequestHeroInfo(m_heroId);
    }
}