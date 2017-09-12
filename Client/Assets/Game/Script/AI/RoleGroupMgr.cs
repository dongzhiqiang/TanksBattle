#region Header
/**
 * 名称：角色群体管理器
 
 * 日期：2016.6.13
 * 描述：管理群体的攻击目标和包围目标，使用时要先把角色加到这个管理器，
 *      然后管理器就会把角色分配去攻击或者包围某个敌人。
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Simple.BehaviorTree;
public class RoleGroupMgr : SingletonMonoBehaviour<RoleGroupMgr>
{
    public const string Group_Attack_CD_Min = "群体攻击cd[";
    public const string Group_Attack_CD_Max = "群体攻击cd]";
    public const string Group_Attack_Min = "群体攻击[";
    public const string Group_Attack_Max = "群体攻击]";

    public class MemberInfo:IdType
    {
        public Role role;
        public float distanceSq;
        public Role target;

        public override void OnClear() {
            role = null;
            distanceSq = 0;
            target = null;
        }
    }

    public float m_attackCDMin = 5f;
    public float m_attackCDMax = 10f;
    public int m_attackMin = 1;//对同一个敌人的攻击数量，随机下限
    public int m_attackMax = 2;//对同一个敌人的攻击数量，随机上限
    public float m_intervalSecond = 1;//一秒1次，注意不宜太短，不然攻击者可能还不知道自己是攻击者就attackCD了

#if UNITY_EDITOR
    public string log ="";
#endif

    HashSet<int> m_members = new HashSet<int>();//组成员，可能被分配成攻击者，或者包围者
    Dictionary<int, int> m_attacks= new Dictionary<int, int>();//攻击者列表，key是攻击者，value是被攻击者
    Dictionary<int, int> m_rounds = new Dictionary<int, int>();//包围者列表，key是包围者，value是被包围者

    float m_lastAttackCd = 0;
    float m_lastAttackTime = 0;//最后需要重新获取攻击对象的时间
    float m_lastUpdateTime = 0;

    void Update()
    {
        if (TimeMgr.instance.IsPause)
        {
            
            return;
        }
            

        if (m_intervalSecond > 0 && TimeMgr.instance.logicTime - m_lastUpdateTime > (1f / m_intervalSecond))
        {
            m_lastUpdateTime = TimeMgr.instance.logicTime;
            CallUpdate();
        }
    }

    public void Clear()
    {
        m_lastUpdateTime = 0;
        m_lastAttackTime = 0;
        m_lastAttackCd = 0;
        m_attacks.Clear();
        m_rounds.Clear();
    }


    public void Add(Role r)
    {
        if(r == null || r.State != Role.enState.alive)
        {
            Debuger.LogError("角色不是alive状态不能添加到群体目标管理");
            return;
        }
        m_members.Add(r.Id);
    }

    

    public void Remove(Role r)
    {
        if (r == null || r.IsInPool)
        {
            Debuger.LogError("角色已经不无效，不能从群体目标管理器中删除");
            return;
        }
        m_members.Remove(r.Id);
    }

    List<List<MemberInfo>> m_temCampMembers = new List<List<MemberInfo>>() { null, null, null, null, null, null, null, null, null };
    HashSet<int> m_temMembers = new HashSet<int>();
    List<Role> m_temRoles = new List<Role>();
    Dictionary<Role, SortedList<float, MemberInfo>> m_temTargets = new Dictionary<Role, SortedList<float, MemberInfo>>();
    Stack<SortedList<float, MemberInfo>> m_temPool = new Stack<SortedList<float, MemberInfo>>();

    public void CallUpdate()
    {
        if (!this.enabled)
            return;

        
       
        //初步收集数据
        foreach (var r in RoleMgr.instance.Roles)
        {
            int camp = (int)r.GetCamp();

            if (r.Cfg.roleType == enRoleType.box || r.Cfg.roleType == enRoleType.trap || camp == (int)enCamp.neutral)
                continue;
            
            //群体成员生成信息
            if (m_members.Contains(r.Id))
            {
                m_temMembers.Add(r.Id);
                var l = m_temCampMembers[camp];
                if (l == null)
                {
                    l = new List<MemberInfo>();
                    m_temCampMembers[camp] = l;
                }

                MemberInfo info = IdTypePool<MemberInfo>.Get();
                info.role = r;
                l.Add(info);
            }    
            m_temRoles.Add(r);
        }
        
        //用替换的方式刷新当前alive的群体成员
        var tem = m_members;
        m_members = m_temMembers;
        m_temMembers = tem;
        m_temMembers.Clear();

        //找到每个群体成员的目标(距离最近的敌人)
        for (int i = 0;i< m_temRoles.Count;++i)
        {
            var r = m_temRoles[i];
            int camp = (int)r.GetCamp();
            for (int j = 0; j < m_temCampMembers.Count; ++j)
            {
                var l = m_temCampMembers[j];
                if (j == camp || l == null)
                    continue;
                
                for(int k = 0;k< l.Count;++k)
                {
                    var info = l[k];
                    var disSq = r.DistanceSq(info.role);
                    if (info.target != null && info.distanceSq <= disSq)
                        continue;
                    info.target =r;
                    info.distanceSq = disSq;
                }
            }
        }
        m_temRoles.Clear();

        //如果不需要攻击，那么不需要找到攻击者，算法会简单一点
        
        bool needAttack = TimeMgr.instance.logicTime - m_lastAttackTime > m_lastAttackCd;
        if(!needAttack)
        {
            
            m_attacks.Clear();
            m_rounds.Clear();
            for (int i = 0; i < m_temCampMembers.Count; ++i)
            {
                var l = m_temCampMembers[i];
                if (l == null)
                    continue;
                for (int j = 0; j < l.Count; ++j)
                {
                    var info = l[j];
                    if(info.target!=null)
                        m_rounds.Add(info.role.Id,info.target.Id);
                    l[j].Put();
                }

                l.Clear();
            }
#if UNITY_EDITOR
            log = string.Format("攻击者:{0} 包围者:{1}", m_attacks.Count, m_rounds.Count);
#endif
            return;
        }
        float cdMin = BehaviorTreeMgr.instance.GetValue(Group_Attack_CD_Min, m_attackCDMin);
        float cdMax = BehaviorTreeMgr.instance.GetValue(Group_Attack_CD_Max, m_attackCDMax);
        m_lastAttackCd = UnityEngine.Random.Range(cdMin, cdMax);
        m_lastAttackTime = TimeMgr.instance.logicTime;
        //如果需要攻击，那么要找到最近的攻击者，先整理出每个敌人最近的列表
        for (int i = 0; i < m_temCampMembers.Count; ++i)
        {
            var l = m_temCampMembers[i];
            if (l == null)
                continue;
            for (int j = 0; j < l.Count; ++j)
            {
                var info = l[j];
                if (info.target == null)
                    continue;
                SortedList<float, MemberInfo> l2 = null;
                if (!m_temTargets.TryGetValue(info.target, out l2))
                {
                    if (m_temPool.Count != 0)
                        l2 = m_temPool.Pop();
                    else
                        l2 = new SortedList<float, MemberInfo>(new RepeatableComparer<float>());
                    m_temTargets.Add(info.target, l2);
                }
                l2.Add(info.distanceSq, info);
            }
        }


        //找到最近的攻击者，最终收集出攻击者列表和包围者列表
        m_attacks.Clear();
        m_rounds.Clear();
        foreach(var pair in m_temTargets)
        {
            var target = pair.Key;
            var l = pair.Value;
            int min = BehaviorTreeMgr.instance.GetValue(Group_Attack_Min, m_attackMin);
            int max = BehaviorTreeMgr.instance.GetValue(Group_Attack_Max, m_attackMax);
            int needAttackCount = UnityEngine.Random.Range(min, max + 1);
            int attackCount = 0;
            foreach (var pair2 in l)
            {
                var role = pair2.Value.role;
                //var st = role.RSM.CurStateType;
                
                if (attackCount < needAttackCount)//&& st != enRoleState.beHit && st != enRoleState.ani)
                {
                    m_attacks.Add(role.Id, target.Id);
                    ++attackCount;
                }
                //加到包围者列表
                else
                    m_rounds.Add(role.Id, target.Id);
            }

            l.Clear();
            m_temPool.Push(l);
        }
        m_temTargets.Clear();
        for (int i = 0; i < m_temCampMembers.Count; ++i)
        {
            var l = m_temCampMembers[i];
            if (l == null)
                continue;
            for (int j = 0; j < l.Count; ++j)
                l[j].Put();

            l.Clear();
        }
#if UNITY_EDITOR
        log = string.Format("攻击者:{0} 包围者:{1}", m_attacks.Count, m_rounds.Count);
#endif
    }


    public Role GetAttackTarget(Role r)
    {
        int targetId;
        if (!m_attacks.TryGetValue(r.Id,out targetId))
            return null;

        return RoleMgr.instance.GetRole(targetId);
    }

    public Role GetRoundTarget(Role r)
    {
        int targetId;
        if (!m_rounds.TryGetValue(r.Id, out targetId))
            return null;

        return RoleMgr.instance.GetRole(targetId);
    }
}
