#region Header
/**
 * 名称：事件组
 
 * 日期：2015.12.17
 * 描述：执行事件帧
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//和事件组绑定的游戏对象(比如状态、移动)，事件组结束的时候会自动结束
public class SkillEventGroupBindObject : IdType
{
    public enum enType
    {
        buff,
        tranPartCxt,
        max
    }
    public IdType obj=null;
    public int objId = 0;
    public Role role = null;
    public int roleId = 0;
    public enType type = enType.max;
    public enSkillStop stopType= enSkillStop.normal;

    public override void OnClear() {
        Check();
        obj = null;
        objId = 0;
        role = null;
        roleId = 0;
        type = enType.max;
        stopType = enSkillStop.normal;
    }

    void Check()
    {
        if (role.IsUnAlive(roleId) || obj.IsDestroy(objId))
            return;

        if(type== enType.buff)
        {
            role.BuffPart.RemoveBuffById(objId);
        }
        else if (type == enType.tranPartCxt)
        {
            var cxt = (TranPartCxt)obj;
            if (stopType != enSkillStop.normal)//如果被中断而结束的情况下，不释放结束技能
                cxt.endSkill = null;
            role.TranPart.RemoveCxt((TranPartCxt)obj);
        }
        else
        {
            Debuger.LogError("未知的类型:{0}", obj.GetType());
        }
    }

  
}

public sealed class SkillEventGroup : IdType//注意这个类的设计决定了它不适合继承
{
    #region Fields
    Role m_parent;
    int m_parentId;
    Role m_target;
    int m_targetId;//一开始自动朝向或者技能传进来的目标
    Skill m_skill;
    SkillEventGroupCfg m_cfg;
    Transform m_root;
    Flyer m_flyer;
    Skill m_parentSkill;

    Vector3 m_happonPos= Vector3.zero;
    float m_playTime = -1;
    bool m_isPlaying = false;
    int m_curFrame;
    int m_maxFrame;
    bool m_haveForeverFrame;
    List<SkillEventFrame> m_frames = new List<SkillEventFrame>();
    List<SkillEventGroupBindObject> m_bindObjs = new List<SkillEventGroupBindObject>();//和事件组绑定的游戏对象(比如状态、移动)，事件组结束的时候会自动结束
    #endregion


    #region Properties
    public SkillEventGroupCfg Cfg { get{return m_cfg;}}
    public bool IsPlaying { get { return m_isPlaying; } }//事件组是不是在使用中
    public SkillEventFrameCfg LastFrameCfg { get{return m_cfg.frames.Count != 0?m_cfg.frames[m_cfg.frames.Count-1]:null;}}
    public bool HaveForeverFrame { get{return m_haveForeverFrame;}}
    public int MaxFrame { get{return m_maxFrame;}}
    public int CurFrame { get{return m_curFrame;}}
    public List<SkillEventFrame> Frames { get{return m_frames;}}
    //绑定的transform，用来做伤害判定时的位置和方向
    public Transform Root { get { return m_root; } }
    //绑定的技能，如果不是技能的事件组，那么为空
    public Skill Skill { get { return m_skill; } }
    //绑定的飞出物，如果不是飞出物的事件组，那么为空
    public Flyer Flyer { get{return m_flyer;}}
    //所属角色，用于做阵营判断等
    public Role Parent { get { return m_parentId == -1 || m_parent.IsDestroy(m_parentId) || m_parent.State != Role.enState.alive ? null : m_parent; } }
    //目标角色，通常是某个敌人
    public Role Target{get { return m_targetId == -1 || m_target.IsDestroy(m_targetId) ||m_target.State!= Role.enState.alive ? null : m_target; } }
    //描述性的名字，用于debug打印出来
    public string Name { get {
            if (m_skill != null)
                return m_skill.Cfg.skillId;
            else if(m_flyer!=null)
                return m_flyer.Cfg.file;
            else
                return Cfg.file;
        } }
  
    //所属技能，如果自己是技能的事件组，那么返回本技能,否则返回根技能，暂时用于技能伤害系数的获取,如果是状态触发的话,为空
    public Skill ParentSkill { get { return m_skill != null ? m_skill : m_parentSkill; } }
    //触发点，事件组触发的时候的参考点
    public Vector3 HappenPos { get { return m_happonPos; } }
    #endregion


    #region Private Methods
    void ClearFrames()
    {
        if(m_frames.Count==0)return;
        SkillEventFrame f;
        for(int i = 0;i< m_frames.Count;++i){
            f =m_frames[i];
            IdTypePool<SkillEventFrame>.Put(f);
        }
        m_frames.Clear();
    }
    #endregion
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cfg">配置</param>
    /// <param name="parent">所属角色</param>
    /// <param name="s">如果是技能的事件组，那么不为空</param>
    /// <param name="t">用于伤害判定</param>
    /// <param name="parentSkill">事件组的父技能</param>
    public void Init(SkillEventGroupCfg cfg,Role parent,Skill s,Transform t,Skill parentSkill)
    {
        //检错下
        if (m_isPlaying)
        {
            Debuger.LogError("逻辑错误，事件组正在播放中，不能初始化");
            return;
        }
        if (m_skill != null && m_skill != parentSkill)
        {
            Debuger.LogError("逻辑错误，技能的事件组，技能和父技能不一样：{0} {1}", s.Cfg.skillId, parentSkill==null?"空": parentSkill.Cfg.skillId);
            parentSkill = m_skill;
        }
       

        m_cfg = cfg;
        m_parent = parent;
        m_parentId = parent.Id;
        m_skill = s;
        m_root = t;
        m_flyer = m_root.GetComponent<Flyer>(); //如果是飞出物，那么记录下来，要取碰撞
        m_parentSkill = parentSkill;
        

#if !UNITY_EDITOR
        //找到最大帧,编辑器下有可能改到技能，每次都算下，非编辑器下init的时候算就可以了
        m_maxFrame = 0;
        int maxFrame;
        SkillEventFrameCfg c;
        for (int i = 0; i < m_cfg.frames.Count; ++i)
        {
            c = m_cfg.frames[i];

            if (!m_haveForeverFrame && c.frameType != enSkillEventFrameType.once && c.frameEnd == -1)
                m_haveForeverFrame = true;
            maxFrame = (c.frameType != enSkillEventFrameType.once && c.frameEnd != -1) ? c.frameEnd : c.frameBegin;
            if (maxFrame > m_maxFrame)
                m_maxFrame = maxFrame;
        }
#endif
    }

    public override void OnClear()
    {
        m_cfg = null;
        m_parent = null;
        m_parentId = -1; 
        m_isPlaying = false;
        m_skill = null;
        m_root = null; ;
        m_flyer = null;
        m_parentSkill = null;
        m_target = null;
        m_targetId = -1;
        
        ClearFrames();
    }
    public void Play(Role target,Vector3 happonPos)
    {
        //检错下
        if(m_isPlaying){
            Debuger.LogError("逻辑错误，事件组正在播放中，不能播放");
            return;
        }

        if (m_bindObjs.Count != 0)
        {
            Debuger.LogError("逻辑错误，播放的时候发现没有清空绑定对象");
        }


        //目标
        if (target != null && !target.IsInPool && target.State == Role.enState.alive)
        {
            m_target = target;
            m_targetId = target.Id;
        }
        else
        {
            if (target == null)
            {

            }
            else if (target.IsInPool)
            {
                Debuger.LogError("技能事件组传进来已经被回收的目标 {0}", m_skill == null ? "" : m_skill.Cfg.skillId);
            }
            else if (target.State != Role.enState.alive)
            {
                Debuger.LogError("技能事件组传进来不在alive的目标 {0} {1}", m_skill == null ? "" : m_skill.Cfg.skillId, target.State);
            }
            m_target = null;
            m_targetId = -1;
        }

        m_happonPos = happonPos;
        m_isPlaying =true;
        m_curFrame= -1;
        m_playTime = TimeMgr.instance.logicTime;
        SkillEventFrameCfg c;
#if UNITY_EDITOR
        //找到最大帧,编辑器下有可能改到技能，每次都算下，非编辑器下init的时候算就可以了
        m_maxFrame = 0;
        int maxFrame;
        
        for (int i = 0; i < m_cfg.frames.Count; ++i)
        {
            c = m_cfg.frames[i];

            if (!m_haveForeverFrame && c.frameType != enSkillEventFrameType.once && c.frameEnd == -1)
                m_haveForeverFrame = true;
            maxFrame = (c.frameType != enSkillEventFrameType.once && c.frameEnd != -1) ? c.frameEnd : c.frameBegin;
            if (maxFrame > m_maxFrame)
                m_maxFrame = maxFrame;
        }
#endif

        //创建事件帧
        if (m_frames.Count != 0)
        {
            Debuger.LogError("逻辑错误，事件帧没有清空");
            ClearFrames();
        }
        SkillEventFrame f;
        for(int i = 0;i< m_cfg.frames.Count;++i){
            c= m_cfg.frames[i];
            //创建事件帧
            f =IdTypePool<SkillEventFrame>.Get();
            f.Init(c, this,m_skill);
            m_frames.Add(f);
        }

        Update(0,!m_haveForeverFrame&& m_maxFrame==0);
        
    }
    public void UpdateAuto(bool end=false)
    {
        if(!IsPlaying)
            return;
        int frame = (int)((TimeMgr.instance.logicTime - m_playTime)/Util.One_Frame);

        //判断结束
        if (end || (!m_haveForeverFrame&& frame>=m_maxFrame)){
            Update(frame,true);
            Stop( enSkillStop.normal);
        }
        else
            Update(frame,false);            
    }

    
    //更新帧，frame表明当前是第几帧
    public void Update(int frame,bool end)
    {
        
        if (frame <= m_curFrame || !m_isPlaying || Parent==null || Parent.State!= Role.enState.alive)
            return;

        
        int i  =m_curFrame+1;
        m_curFrame = frame;
        int poolId = this.Id;
        for (; i <= frame; ++i)
        {
            for(int j=0;j<m_frames.Count;++j){
                m_frames[j].Update(i, end);
                //FIX: 上一行的处理可能导致这个对象被回收了，这个时候要马上返回，不能做任何修改
                if (this.IsDestroy(poolId) || Parent==null || Parent.State != Role.enState.alive)
                   return;
            }
        }
        
    }

    //主动停止
    public void Stop(enSkillStop stopType)
    {
        if (!m_isPlaying)return;
        OnStop();
    }

    void OnStop()
    {
        //检错下
        if (!m_isPlaying)
        {
            Debuger.LogError("逻辑错误，事件组已经结束了");
            return;
        }

        m_isPlaying = false;
        ClearFrames();

        foreach (var bindObj in m_bindObjs)
        {
            bindObj.Put();
        }
        m_bindObjs.Clear();
        
    }
    
    public SkillEventFrame GetFrameById(int id)
    {
        for(int i=0;i< m_frames.Count;++i)
        {
            if (m_frames[i].Cfg.id == id)
                return m_frames[i];
        }

        
        return null;
    }

    public void AddBindObj(Role role,IdType obj, SkillEventGroupBindObject.enType type)
    {
        if (obj.IsInPool )
        {
            Debuger.LogError("逻辑错误，事件组要绑定的对象已经被回收了");
            return;
        }

        if (role.State!= Role.enState.alive)
        {
            Debuger.LogError("逻辑错误，事件组不能绑定一个不在生存态的角色");
            return;
        }

        SkillEventGroupBindObject bindObj = IdTypePool<SkillEventGroupBindObject>.Get();
        bindObj.obj = obj;
        bindObj.objId = obj.Id;
        bindObj.role = role;
        bindObj.roleId = role.Id;
        bindObj.type = type;
     
        m_bindObjs.Add(bindObj);
    }
}
