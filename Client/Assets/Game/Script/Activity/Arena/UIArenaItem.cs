using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIArenaItem : MonoBehaviour
{
    private int         heroId;
    private string      pet1Guid;
    private string      pet2Guid;

    public string       prefixBigNum;
    public string       prefixSmallNum;
    public UIArtFont    rankVal;
    public TextEx       heroName;
    public ImageEx      heroHead;
    public ImageEx      pet1Head;
    public ImageEx      pet2Head;
    public StateHandle  btnHeroHead;
    public StateHandle  btnPet1Head;
    public StateHandle  btnPet2Head;
    public TextEx       score;
    public StateHandle  btnChallenge;
    public StateHandle  btnCombatLog;    

    public void Init(int rankVal, string heroName, int heroId, string heroRoleId, string pet1Guid, string pet1RoleId, string pet2Guid, string pet2RoleId, int score, bool isMyHero)
    {    
        btnChallenge.AddClick(TryDoChallenge);
        btnCombatLog.AddClick(ReqArenaLog);
        btnHeroHead.AddClick(OnHeadClick);
        btnPet1Head.AddClick(OnHeadClick);
        btnPet2Head.AddClick(OnHeadClick);

        this.heroId = heroId;
        this.pet1Guid = pet1Guid;
        this.pet2Guid = pet2Guid;
        
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
        if (string.IsNullOrEmpty(pet1RoleId))
        {
            this.pet1Head.gameObject.SetActive(false);
            this.pet1Head.Set(null);
        }
        else
        {
            this.pet1Head.gameObject.SetActive(true);
            this.pet1Head.Set(RoleCfg.GetHeadIcon(pet1RoleId));
        }            
        if (string.IsNullOrEmpty(pet2RoleId))
        {
            this.pet2Head.gameObject.SetActive(false);
            this.pet2Head.Set(null);
        }            
        else
        {
            this.pet2Head.gameObject.SetActive(true);
            this.pet2Head.Set(RoleCfg.GetHeadIcon(pet2RoleId));
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

    public void UpdatePetIcon(string pet1RoleId, string pet2RoleId)
    {
        if (string.IsNullOrEmpty(pet1RoleId))
        {
            this.pet1Head.gameObject.SetActive(false);
            this.pet1Head.Set(null);
        }
        else
        {
            this.pet1Head.gameObject.SetActive(true);
            this.pet1Head.Set(RoleCfg.GetHeadIcon(pet1RoleId));
        }
        if (string.IsNullOrEmpty(pet2RoleId))
        {
            this.pet2Head.gameObject.SetActive(false);
            this.pet2Head.Set(null);
        }
        else
        {
            this.pet2Head.gameObject.SetActive(true);
            this.pet2Head.Set(RoleCfg.GetHeadIcon(pet2RoleId));
        }
    }
}