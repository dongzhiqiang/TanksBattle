#region Header
/**
 * 名称：状态基类
 
 * 日期：2016.2.25
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;




public class Buff :IdType
{
    protected int m_parentId;
    protected Role m_parent;
    int m_sourceId;
    Role m_source;
    protected BuffCfg m_cfg;
    protected bool m_isPlaying = false;//状态是不是已经结束了
    protected int m_count;//执行次数的计数
    protected float m_beginTime = -1;
    protected float m_lastHandelTime = -1;
    protected GameObject m_roleFx= null;
    protected float m_time = -3;//时间，可以修改，-3的话就是用配置表的时间
    protected SoundFx3D m_sfx;

    int m_lv = 0;

    public BuffCfg Cfg { get { return m_cfg;} }
    //状态加在哪个角色身上
    public Role Parent { get { return m_parentId == -1 || m_parent.IsDestroy(m_parentId) || (!this.Cfg.exCfg.SupportUnalive && m_parent.State != Role.enState.alive) ? null : m_parent; } }
    //释放者，加这个状态的人,这个一般很少用
    //增减血等状态可能会用到，用来计算谁扣血的
    //加印记可能会用到，用来计算谁加的印记
    public Role Source
    {
        get { return m_sourceId == -1 || m_source.IsDestroy(m_sourceId) || (!this.Cfg.exCfg.SupportUnalive && m_source.State != Role.enState.alive) ? null : m_source; }
        set
        {
            if (value == null)
            {
                m_source = null;
                m_sourceId = -1;
            }
            else if (!this.Cfg.exCfg.SupportUnalive && value.State != Role.enState.alive)
                Debuger.LogError("{0} {1}状态目标设置进来不是alive的角色", m_parent.Cfg.id, m_cfg.id);
            else
            {
                m_source = value;
                m_sourceId = value.Id;
            }

        }
    }
    public bool IsPlaying {get{return m_isPlaying;}}

    public float Time {get{return m_time == -3?m_cfg.time:m_time;}set{m_time =value;} }

    //永久的状态返回-1，已经结束的状态返回0，未结束的状态返回正数
    public float TimeLeft { get { 
        if(Time < 0)
            return Time;
        else if(!m_isPlaying)
            return 0;
        else
            return Mathf.Max(0, m_beginTime + Time - TimeMgr.instance.logicTime); 
        }
    }

    //是不是可以作为非战斗状态，一般都是可以的
    public virtual bool CanCreateUnalive{ get{return true;} }
    
    #region frame
    //初始化，状态创建的时候调用
    public void Init(Role parent,Role source,BuffCfg cfg)
    {
        m_parentId = parent.Id;
        m_parent = parent;
        m_cfg = cfg;
        Source = source;
        
        if (m_isPlaying)
        {
            Debuger.LogError("逻辑错误，状态正在播放中");
            return;
        }
        m_isPlaying = true;
        m_count = 0;
        m_beginTime = TimeMgr.instance.logicTime;
        m_time = -3;
        InitLv();
        OnBuffInit();

        //非战斗状态处理
        if (!Cfg.IsAliveBuff)
        {
            Handle();
        }
        
    }

    public override void OnClear() { 
        m_parent=null;
        m_cfg = null;
        m_beginTime = -1;
        m_lastHandelTime =-1;
        m_roleFx = null;
    }

    //如果角色模型正在loading中，那么会等模型加载完之后才调用这个函数
    public void ModelInit()
    {
        //角色特效
        CreateRoleFx();

        //音效
        if(m_cfg.soundId!=-1)
        {
            var sfx =SoundMgr.instance.Play3DSound(m_cfg.soundId, this.Parent.RoleModel.Model);
            if (m_cfg.stopIfEnd)
                m_sfx = sfx;
        }
            

        //战斗状态处理
        if (Cfg.IsAliveBuff)
        {
            Handle();
        }
        
    }

    public void ModelDestroy()
    {
        if (Cfg.IsAliveBuff)//战斗状态在模型销毁时销毁
        {
            Remove(true);
        }
        else//非战斗状态只需要删除模型上的特效就可以了
        {
            DestroyRoleFx();
            if (m_sfx != null)
            {
                SoundMgr.instance.Stop(m_sfx);
                m_sfx = null;
            }
            
        }
    }

    
    //模型创建
    public void Update()
    {
        //一些检错
        if (m_lastHandelTime == -1 || m_beginTime == -1)
        {
            Debuger.LogError("状态update的时候发现可能没有初始化完:m_beginTime:{0} m_lastHandelTime:{1}", m_beginTime, m_lastHandelTime);
            return;
        }

        //超时删除
        if (Time >= 0 && TimeMgr.instance.logicTime - m_beginTime >= Time)
        {
            Remove();
            return;
        }

        //时间间隔调用handle
        if (m_cfg.interval > 0 && TimeMgr.instance.logicTime - m_lastHandelTime >= m_cfg.interval )
        {
            Handle();
            if(m_parent.State != Role.enState.alive)
                return;
        }


        OnBuffUpdate();
    }

    

    //isClear表示是不是强制清空这个状态，是的话不需要创建结束状态,注意除了BuffPart其他地方不要用这个接口删除特效，应该用remove
    public void OnStop(bool isClear = false)
    {
        if (!m_isPlaying)
            return;
        int[] endBuffId = m_cfg.endBuffId;//结束状态ID,-1则表示没有
        string endEventGroupId = m_cfg.endEventGroupId;
        int parentId = m_parentId;
        Role parent = m_parent;
        Role source = this.Source;
        BuffPart buffPart = parent.BuffPart;
        bool isAlive = !parent.IsUnAlive(parentId);
        m_isPlaying = false;
        DestroyRoleFx();
        if (m_sfx != null)
        {
            SoundMgr.instance.Stop(m_sfx);
            m_sfx = null;
        }
        OnBuffStop(isClear);
        m_beginTime = -1;
        m_lastHandelTime = -1;
        
        this.Put();//放回对象池

        if(!isAlive || isClear)
            return; 

        //结束状态
        if (endBuffId != null && endBuffId.Length != 0)
        {
            for (int i = 0; i < endBuffId.Length; ++i)
            {
                buffPart.AddBuff(endBuffId[i], source);
            }
            isAlive = !parent.IsDestroy(parentId) && parent.State == Role.enState.alive;
        }

        if (!isAlive)
            return; 

        //结束事件组
        if (!string.IsNullOrEmpty(endEventGroupId))
            CombatMgr.instance.PlayEventGroup(parent, endEventGroupId, parent.transform.position);
    }

    //创建角色特效
    public void CreateRoleFx()
    {
        if (m_cfg.roleFxId == -1)return;
        if (m_roleFx != null)
        {
            Debuger.LogError("逻辑错误，状态创建角色特效的时候发现有残留:{0} {1}", m_cfg.id, m_roleFx.name);
            FxDestroy.DoDestroy(m_roleFx);
            m_roleFx = null;
        }
        m_roleFx = RoleFxCfg.Play(m_cfg.roleFxId,m_parent,null,false);
    }

    //销毁角色特效
    public void DestroyRoleFx()
    {
        if (m_roleFx != null)
        {
            FxDestroy.DoDestroy(m_roleFx);
            m_roleFx = null;
        }
        
    }


    void Handle()
    {
        if (!m_isPlaying)
        {
            Debuger.LogError("逻辑错误，状态已经结束仍然要求处理");
            return;
        }
        if (this.Parent == null)
        {
            Debuger.LogError("逻辑错误,状态的parent已经不在生存态仍然要求处理");
            return;
        }

        ++m_count;
        m_lastHandelTime= TimeMgr.instance.logicTime;

        if(Cfg.exCfg==null)
        {
            Debuger.LogError("状态的参数解析失败，不能执行:{0}", Cfg.id);
            return;
        }
        OnBuffHandle();
    }

    //初始化，状态创建的时候调用，一般用来解析下参数
    public virtual void OnBuffInit() { }

    //处理，可能会调用多次
    public virtual void OnBuffHandle() { 
        string log =m_cfg.param == null ? "" : string.Join(",", m_cfg.param);
        if(string.IsNullOrEmpty(log))
            return;
        Debuger.Log("状态id:{0} 执行次数:{1} 参数:{2}", m_cfg.id, m_count,log) ; 
    }
   

    //每帧更新
    public virtual void OnBuffUpdate() { }

    //结束
    public virtual void OnBuffStop(bool isClear) { }

    #endregion


    public void Remove(bool isClear = false)
    {
        if (m_parent.IsDestroy(m_parentId))
        {
            Debuger.LogError("逻辑错误，销毁状态的时候发现所属角色已经被销毁");
            OnStop(true);
            return;
        }
        m_parent.BuffPart.RemoveBuff(this, isClear);
    }

    void InitLv()
    {
        m_lv = -1;
        Role r = Source;
        if (r == null)
            return;
        //如果受到技能影响
        SystemSkillCfg skillCfg = SystemSkillCfg.GetByBuff(Cfg.id);
        if (skillCfg != null)
        {
            CombatPart combatPart = r.CombatPart;
            Skill s = combatPart.GetSkill(skillCfg.id);
            if (s == null)
            {
                Debuger.LogError("状态值结算出错，状态找不到所属技能，状态:{0},技能:{1}", this.Cfg.id, skillCfg.id);
                return;
            }
            if (s.Lv == -1)
            {
                Debuger.LogError("状态值结算出错，状态找不到所属技能的等级，状态:{0},技能:{1}", this.Cfg.id, skillCfg.id);
                return;
            }
            m_lv = s.Lv;
            return;
        }

        //如果受到铭文影响
        HeroTalentCfg talentCfg = HeroTalentCfg.GetByBuff(Cfg.id);
        if (talentCfg != null)
        {
            WeaponPart weaponPart = r.WeaponPart;
            if (weaponPart == null)
            {
                Debuger.LogError("状态值结算出错，没有武器部件，状态:{0},铭文id:{1}", this.Cfg.id, talentCfg.id);
                return ;
            }
            WeaponSkillTalent talent = weaponPart.GetTalent(talentCfg.id);
            if (talent == null)
            {
                Debuger.LogError("状态值结算出错，武器部件找不到所属铭文，状态:{0},铭文id:{1}", this.Cfg.id, talentCfg.id);
                return ;
            }
            m_lv = talent.lv;
            return ;
        }

        //如果受到宠物天赋影响
        TalentsPart talentsPart = r.TalentsPart;
        if (talentsPart != null)
        {
            var talent = talentsPart.GetTalentByBuffId(Cfg.id);
            if (talent != null)
            {
                m_lv = talent.level;
                return ;
            }
        }
    }
    
    public float GetLvValue(LvValue v)
    {
        if (v == null)
            return 1;//可能配置没有填，那么当成1
        if (v.error)
        {
            Debuger.LogError("状态的值计算有问题，状态id:{0}",this.Cfg.id );
            return 0;
        }

        if (!v.NeedLv)
            return v.Get();

        //检错下
        if (m_lv == -1)
        {
            Debuger.LogError("状态的等级计算出错，可能状态释放者已经死亡或者状态等级不能找到，状态id:{0}", this.Cfg.id);
            return 0;
        }
        
        return v.GetByLv(m_lv);
    }

    //是不是不需要再往下处理了
    protected bool IsUnneedHandle(int buffPoolId,Role r1,int r1PooldId, Role r2=null, int r2PooldId=0)
    {
        if (this.IsDestroy(buffPoolId))
        {
            Debuger.Log("状态执行中销毁");
            return true;
        }
            
        if (r1 != null && (r1.IsDestroy(r1PooldId) || r1.State != Role.enState.alive))
        {
            Debuger.Log("状态执行中角色不在alive状态");
            return true;
        }

        if (r2 != null && (r2.IsDestroy(r2PooldId) || r2.State != Role.enState.alive))
        {
            Debuger.Log("状态执行中角色2不在alive状态");
            return true;
        }

        return false;
    }

    //0默认自己，1释放者，2别人,3仇恨目标,4仇恨值目标,5仇恨值目标(不自动查找),6最近的友方,7最近的敌方,8最近的中立阵营,9主人,10主角
    protected Role GetRole(enBuffTargetType t,Role another)
    {
        switch (t)
        {
            case enBuffTargetType.self: return m_parent; 
            case enBuffTargetType.source:
                {
                    if (Source == null) { Debuger.LogError("释放者为空，状态id:{0}", Cfg.id); return null; }
                    return Source;
                }; 
            case enBuffTargetType.another: return another;
            case enBuffTargetType.hate: return m_parent.HatePart.GetTargetLegacy();
            case enBuffTargetType.hateNew: return m_parent.HatePart.GetTarget();
            case enBuffTargetType.hateNewNotFind: return m_parent.HatePart.GetTargetLegacy(false);
            case enBuffTargetType.closestSame:return RoleMgr.instance.GetClosestTarget(m_parent, enSkillEventTargetType.same);
            case enBuffTargetType.closestEnemy: return RoleMgr.instance.GetClosestTarget(m_parent, enSkillEventTargetType.enemy);
            case enBuffTargetType.closestNeutral: return RoleMgr.instance.GetClosestTarget(m_parent, enSkillEventTargetType.neutral);
            case enBuffTargetType.parent: return m_parent.Parent;
            case enBuffTargetType.hero:
                {
                    Role r = RoleMgr.instance.Hero;
                    return r != null && r.State == Role.enState.alive ? r : null;
                }
            default: Debuger.LogError("未知的类型:{0} 状态id:{1}", t, Cfg.id);return null;
        }
    }


    public void LogError(string format, params object[] ps)
    {
        string s = string.Format("状态出错 类型:{0} id:{1}",m_cfg.type,m_cfg.id );
        Debuger.LogError(s + format, ps);
    }
}
