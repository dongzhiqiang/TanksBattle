#region Header
/**
 * 名称：ai部件
 
 * 日期：2015.9.21
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Simple.BehaviorTree;
//ai类型
public enum enSimpleAIType
{
    none,
    attack,//攻击范围内可以使用技能就使用，否则追击敌人
    simpleAni,//循环播放一个动作,设置ai的时候要传进来RoleStateAniCxt对象作为参数
    followAttack,//跟随，距离主人一定范围则跟随主人，主人打某只怪物则打某只怪物
    guaji,//主角挂机，会自动走到关卡中箭头指向的地方，会自动打怪
    xunluo,//寻路到一点 转为战斗状态
}



public class AIPart:RolePart
{
    public const string NoneAI = "";
    public const string MonsterAI = "ai_tongyong:小怪";
    public const string PetAI = "ai_tongyong:宠物";
    public const string HeroAI = "ai_tongyong:主角挂机";

    #region Fields
    string m_curBehavior;
    BehaviorTree m_behaviorTree;
    float m_lastOperation;
    #endregion


    #region Properties
    public override enPart Type { get{return enPart.ai;} }
    public BehaviorTree BehaviorTree { get { return m_behaviorTree; } }
    public bool IsPlaying { get { return m_behaviorTree.IsPlaying; } }

    //上次手动操作时间
    public float LastOperation { get { return m_lastOperation; } }
    #endregion


    #region Frame

    //属于角色的部件在角色第一次创建的时候调用，属于模型的部件在模型第一次创建的时候调用
    public override void OnCreate(RoleModel model) {
        m_behaviorTree = model.AddComponentIfNoExist<BehaviorTree>();
        m_behaviorTree.onCanUpdateTree += CanUpdateAI;
    }
    bool CanUpdateAI()
    {
        if (TimeMgr.instance.IsPause)
            return false;

        if (DebugUI.instance.unAttack)
            return false;

        if (this.Parent == null || RoleModel == null|| this.Parent.State != Role.enState.alive)
            return false;

        if (RoleModel.m_unAttack)
            return false;

        var hero = RoleMgr.instance.Hero;
        if (hero != null && hero.State == Role.enState.alive&& hero.RSM.IsBigQte)
            return false;

        return true;
    }

    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        m_curBehavior = string.Empty;
        m_lastOperation = -1;
        return true;
    }

    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
        m_behaviorTree.SetOwner(this.Parent);
    }

    public override void OnDestroy()
    {
        m_behaviorTree.Stop();
        m_behaviorTree.ClearOwner();

    }

    public override void OnUpdate()
    {
        

    }
    #endregion


    #region Private Methods
   
    
    #endregion
    //播放，如果已经在播放中了会报错
    public void Play(string behavior)
    {
        if (m_parent.State != Role.enState.alive)
        {
            Debuger.LogError("角色不在生存态仍然被设置ai,{0}", m_parent.State);
            return;
        }


        m_lastOperation = -1;
        m_behaviorTree.Play(behavior);
        Parent.Fire(MSG_ROLE.AI_CHANGE, null);
        
    }

    //停止
    public void Stop()
    {
        m_behaviorTree.Stop();
        Parent.Fire(MSG_ROLE.AI_CHANGE, null);
    }

    //暂停
    public void Pause()
    {
        m_behaviorTree.Pause();
        Parent.Fire(MSG_ROLE.AI_CHANGE, null);
    }

    //重新播放
    public void RePlay(bool reset = true)
    {
        m_lastOperation = -1;
        m_behaviorTree.RePlay(reset);
        Parent.Fire(MSG_ROLE.AI_CHANGE, null);
    }

    //刷新手动操作的时间
    public void FreshOperation()
    {
        m_lastOperation = TimeMgr.instance.logicTime;
    }
    
}
