using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ArenaScene : LevelBase
{
    enum ArenaSceneState
    {
        none,
        weWin,
        theyWin,
    }

    public List<DamageDataParamItem> m_leftDmgParams = new List<DamageDataParamItem>();
    public List<DamageDataParamItem> m_rightDmgParams = new List<DamageDataParamItem>();   
    private UILevel m_uiLevel = null;
    private UILevelAreaTime m_uiTime = null;
    private UILevelAreaArena m_uiArenaArea = null;
    private UICombatBegin m_uiBegin = null;
    private RoleBornCxt m_enemyBornCxt = null;
    private Role m_enemyHero = null;
    private int m_enemyHeroId ;

    private int m_friendRoleCount = 0;
    private int m_enemyRoleCount = 0;
    private ArenaSceneState m_curState = ArenaSceneState.none;

    public override IEnumerator OnLoad()
    {
        //获取数据
        ArenaBasicCfg cfg = ArenaBasicCfg.Get();
        var roleVo = (FullRoleInfoVo)this.mParam;

        //预加载状态
        BuffCfg.ProLoad(cfg.heroBuffId);
        BuffCfg.ProLoad(cfg.enemyBuffId);      

        //预加载对方主角
        m_enemyBornCxt = IdTypePool<RoleBornCxt>.Get();
        m_enemyBornCxt.OnClear();
        var enemyHeroRoleId = roleVo.props["roleId"].String;
        RoleCfg enemyHeroRoleCfg = RoleCfg.Get(enemyHeroRoleId);
        m_enemyBornCxt.Init(
            roleVo.props["guid"].String,
            roleVo.props["name"].String, 
            enemyHeroRoleId, 
            roleVo.props["level"].Int, 
            enCamp.camp2, 
            new Vector3(cfg.itsPos2[0], 0, cfg.itsPos2[1]), 
            Vector3.zero,
            cfg.itsHeroBornType, 
            cfg.itsHeroDeadType);

        m_enemyHero = RoleMgr.instance.CreateNetRole(roleVo, true, m_enemyBornCxt);
        m_enemyHero.RuntimeShieldBuff = cfg.heroShieldBuff; //提前设置气力状态，下面就可以预加载了
        m_enemyHero.PreLoad(); //预加载，宠物也会在这里预加载
        m_enemyHeroId = m_enemyHero.Id;
        //设置竞技场血条标记
       /* m_enemyHero.SetFlag(GlobalConst.FLAG_ARENA_BLOOD, 1);
        List<Role> enemyPets = m_enemyHero.PetsPart.GetMainPets();
        for(int i=0;i<enemyPets.Count;++i)
        {
            enemyPets[i].SetFlag(GlobalConst.FLAG_ARENA_BLOOD, 2);
        }*/


        //给自己的主角设置气力状态
        var myHero = RoleMgr.instance.Hero;        
        myHero.RuntimeShieldBuff = cfg.heroShieldBuff;
        yield return 0;
    }

    public override void OnLoadFinish() 
    {       
        m_friendRoleCount = 0;
        m_enemyRoleCount = 0;
        m_curState = ArenaSceneState.none;

        ArenaBasicCfg cfg = ArenaBasicCfg.Get();

        m_uiLevel = UIMgr.instance.Open<UILevel>();
        m_uiLevel.Close<UILevelAreaBossHead>();
        m_uiLevel.Close<UILevelAreaHead>();
        m_uiLevel.Close<UILevelAreaJoystick>(); //不要技能和摇杆，自动战斗
        m_uiLevel.Get<UILevelAreaSetting>().SetGuaJiButtonVisible(false);

        m_uiTime = m_uiLevel.Open<UILevelAreaTime>();
        m_uiTime.SetTime(cfg.limitTime);
        m_uiTime.OnPause(true);

        m_uiArenaArea = m_uiLevel.Open<UILevelAreaArena>();

        Role hero = RoleMgr.instance.Hero;
        var roleVo = (FullRoleInfoVo)this.mParam;
        var beginParams = new UICombatBegin.CombatBeginParam();
        beginParams.roleIdLeft = hero.GetString(enProp.roleId);
        beginParams.roleIdRight = roleVo.props.ContainsKey("roleId") ? roleVo.props["roleId"].String : "";

        beginParams.roleNameLeft = hero.GetString(enProp.name);
        beginParams.roleNameRight = roleVo.props.ContainsKey("name") ? roleVo.props["name"].String : "";

        beginParams.rolePowerLeft = hero.GetInt(enProp.powerTotal);
        beginParams.rolePowerRight = roleVo.props.ContainsKey("power") ? roleVo.props["power"].Int : 0;

        beginParams.pet1RoleIdLeft = hero.GetString(enProp.pet1MRId);
        beginParams.pet1RoleIdRight = roleVo.props.ContainsKey("pet1MRId") ? roleVo.props["pet1MRId"].String : "";

        beginParams.pet2RoleIdLeft = hero.GetString(enProp.pet2MRId);
        beginParams.pet2RoleIdRight = roleVo.props.ContainsKey("pet2MRId") ? roleVo.props["pet2MRId"].String : "";
        m_uiBegin = UIMgr.instance.Open<UICombatBegin>(beginParams);

        Room.instance.StartCoroutine(CoStartScene());
    }
    //创建全局敌人的时候，返回全局敌人的阵营，如果不希望创建可以返回enCamp.max
    public override enCamp OnCreateGlobalEnemy() { return enCamp.max; }// return enCamp.camp3; }

    //用于修正英雄和宠物血量的等级值，一般取主角等级，竞技场等活动中应该取双方角色的均值
    public override int GetHpRateLv() {
        var roleVo = (FullRoleInfoVo)this.mParam;
        return (RoleMgr.instance.Hero.GetInt(enProp.level)+ roleVo.props["level"].Int)/2;
    }

    private IEnumerator CoStartScene()
    {
        //等战斗开始界面关闭后才可以创建其它怪
        while (m_uiBegin.IsOpen)
            yield return 0;

        SceneEventMgr.instance.FireAction("begin");
        SceneEventMgr.instance.FireAction("sparta");
    }

    private IEnumerator CoLoadRoles()
    {
        TimeMgr.instance.AddPause();

        ArenaBasicCfg basicCfg = ArenaBasicCfg.Get();

        m_enemyHero.Load(m_enemyBornCxt);
        m_enemyBornCxt = null; //表明已用它了，这里不用Put回池里了
        while (m_enemyHero.State != Role.enState.alive)
            yield return 0;

        mRoleDic.Add(m_enemyHero.Id, m_enemyHero);
        OnRoleEnter(m_enemyHero);
        EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.ROLEENTER, m_enemyHero);

        Role hero = RoleMgr.instance.Hero;
        Vector3 enemyTofrientForward = hero.transform.position - m_enemyHero.transform.position;
        enemyTofrientForward.y = 0;

        m_enemyHero.transform.forward = enemyTofrientForward;
        hero.transform.forward = -enemyTofrientForward;
        
        ++m_friendRoleCount;
        hero.Add(MSG_ROLE.DEAD, OnFriendRoleDead);
        hero.RoleBornCxt.deadAniId = basicCfg.myHeroDeadType;
        hero.RoleBornCxt.groundDeadAniId = basicCfg.myHeroDeadType;
        m_uiArenaArea.AddMyPartRole(hero);
        m_leftDmgParams.Add(new DamageDataParamItem(hero.Id, hero.GetString(enProp.name), hero.GetString(enProp.roleId)));
        //设置仇恨目标
        hero.HatePart.AddHate(m_enemyHero, basicCfg.addHateValue);

        string heroArenaPosStr = hero.ActivityPart.GetString(enActProp.arenaPos);
        List<int> heroArenaPos = heroArenaPosStr == "" ? ArenaBasicCfg.GetArenaPos("1,0,2") : ArenaBasicCfg.GetArenaPos(heroArenaPosStr);
        string enemyArenaPosStr = m_enemyHero.ActivityPart.GetString(enActProp.arenaPos);
        List<int> enemyArenaPos = enemyArenaPosStr == "" ? ArenaBasicCfg.GetArenaPos("1,0,2") : ArenaBasicCfg.GetArenaPos(enemyArenaPosStr);
        
        //设置主角位置
        for (int i = 0; i < heroArenaPos.Count; ++i)
        {
            if (heroArenaPos[i] == 0)
            {
                switch (i)
                {
                    case 0:
                        hero.TranPart.SetPos(new Vector3(basicCfg.myPos1[0], hero.TranPart.Pos.y, basicCfg.myPos1[1]));
                        break;
                    case 1:
                        hero.TranPart.SetPos(new Vector3(basicCfg.myPos2[0], hero.TranPart.Pos.y, basicCfg.myPos1[1]));
                        break;
                    case 2:
                        hero.TranPart.SetPos(new Vector3(basicCfg.myPos3[0], hero.TranPart.Pos.y, basicCfg.myPos1[1]));
                        break;
                }               
            }
        }
        ++m_enemyRoleCount;
        m_enemyHero.Add(MSG_ROLE.DEAD, OnEnemyRoleDead);
        //对方主角的死亡方式已在预加载时修改了
        m_uiArenaArea.AddItsPartRole(m_enemyHero);
        m_rightDmgParams.Add(new DamageDataParamItem(m_enemyHero.Id, m_enemyHero.GetString(enProp.name), m_enemyHero.GetString(enProp.roleId)));
        //设置仇恨目标
        m_enemyHero.HatePart.AddHate(hero, basicCfg.addHateValue);


        //设置敌方主角位置
        for (int i = 0; i < enemyArenaPos.Count; ++i)
        {
            if (enemyArenaPos[i] == 0)
            {
                switch (i)
                {
                    case 0:
                        m_enemyHero.TranPart.SetPos(new Vector3(basicCfg.itsPos1[0], m_enemyHero.TranPart.Pos.y, basicCfg.itsPos1[1]));
                        break;
                    case 1:
                        m_enemyHero.TranPart.SetPos(new Vector3(basicCfg.itsPos2[0], m_enemyHero.TranPart.Pos.y, basicCfg.itsPos2[1]));
                        break;
                    case 2:
                        m_enemyHero.TranPart.SetPos(new Vector3(basicCfg.itsPos3[0], m_enemyHero.TranPart.Pos.y, basicCfg.itsPos3[1]));
                        break;
                }
            }
        }
        //加上状态

        if (basicCfg.heroBuffId != 0)
        {
            hero.BuffPart.AddBuff(basicCfg.heroBuffId);
        }
        if (basicCfg.enemyBuffId != 0)
        {
            m_enemyHero.BuffPart.AddBuff(basicCfg.enemyBuffId);
        }
        m_uiArenaArea.InitAllRolesGroup();
        m_uiArenaArea.RefreshUI();            
        m_uiTime.OnPause(false);

        TimeMgr.instance.SubPause();

        //开启自动战斗
        if (hero.AIPart != null)
            hero.AIPart.Play(AIPart.HeroAI);


        
    }
    

    public override void OnHeroEnter(Role hero) 
    {
        Room.instance.StartCoroutine(CoLoadRoles());
    }

    public override void OnRoleEnter(Role role) 
    {
        if (!role.IsNetRole)
        {
            Role roleMaster;
            var camp = role.GetCamp();
            if (camp == enCamp.camp1)
            {
                roleMaster = RoleMgr.instance.Hero;
            }
            else if (camp == enCamp.camp2)
            {
                roleMaster = m_enemyHero;
            }
            else
            {
                return;
            }

            if (roleMaster == null)
            {
                Debuger.LogError("斯巴达战士主角不存在");
                return;
            }

            ArenaBasicCfg cfg = ArenaBasicCfg.Get();
            PropertyTable propValues = new PropertyTable();
            PropertyTable.Mul(roleMaster.PropPart.Values, PropRateCfg.Get(cfg.spartaPropRateId).props, propValues);
            role.PropPart.SetBaseProps(propValues, roleMaster.PropPart.Rates);
            role.SetInt(enProp.level, roleMaster.GetInt(enProp.level));
        }
    }

    public override void OnTimeout(int time) {
        if (m_curState == ArenaSceneState.none)
        {
            m_curState = ArenaSceneState.theyWin;
            OnCombatEnd(m_curState == ArenaSceneState.weWin, null);
        }
    }

    public override void OnExit() 
    {
        //TimeMgr.instance.SubPause();

        if (m_enemyHero != null && !m_enemyHero.IsDestroy(m_enemyHeroId))
        {
            RoleMgr.instance.DestroyRole(m_enemyHero, false);
            m_enemyHero = null;
            m_enemyHeroId = 0;
        }

        if (m_enemyBornCxt != null)
            m_enemyBornCxt.Put();
    }

    public override void OnUpdate() {}

    public override void SendResult(bool isWin) 
    {
    }

    private void OnEnemyRoleDead(object p, object p2, object p3, EventObserver ob)
    {
        var role = ob.GetParent<Role>();
        --m_enemyRoleCount;
        if (m_enemyRoleCount <= 0 && m_curState == ArenaSceneState.none)
        {            
            m_curState = ArenaSceneState.weWin;
            OnCombatEnd(m_curState == ArenaSceneState.weWin, role);
        }
    }

    private void OnFriendRoleDead(object p, object p2, object p3, EventObserver ob)
    {
        var role = ob.GetParent<Role>();
        --m_friendRoleCount;
        if (m_friendRoleCount <= 0 && m_curState == ArenaSceneState.none)
        {
            m_curState = ArenaSceneState.theyWin;
            OnCombatEnd(m_curState == ArenaSceneState.weWin, role);
        }
    }

    private void OnCombatEnd(bool weWin, Role lastDieRole)
    {
        m_uiTime.OnPause(true);
        TimeMgr.instance.AddPause();

        if (lastDieRole != null)
        {
            const float moveTime = 3.0f;
            const float stayTime = 3.0f;
            CameraMgr.instance.Still(lastDieRole.transform.position, lastDieRole.transform.forward, Vector3.zero, moveTime, stayTime, -1, 180, 30, 3.0f);
            TimeMgr.instance.AddTimer(moveTime, () =>
            {                
                NetMgr.instance.ActivityHandler.SendReqEndArenaCombat(weWin);
            });
        }
        else
        {
            NetMgr.instance.ActivityHandler.SendReqEndArenaCombat(weWin);
        }

        //////////这些语句放下面，因为如果lastDieRole是宠物，会把transform清空//////////
        var myHero = RoleMgr.instance.Hero;
        var myCamp = myHero.GetCamp();
        var itsHero = m_enemyHero;
        //var itsCamp = m_enemyHero.GetCamp();

        //杀死已我两方的小怪
        List<Role> rs = new List<Role>(RoleMgr.instance.Roles);
        foreach (var r in rs)
        {
            
            //主角不删除、全局敌人不删
            if (r == myHero || r == itsHero|| r == RoleMgr.instance.GlobalEnemy)
                continue;

            RoleMgr.instance.DestroyRole(r);
        }
    }
}