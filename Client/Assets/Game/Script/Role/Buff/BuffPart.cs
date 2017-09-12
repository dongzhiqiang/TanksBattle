#region Header
/**
 * 名称：状态部件
 
 * 日期：2016.2.7
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BuffPart:RolePart
{
    #region Fields
    LinkedList<Buff> m_buffs = new LinkedList<Buff>();
    Dictionary<enBuff,int> m_typeCounter = new Dictionary<enBuff,int>();//判断某个类型的状态有多少个
    Dictionary<int, float> m_lastAddTime = new Dictionary<int, float>();

    LinkedListNode<Buff> m_next;//用于遍历中写操作的时候防止遍历出错的变量
    
    bool m_isNextFinding = false;

    bool m_isPostIniting = false;
    HashSet<int> m_unAlives = new HashSet<int>();
    #endregion


    #region Properties

    public override enPart Type { get { return enPart.buff; } }
    public LinkedList<Buff> Buffs { get{return m_buffs;}}
    #endregion


    #region Frame    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        if (m_buffs.Count != 0)
        {
            Debuger.LogError("逻辑出错，角色创建的时候发现有状态残留");
            m_buffs.Clear();
        }

        if (m_typeCounter.Count !=0)
        {
            Debuger.LogError("逻辑出错，角色创建的时候发现有状态计数残留");
            m_typeCounter.Clear();
        }

        return true;
    }
    
    //角色销毁的时候被调用，只有角色创建的部件会被调用，模型创建的和外部的部件不会调用
    public override void OnClear()
    {
        Buff buff = null;
        LinkedListNode<Buff> cur = null;
        LinkedListNode<Buff> next = m_buffs.First;
        while (next != null)
        {
            cur = next;
            buff = cur.Value;
            next = cur.Next;
            buff.OnStop(true);
        }
        m_buffs.Clear();
        m_typeCounter.Clear();
        m_lastAddTime.Clear();
        m_unAlives.Clear();
    }

    
    //模型创建的时候被调用
    public override void OnPostInit()
    {
        m_isNextFinding = true;
        m_lastAddTime.Clear();
        Buff buff = null;
        LinkedListNode<Buff> cur = null;
        m_next = m_buffs.First;
        while (m_next != null)
        {
            cur = m_next;
            buff = cur.Value;
            m_next = cur.Next;
            buff.ModelInit();
        }
        m_isNextFinding = false;


        //非战斗战斗状态且该状态类型不支持非战斗的延迟到角色出生加
        m_isPostIniting = true;
        foreach (var buffId in m_unAlives)
        {
            AddBuff(buffId);
        }
        m_isPostIniting = false;

        //角色出生状态
        List<int> bornBuffs = m_parent.Cfg.bornBuffs;
        for (int i = 0; i < bornBuffs.Count; ++i)
        {
            AddBuff(bornBuffs[i]);
        }

        //气力状态
        if (m_parent.RuntimeShieldBuff > 0)
        {
            BuffCfg cfg = BuffCfg.Get(m_parent.RuntimeShieldBuff);
            if (cfg == null || cfg.endBuffId.Length == 0)
                Debuger.LogError("找不到气力状态或者气绝状态(在气力状态的结束状态里):{0}", m_parent.Cfg.id);
            else
                AddBuff(m_parent.RuntimeShieldBuff);
        }

        
    }

    
    //模型销毁时被调用
    public override void OnDestroy() {
        m_isNextFinding = true;
        Buff buff= null;
        LinkedListNode<Buff> cur = null;
        m_next = m_buffs.First;
        while (m_next != null)
        {
            cur = m_next;
            buff = cur.Value;
            m_next = cur.Next;
            buff.ModelDestroy();
        }
        m_isNextFinding = false;
    }

    //每帧更新
    public override void OnUpdate() {
        //如果角色死了，不要更新
        if (Parent.State != Role.enState.alive)
            return;
        
        m_isNextFinding = true;//延迟删除标记，遍历中要延迟删除
        Buff buff = null;
        LinkedListNode<Buff> cur = null;
        m_next = m_buffs.First;
        while (m_next != null)
        {
            cur = m_next;
            buff = cur.Value;
            m_next = cur.Next;
            buff.Update();
            

        }
        m_isNextFinding=false;
    } 

    #endregion


    #region Private Methods
    //组和组优先级判断，优先级相同的情况下将看时间,如果需要删除掉一个状态那么在needRemove中返回
    bool CanAddToGroup(BuffCfg cfg,ref Buff needRemove)
    {
        needRemove= null;
        if(cfg.BuffType == enBuff.min)return false;
        BuffType typeCfg = BuffType.Get(cfg.BuffType);

        //默认组切类型是叠加类型的情况下一定可以加
        if(typeCfg.overlay&&string.IsNullOrEmpty(cfg.group))
            return true;
        
        //遍历一次收集一些消息,以做进一步的判断
        LinkedListNode<Buff> next = m_buffs.First;
        Buff buff;
        bool isSameGroup;
        while (next != null)
        {
            buff = next.Value;
            next = next.Next;
            if(buff.Cfg.BuffType != cfg.BuffType)continue;
            
            isSameGroup = !typeCfg.overlay ||(!string.IsNullOrEmpty(cfg.group) &&cfg.group == buff.Cfg.group );//默认组可以认为不是同个组，类型不可叠加的状态可以认为永远是同个组
            if(!isSameGroup)continue;//不同组的话互相之间没有关系

            //战斗状态不能替换非战斗状态
            if (!buff.Cfg.IsAliveBuff && cfg.IsAliveBuff)
                return false;

            //有优先级更高的
            if(buff.Cfg.groupPriority >cfg.groupPriority||
                (buff.Cfg.useOldIfSame && buff.Cfg.groupPriority == cfg.groupPriority))
                return false;

            //不可叠的情况，同优先级的话看谁的剩余时长多
            if(buff.Cfg.groupPriority ==cfg.groupPriority)
            {
                if(buff.TimeLeft<0|| (cfg.time >0 && buff.TimeLeft >=cfg.time))   
                    return false;    
            }

            //被替换的情况
            needRemove=  buff;
            
            return true;
        }
        
        return true;
    }
    #endregion
    public Buff AddBuff(int id, Role source = null)
    {
        if(id<=0)
        {
            Debuger.LogError("传进来一个无效的状态id:{0}",id);
            return null;
        }
        BuffCfg cfg = BuffCfg.Get(id);
        if(cfg== null)
            return null;
        
        return AddBuff(cfg, source==null ?this.Parent: source);
    }

    public Buff AddBuff(BuffCfg cfg,Role source)
    {
        string roleId= m_parent.Cfg.id;
        //if (cfg.id == 1)
        //{
        //    Debuger.Log("{0}添加了状态:{1}", roleId, cfg.id);
        //}
        //一些检错
        if (cfg == null){
            Debuger.LogError("{0}添加状态的时候发现配置为空",roleId);
            return null;
        }
        if (cfg.IsAliveBuff && Parent.State != Role.enState.alive&&!Parent.IsLoading)//只有角色已经创建好或者创建中才可以添加战斗状态
        {
            Debuger.LogError("{0}不能在角色没有加载模型的时候添加战斗状态:{1}", roleId, cfg.id);
            return null;
        }

        //非战斗状态只能在特定的场景加,如果是不支持非战斗逻辑的状态，会延迟到角色出生的时候才加上
        
        if (!cfg.IsAliveBuff )
        {
            if (!cfg.IsAliveBuff && cfg.interval != -1)//非战斗状态不能有时间间隔
            {
                Debuger.LogError("{0}非战斗状态不能有时间间隔:{1}", roleId, cfg.id);
                return null;
            }
            if (!cfg.IsAliveBuff && cfg.endBuffId != null && cfg.endBuffId.Length > 0)//非战斗状态不能有结束状态
            {
                Debuger.LogError("{0}非战斗状态不能有结束状态:{1}", roleId, cfg.id);
                return null;
            }

            //init中或者处于合适的场景可以加非战斗状态
            if (LevelMgr.instance.CurLevel==null||LevelMgr.instance.CurLevel.CanAddUnaliveBuff|| Parent.State == Role.enState.init)
            {
                if (!cfg.exCfg.SupportUnalive)//加到列表以延迟加
                {
                    if (!m_unAlives.Add(cfg.id))
                        Debuger.LogError("非战斗状态不支持重复添加.状态id:{0}", cfg.id);

                    return null;
                }
                //else
                //    允许加支持非战斗逻辑的非战斗状态
            }
            else
            {
                if (!m_isPostIniting)//延迟加的时候不报错
                {
                    Debuger.LogError("{0}非战斗状态只能在特定的场景加,状态id:{1} scene:{2}", roleId, cfg.id, LevelMgr.instance.CurLevel.GetType().Name);
                    return null;
                }
                //else
                //    允许加不支持非战斗逻辑的非战斗状态
                
            }
        }


        //是不是在cd中
        if (cfg.cd > 0)
        {
            float lastTime;
            if(m_lastAddTime.TryGetValue(cfg.id,out lastTime)&& (TimeMgr.instance.logicTime - lastTime) < cfg.cd)
            {
                Debuger.Log("状态cd中，加不上:{0}", cfg.id);
                return null;
            }
        }

        //同id叠加先判断是不是加不上去
        if (cfg.useOldIfSame && cfg.overlayLimit >= 1 && GetBuffIdCount(cfg.id) >= cfg.overlayLimit)
            return null;

        //有没有人否决状态的创建
        if (!m_parent.Fire(MSG_ROLE.BUFF_ADD_AVOID, cfg))
            return null;
        
        //组和组优先级判断，
        Buff needRemove =null;
        bool canAdd = CanAddToGroup(cfg, ref needRemove);
        if (!canAdd)
        {
            //Debuger.Log("{0}同组优先级低于其他状态，不能添加:{1} {2}",m_parent.Cfg.id, cfg.id,cfg.type);
            return null;
        }
            
        if (needRemove != null) //有要替换的状态的话删除
            RemoveBuff(needRemove);

        //同id叠加限制,超过这个限制的话删掉最老的那个同id的状态
        if (cfg.overlayLimit>=1){
            int sameCount = 0;
            Buff oldBuff;
            LinkedListNode<Buff> cur = null;
            LinkedListNode<Buff> next = m_buffs.First;
            while (next != null)
            {
                cur = next;
                oldBuff = cur.Value;
                next = cur.Next;
                if (oldBuff.Cfg.id != cfg.id)continue;

                ++sameCount;
                if (sameCount >= cfg.overlayLimit)
                {
                    RemoveBuff(oldBuff);
                    break;
                }
            }

        }

        //创建状态
        Buff buff = BuffFactory.CreateBuff(cfg.BuffType);
        if (buff == null)return null;

        //状态列表维护
        m_buffs.AddLast(buff);//先执行
        if (m_isNextFinding)//如果是在遍历,那么要检查下next
        {
            if(m_next == null)
                m_next = m_buffs.Last;    
        }
        int counter =0;
        if(!m_typeCounter.TryGetValue(cfg.BuffType, out counter))
            counter= 1;
        else
            ++counter;
        m_typeCounter[cfg.BuffType]=counter;

        //初始化
        m_lastAddTime[cfg.id] = TimeMgr.instance.logicTime;
        buff.Init(this.Parent, source, cfg);
        if (Parent.State == Role.enState.alive)
            buff.ModelInit();

        m_parent.Fire(MSG_ROLE.BUFF_ADD, cfg, source);
        
        //Debuger.Log("{0}添加状态:{1} {2}", m_parent.Cfg.id,cfg.id, cfg.type);
        return buff;
    }
    public void RemoveBuff(LinkedListNode<Buff> node, bool isClear = false)
    {
        //状态列表维护
        if (m_isNextFinding)//如果是在遍历,那么要检查下next
        {
            if (m_next == node)
                m_next = node.Next;
        }
        enBuff bt =node.Value.Cfg.BuffType;
        int counter = 0;
        if (!m_typeCounter.TryGetValue(bt, out counter))
            counter = 0;
        else
            --counter;
        if (counter<0)
        {
            Debuger.LogError("逻辑错误，状态部件的类型计数小于0:{0} {1} {2}", m_parent.Cfg.id, node.Value.Cfg.id, node.Value.Cfg.type);
            counter =0;
        }
        m_typeCounter[bt] = counter;

        m_buffs.Remove(node);
        //Debuger.Log("{0}删除状态:{1} {2}", m_parent.Cfg.id, node.Value.Cfg.id, node.Value.Cfg.type);
        

        //销毁
        node.Value.OnStop(isClear);
    }

    public void RemoveBuffByIds(List<int> ids, bool isClear = false)
    {
        if (ids.Count == 0)
            return;
        foreach (int id in ids)
        {
            RemoveBuffById(id, isClear);
        }
    }

    public void  RemoveBuffById(int id, bool isClear = false)
    {
        Buff buff = null;
        LinkedListNode<Buff> cur = null;
        LinkedListNode<Buff> next = m_buffs.First;
        while (next != null)
        {
            cur = next;
            buff = cur.Value;
            next = cur.Next;
            if (buff.Id == id)
            {
                RemoveBuff(cur,isClear);
                return;
            }
        }
    }
    public void RemoveBuff(Buff buff, bool isClear = false)
    {
        if (buff == null)
        {
            Debuger.LogError("{0}要删除状态的时候发现状态为空", m_parent.Cfg.id);
            return;
        }
        Buff curBuff;
        LinkedListNode<Buff> cur = null;
        LinkedListNode<Buff> next = m_buffs.First;
        while (next != null)
        {
            cur = next;
            curBuff = cur.Value;
            next = cur.Next;
            if (curBuff == buff)
            {
                RemoveBuff(cur, isClear);
                return;
            }
        }

        Debuger.LogError("{0}要删除状态的时候发现状态已经被删除:{1}", m_parent.Cfg.id, buff.Cfg == null ? "" : buff.Cfg.id.ToString());
    }
    
    //按照状态id删除状态，-1则删除非战斗状态 -2删除所有战斗状态
    public int RemoveBuffByBuffId(int buffId, bool isClear = false)
    {
        //如果是非延迟状态，从延迟列表中删除
        m_unAlives.Remove(buffId);


        List<int> removes = new List<int>();
        int num=0;
        Buff buff = null;
        LinkedListNode<Buff> cur = null;
        LinkedListNode<Buff> next = m_buffs.First;
        while (next != null)
        {
            cur = next;
            buff = cur.Value;
            next = cur.Next;
            if ((buffId == -1 && !buff.Cfg.IsAliveBuff) || (buffId == -2 && buff.Cfg.IsAliveBuff) || (buffId == buff.Cfg.id))
            {
                removes.Add(buff.Id);
                ++num;
            } 
        }
        RemoveBuffByIds(removes,isClear);
        return num;
    }

    public void GetBuffsByType(enBuff b,ref List<Buff> buffs)
    {
        buffs.Clear();
        LinkedListNode<Buff> next = m_buffs.First;
        while (next != null)
        {
            if (next.Value.Cfg.BuffType == b)
            {
                buffs.Add(next.Value);
            }
        }
    }

    //返回找到的第一个id符合的状态，没有则返回null，如果传进来-1则为任意非战斗状态 -2任意战斗状态
    public Buff GetBuffByBuffId(int buffId)
    {
        Buff buff = null;
        LinkedListNode<Buff> cur = null;
        LinkedListNode<Buff> next = m_buffs.First;
        while (next != null)
        {
            cur = next;
            buff = cur.Value;
            next = cur.Next;
            if ((buffId == -1 && !buff.Cfg.IsAliveBuff) || (buffId == -2 && buff.Cfg.IsAliveBuff) || (buffId == buff.Cfg.id))
            {
               return buff;
            }
        }
        return null;
    }

    public int GetBuffTypeCount(enBuff b)
    {
        int c;
        if (!m_typeCounter.TryGetValue(b, out c))
            return 0;
        else
            return c;
    }

    public int GetBuffIdCount(int buffId)
    {
        int c = 0;
        
        LinkedListNode<Buff> next = m_buffs.First;
        while (next != null)
        {
            if ((buffId == -1 && !next.Value.Cfg.IsAliveBuff) || (buffId == -2 && next.Value.Cfg.IsAliveBuff) || (buffId == next.Value.Cfg.id))
            {
                ++c;
            }
            next = next.Next;
        }
        return c;
    }
}

