#region Header
/**
 * 名称：飞出物
 
 * 日期：2016.1.21
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Flyer : MonoBehaviour
{
    #region Fields
    int m_sourceId;
    int m_targetId;
    FlyerCfg m_cfg;
    Role m_source;
    Role m_target;
    Transform m_root;
    bool m_isPlaying=false;
    float m_playTime;
    SkillEventGroup m_eventGroup = new SkillEventGroup();
    FlyerPath m_path;
    bool m_cache=false;
    Collider m_collider ;//碰撞
    HashSet<RoleModel> m_colliderRoles = new HashSet<RoleModel>();
    float m_touchEnemyFrame = -1;
    float m_touchTargetFrame = -1;
    Skill m_parentSkill;
    #endregion


    #region Properties
    public SkillEventGroup EventGroup { get{return m_eventGroup;}}
    public Transform Root { get{return m_root;}}
    public FlyerCfg Cfg { get { return m_cfg; } }
    public Role Source { get { return m_source == null ||m_source.IsDestroy(m_sourceId) ? null : m_source; ; } }
    public Role Target { get { return m_target== null||m_target.IsDestroy(m_targetId) ? null : m_target; } }
    public bool IsPlaying { get{return m_isPlaying;}}
    public bool HasCollider { get { return m_collider!= null; } }
    public Collider Collider { get{return m_collider;}}
    public HashSet<RoleModel> ColliderRoles { get{return m_colliderRoles;}}
    #endregion

    #region Static Methods
    public static void Add(GameObject fx,string flyerId,Role source,Role target,Skill parentSkill)
    {
        if (string.IsNullOrEmpty(flyerId) || !fx.activeSelf)
            return;
        FlyerCfg cfg = FlyerCfg.Get(flyerId);
        if(cfg == null)
            return;

        fx.AddComponentIfNoExist<Flyer>().Init(cfg, source, target, parentSkill);
    }
    #endregion


    #region Mono Frame
    void Cache()
    {
        if (m_cache)return;
        m_cache = true;
        m_collider = this.GetComponent<Collider>();
        if (m_collider != null)
        {
            //必须加刚体并设置动力学，见unity物理引擎的文档，静态碰撞和触发型碰撞的区别
            Rigidbody r = this.AddComponentIfNoExist<Rigidbody>();
            r.isKinematic = true;
            m_collider.enabled=false;
        }
        LayerMgr.instance.SetLayer(this.gameObject, enGameLayer.flyerTrigger);
    }
    void Awake()
    {
        Cache();
    }
    void OnDisable()
    {
        Stop();
        this.enabled = false;
       
    }
    

    // Update is called once per frame
    void Update()
    {
        if(!m_isPlaying||TimeMgr.instance.IsPause)return;

        //主人死亡则销毁
        if(Source == null || Source.State != Role.enState.alive)
        {
            Stop();
            return;
        }

        //弹道
        if (m_path != null)
            m_path.OnUpdate();

        //可能弹道会销毁飞出物
        if(!m_isPlaying)return;

        //事件组
        int frame = (int)((TimeMgr.instance.logicTime - m_playTime) / Util.One_Frame);
        m_eventGroup.Update(frame, false);

        //可能事件组会销毁飞出物
        if (!m_isPlaying)return;

        //接触销毁
        if ((m_touchEnemyFrame != -1 && frame > (m_touchEnemyFrame + m_cfg.touchDestroyFrame) )||
            (m_touchTargetFrame != -1 && frame > (m_touchTargetFrame + m_cfg.touchTargetDestroyFrame)))
        {
            Stop();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        RoleModel roleModel =other.GetComponent<RoleModel>();
        if(roleModel == null )return;
        if(!m_colliderRoles.Add(roleModel))
        {
            Debuger.Log("逻辑错误 飞出物碰撞重复碰到角色:{0} 飞出物名:{1}", roleModel.name,this.gameObject.name);
            return;
        }

        //碰到敌人后销毁
        if (m_isPlaying&&Source!=null)
        {
            Role r = roleModel.Parent;
            if (r != null && r.State == Role.enState.alive  )
            {
                if(m_touchEnemyFrame == -1 && m_cfg.touchDestroyFrame >= 0 &&RoleMgr.instance.MatchTargetType(enSkillEventTargetType.enemy, Source, r))
                {
                    m_touchEnemyFrame = (int)((TimeMgr.instance.logicTime - m_playTime) / Util.One_Frame);
                }
                

                if(m_touchTargetFrame ==-1&& m_cfg.touchTargetDestroyFrame>= 0&&Target == r)
                {
                    m_touchTargetFrame = (int)((TimeMgr.instance.logicTime - m_playTime) / Util.One_Frame);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other == null || other.gameObject == null )
            return;
        RoleModel roleModel = other.GetComponent<RoleModel>();
        if (roleModel == null) return;
        m_colliderRoles.Remove(roleModel);
    }
    #endregion
   


    #region Private Methods
    
    #endregion

    public void Init(FlyerCfg cfg, Role source, Role target,Skill parentSkill)
    {
        Cache();
        if (source.State != Role.enState.alive)
        {
            Debuger.LogError("飞出物创建的时候主人已经死亡:{0} {1}", cfg.file, this.name);
            return;
        }

        //检错下
        if (m_isPlaying)
        {
            Debuger.LogError("逻辑错误，飞出物初始化时正在播放中:{0} {1}", cfg.file, this.name);
            return;
        }
            
        if (m_path != null)
            Debuger.LogError("逻辑错误，飞出物初始化时发现弹道没有销毁:{0} {1}", cfg.file, this.name);

        this.enabled = true;
        m_root = this.transform;
        m_sourceId = source.Id;
        m_targetId = target == null ? -1 : target.Id;
        m_cfg =cfg;
        m_source =source;
        m_target = target;
        m_target = GetRole(m_cfg.targetType);
        m_targetId = m_target == null ? -1 : m_target.Id;
        m_playTime = TimeMgr.instance.logicTime;
        m_parentSkill = parentSkill;
        m_touchEnemyFrame = -1;
        m_touchTargetFrame = -1;

        m_path = FlyerPathFactory.Create(m_cfg.pathCfg.Type);
        m_path.Init(this);
        
        if (m_collider != null)
        {
            if (m_colliderRoles.Count > 0)
            {
                Debuger.LogError("逻辑错误，飞出物初始化的时候发现碰撞到的角色列表不为空:{0} {1}", cfg.file, this.name);
                m_colliderRoles.Clear();
            }
                
            m_collider.enabled = true;
        }
        
        

        m_isPlaying = true;
        CombatMgr.instance.AddFlyer(this);

        //关卡结束后销毁
        if (m_cfg.durationFrame == FxDestroy.Destroy_Change_Scene)
        {
            float delayDestroy = FxDestroy.GetRunTimeDelayIfExist(this.gameObject);
            if(delayDestroy==-1)
                FxDestroy.Add(this.gameObject, FxDestroy.Destroy_Change_Scene);
        }
        //一定时间后销毁
        else if ( m_cfg.durationFrame > 0)
        {
            float delayDestroy = FxDestroy.GetRunTimeDelayIfExist(this.gameObject);
            if (delayDestroy <0 || m_cfg.durationFrame * Util.One_Frame < delayDestroy)
                FxDestroy.Add(this.gameObject, m_cfg.durationFrame * Util.One_Frame);
        }

        m_eventGroup.Init(m_cfg.eventGroup, m_source, null, m_root, m_parentSkill);
        m_eventGroup.Play(null, m_root.position);
        //m_eventGroup.Update(0, false);

    }
    //isClear表示是不是关卡退出或者强制销毁飞出物，是的话不用创建结束飞出物等其他东西
    public void Stop(bool isClear=false)
    {
        if (!m_isPlaying) return;
        OnStop(isClear);
    }
    void OnStop(bool isClear)
    {
        if (Main.instance == null || CombatMgr.instance == null || TimeMgr.instance == null)//应用程序已经退出了
            return;
        //检错下
        if (!m_isPlaying)
        {
            Debuger.LogError("逻辑错误，飞出物已经结束了");
            return;
        }

        m_isPlaying = false;
        m_touchEnemyFrame = -1;
        m_touchTargetFrame = -1;

        if (m_collider != null)
        {
            m_collider.enabled = false;
            m_colliderRoles.Clear();
        }

        if (m_path != null)
        {
            m_path.OnStop();
            m_path.Put();
            m_path = null;
        }
        
        m_eventGroup.Stop(isClear? enSkillStop.force: enSkillStop.normal);
        CombatMgr.instance.RemoveFlyer(this);
        FxDestroy.DoDestroy(this.gameObject);

        
        if (!isClear  && Source != null && Source.State == Role.enState.alive)
        {
            //结束飞出物
            if(!string.IsNullOrEmpty(m_cfg.endFlyerCreateCfg.name))
            {
                m_cfg.endFlyerCreateCfg.Create(Source, Target, m_root.position,enElement.none, OnLoadEndFx);
            }
            
            //结束事件组
            if (Source != null && Source.State == Role.enState.alive&&!string.IsNullOrEmpty(m_cfg.endEventGroupId))
                CombatMgr.instance.PlayEventGroup(Source, m_cfg.endEventGroupId,m_root.position, Target, this.m_parentSkill);
        }
        m_cfg = null;

    }
    void OnLoadEndFx(GameObject go, object param)
    {
        //如果飞出物上没有任何销毁的脚本，那么提示下
        if (string.IsNullOrEmpty(m_cfg.endFlyer) &&!FxDestroy.HasDelay(go))
        {
            Debuger.LogError("飞出物的结束特效上没有绑销毁脚本,而且这个结束特效也没有填结束飞出物去结束它.特效名:{0}", go.name);
        }
        Add(go,m_cfg.endFlyer,Source,Target,this.m_parentSkill);
    }

    //0默认自己，1释放者，2别人,3仇恨目标,4仇恨值目标,5仇恨值目标(不自动查找),6最近的友方,7最近的敌方,8最近的中立阵营,9主人,10主角
    public Role GetRole(enFlyerTargetType t)
    {
        switch (t)
        {
            case enFlyerTargetType.source: return Source;
            case enFlyerTargetType.target: return Target;
            case enFlyerTargetType.autoTarget: return Target != null && RoleMgr.instance.IsEnemy(Target, Source) ? Target : RoleMgr.instance.GetClosestTarget(Source, enSkillEventTargetType.enemy);
            case enFlyerTargetType.hate: return Source.HatePart.GetTargetLegacy();
            case enFlyerTargetType.hateNew: return Source.HatePart.GetTarget();
            case enFlyerTargetType.hateNewNotFind: return Source.HatePart.GetTargetLegacy(false);
            case enFlyerTargetType.closestSame: return RoleMgr.instance.GetClosestTarget(Source, enSkillEventTargetType.same);
            case enFlyerTargetType.closestEnemy: return RoleMgr.instance.GetClosestTarget(Source, enSkillEventTargetType.enemy);
            case enFlyerTargetType.closestNeutral: return RoleMgr.instance.GetClosestTarget(Source, enSkillEventTargetType.neutral);
            case enFlyerTargetType.parent: return Source.Parent;
            case enFlyerTargetType.hero:
                {
                    Role r = RoleMgr.instance.Hero;
                    return r != null && r.State == Role.enState.alive ? r : null;
                }
            default: Debuger.LogError("未知的类型:{0} ", t); return null;
        }
    }

}
