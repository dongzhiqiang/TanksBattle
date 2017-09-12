#region Header
/**
 * 名称：角色行为状态机部件
 
 * 日期：2015.9.21
 * 描述：
 * 1.行为状态维护，角色在任意时刻有且只有一个行为
 * 2.角色是不是处于某些特殊状态(和当前行为没有必然联系，但是可能会影响到行为间的切换，所以也放到这里了)的维护，比如空中状态、气力状态、眩晕
 * 
 * 
 * 关于没有模型角色的特殊之处，没有模型的角色的SimpleRole上勾选着"无模型角色",没有模型角色由于不能播放动作所以不能进入任何行为(待机、移动、攻击、被击等等),它处于空行为中
 * 关于没有碰撞的角色，即没有CharacterController组件，移动需要依靠CharacterController，所以没有碰撞的角色不可移动或者被移动，但是可以设置位置
 * 关于不受伤害，免疫伤害事件+免疫伤害反弹+免疫增减血就可以实现，见状态表里不受伤害事件的实现
 * 关于不可移动，对于没有模型的角色是不能进入移动行为的(一直都是empty状态),但是TranPart还是会计算其他移动
 *               对于没有碰撞的角色是不能位移(除了直接设置位置)，TranPart不会计算（移动事件、移动行为、被击行为都内部做了处理）
 *               对于普通角色，要实现类似于束缚效果的话可以给角色添加一个不可位移的状态类型
 * 关于不可受击(霸体),对于没有模型的角色是不会进入受击行为的(一直都是empty状态)
 *               对于普通角色，可以给这个角色添加免疫受击行为的状态(被击、浮空、击飞、抓取)
 * 关于无视攻击的角色如何配，对陷阱类型的角色，不会被自动朝向和ai监测到，再给角色加个免疫所有事件和不受伤害的状态
 *               对于普通类型的角色，暂时没有手段可以不被自动朝向和ai检测到
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum enRoleState
{
    empty = 0x0000, //空
    born = 0x0001,  //出生
    free = 0x0002,  //空闲
    move = 0x0004,  //移动
    combat = 0x0008,//战斗
    beHit = 0x0010, //被击(支持击飞、击倒、击飞、二次浮空、被抓取)
    dead = 0x0020,  //死亡
    ani = 0x0040,   //动作序列
    fall = 0x0080,  //下落
    switchWeapon = 0x0100, //换武器
    round = 0x0200, //包围
    pauseAni = 0x0400, //定身
}

public class RoleStateMachine:RolePart
{
    #region Fields
    Dictionary<enRoleState, RoleState> m_states = new Dictionary<enRoleState, RoleState>();
    Dictionary<string,RoleState> m_statesByName = new Dictionary<string,RoleState>();
    RoleState m_curState;
    RoleStateEmpty m_stateEmpty;
    RoleStateBorn m_stateBorn;
    RoleStateFree m_stateFree;
    RoleStateMove m_stateMove;
    RoleStateCombat m_stateCombat;
    RoleStateBehit m_stateBihit;
    RoleStateDead m_stateDead;
    RoleStateAni m_stateAni;
    RoleStateFall m_stateFall;
    RoleStateRound m_stateRound;    
    RoleStateSwitchWeapon m_stateSwitchWeapon;
    RoleStatePauseAni m_statePauseAni;

    bool m_isChanging = false;//防止GotoState多次调用
    enRoleState m_dalayGotoState = enRoleState.empty;//防止GotoState多次调用
    object m_dalayGotoParam;//防止GotoState多次调用
	bool m_delayForce = false;
    

    bool m_air;
    CameraHandle m_airCameraHandle;

    int m_limitMoveCounter=0;
    int m_silentCounter = 0;
    
    public static Dictionary<string,enRoleState> s_stateTypes = new Dictionary<string,enRoleState>();
    #endregion

    #region 特殊状态维护
    //是不是在空中
    public bool IsAir
    {
        get { return m_air; }
        set
        {
            if (m_air == true && value == true)
            {
                Debuger.LogError("重复进入空中状态");
                return;
            }
            m_air = value;

            if (m_air)
            {
                //切换到空中镜头
                if (m_airCameraHandle != null)
                {
                    m_airCameraHandle.Release();
                    m_airCameraHandle = null;
                    Debuger.LogError("逻辑错误，空中镜头没有释放");
                }
                m_airCameraHandle = CameraMgr.instance.StillLook(this.RoleModel.Model, m_stateFall.CameraMoveTime, m_stateFall.CameraOverTime);
            }
            else
            {
                //释放空中镜头
                if (m_airCameraHandle != null)
                {
                    m_airCameraHandle.Release();
                    m_airCameraHandle = null;
                }
                m_stateFall.ResetHang();
            }
        }
    }

    //是不是在气力状态下，否则则在气绝状态
    public bool IsShield
    {
        get
        {
            if (m_parent.RuntimeShieldBuff <= 0)//角色表里没有填气力状态buffId则永久都在气力状态下
                return true;

            return BuffPart.GetBuffByBuffId(m_parent.RuntimeShieldBuff) != null;//有气力状态buff在气力状态下
        }
    }

    //是不是处于动画序列播放状态中(可能状态机不处于这个状态)
    public bool IsAnis{get { return m_stateAni.IsPlaying;}}

    //是不是在使用大qte
    public bool IsBigQte
    {
        get {
            if (BigQte.CurQte != null && BigQte.CurQte.IsPlaying)
                return true;
            return false;
        }
    }

    //是不是没有模型
    public bool IsModelEmpty { get { return RoleModel.SimpleRole.m_isEmpty; } }
    
    //是不是没有碰撞,不能进入移动状态，获取移动上下文失败
    public bool IsNoCollider { get { return RoleModel.CC == null; } }
    
    //是不是不可移动，移动上下文不起作用
    public bool IsLimitMove
    {
        get { return m_limitMoveCounter > 0; }
    }
   
    //是否处于沉默状态
    public bool IsSilent
    {
        get { return m_silentCounter > 0; }
    }
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.rsm; } }
    public RoleState CurState { get { return m_curState; } }
    public enRoleState CurStateType { get { return m_curState.Type; } }
    public RoleStateEmpty StateEmpty { get { return m_stateEmpty; } }
    public RoleStateBorn StateBorn { get { return m_stateBorn;} }
    public RoleStateFree StateFree { get { return m_stateFree; } }
    public RoleStateMove StateMove { get { return m_stateMove; } }
    public RoleStateCombat StateCombat { get { return m_stateCombat; } }
    public RoleStateBehit StateBehit { get { return m_stateBihit; } }
    public RoleStateDead StateDead { get { return m_stateDead; } }
    public RoleStateAni StateAni { get { return m_stateAni; } }
    public RoleStateFall StateFall { get { return m_stateFall; } }
    public RoleStateRound StateRound { get { return m_stateRound; } }
    public RoleStateSwitchWeapon StateSwitchWeapon { get { return m_stateSwitchWeapon; } }
    public RoleStatePauseAni StatePauseAni { get { return m_statePauseAni; } }
    #endregion



    #region Frame    
    static RoleStateMachine()
    {
        s_stateTypes["空"] = enRoleState.empty;
        s_stateTypes["出生"] = enRoleState.born;
        s_stateTypes["空闲"] = enRoleState.free;
        s_stateTypes["移动"] = enRoleState.move;
        s_stateTypes["战斗"] = enRoleState.combat;
        s_stateTypes["被击"] = enRoleState.beHit;
        s_stateTypes["死亡"] = enRoleState.dead;
        s_stateTypes["动作序列"] = enRoleState.ani;
        s_stateTypes["下落"] = enRoleState.fall;
        s_stateTypes["换武器"] = enRoleState.switchWeapon;
        s_stateTypes["包围"] = enRoleState.round;
        s_stateTypes["定身"] = enRoleState.pauseAni;
    }

    public static bool TryParse(string param,ref HashSet<enRoleState> sts,char spilt = '|')
    {
        sts.Clear();
        if (string.IsNullOrEmpty(param))
            return true;
        string[] pp = param.Split(spilt);
        for (int i = 0; i < pp.Length; ++i)
        {
            enRoleState st;
            if (s_stateTypes.TryGetValue(pp[i], out st))
                sts.Add(st);
            else
                return false;
        }
        return true;
    }


    public RoleStateMachine()
    {
        //创建状态机
        m_states[enRoleState.empty] = m_stateEmpty = new RoleStateEmpty(this, enRoleState.empty);
        m_states[enRoleState.born] = m_stateBorn = new RoleStateBorn(this, enRoleState.dead | enRoleState.beHit);
        m_states[enRoleState.free] = m_stateFree = new RoleStateFree(this, enRoleState.dead | enRoleState.beHit| enRoleState.pauseAni);
        m_states[enRoleState.move] =m_stateMove = new RoleStateMove(this, enRoleState.dead | enRoleState.beHit | enRoleState.pauseAni);
        m_states[enRoleState.combat] = m_stateCombat = new RoleStateCombat(this, enRoleState.dead | enRoleState.beHit | enRoleState.pauseAni);
        m_states[enRoleState.beHit] = m_stateBihit = new RoleStateBehit(this, enRoleState.dead | enRoleState.pauseAni);
        m_states[enRoleState.dead] = m_stateDead = new RoleStateDead(this, enRoleState.empty);
        m_states[enRoleState.ani] = m_stateAni = new RoleStateAni(this, enRoleState.dead | enRoleState.pauseAni);
        m_states[enRoleState.fall] = m_stateFall = new RoleStateFall(this, enRoleState.combat | enRoleState.beHit | enRoleState.dead | enRoleState.pauseAni);
        m_states[enRoleState.switchWeapon] = m_stateSwitchWeapon = new RoleStateSwitchWeapon(this, enRoleState.dead | enRoleState.pauseAni);
        m_states[enRoleState.round] = m_stateRound = new RoleStateRound(this, enRoleState.dead | enRoleState.beHit | enRoleState.pauseAni);
        m_states[enRoleState.pauseAni] = m_statePauseAni = new RoleStatePauseAni(this, enRoleState.dead);

        foreach (RoleState s in m_states.Values){
            m_statesByName[s.GetType().ToString()] =s;
        }
        m_curState = m_states[enRoleState.born];

    }
  

    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        IsAir = false;
        m_curState = m_states[enRoleState.born];
        
        return true;
    }

    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
        m_limitMoveCounter = 0;
        //手动进入出生状态，绕过GotoState
        if (!IsModelEmpty)
        {
            m_curState = m_states[enRoleState.born];
            m_curState.Enter(new RoleStateBornCxt(m_parent.RoleBornCxt.bornAniId));
        }
        //如果是空模型的话进入空状态
        else
        {
            m_curState = m_states[enRoleState.empty];
            m_curState.Enter(null);
        }
    }

    public override void OnUpdate() {
        m_curState.Update();
    }

    public override void OnDestroy() {
        m_limitMoveCounter = 0;
        m_silentCounter = 0;
        m_curState.Leave();
        foreach(RoleState s in m_states.Values){
            s.OnDestroy();
        }
        m_curState = null;
    }
    #endregion


    #region Private Methods
    
    #endregion

    
    public bool GotoState(enRoleState type, object param=null,bool force=false,bool putIdTypeParam=false)
    {
        //如果准备要销毁或者死亡，就不要做切换了，会引起其他问题
        if (RoleMgr.instance.IsNeedDestroy(m_parent) || (m_curState.Type == enRoleState.dead &&RoleMgr.instance.IsNeedDead(m_parent)))
            return false;

        //如果在销毁中了就不要切换状态了
        if (m_parent.IsDestroying){
            Debuger.LogError("销毁中仍然要求切换角色状态:{0}",m_parent.Cfg.id);
            return false;
        }
    
        if (TimeMgr.instance.IsPause && type != enRoleState.dead && m_curState.Type != enRoleState.born&& type != enRoleState.free)
        {
            //if (m_curState.Type != enRoleState.born && m_curState.Type != enRoleState.dead)
            //{
                Debuger.LogError("逻辑暂停，却仍然要求切换状态,当前状态:{0},要切换的状态:{1}", m_curState.Type, type);
            //}
            
            return false;
        }
        if (m_curState.Type == enRoleState.dead)
        {
            Debuger.LogError("逻辑错误，暂时不支持死亡状态挑到别的状态:{0}", m_parent.Cfg.id);
            return false;
        }
        if (m_isChanging)
        {
            Debuger.Log("Need DelayGoto " + type);
            m_dalayGotoState = type;
            m_dalayGotoParam = param;
			m_delayForce = force;
            return true;
        }

#if UNITY_EDITOR
        //调试状态
        string debugState= m_parent.RoleModel.m_debugState;//老状态|新状态，默认都可以填-1，那么就是全部调试，出生,空闲,移动,战斗,被击,死亡,动作序列,下落,换武器
        enRoleState oldType = enRoleState.empty;
        enRoleState newType = enRoleState.empty;
        bool needDebug = !string.IsNullOrEmpty(debugState);
        
        if (needDebug)
        {
            string[] ss = debugState.Split('|');
            if (ss.Length <= 0 || !s_stateTypes.TryGetValue(ss[0], out oldType))
                oldType = enRoleState.empty;

            if (ss.Length <= 1 || !s_stateTypes.TryGetValue(ss[1], out newType))
                newType = enRoleState.empty;    

            needDebug = (oldType == enRoleState.empty || oldType == m_curState.Type) && (newType == enRoleState.empty || newType == type);
            if (needDebug)
            {
                RoleState state;
                m_states.TryGetValue(type, out state);
                bool ret=m_curState.Type ==type || !(!force && !m_curState.IsForceLeave(state) && !m_curState.CanLeave(state));
                Debuger.Log("{0}_{1}从{2}要进入{3} 强制:{4} 结果:{5}", m_parent.Cfg.id, m_parent.Id, m_curState.Type, type,force, ret);    
            }


        }

#endif 

        m_isChanging = true;
        try  
        {
            RoleState state= m_states[type];

            //如果新老状态一样，那么传递下参数就行了,否则切换状态
            if (state.Type == m_curState.Type)
            {
                state.Do(param);
                return true;
            }



            //检查能不能切换
            if (!force && !m_curState.IsForceLeave(state) && (!m_curState.CanLeave(state)|| !state.CanEnter()))
            {

                //如果传进来的参数是需要回收的，在这里回收下，因为对应的状态无法回收
                if (putIdTypeParam)
                    ((IdType)param).Put();
                return false;
            }
            


            m_curState.Leave();
            m_curState = state;
            m_curState.Enter(param);
        }
        finally
        {
            m_isChanging = false;   
        }

        //切换状态多次的延时处理
        if (m_dalayGotoState != enRoleState.empty)
        {
            enRoleState s = m_dalayGotoState;
            object p = m_dalayGotoParam;
			bool f =m_delayForce;
            m_dalayGotoState = enRoleState.empty;
            m_dalayGotoParam = null;
			m_delayForce = false;
			GotoState(s, p,f);
        }
        return true;
    }

    public T GetState<T>()where T:RoleState
    {
        return m_statesByName.Get(typeof(T).ToString()) as T;
    }

    public RoleState GetState(enRoleState type)
    {
         return m_states.Get(type);
    }

    public bool CheckFall()
    {
        if(!IsAir)
            return false;
        if (IsAnis && m_stateAni.IsAvoidState(enRoleState.fall))
            return m_stateAni.Goto(null);
        else
            return GotoState(enRoleState.fall);
    }

    //用于非持久状态退出时判断要不要切换到移动
    public bool CheckMove()
    {
        if (!m_stateMove.NeedMove)
            return false;
        if (IsAnis && m_stateAni.IsAvoidState(enRoleState.move))
            return m_stateAni.Goto(null);
        else
            return GotoState(enRoleState.move);

    }

    public bool CheckDead(bool check=true)
    {
        return DeadPart.CheckAndHandle(check);
    }

    public bool CheckFree(bool force=false)
    {
        if (IsAnis )
            return m_stateAni.IsCur ? true : m_stateAni.Goto(null, force);
        else
            return m_stateFree.IsCur ? true : GotoState(enRoleState.free, null, force);
    }

    public void AddLimitMove()
    {
        ++m_limitMoveCounter;
    }

    public void SubLimitMove()
    {
        
        --m_limitMoveCounter;
        if (m_limitMoveCounter < 0)
        {
            Debuger.LogError("不可位移计数出错:{0}", m_parent.Cfg.id);
        }

    }

    public void AddSilent()
    {
        ++m_silentCounter;
    }

    public void SubSilent()
    {

        --m_silentCounter;
        if (m_silentCounter < 0)
        {
            Debuger.LogError("沉默状态计数出错:{0}", m_parent.Cfg.id);
        }

    }
}
