using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIArenaItem : MonoBehaviour
{
    private int         heroId;
    
    public string       prefixBigNum;
    public string       prefixSmallNum;
    public UIArtFont    rankVal;
    public TextEx       heroName;
    public ImageEx      heroHead;
    public StateHandle  btnHeroHead;
    public TextEx       score;
    public StateHandle  btnChallenge;
    public StateHandle  btnCombatLog;    

    public void Init(int rankVal, string heroName, int heroId, string heroRoleId, int score, bool isMyHero)
    {    
        btnChallenge.AddClick(TryDoChallenge);
        btnCombatLog.AddClick(ReqArenaLog);
        btnHeroHead.AddClick(OnHeadClick);
        
        this.heroId = heroId;
        
        this.rankVal.gameObject.SetActive(true);
        this.rankVal.m_prefix = rankVal <= 3 ? prefixBigNum : prefixSmallNum;
        this.rankVal.SetNum(rankVal.ToString());

        this.heroName.text = heroName;

        if (string.IsNullOrEmpty(heroRoleId))
        {
            this.heroHead.gameObject.SetActive(false);
            this.heroHead.Set(null);
        }            
        else
        {
            this.heroHead.gameObject.SetActive(true);
            this.heroHead.Set(RoleCfg.GetHeadIcon(heroRoleId));
        }            
        
        this.score.text = score.ToString();

        this.GetComponent<StateHandle>().SetState(isMyHero ? 0 : 1);
    }

    private void TryDoChallenge()
    {        
        NetMgr.instance.ActivityHandler.SendReqGetArenaPos(heroId);
         
    }

    private void ReqArenaLog()
    {
        UIMgr.instance.Open<UICombatLog>(this.heroId);
        NetMgr.instance.ActivityHandler.SendReqArenaLog();
    }

    private void OnHeadClick()
    {
        NetMgr.instance.RoleHandler.RequestHeroInfo(heroId);
    }

    public int GetHeroId()
    {
        return heroId;
    }
    
}