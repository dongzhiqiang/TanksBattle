#region Header
/**
 * 名称：角色管理器
 
 * 日期：2015.9.21
 * 描述：
 *      这里继承自SingletonMonoBehaviour主要为了观察相关信息
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public enum enOrderRole
{
    normal,
    closest,
}

public class RoleMgr : SingletonMonoBehaviour<RoleMgr>, IRoleMgr
{

    public class CloseComparer : IComparer<float>
    {
        public int Compare(float a, float b)
        {
            return a <= b ? -1 : 1;//这样保证了键值重复的情况下不会报错，如果有等于0的情况，那么就SortedList就不能add相同的键值
        }
    }

    public static string[] OrderRoleTypeName = new string[] { "默认", "最近" };
    public const string GlobalEnemyId = "xj_global_enemy";
    public const string RoleMgrLocking = "RoleMgrLocking";

    #region Fields
    Dictionary<int, Role> m_initRoles = new Dictionary<int, Role>();
    Dictionary<int, Role> m_roles = new Dictionary<int, Role>();
    Dictionary<int, Role> m_deadRoles = new Dictionary<int, Role>();

    Role m_updatingDestroy;//正在update中的角色要update完才能删除，记下来
    Role m_updatingDead;//正在update中的角色要update完才能死亡，记下来
    Role m_hero;
    Role m_globalEnemy;
    int m_globalEnemyId;
    #endregion


    #region Properties
    public Role Hero { get { return m_hero; } }
    //注意这个接口比较底层，会取到宝箱和陷阱，取的时候要判断下是不是宝箱和陷阱
    public ICollection<Role> Roles { get { return m_roles.Values; } }
    public Role GlobalEnemy
    {
        get
        {
            if (m_globalEnemy != null && m_globalEnemy.IsUnAlive(m_globalEnemyId))
            {
                Debuger.LogError("全局敌人已经死亡却被访问");
                return null;
            }
            return m_globalEnemy;
        }
    }

    #endregion

    #region Mono Frame
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion



    #region Private Methods

    #endregion
    //只创建主角，但是不创建模型
    public void CreateHero(FullRoleInfoVo vo, enCamp camp = enCamp.camp1)
    {

        RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
        cxt.OnClear();
        cxt.roleId = "kratos";
        cxt.pos = Vector3.zero;
        cxt.euler = Vector3.zero;
        cxt.camp = camp;
        cxt.aiBehavior = "";
        m_hero = CreateNetRole(vo, true, cxt);

        EventMgr.FireAll(MSG.MSG_ROLE, MSG_ROLE.HERO_CREATED);
    }

    //从网络数据创建一个角色，注意这里不创建模型
    public Role CreateNetRole(FullRoleInfoVo vo, bool dontLoadModel, RoleBornCxt cxt)
    {
        RoleCfg cfg = RoleCfg.Get(cxt.roleId);
        if (cfg == null)
        {
            Debuger.LogError("找不到角色id:{0}", cxt.roleId);
            return null;
        }

        Role role = IdTypePool<Role>.Get();

        //检错的代码，以后删除
        if (m_initRoles.ContainsKey(role.Id))
            Debuger.LogError("角色对象池使用了仍在使用的角色 m_initRoles id:{0}", role.Id);
        else if (m_roles.ContainsKey(role.Id))
            Debuger.LogError("角色对象池使用了仍在使用的角色 m_roles id:{0}", role.Id);
        else if (m_deadRoles.ContainsKey(role.Id))
            Debuger.LogError("角色对象池使用了仍在使用的角色 m_deadRoles id:{0}", role.Id);

        role.State = Role.enState.init;
        m_initRoles[role.Id] = role;
        if (!role.Init(cfg))
        {
            Debuger.LogError("初始化角色失败");
            if (role.State == Role.enState.init)//这里要先判断下是不是这个状态，内部可能调用到了AliveRole
                DestroyRole(role);
            return null;
        }


        //添加外部部件
        role.SetPart(new EquipsPart());
        role.EquipsPart.Init(role);
        Property heroIdProp = null;
        if (vo.props.TryGetValue("heroId", out heroIdProp) && heroIdProp.Int != 0)//是玩家
        {
            role.SetPart(new ItemsPart());
            role.SetPart(new LevelsPart());
            role.SetPart(new ActivityPart());
            role.SetPart(new WeaponPart());
            role.SetPart(new SystemsPart());
            role.SetPart(new MailPart());
            role.SetPart(new OpActivityPart());
            role.SetPart(new FlamesPart());
            role.SetPart(new SocialPart());
            role.SetPart(new TaskPart());
            role.SetPart(new CorpsPart());
            role.SetPart(new ShopsPart());
            role.SetPart(new EliteLevelsPart());
            role.SetPart(new TreasurePart());
            role.ItemsPart.Init(role);
            role.LevelsPart.Init(role);
            role.ActivityPart.Init(role);
            role.WeaponPart.Init(role);
            role.OpActivityPart.Init(role);
            role.SystemsPart.Init(role);
            role.MailPart.Init(role);   //邮件初始化
            role.FlamesPart.Init(role);
            role.SocialPart.Init(role);
            role.TaskPart.Init(role);
            role.CorpsPart.Init(role);
            role.ShopsPart.Init(role);
            role.EliteLevelsPart.Init(role);
            role.PetFormationsPart.Init(role);
            role.TreasurePart.Init(role);
        }

        //设置相关属性
        role.SetInt(enProp.camp, (int)cxt.camp);

        //初始化网络数据
        role.InitNet(vo);

        //加载模型
        if (!dontLoadModel)
            role.Load(cxt);
        else
            role.PropPart.FreshBaseProp();

        return role;
    }

    public Role CreateRole(RoleBornCxt cxt, bool dontLoadModel = false)
    {
        if (cxt == null)
            return null;

        RoleCfg cfg = RoleCfg.Get(cxt.roleId);
        if (cfg == null)
        {
            Debuger.LogError("找不到角色id:{0}", cxt.roleId);
            return null;
        }

        Role role = IdTypePool<Role>.Get();

        //检错的代码，以后删除
        if (m_initRoles.ContainsKey(role.Id))
            Debuger.LogError("角色对象池使用了仍在使用的角色 m_initRoles id:{0}", role.Id);
        else if (m_roles.ContainsKey(role.Id))
            Debuger.LogError("角色对象池使用了仍在使用的角色 m_roles id:{0}", role.Id);
        else if (m_deadRoles.ContainsKey(role.Id))
            Debuger.LogError("角色对象池使用了仍在使用的角色 m_deadRoles id:{0}", role.Id);

        role.State = Role.enState.init;
        m_initRoles[role.Id] = role;
        if (!role.Init(cfg))
        {
            Debuger.LogError("初始化角色失败");
            if (role.State == Role.enState.init)//这里要先判断下是不是这个状态，内部可能调用到了AliveRole
                DestroyRole(role);
            return null;
        }

        //设置相关属性
        role.SetInt(enProp.camp, (int)cxt.camp);
        role.SetInt(enProp.level, cxt.level);
        role.SetString(enProp.roleId, cxt.roleId);
        role.SetString(enProp.name, cfg.name);


        //加载模型
        if (!dontLoadModel)
            role.Load(cxt);
        else
            role.PropPart.FreshBaseProp();
        return role;
    }

    public Role CreateGlobalEnemy(enCamp camp)
    {
        var info = SceneMgr.instance.GetBornInfo();
        if (camp == enCamp.max || info == null)
        {
            m_globalEnemy = null;
            m_globalEnemyId = 0;
            return null;
        }

        RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
        cxt.roleId = RoleMgr.GlobalEnemyId;
        cxt.level = Room.instance.roomCfg.levelLv;
        cxt.camp = camp;
        cxt.pos = info == null ? Vector3.zero : info.mPosition;
        cxt.euler = info.mEulerAngles;

        if (m_globalEnemy != null && !m_globalEnemy.IsUnAlive(m_globalEnemyId))
        {
            Debuger.LogError("创建全局敌人的时候发现之前的全局敌人没有销毁");
            DestroyRole(m_globalEnemy);
        }

        Role globalEnemy = CreateRole(cxt);
        m_globalEnemy = globalEnemy;
        m_globalEnemyId = globalEnemy.Id;
        LockRole(globalEnemy);
        return globalEnemy;
    }

    //锁定角色，如果这个角色被杀死会报错，用于调试和防止全局敌人被销毁
    public void LockRole(Role r)
    {
        r.AddFlag(RoleMgrLocking);
    }

    public void UnlockRole(Role r, bool reset = false)
    {
        if (reset)
            r.SetFlag(RoleMgrLocking, 0);
        else
            r.AddFlag(RoleMgrLocking, -1);
    }

    public void AliveRole(Role role)
    {
        if (!m_initRoles.ContainsKey(role.Id))
        {
            Debuger.LogError("角色不在m_initRoles中 id:{0} 状态:{1}", role.Id, role.State);
            return;
        }
        else
            m_initRoles.Remove(role.Id);
        role.State = Role.enState.alive;
        m_roles[role.Id] = role;
    }

    public void DeadRole(Role role, bool isGround = false, bool checkLocking = true, bool bHeroKill = true)
    {
        if (checkLocking && role.GetFlag(RoleMgrLocking) != 0)
        {
            Debuger.LogError("逻辑错误，锁定中的角色不能被杀死:{0}", role.Cfg.id);
            return;
        }

        if (role.RoleModel != null && role.RoleModel.IsUpdating)
        {
            //Debuger.Log("更新中死亡");
            role.RoleModel.IsGround = isGround;
            m_updatingDead = role;
            return;
        }

        if (!m_roles.ContainsKey(role.Id))
        {
            Debuger.LogError("角色不在m_roles中 id:{0} 状态:{1}", role.Id, role.State);
            return;
        }
        else
            m_roles.Remove(role.Id);

        role.State = Role.enState.dead;

        RoleStateDeadCxt cxt;
        if (isGround)
            cxt = new RoleStateDeadCxt(role.RoleBornCxt.groundDeadAniId);
        else
            cxt = new RoleStateDeadCxt(role.RoleBornCxt.deadAniId);
        cxt.bHeroKill = bHeroKill;
        role.RSM.GotoState(enRoleState.dead, cxt);
        m_deadRoles[role.Id] = role;
    }

    public bool CheckDead()
    {
        if (m_updatingDead == null)
            return false;

        if (m_updatingDead.State == Role.enState.alive)//有可能已经被销毁了，这里要判断下
            DeadRole(m_updatingDead, m_updatingDead.RoleModel.IsGround);
        m_updatingDead = null;
        return true;
    }

    public bool IsNeedDead(Role r)
    {
        return m_updatingDead == r;
    }

    public void DestroyRole(Role role, bool notDestroyNet = true, bool checkLocking = true)
    {

        if (role.RoleModel != null && role.RoleModel.IsUpdating)
        {
            //Debuger.Log("更新中销毁");
            m_updatingDestroy = role;
            return;
        }

        //从索引表里删除
        bool needDestroyModel = false;
        if (role.State == Role.enState.init)
        {
            if (!m_initRoles.ContainsKey(role.Id))
                Debuger.LogError("角色不在m_initRoles中 id:{0} 状态:{1}", role.Id, role.State);
            else
                m_initRoles.Remove(role.Id);
        }
        else if (role.State == Role.enState.alive)
        {
            if (!m_roles.ContainsKey(role.Id))
                Debuger.LogError("角色不在m_roles中 id:{0} 状态:{1}", role.Id, role.State);
            else
                m_roles.Remove(role.Id);
            needDestroyModel = true;

        }
        else if (role.State == Role.enState.dead)
        {
            if (!m_deadRoles.ContainsKey(role.Id))
                Debuger.LogError("角色不在m_deadRoles中 id:{0} 状态:{1}", role.Id, role.State);
            else
                m_deadRoles.Remove(role.Id);
            needDestroyModel = true;

        }
        else
        {
            Debuger.LogError("未知的状态 id:{0} 状态:{1}", role.Id, role.State);
            return;
        }

        //检查下锁定
        if (role.State != Role.enState.init && checkLocking && role.GetFlag(RoleMgrLocking) > 0)
        {
            Debuger.LogError("逻辑错误，锁定中的角色不能被销毁:{0}", role.Cfg.id);
            return;
        }


        //先设置状态，注意要再下面OnDestroy之前，不然可能出现销毁中可以使用技能或者添加状态而没有任何报错的情况
        bool needDestroyRole = !(role.IsNetRole && notDestroyNet);
        if (needDestroyRole)
            role.State = Role.enState.none;
        else
        {
            role.State = Role.enState.init;
            m_initRoles[role.Id] = role;
        }

        //模型销毁，如果需要的话
        if (needDestroyModel)
            role.OnDestroy();

        //放回对象池，如果需要的话
        if (needDestroyRole)
        {
            if (m_hero == role)
                m_hero = null;
            IdTypePool<Role>.Put(role);
        }
        //重置下模型绑定的监听，保证切换场景的时候不会残留
        else
            role.RemoveModelObjs();
    }

    public bool CheckDestroy()
    {
        if (m_updatingDestroy == null)
            return false;

        DestroyRole(m_updatingDestroy);
        m_updatingDestroy = null;
        return true;
    }

    public bool IsNeedDestroy(Role r)
    {
        return m_updatingDestroy == r;
    }

    public void DestroyAllRole(bool isIncludeHero = true, bool checkLocking = true)
    {
        List<Role> l = new List<Role>();
        l.AddRange(m_initRoles.Values);
        l.AddRange(m_roles.Values);
        l.AddRange(m_deadRoles.Values);
        if (!isIncludeHero)
        {
            if (Hero.State == Role.enState.alive)
            {
                l.Remove(Hero);
                List<Role> pets = Hero.PetsPart.GetMainPets();
                foreach (Role p in pets)
                {
                    if (p != null && p.State == Role.enState.alive)
                        l.Remove(p);
                }
            }
        }

        for (int i = 0; i < l.Count; ++i)
        {
            DestroyRole(l[i], true, checkLocking);
        }

    }

    public Role GetRole(int id)
    {
        return m_roles.Get(id);
    }

    public Role GetRoleByRoleId(string roleId)
    {
        foreach (Role r in m_roles.Values)
        {
            if (r.GetString(enProp.roleId) == roleId)
                return r;
        }
        return null;
    }

    public List<Role> GetRolesByRoleId(string roleId)
    {
        List<Role> l = new List<Role>();
        foreach (Role r in m_roles.Values)
        {
            if (r.GetString(enProp.roleId) == roleId)
                l.Add(r);
        }
        return l;
    }


    float OrderRole(Role source, Role target, enOrderRole orderType)
    {
        switch (orderType)
        {
            case enOrderRole.normal: return float.MinValue;
            case enOrderRole.closest: return source.DistanceSq(target);
            default:
                {
                    Debuger.LogError("未知的类型:{0}", orderType);
                    return 0;
                }
        }
    }

    //找到有这个标记的角色，如果n=-1,那么有标记就行了，否则标记要==n
    public Role GetRoleByFlag(string flag, int n = -1, Role except = null, Role source = null, enOrderRole orderType = enOrderRole.normal)
    {
        float min = float.MaxValue;
        Role find = null;
        int tem;

        source = source == null ? Hero : source;

        foreach (Role r in m_roles.Values)
        {
            if (r == except)
                continue;
            tem = r.GetFlag(flag);
            if (!(n == -1 && tem != 0) && !(n != -1 && tem == n))
                continue;

            float order = OrderRole(source, r, orderType);
            if (order == float.MinValue)
                return r;
            else if (order < min)
            {
                min = order;
                find = r;
            }
        }
        return find;
    }


    //找到当前角色id的下一个角色
    public Role FindNextRole(string roleId, Role curRole)
    {
        List<Role> l = RoleMgr.instance.GetRolesByRoleId(roleId);
        if (l.Count == 0) return null;

        int i = l.IndexOf(curRole);
        if (i == -1)
            return l[0];
        else if (i == 0 && l.Count == 1)//有且只有当前这一个的情况
            return null;
        else if (i == l.Count - 1)//最后一个的话，选择第一个
            return l[0];
        else//选择下一个
            return l[i + 1];
    }

    public Role GetClosestTarget(Role source, Vector3 srcPos, enSkillEventTargetType targetType, bool canBeTrap = false, bool canBeBox = false, Role except = null)
    {
        Role target = null;
        float d1 = 0, d2 = 0;


        foreach (Role r in m_roles.Values)
        {
            if (r == except)
                continue;
            //怪物类型限制
            if (r.Cfg.roleType == enRoleType.box && !canBeBox)
                continue;
            if (r.Cfg.roleType == enRoleType.trap && !canBeTrap)
                continue;

            //阵营限制
            if (!MatchTargetType(targetType, source, r))
                continue;

            d2 = (srcPos - r.transform.position).sqrMagnitude;
            if (target == null || d2 < d1)
            {
                target = r;
                d1 = d2;
            }
        }

        return target;
    }

    public Role GetClosestTarget(Role source, enSkillEventTargetType targetType, bool canBeTrap = false, bool canBeBox = false, Role except = null)
    {
        Vector3 srcPos = source.transform.position;
        return GetClosestTarget(source, srcPos, targetType, canBeTrap, canBeBox, except);
    }



    //获取某类型的对象，获取之后按照由近到远的顺序排列,注意SortedList创建的时候要用这个比较器new RoleMgr.CloseComparer()，否则add键值一样的话会报错
    public void GetCloseTargets(Role source, enSkillEventTargetType targetType, ref SortedList<float, Role> targets, bool canBeTrap = false, bool canBeBox = false)
    {

        if (targets.Count != 0) targets.Clear();
        Vector3 srcPos = source.transform.position;

        //遍历进行碰撞检测
        foreach (Role r in m_roles.Values)
        {
            if (r.Cfg.roleType == enRoleType.box && !canBeBox)
                continue;
            if (r.Cfg.roleType == enRoleType.trap && !canBeTrap)
                continue;

            //阵营限制
            if (!MatchTargetType(targetType, source, r))
                continue;

            targets.Add((srcPos - r.transform.position).sqrMagnitude, r);
        }
    }



    public Role GetTarget(Role source, RangeCfg c)
    {
        Vector3 srcPos = source.TranPart.Pos;
        Vector3 srcDir = source.transform.forward;
        //遍历进行碰撞检测
        foreach (Role r in m_roles.Values)
        {
            //碰撞检测
            if (!CollideUtil.Hit(srcPos, srcDir, r.TranPart.Pos, r.RoleModel.Radius, c))
                continue;

            return r;
        }
        return null;
    }


    public bool MatchTargetType(enSkillEventTargetType targetType, Role source, Role target)
    {
        if (targetType == enSkillEventTargetType.selfAlway || targetType == enSkillEventTargetType.self)
            return source == target;

        if (targetType == enSkillEventTargetType.target)
        {
            Skill s = source.CombatPart.CurSkill;
            return (s != null && s.Target == target);
        }

        enCamp srcCamp = source.GetCamp();
        enCamp targetCamp = target.GetCamp();
        if (targetType == enSkillEventTargetType.enemy)
            return srcCamp != enCamp.neutral && targetCamp != enCamp.neutral && srcCamp != targetCamp;
        else if (targetType == enSkillEventTargetType.same)
            return srcCamp == targetCamp && source != target;
        else if (targetType == enSkillEventTargetType.neutral)
            return targetCamp == enCamp.neutral;
        else if (targetType == enSkillEventTargetType.selfSame)
            return srcCamp == targetCamp;
        else if (targetType == enSkillEventTargetType.exceptSelf)
            return source != target;
        else
            Debuger.LogError("未知的类型:{0}", targetType);

        return false;
    }

    public bool IsEnemy(Role a, Role b)
    {
        if (a == null || b == null || a.State != Role.enState.alive || b.State != Role.enState.alive)
        {
            Debuger.LogError("角色死亡或者不存在了，不能判断是不是敌人");
            return true;
        }
        enCamp campA = a.GetCamp();
        enCamp campB = b.GetCamp();
        if (campA == enCamp.neutral || campB == enCamp.neutral)
            return false;

        return campA != campB;
    }

    //显示或者隐藏所有角色
    public void ShowAllRole(bool show, bool isIncludeHero = true)
    {
        List<Role> l = new List<Role>();
        l.AddRange(m_roles.Values);
        l.AddRange(m_deadRoles.Values);
        if (!isIncludeHero)
        {
            if (Hero.State == Role.enState.alive)
            {
                l.Remove(Hero);
            }
        }

        RoleModel roleModel;
        for (int i = 0; i < l.Count; ++i)
        {
            roleModel = l[i].RoleModel;
            if (roleModel != null)
            {
                roleModel.Show(show);
            }
        }
    }

}
