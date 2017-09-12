using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIArenaPos : UIPanel
{

    #region SerializeFields
    public TextEx heroName;
    public TextEx enemyName;
    public List<UIArenaPosItem> heroPos;
    public List<UIArenaPosItem> enemyPos;
    private Role[] myRoles = new Role[3];
    private Role[] itsRoles = new Role[3];
    public StateHandle battleBtn;
    public List<int> enemyPosList;
    List<int> defaultPos = new List<int>();  //默认位置,主角是0，宠物1是1，宠物2是2
    Role role;

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        defaultPos.Add(1);
        defaultPos.Add(0);
        defaultPos.Add(2);
        battleBtn.AddClick(OnChallenge);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        ClearEnemyRoles();
        ClearHeroRoles();
        Role enemy;
        FullRoleInfoVo roleVo = (FullRoleInfoVo)param;
        RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
        cxt.OnClear();
        cxt.roleId = roleVo.props["roleId"].String;
        try
        {
            enemy = RoleMgr.instance.CreateNetRole(roleVo, true, cxt);
            role = enemy;
        }
        catch (Exception)
        {
            return;
        }       

        Role hero = RoleMgr.instance.Hero;
        heroName.text = hero.GetString(enProp.name);
        enemyName.text = enemy.GetString(enProp.name);
        RefreshHeroArenaPos();
        RefreshEnemyArenaPos();
      
    }
    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        if(role!= null)
        {
            RoleMgr.instance.DestroyRole(role);
            role = null;
        }
            
        ClearEnemyRoles();
        ClearHeroRoles();
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods
    
    
    void OnChallenge()
    {
        Role hero = RoleMgr.instance.Hero;
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
            UIMessage.Show(LanguageCfg.Get("challenge_in_cool_down"));
            return;
        }

        int leftCnt = Math.Max(0, basicCfg.freeChance + (TimeMgr.instance.IsToday(arenaBuyCntTime) ? arenaBuyCnt : 0) - (TimeMgr.instance.IsToday(arenaTime) ? arenaCnt : 0));
        if (leftCnt <= 0)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.DAY_MAX_CNT));
            return;
        }

        NetMgr.instance.ActivityHandler.SendReqStartChallenge(role.GetInt(enProp.heroId));
    }

    #endregion

    public void  ClearHeroRoles()
    {
        for(int i=0;i<myRoles.Length;++i)
        {
            myRoles[i] = null;
        }
    }

    public void ClearEnemyRoles()
    {
        for (int i = 0; i < itsRoles.Length; ++i)
        {
            itsRoles[i] = null;
        }
    }

    public void RefreshHeroArenaPos()
    {       
        Role hero = RoleMgr.instance.Hero;     
        string heroArenaPosStr = hero.ActivityPart.GetString(enActProp.arenaPos);  
        List<int> heroArenaPos = heroArenaPosStr == "" ? defaultPos : ArenaBasicCfg.GetArenaPos(heroArenaPosStr);
        PetFormation myPetFormation = hero.PetFormationsPart.GetCurPetFormation();
        Role heroPet1 = hero.PetsPart.GetPet(myPetFormation.GetPetGuid(enPetPos.pet1Main));
        Role heroPet2 = hero.PetsPart.GetPet(myPetFormation.GetPetGuid(enPetPos.pet2Main));
        myRoles[0] = hero;
        myRoles[1] = heroPet1;
        myRoles[2] = heroPet2;
        for(int i=0;i<heroArenaPos.Count;++i)
        {
            heroPos[i].Init(myRoles[heroArenaPos[i]], heroArenaPos[i]);
        }
        ClearHeroRoles();
        
    }
    public void RefreshEnemyArenaPos()
    {        
        Role enemy = role;       
        string enemyArenaPosStr = enemy.ActivityPart.GetString(enActProp.arenaPos);       
        List<int> enemyArenaPos = enemyArenaPosStr == "" ? defaultPos : ArenaBasicCfg.GetArenaPos(enemyArenaPosStr);
        enemyPosList = enemyArenaPos;
        Role enemyPet1 = null;
        Role enemyPet2 = null;
        List<Role> enemyPets = enemy.PetsPart.GetMainPets();
        PetFormation enemyPetFormation = enemy.PetFormationsPart.GetCurPetFormation();
        for (int i = 0; i < enemyPets.Count; ++i)
        {
            if (enemyPets[i].GetString(enProp.guid) == enemyPetFormation.GetPetGuid(enPetPos.pet1Main))
                enemyPet1 = enemyPets[i];
            if (enemyPets[i].GetString(enProp.guid) == enemyPetFormation.GetPetGuid(enPetPos.pet2Main))
                enemyPet2 = enemyPets[i];
        }

        itsRoles[0] = enemy;
        itsRoles[1] = enemyPet1;
        itsRoles[2] = enemyPet2;

        for (int i = 0; i < enemyArenaPos.Count; ++i)
        {
            enemyPos[i].Init(itsRoles[enemyArenaPos[i]], enemyArenaPos[i]);
        }
        ClearEnemyRoles();



    }
}



