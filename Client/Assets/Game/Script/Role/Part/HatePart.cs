#region Header
/**
 * 名称：仇恨部件
 
 * 日期：2015.9.21
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RefGroupHateCfg
{
    public string flag = "";
    public int hate = 0;
    public void CopyFrom(RefGroupHateCfg cfg)
    {
        this.flag = cfg.flag;
        this.hate = cfg.hate;
    }
}

public class HateCfg
{
    //仇恨相关
    public int hateIfHit = 0;//攻击别人增加对此人的仇恨
    public int hateIfBeHit = 0;//被别人攻击增加对此人的仇恨
    public List<RefGroupHateCfg> hateIfRefGroup = new List<RefGroupHateCfg>();//增加对刷新组刷出来的敌人的仇恨
    public int hateIfChange = 0;//仇恨切换时再多增加对此人的仇恨
    public int behateIfBegin = 0;//所有敌人对此人的初始仇恨

    public void CopyFrom(HateCfg cfg)
    {
        this.hateIfHit = cfg.hateIfHit;
        this.hateIfBeHit = cfg.hateIfBeHit;
        this.hateIfChange = cfg.hateIfChange;
        this.behateIfBegin = cfg.behateIfBegin;
        hateIfRefGroup.Clear();
        foreach(var c in cfg.hateIfRefGroup)
        {
            var c2 = new RefGroupHateCfg();
            c2.CopyFrom(c);
            hateIfRefGroup.Add(c2);
        }
    }

    public void OnClear()
    {
        hateIfHit = 0;
        hateIfBeHit = 0;
        hateIfChange = 0;
        behateIfBegin = 0;
        hateIfRefGroup.Clear();
    }

#if UNITY_EDITOR
    public void Draw()
    {
        using(new AutoEditorTipButton("角色攻击其他角色时，增加对这个角色的仇恨"))
            hateIfHit = EditorGUILayout.IntField("攻击仇恨", hateIfHit);
        using (new AutoEditorTipButton("角色被其他角色攻击时，增加对这个角色的仇恨"))
            hateIfBeHit = EditorGUILayout.IntField("被击仇恨", hateIfBeHit);
        using (new AutoEditorTipButton("仇恨目标切换的时候，增加对仇恨目标的仇恨"))
            hateIfChange = EditorGUILayout.IntField("切换仇恨", hateIfChange);


        using (new AutoBeginHorizontal())
        {
            if(GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus More"), EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                hateIfRefGroup.Add(new RefGroupHateCfg());
            }
            GUILayout.Button("刷新组名", EditorStyles.toolbarButton);
            GUILayout.Button("增加仇恨", EditorStyles.toolbarButton, GUILayout.Width(70));
           
        }
        for (int i = 0, j = hateIfRefGroup.Count; i < j; ++i)
        {
            var v = hateIfRefGroup[i];
            using (new AutoBeginHorizontal())
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    hateIfRefGroup.RemoveAt(i);
                    break;
                }

                //类型
                v.flag =EditorGUILayout.TextField(v.flag);

                //名字
                v.hate=EditorGUILayout.IntField(v.hate, GUILayout.Width(70));
            }

        }
    }
#endif
}



//仇恨信息
public class HateInfo:IdType
{
    int targetId;//攻击者的id
    Role target;
    public int hate;

    public Role Target { get { return target.IsUnAlive(targetId) ?null:target; } }
    public int TargetId { get { return targetId; } }
    public void Init(Role target)
    {
        if(target.State!= Role.enState.alive)
        {
            Debuger.LogError("逻辑错误，角色已经死亡，不能作为仇恨对象");
        }
        this.target = target;
        this.targetId = target.Id;
    }

    public override void OnClear() {

        targetId = -1;
        target = null;
        hate = 0;
    }
}
public class HatePart:RolePart
{
    #region Fields
    //老的仇恨计算系统，用最后一个攻击或者被击者充当仇恨目标
    int m_lastBeHitId = -1;//最后被击者
    int m_lastHitId = -1; //最后攻击者
    float m_lastBeHitTime ;
    float m_lastHitTime;
    int m_lastBeHitHp;

    //新的仇恨值计算系统，用仇恨值判断仇恨目标
    LinkedList<HateInfo> m_hateInfos = new LinkedList<HateInfo>();
    Dictionary<int, LinkedListNode<HateInfo>> m_hateInfosIdxRole = new Dictionary<int, LinkedListNode<HateInfo>>();
    int m_lastHateTargetId = -1;
    int m_obRefGroup = EventMgr.Invalid_Id;
    int m_obBorn = EventMgr.Invalid_Id;
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.hate; } }
    public float LastHitTime { get{return m_lastHitTime;}}
    public float LastBeHitTime { get { return m_lastBeHitTime; } }
    public int LastBeHitHp { get{return m_lastBeHitHp;}}
    #endregion


    #region Frame    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
        m_lastBeHitId = -1;
        m_lastHitId = -1;
        m_lastBeHitTime = -1;
        m_lastHitTime = -1;
        m_lastBeHitHp = -1;
        m_lastHateTargetId = -1;

        //将角色表里的仇恨配置设置到角色上下文
        var hateCxt = Parent.RoleBornCxt.hate;
        var roleCfg = Parent.Cfg;
        hateCxt.behateIfBegin += roleCfg.behateIfBegin;
        hateCxt.hateIfHit += roleCfg.hateIfHit;
        hateCxt.hateIfBeHit += roleCfg.hateIfBeHit;
        hateCxt.hateIfChange += roleCfg.hateIfChange;

        //敌人对自己的初始仇恨
        if(hateCxt.behateIfBegin!=0)
        {
            foreach(var r in RoleMgr.instance.Roles)
            {
                CheckEnemyBornHate(r);
            }

            m_obBorn = EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.BORN, OnRoleBorn);
        }

        //对刷新组的仇恨的处理
        List<RefGroupHateCfg> refGroupHates = hateCxt.hateIfRefGroup;
        if(refGroupHates.Count!=0)
        {
            foreach(var r in RoleMgr.instance.Roles)
            {
                if (r.Cfg.roleType == enRoleType.trap )
                    continue;
                CheckRefGroupHate(r);
            }
                

            m_obRefGroup = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.ROLEENTER, OnSceneRoleCreate);
        }
            
    }
    
    //模型销毁时被调用
    public override void OnDestroy()
    {
        var n = m_hateInfos.Last;
        while(n != null){
            n.Value.Put();
            n = n.Previous;
        }
        m_hateInfos.Clear();
        m_hateInfosIdxRole.Clear();

        if (m_obRefGroup != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_obRefGroup);
            m_obRefGroup = EventMgr.Invalid_Id;
        }
        if (m_obBorn != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_obBorn);
            m_obBorn = EventMgr.Invalid_Id;
        }
    }
    #endregion


    #region Private Methods
    void CheckRefGroupHate(Role r)
    {
        if (this.Parent.State != Role.enState.alive || r.State != Role.enState.alive)
        {
            return;
        }

        
        List<RefGroupHateCfg> refGroupHates = Parent.RoleBornCxt.hate.hateIfRefGroup;
        var flags = r.PropPart.FightFlags;
        for (int i = 0; i < refGroupHates.Count; ++i)
        {
            var hate = refGroupHates[i];
            if (string.IsNullOrEmpty(hate.flag) ||
                hate.hate == 0 ||
                !flags.ContainsKey(hate.flag))
                continue;
            AddHate(r, hate.hate);
            break;
        }
        
    }


    void OnSceneRoleCreate(object param)
    {
        Role r = (Role)param;
        CheckRefGroupHate(r);
    }

    void CheckEnemyBornHate(Role r)
    {
        if (this.Parent.State != Role.enState.alive || r.State != Role.enState.alive || !RoleMgr.instance.IsEnemy(r,Parent))
        {
            return;
        }

        var h = Parent.RoleBornCxt.hate.behateIfBegin;
        if(h > 0)
        {
            r.HatePart.AddHate(this.Parent, h);
        }
        

    }
    void OnRoleBorn(object param)
    {
        Role r = (Role)param;
        CheckEnemyBornHate(r);
    }
    #endregion

    //被击记录下仇恨
    public void BeHit(Role role, int hp)
    {
        int id = role.Id;
       
        m_lastBeHitHp = hp;
        m_lastBeHitId = id;
        m_lastHitId = -1;
        m_lastBeHitTime = Time.time;
        m_lastHitTime = -1;

        int hate = Parent.RoleBornCxt.hate.hateIfBeHit;
        if (hate != 0)
            AddHate(role, hate);
    }

    //攻击记录下被击者
    public void Hit(Role role)
    {
        m_lastHitId = role.Id;
        m_lastBeHitId =-1;
        m_lastBeHitTime = -1;
        m_lastHitTime = Time.time;

        int hate = Parent.RoleBornCxt.hate.hateIfHit;
        if (hate != 0)
            AddHate(role, hate);
    }

    public Role GetLastHit() {
        if (m_lastHitId== -1)
            return null;
        return RoleMgr.instance.GetRole(m_lastHitId);
    }

    public Role GetLastBehit()
    {
        if (m_lastBeHitId == -1)
            return null;
        return RoleMgr.instance.GetRole(m_lastBeHitId);
    }
    public Role GetTargetLegacy(bool autoFind =true)
    {
        Role r = HatePart.GetLastHit();
        if (r != null)
            return r;

        r = HatePart.GetLastBehit();
        if (r != null)
            return r;

        if (!autoFind)
            return null;

        return RoleMgr.instance.GetClosestTarget(Parent, enSkillEventTargetType.enemy);
    }

    public Role GetTarget(bool autoFind = true)
    {
        var node = m_hateInfos.Last;
        while (node != null)
        {
            if (node.Value.Target != null)
                break;

            m_hateInfos.RemoveLast();
            m_hateInfosIdxRole.Remove(node.Value.TargetId);
            node = m_hateInfos.Last;
        }

        if(node == null)
        {
            if (!autoFind)
                return null;

            return RoleMgr.instance.GetClosestTarget(Parent, enSkillEventTargetType.enemy);
        }

        //切换仇恨目标的时候增加仇恨
        Role target = node.Value.Target;
        if (target.Id!= m_lastHateTargetId)
        {
            m_lastHateTargetId = target.Id;

            //切换仇恨的时候增加下仇恨值
            int hateAdd = Parent.RoleBornCxt.hate.hateIfChange;
            if (hateAdd > 0)
                node.Value.hate += hateAdd;
        }

        return target;
      
    }
    public void AddHate(Role role,int hate)
    {
        int id = role.Id;
        var hateNode =m_hateInfosIdxRole.Get(id);
        HateInfo hateInfo =null;

        //没有的话先加到仇恨列表里
        if (hateNode == null)
        {
            hateInfo = IdTypePool<HateInfo>.Get();
            hateInfo.Init(role);

            //这里不能直接加到最前
            if (m_hateInfos.Count == 0)
                hateNode =m_hateInfos.AddLast( hateInfo);
            else
            {
                var node = m_hateInfos.First;
                while (node != null)
                {
                    if (node.Value.hate >= 0)
                        break;
                    node = node.Next;
                }

                if(node != null)
                    hateNode = m_hateInfos.AddBefore(node, hateInfo);
                else
                    hateNode = m_hateInfos.AddLast(hateInfo);
            }

            //加索引
            m_hateInfosIdxRole.Add(id, hateNode);
        }

        //排序
        hateInfo = hateNode.Value;
        hateInfo.hate += hate;
        if (hate<0)
        {
            var node = hateNode.Previous;
            while (node != null)
            {
                if (node.Value.hate<= hateNode.Value.hate)
                    break;
                node = node.Previous;
            }

            if(node != hateNode.Previous)
            {
                m_hateInfos.Remove(hateNode);
                if (node != null)
                    hateNode = m_hateInfos.AddAfter(node, hateInfo);
                else
                    hateNode = m_hateInfos.AddFirst( hateInfo);
                m_hateInfosIdxRole[id] = hateNode;
            }
        }
        else
        {
            var node = hateNode.Next;
            while (node != null)
            {
                if (node.Value.hate >= hateNode.Value.hate)
                    break;
                node = node.Next;
            }
            if (node != hateNode.Next)
            {
                m_hateInfos.Remove(hateNode);
                if (node != null)
                    hateNode = m_hateInfos.AddBefore(node, hateInfo);
                else
                    hateNode = m_hateInfos.AddLast( hateInfo);
                m_hateInfosIdxRole[id] = hateNode;
            }
        }
    }

    public HateInfo GetHate(Role role)
    {
        var hateNode = m_hateInfosIdxRole.Get(role.Id);
        if (hateNode == null)
            return null;
        else
            return hateNode.Value;
    }
    public string LogHates()
    {
        string log = "";
        log += string.Format("唯一id:{0}\n", m_parent.Id);
        Role r = GetLastHit();
        log += string.Format("最后攻击者id:{0} \n",r==null?"无":r.Id.ToString());
        r = GetLastBehit();
        log += string.Format("最后被击者id:{0} \n", r == null ? "无" : r.Id.ToString());

        var node = m_hateInfos.Last;
        while(node != null)
        {
            r = node.Value.Target;
            if(r!=null)
            {
                log += string.Format("仇恨:{0} id:{1}\n", node.Value.hate, r.Id);
            }
            node = node.Previous;
        }
          
        return log;
    }
}
