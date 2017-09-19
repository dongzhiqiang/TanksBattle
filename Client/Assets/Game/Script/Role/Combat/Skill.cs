#region Header
/**
 * 名称：技能类
 
 * 日期：2015.12.17
 * 描述：技能生命周期的管理，cd的管理等
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum enSkillStop
{
    normal,//自然结束
    behit,//被击等外力中断
    cancel,//主角自己连击或者取消
    force,//强制结束，角色死亡、切换关卡等情况下
}

public enum enSkillState
{
    noSkill,//没有技能
    normal,//可以播放
    playing,//播放中，如果没有连击技能
    preFrame,//播放开始到连击前帧之间，如果有连击技能
    buffFrame,//连击前帧到连击后帧之间，如果有连击技能
    postFrame,//连击后帧到等待帧之前，如果有连击技能
    cd,//cd中
    silent,//沉默状态中
}
public sealed class Skill: IdType//注意这个类的设计决定了它不适合继承
{
    #region Fields
    Role m_parent;
    SkillCfg m_cfg;
    SystemSkillCfg m_systemSkillCfg;
    Role m_target;
    int m_targetId;//一开始自动朝向或者技能传进来的目标
    float m_lastPlayTime=-1;
    float m_lastStopTime=-1;
    int m_curFrame;
    int m_maxFrame;
    bool m_isPlaying = false;
    AniPart m_aniPart;
    CombatPart m_combatPart;
    SkillEventGroup m_eventGroup= new SkillEventGroup();
    AniFxGroup m_aniFxGroup;
    string m_comboSkill;
    string m_cancelSkill;
    bool m_needCalcCombos = true;
    List<Skill> m_combos = new List<Skill>();
    Skill m_comboFist;//连击组里的第一个技能
    List<int> m_buffs=new List<int>();//和技能绑定的状态，技能结束状态销毁
    WeaponSkill m_weaponSkill;
    Skill m_internalParentSkill;//如果一个技能是内部技能，那么这个值指向所属的技能
    int m_lv;
    int m_comboIdx=-1;
    #endregion


    #region Properties
    public Role Parent { get{return m_parent;}}
    public Role Target
    {
        get { return m_targetId == -1 || m_target.IsDestroy(m_targetId) ||m_target.State!= Role.enState.alive ? null : m_target; } 
        set{
            if(value == null){
                m_target = null;
                m_targetId = -1;
            } 
            else if(value.State != Role.enState.alive)
                Debuger.LogError("{0} {1}技能目标设置进来不是alive的角色", m_parent.Cfg.id, m_cfg.skillId);
            else
            {
                m_target = value;
                m_targetId = value.Id;
            }
                
        }
    }
    public SkillCfg Cfg { get { return m_cfg; } }
    //注意允许为空
    public SystemSkillCfg SystemSkillCfg { get { return m_systemSkillCfg; } }
    public int Lv { get { return m_lv; } }

    //对应的武器技能，记录着技能的等级
    public WeaponSkill WeaponSkill { get { return m_weaponSkill; } }
    public SkillEventGroup EventGroup { get { return m_eventGroup; } }
    public AniFxGroup AniFxGroup { get { return m_aniFxGroup; } }

    //如果这个技能是内部技能，那么返回父技能，否则返回空
    public Skill InternalParentSkill { get { return m_internalParentSkill; } }

    //技能是不是在播放中,注意如果当前技能已经结束，但是仍然有内部技能在播放中中，那么这个技能仍然算播放中
    public bool IsPlaying {
        get {
            if (!m_cfg.isInternal && m_combatPart.CurSkill == this)
                return true ;
            return m_isPlaying;
    } }
    public bool IsPlayOrCD { get { return IsPlaying ? true : CDNeed > 0; } }
    public bool IsPlayingSelf { get { return m_isPlaying; } }
    //最后一个技能是不是自己的内部技能
    public bool IsLastSkillOfThis
    { get {
            return !this.m_cfg.isInternal && m_combatPart.LastSkillSelf !=null&& m_combatPart.LastSkillSelf.InternalParentSkill == this;
        } }

    //总的cd时间，如果是内部技能，那么算所属技能的
    public float CD {
        get {
            if (m_parent == RoleMgr.instance.Hero && DebugUI.instance.unCD)
                return 0;

            if (m_cfg.isInternal && InternalParentSkill != null)
                return InternalParentSkill.CD;
            
            return m_cfg.cd;
    } }
    public float CDNeed
    { //cd还剩多久
        get
        {
            //如果内部技能是最后的技能，那么要和自己的cd比较下，看哪个长点
            float needInternal = 0;
            if (IsLastSkillOfThis)
                needInternal =m_combatPart.LastSkillSelf.CDNeed;


            //内部技能和非内部技能有不同的判断
            float needSelf = 0;
            if (m_cfg.isInternal && this.InternalParentSkill != null)
            {
                
                if (m_lastStopTime != -1 && CD > 0)
                {
                    SkillCfg cfg = this.InternalParentSkill.Cfg; ;
                    float waitFrame = cfg.comboWaitFrame <= 0 ? 0 : cfg.comboWaitFrame * Util.One_Frame;//等待帧过程cd保持最大值
                    needSelf = Mathf.Clamp(CD + waitFrame + m_lastStopTime - TimeMgr.instance.logicTime, 0, CD);
                }
                    
            }
            else
            {
                
                if (m_lastStopTime != -1 && CD > 0)
                {
                    float waitFrame = m_cfg.comboWaitFrame <= 0 ? 0 : m_cfg.comboWaitFrame * Util.One_Frame;//等待帧过程cd保持最大值
                    needSelf = Mathf.Clamp(CD + m_lastStopTime - TimeMgr.instance.logicTime, 0, CD);
                }
                    
            }

            return Mathf.Max(needInternal, needSelf);
        }
        set //这里暂时没有做内部技能的判断，慎用
        {
            if (value == 0)
                m_lastStopTime = -1;

            m_lastStopTime = TimeMgr.instance.logicTime - CD + (value == -1 ? CD : value);
        }
    }

    public int CurFrame
    {
        get
        {
            if(!m_isPlaying)
                return 0;

            return m_curFrame;
            
        }
    }
   
    public int MaxFrame
    {
        get { return m_maxFrame;}
    }
    
    public bool IsMpEnough
    {
        get
        {
            if (DebugUI.instance.unMp)
                return true;
            else if (m_cfg.mpNeed == 0)
                return true;
            else if (m_cfg.mpNeed > 0)
                return this.m_parent.GetInt(enProp.mp) >= m_cfg.mpNeed;
            else if (m_cfg.mp == 0)
                return true;
            else
                return this.m_parent.GetInt(enProp.mp) >= m_cfg.mp;
        }
    }
    
    public enSkillState State
    {
        get
        {
            //沉默状态
            if (Parent.RSM.IsSilent)
                return enSkillState.silent;

            //如果内部技能是最后的技能，那么算内部技能的
            if (IsLastSkillOfThis)
                return m_combatPart.LastSkillSelf.State;

            //对于内部技能，取父技能的连击帧配置
            SkillCfg parentCfg = null;
            if (m_cfg.isInternal && this.InternalParentSkill != null)
                parentCfg = this.InternalParentSkill.Cfg;

            //没有连击技能(或者是连击的最后一下)
            int idx = this.ComboIdx;
            if (idx ==-1)
            {
                if (IsPlayingSelf)
                    return enSkillState.playing;
                else if (CDNeed > 0)
                    return enSkillState.cd;
                else
                    return enSkillState.normal;
                    
            }
            //有连击技能
            else
            {
                if(IsPlayingSelf)
                {
                    int preFrame = parentCfg != null ? 0 : m_cfg.comboPreFrame;//前帧都算父技能的
                    int postFrame = m_cfg.comboPostFrame;//后帧算自己的
                    int curFrame = IsPlayingSelf ? CurFrame : -1;
                    //连击前帧判断
                    if (curFrame < preFrame)
                        return enSkillState.preFrame;

                    //缓冲帧的判断
                    if ((postFrame == -1 || curFrame < postFrame) && curFrame >= preFrame)
                        return enSkillState.buffFrame;

                    return enSkillState.postFrame;
                }
                else
                {
                    int postFrame = m_cfg.comboPostFrame;//后帧算自己的
                    int waitFrame = parentCfg != null ? parentCfg.comboPostFrame : m_cfg.comboPostFrame;//等待帧都算父技能的

                    //等待帧之前
                    if (TimeMgr.instance.logicTime == m_lastStopTime && postFrame == -1)
                        return enSkillState.postFrame;
                    if (m_lastStopTime != -1 && (TimeMgr.instance.logicTime - m_lastStopTime <= waitFrame * Util.One_Frame))
                        return enSkillState.postFrame;

                    //cd                    
                    if (CDNeed > 0)
                        return enSkillState.cd;

                    return enSkillState.normal;
                }
            }
            
        }
    }
    public bool HasComboBuff { get { return IsPlaying && !string.IsNullOrEmpty(m_comboSkill); } }

    //用于界面显示连击，如果之前有连击也算
    public bool CanShowCombo
    {
        get
        {
            //看下有没有连击列表
            Skill comboFirst = ComboFirst;
            if (comboFirst == null) return false;
            if (!comboFirst.Cfg.showComboIcon) return false;
            if (ComboFirst.Combos.Count <=1) return false;//如果以后一连击也要显示，那么这行去掉
            

            //必须cd中或者不在释放状态下不能显示,注意这里连enSkillState.playing也要判断下可能是最后一个连击(没有连击技能)
            enSkillState st = this.State;
            return st == enSkillState.playing ||st == enSkillState.postFrame || st == enSkillState.preFrame|| st == enSkillState.buffFrame;
        }
    }
    
    //能不能连击
    public bool CanComboNext
    {
        get
        {
            if (this.InternalParentSkill != null)
                return this.InternalParentSkill.CanComboNext;

            if (string.IsNullOrEmpty(Cfg.comboSkillId))return false;
            enSkillState nextSt = this.State;
            return nextSt == enSkillState.buffFrame || nextSt == enSkillState.postFrame;
        }
    }
    
    //连击的第一个技能，如果
    public Skill ComboFirst {
        get {
            if (this.InternalParentSkill != null)
                return this.InternalParentSkill.ComboFirst;
            return m_comboFist;
        }
    }

    public int ComboIdx
    {
        get {
            if (this.InternalParentSkill != null)
                return this.InternalParentSkill.ComboIdx;

            
            if (string.IsNullOrEmpty(m_cfg.comboSkillId))
                return -1;
            
            if (ComboFirst == null || m_comboIdx >= ( ComboFirst.Combos.Count - 1))
                return -1;
            return m_comboIdx;
        }
        set { m_comboIdx=value; }
    }

    //连击列表，用于从连击的第一个技能和当前使用中的技能计算出可以用的连击技能
    public List<Skill> Combos { get{return m_combos;}}

    //可以用的连击技能。如果没有可以用的将返回null
    public Skill ComboSkill
    {
        get
        {
            if (this.InternalParentSkill != null)
                return this.InternalParentSkill.ComboSkill;

            CacheCombo();
            enSkillState thisSt = this.State;
            Skill thisIfCanplay = thisSt == enSkillState.normal ? this : null;

            if (m_combos.Count <=1)
                return thisIfCanplay;

            //找到可以连击的下一个技能
            Skill lastSkill = m_combatPart.LastSkill;
            int idx = lastSkill == null?-1:m_combos.IndexOf(lastSkill);
            if(idx == -1)//如果最后的技能不属于这个连击,返回第一个
                return thisIfCanplay;
            
            //正在播放中的情况
            enSkillState lastSt = lastSkill.State;
            if (lastSt == enSkillState.playing || lastSt == enSkillState.preFrame)//playing表示是最后一个技能了
                return null;
            //可以连击的情况
            if (lastSt == enSkillState.buffFrame || lastSt == enSkillState.postFrame)
            {
                Skill nextSkill = (idx + 1) < m_combos.Count ? m_combos[idx + 1] : null;
                if (nextSkill != null)//如果是连击的最后一下,返回第一个
                {
                    return nextSkill;
                }
                else
                {
                    Debuger.LogError("{2}逻辑错误，找不到下一个连击技能:{0} 当前第几个:{1}",lastSkill.Cfg.skillId, idx,Parent.Cfg.id);
                    return null;
                }
            }
                

            return thisIfCanplay;
        }
    }

    //界面上显示的技能，和上面的区别是上面的能用才不为空，而这里则不是
    public Skill ShowSkill
    {
        get
        {
            if (this.InternalParentSkill != null)
                return this.InternalParentSkill.ShowSkill;
            CacheCombo();
            if (m_combos.Count <=1)
                return  this;

            Skill lastSkill = m_combatPart.LastSkill;
            if (lastSkill == null || lastSkill == this)
                return this;
            int idx = lastSkill == null ? -1 : m_combos.IndexOf(lastSkill);
            if (idx == -1)
                return this;

            //如果在播放中或者等待帧中，那么显示
            enSkillState st = lastSkill.State;
            if (st == enSkillState.playing ||st == enSkillState.postFrame || st == enSkillState.preFrame|| st == enSkillState.buffFrame)
                return lastSkill;

            
            return this;
        }
    }

   
    
    #endregion


    #region Private Methods
    void OnStop(enSkillStop stopType)
    {
        //检错下
        if (!m_isPlaying)
        {
            Debuger.LogError("逻辑错误，技能已经结束了");
            return;
        }

        int poolId = this.Id;
        int parentId = m_parent.Id;

        Role target = this.Target;
        int targetId = target != null ? target.Id : -1;
        m_lastStopTime = TimeMgr.instance.logicTime;
        m_isPlaying = false;
        
        m_targetId = -1;

        //显示武器
        if (!Cfg.showWeapon)
        {
            m_parent.RenderPart.ShowWeapon(true);
        }

        m_eventGroup.Stop(stopType);

        //结束处理相关
        do
        {
            if (m_parent.State != Role.enState.alive ||
                (stopType == enSkillStop.cancel && !m_cfg.endIfCancel) ||
                (stopType == enSkillStop.behit && !m_cfg.endIfBehit)
            )
                break;

            BuffPart buffPart = m_parent.BuffPart;
            //技能结束状态
            for (int i = 0; i < m_cfg.endStates.Count; ++i)
            {
                buffPart.AddBuff(m_cfg.endStates[i]);
            }
            if (this.IsDestroy(poolId) || m_parent.IsUnAlive(parentId))
                return;

            //技能结束事件组
            if (!string.IsNullOrEmpty(Cfg.endEventGroupId))
                CombatMgr.instance.PlayEventGroup(m_parent, Cfg.endEventGroupId, m_parent.transform.position, (targetId == -1 || target.IsDestroy(targetId)) ? null : target, this);

            if (this.IsDestroy(poolId) || m_parent.IsUnAlive(parentId))
                return;
        } while (false);

        //技能绑定的状态都销毁掉
        if (m_buffs.Count != 0)
        {
            m_parent.BuffPart.RemoveBuffByIds(m_buffs);
            m_buffs.Clear();
        }

    }

    void CacheCombo()
    {
        //先计算下连击技能,如果需要的话
        if (m_needCalcCombos || m_combatPart.RoleSkillCfg.NeedCalcCombo)
        {
            m_combos.Clear();
            m_combatPart.RoleSkillCfg.NeedCalcCombo = false;
            m_needCalcCombos = false;
            Skill cur = this;
            
            //技能等级对连击的限制
            int comboLimit = -1;
            if(WeaponSkill != null)
            {
                int limitLv ;
                WeaponSkill.GetComboLimit(out comboLimit, out limitLv);
            }

            //能连击几下
            int i = 0;
            do
            {
                cur.ComboIdx = i;
                cur.m_comboFist = this;
                m_combos.Add(cur);
                cur = string.IsNullOrEmpty(cur.Cfg.comboSkillId) || cur.Cfg.skillId == cur.Cfg.comboSkillId ? null : m_combatPart.GetSkill(cur.Cfg.comboSkillId);
                
                ++i;
            } while (cur != null && (comboLimit==-1|| i< comboLimit));
        }
    }

    //对象池回收的时候要进行一些清空操作
    public override void OnClear()
    {
        m_parent = null;
        m_aniPart = null;
        m_eventGroup.OnClear();
        m_aniFxGroup = null;
    }

    void InitLv()
    {
        m_lv = -1;
        m_weaponSkill = null;
        if (SystemSkillCfg == null)
            return;
        if (string.IsNullOrEmpty(SystemSkillCfg.parentSkillId))
            return;

        //主角
        WeaponPart weaponPart = Parent.WeaponPart;
        if (weaponPart != null)
        {
            m_weaponSkill = weaponPart.GetSkill(SystemSkillCfg.parentSkillId);
            if (m_weaponSkill != null)
            {
                m_lv = m_weaponSkill.lv;
                return;
            }
        }

        //神器
        var treasurePart = Parent.TreasurePart;
        if(treasurePart!= null)
        {
            int lv = treasurePart.GetTreasureSkillLevel(SystemSkillCfg.parentSkillId);
            if(lv!=-1)
            {
                m_lv = lv;
                return;
            }
        }

    }
    #endregion

    public void Init(SkillCfg cfg, Role parent)
    {
        m_cfg=cfg;
        m_lastPlayTime = -1;
        m_lastStopTime = -1;
        m_isPlaying = false;
        m_parent=parent;
        m_aniPart = m_parent.AniPart;
        m_combatPart = m_parent.CombatPart;
        m_systemSkillCfg = SystemSkillCfg.Get(m_parent.Cfg.id, cfg.skillId);
        m_eventGroup.Init(cfg.eventGroup,m_parent,this,m_parent.RoleModel.Model,this);
        m_aniFxGroup = m_aniPart.Ani.GetGroup(m_cfg.aniName);

        AnimationState st = m_aniPart.GetSt(m_cfg.aniName);
        if (st!=null&&m_cfg.duration == -1 )
            m_maxFrame = (int)(st.length / Util.One_Frame);
        else if (m_cfg.duration >=0)
            m_maxFrame = (int)(m_cfg.duration / Util.One_Frame);
        else
            m_maxFrame = m_eventGroup.MaxFrame;

        m_needCalcCombos = true;
        m_combos.Clear();
        m_comboFist = null;
        m_comboIdx = -1;
        if (cfg.isInternal)
        {
            m_internalParentSkill = m_combatPart.GetSkill(cfg.parentSkillId);
            if (m_internalParentSkill == null)
                Debuger.LogError("技能{0}在技能编辑器勾选为内部技能，但是却找不到父技能{1}", cfg.skillId,cfg.parentSkillId);
        }
        else
            m_internalParentSkill = null;

        InitLv();//找下养成相关的属性
    }

    public void Play()
    {
        //检错下
        if(m_isPlaying){
            Debuger.LogError("逻辑错误，技能正在播放中");
            return;
        }
        if (m_parent.State != Role.enState.alive)
        {
            Debuger.LogError("角色已经死亡，不能使用技能");
            return;
        }


        m_lastPlayTime =  TimeMgr.instance.logicTime;
        m_lastStopTime = -1;
        m_isPlaying =true;
        m_curFrame = 0;
        m_comboSkill = string.Empty;
        m_cancelSkill = string.Empty;
        m_buffs.Clear();
        
        //扣mp
        if (Cfg.mp > 0&&!DebugUI.instance.unMp)
        {
            int mp = m_parent.GetInt(enProp.mp) - Cfg.mp;
            m_parent.SetInt(enProp.mp, mp >= 0 ? mp : 0);
        }

        //技能期间状态
        Buff buff;
        BuffPart buffPart = m_parent.BuffPart;
        for (int i = 0; i < m_cfg.skillStates.Count; ++i)
        {
            buff = buffPart.AddBuff(m_cfg.skillStates[i]);
            if(buff != null)
                m_buffs.Add(buff.Id);
        }

        //隐藏武器
        if (!Cfg.showWeapon)
        {
            m_parent.RenderPart.ShowWeapon(false);
        }

        //动作
        m_aniPart.Play(m_cfg.aniName, m_cfg.wrapMode, m_cfg.aniRate, m_cfg.fade);

        //广播出去
        m_parent.Fire(MSG_ROLE.SKILL, this);
        if (!m_isPlaying)//技能可能被停止，这里要判断下
            return;

        //事件组
        m_eventGroup.Play(Target,Parent.transform.position);
    }

    public void Stop(enSkillStop stopType)
    {
        if (!m_isPlaying) return;
        OnStop(stopType);
    }

    
    public void Update()
    {
        int poolId = this.Id;
        AnimationState st = m_aniPart.CurSt;
        //按照动作时间判断结束
        if (st!=null&&m_cfg.duration == -1 && m_cfg.aniType == SkillCfg.enSkillAniType.ClampForever)
        {
            //判断结束
            if (st.normalizedTime>=1)//TimeMgr.instance.logicTime - m_lastPlayTime >= st.length)
            {
                m_curFrame = (int)(st.length / Util.One_Frame);
                m_eventGroup.Update(m_curFrame, true);//注意这里的计算方式不一样，为了保证这个动作的最后一帧会加进来
                if (this.IsDestroy(poolId))//FIX: 上一行的处理可能导致这个对象被回收了，这个时候要马上返回，不能做任何修改
                    return;
                Stop(enSkillStop.normal);
            }
            else
            {
                m_curFrame = m_aniPart.Ani.CurTotalFrame;
                m_eventGroup.Update(m_curFrame, false);
                if (this.IsDestroy(poolId))//FIX: 上一行的处理可能导致这个对象被回收了，这个时候要马上返回，不能做任何修改
                    return;
            }  
        }
        //按照普通时间判断结束
        else
        {
            //判断结束
            float duration = m_cfg.duration;
            if (m_cfg.duration == -1 )
            {
                if (!m_cfg.continueIfPress)//先检错下,按紧的情况下不用报错
                    Debuger.LogError("循环方式不是单次的情况下，技能时间不能填-1.roleId:{0} skillId:{1}", m_parent.Cfg.id, m_cfg.skillId);
                duration =st!= null?st.length: m_eventGroup.MaxFrame*Util.One_Frame;
            }

            //检查按紧，如果技能允许按紧，那么不结束
            bool isPress = m_cfg.continueIfPress && m_parent.RSM.StateCombat.PressSkill==this;
            float time = TimeMgr.instance.logicTime - m_lastPlayTime ;
            m_curFrame = (int)(time / Util.One_Frame);

            if (!isPress && time >= duration)
            {
                m_eventGroup.Update(m_curFrame, true);
                if (this.IsDestroy(poolId))//FIX: 上一行的处理可能导致这个对象被回收了，这个时候要马上返回，不能做任何修改
                    return;
                Stop(enSkillStop.normal);
            }
            else
            {
                m_eventGroup.Update(m_curFrame, true);
                if (this.IsDestroy(poolId))//FIX: 上一行的处理可能导致这个对象被回收了，这个时候要马上返回，不能做任何修改
                    return;
            }
                
                
        }
       
    }

    

    //设置技能结束或者打击后帧播放这个技能
    public void SetComboBuff(string s){
        if (m_internalParentSkill != null)
        {
            Debuger.LogError("逻辑错误，给内部技能设置了连击技能:{0}",this.m_cfg.skillId);
            return;
        }
        m_comboSkill = s;
    }

    public void SetCancelBuff(string s){m_cancelSkill = s;}

    public bool PlayComboBuff()
    {
        //检查下父技能的
        if (m_internalParentSkill != null)
            return m_internalParentSkill.PlayComboBuff(); 

        if (string.IsNullOrEmpty(m_comboSkill))
            return false;

        if(string.IsNullOrEmpty(m_cfg.comboSkillId))
        {
            Debuger.LogError("{0}的连击技能为空，却设置了连击技能，设置进来的",m_cfg.skillId,m_comboSkill);
            return false;
        }
        
        enSkillState st = this.State;
        if (st == enSkillState.postFrame)
        {
            CombatPart.enPlaySkill ret = m_parent.CombatPart.Play(m_comboSkill,null, false, true);
#if UNITY_EDITOR
            //调试技能
            string debugSkillId = m_parent.RoleModel.m_debugSkillId;//技能id|连击技能id|返回值类型，默认都可以填-1，那么就是全部调试
            string debugComboId = "-1";
            int resultType  = -1;

            string[] ss = debugSkillId.Split('|');
            debugSkillId = ss[0];
            if (ss.Length >=2) 
                debugComboId = ss[1];
            if(ss.Length >=3) 
                if (!int.TryParse(ss[2], out resultType)) resultType = -1;


            bool needDebug = !string.IsNullOrEmpty(debugSkillId) ;
            if (needDebug)
            {
                needDebug = debugComboId == "-1" || debugComboId == m_comboSkill;
                if (needDebug)
                {

                    needDebug = resultType == -1 || resultType == (int)ret;
                    if (needDebug)
                        Debuger.Log("{3}.{0}_{1}使用连击技能:{2}", m_parent.Cfg.id, m_parent.Id, m_comboSkill, ret);
                }
            }
            
#endif
            return true;
        }
        else
            return false;
    }

    public bool PlayCancelBuff()
    {
        if (string.IsNullOrEmpty(m_cancelSkill))
            return false;

        
        if ((m_cfg.cancelPostFrame > 0 && m_curFrame >= m_cfg.cancelPostFrame) ||
            (m_cfg.cancelPostFrame == -1 && !m_isPlaying))
        {
            CombatPart.enPlaySkill ret = m_parent.CombatPart.Play(m_cancelSkill,null, false, true);
#if UNITY_EDITOR
            //调试技能
            string debugSkillId = m_parent.RoleModel.m_debugSkillId;//技能id|连击技能id|返回值类型，默认都可以填-1，那么就是全部调试
            string debugComboId = "-1";
            int resultType = -1;

            string[] ss = debugSkillId.Split('|');
            debugSkillId = ss[0];
            if (ss.Length >= 2)
                debugComboId = ss[1];
            if (ss.Length >= 3)
                if (!int.TryParse(ss[2], out resultType)) resultType = -1;

            bool needDebug = !string.IsNullOrEmpty(debugSkillId) ;
            if (needDebug)
            {
                needDebug = debugComboId == "-1" || debugComboId == m_cancelSkill;
                if (needDebug)
                {

                    needDebug = resultType == -1 || resultType == (int)ret;
                    if (needDebug)
                        Debuger.Log("{3}.{0}_{1}使用连击技能:{2}", m_parent.Cfg.id, m_parent.Id, m_cancelSkill, ret);
                }
            }
#endif
            return true;
        }
        else
            return false;
    }

    //添加技能绑定的状态，这个状态会在技能结束的时候结束
    public void AddSkillBindBuff(int id)
    {
        if (!m_isPlaying)
        {
            Debuger.LogError("技能没有在使用中，不能将状态绑定到技能上");
            return;
        }
        m_buffs.Add(id);
    }

    
    

    public float GetLvValue(LvValue v)
    {
        if (v == null)
            return 1;//可能配置没有填，那么当成1
        if(v.error)
        {
            Debuger.LogError("技能的值计算有问题，所属技能:{0}, 当前技能:{1}", SystemSkillCfg.parentSkillId, this.Cfg.skillId);
            return 0;
        }

        if (!v.NeedLv)
            return v.Get();

        if(m_lv ==-1)
        {
            Debuger.LogError("当前技能找不到等级，所属技能:{0},当前技能:{1}", SystemSkillCfg.parentSkillId, this.Cfg.skillId);
            return 0;
        }

        return v.GetByLv(m_lv);

    }
    
}
