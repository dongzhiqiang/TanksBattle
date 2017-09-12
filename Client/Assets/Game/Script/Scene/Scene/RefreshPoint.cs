using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RefreshPoint
{
    public string mGroupFlag;   //刷新组标记
    public int mRefreshNum;  //总共要刷的波数
    public bool isFinish = false;   //标记是否刷新完
    public int isNpcAlive = 0;     //当前刷新点的npc是否活着  0死了 1活着
    public RefreshBase groupRefresh;    //保存一下刷新组对象的引用 方便设置波数
    public Dictionary<int, Role> roleDic = new Dictionary<int, Role>();

    int curRefreshNum = 0; //当前的刷怪波数
    float startTime;
    float delayTime;

    Dictionary<string, int> saveNpcData = new Dictionary<string, int>();

    public SceneCfg.RefPointCfg mPointCfg;

    public int CurRefreshNum { get { return curRefreshNum; } }
    public RefreshPoint() { }

    public RefreshPoint(SceneCfg.RefPointCfg pointCfg)
    {
        mPointCfg = pointCfg;
    }

    public void Init(float start)
    {
        startTime = start;
        curRefreshNum = 0;
        isFinish = false;
        roleDic.Clear();
    }

    public void Update()
    {
        if (!isFinish && groupRefresh.mState == RefreshBase.RefreshState.RUN)
        {
            //多波怪设为刷新点死后再刷另一个 一个刷新点只会同时存在一个怪
            if (isNpcAlive == 0)
            {
                delayTime = TimeMgr.instance.logicTime - startTime;
                if (delayTime >= mPointCfg.bornDelay + groupRefresh.mGroupCfg.delayTime)
                {
                    curRefreshNum++;
                    if (curRefreshNum >= mRefreshNum)
                    {
                        isFinish = true;
                    }

                    if (groupRefresh.mGroupCfg.refreshType == SceneCfg.RefreshType.SameTime)
                    {
                        //更新最大刷新波数
                        if (groupRefresh.waveNum < curRefreshNum)
                            groupRefresh.waveNum = curRefreshNum;
                    }

                    CreateNpc();
                }
            }

        }

    }

    public Role CreateNpc(bool isHaveEffect = true)
    {
        RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();


        //关卡里没有配置出生死亡则取表里默认的配置
        RoleCfg cfg = RoleCfg.Get(mPointCfg.roleId);
        if (isHaveEffect)
        {
            if (mPointCfg.bornTypeId == "")
            {
                cxt.bornAniId = cfg.bornType;
            }
            else
                cxt.bornAniId = mPointCfg.bornTypeId;
        }
        else
            cxt.bornAniId = "";

        if (string.IsNullOrEmpty(mPointCfg.deadTypeId))
            cxt.deadAniId = cfg.deadType;
        else
            cxt.deadAniId = mPointCfg.deadTypeId;

        if (string.IsNullOrEmpty(mPointCfg.groundDeadTypeId))
            cxt.groundDeadAniId = cfg.groundDeadType;
        else
            cxt.groundDeadAniId = mPointCfg.groundDeadTypeId;

        if (mPointCfg.ai == "-1")
            cxt.aiBehavior = cfg.aiType;
        else
            cxt.aiBehavior = mPointCfg.ai;

        cxt.guid = Util.GenerateGUID();
        cxt.roleId = mPointCfg.roleId;
        cxt.level = Room.instance.roomCfg.levelLv;
        cxt.camp = mPointCfg.camp;
        cxt.pos = mPointCfg.pos;
        cxt.euler = mPointCfg.dir;
        cxt.hate.CopyFrom(mPointCfg.hate);

        Role role = LevelMgr.instance.CreateRole(cxt, mGroupFlag, mPointCfg.pointFlag);
        if (role != null)
        {
            if (mPointCfg.isShowBloodBar != 0)
                role.SetFlag(GlobalConst.FLAG_SHOW_BLOOD, mPointCfg.isShowBloodBar);
            if (mPointCfg.isShowFriendBloodBar != 0)
                role.SetFlag(GlobalConst.FLAG_SHOW_FRIENDBLOOD, mPointCfg.isShowFriendBloodBar);
            if (mPointCfg.isShowTargetBar != 0)
                role.SetFlag(GlobalConst.FLAG_SHOW_TARGET, mPointCfg.isShowTargetBar);
            UILevelAreaWave uiAreaWave = UIMgr.instance.Get<UILevel>().Get<UILevelAreaWave>();
            if (uiAreaWave.IsOpen)
            { role.SetFlag(GlobalConst.FLAG_REFLESH_WAVE, uiAreaWave.CurWave); }

            role.Add(MSG_ROLE.DEAD, OnPointDead);
            if (mPointCfg.buffId != 0)
                role.BuffPart.AddBuff(mPointCfg.buffId);
            roleDic[role.Id] = role;
            isNpcAlive = 1;
        }
        else
            Debug.LogError(string.Format("场景创建怪物失败{0}", mPointCfg.roleId));

        return role;

    }

    void OnPointDead(object param, object param2, object param3, EventObserver observer)
    {
        Role role = observer.GetParent<Role>();
        if (role == null)
            return;

        if (role.GetFlag(mPointCfg.pointFlag) > 0)
        {
            roleDic.Remove(role.Id);
            delayTime = 0;
            startTime = TimeMgr.instance.logicTime;
            isNpcAlive = 0;

            if (role.Cfg.addBuffType > 0)
            {
                Role hero = RoleMgr.instance.Hero;
                if (hero != null && hero.State == Role.enState.alive && mPointCfg.boxBuffId != 0)
                {
                    UILevelAreaReward uiReward = UIMgr.instance.Get<UILevel>().Get<UILevelAreaReward>();
                    if (!uiReward.IsOpen)
                        uiReward.OpenArea();

                    if (mPointCfg.boxAddType == SceneCfg.BoxStateType.AddMp)
                        uiReward.PlayBlueSoulFly(role, mPointCfg.boxAddNum, OnFlayEnd, mPointCfg.boxBuffId);
                    else if (mPointCfg.boxAddType == SceneCfg.BoxStateType.AddHp)
                        uiReward.PlayGreenSoulFly(role, mPointCfg.boxAddNum, OnFlayEnd, mPointCfg.boxBuffId);
                }
            }
        }
    }

    void OnFlayEnd(int addNum, object param)
    {
        int buffId = (int)param;
        Role hero = RoleMgr.instance.Hero;
        if (buffId != 0 && hero != null && hero.State == Role.enState.alive)
            hero.BuffPart.AddBuff(buffId);

    }

    public void OnSaveExit()
    {
        if (roleDic.Count > 1)
            Debuger.Log("同一个刷新点同时存在" + roleDic.Count + "个怪,再次进入时会刷在一个点");

        saveNpcData.Clear();

        foreach (var pair in roleDic)
        {
            var role = pair.Value;
            if (!role.IsUnAlive(pair.Key))
            {
                saveNpcData.Add(role.Cfg.id, role.GetInt(enProp.hp));
            }
        }
        roleDic.Clear();
    }

    public void OnLoadSave()
    {
        foreach (int hp in saveNpcData.Values)
        {
            Role role = CreateNpc(false);
            role.SetInt(enProp.hp, hp);
        }
        saveNpcData.Clear();
    }

}
