#region Header
/**
 * 名称：主动印记触发
 
 * 日期：2016.4.19
 * 描述：
印记名，达到数量，状态列表,作用对象类型
当角色给别人加印记的时候

 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public class BuffTriggerFlagCfg : BuffExCfg
{
    public string flag;
    public int num;
    public List<int> buffIds = new List<int>();
    public enBuffTargetType targetType = enBuffTargetType.self;
    public enBuffTargetType sourceType = enBuffTargetType.self;

    public override bool Init(string[] pp)
    {
        if (pp.Length < 3)
            return false;

        //印记名
        flag = pp[0];
        if (string.IsNullOrEmpty(pp[0]))
            return false;

        //数量
        if (!int.TryParse(pp[1], out num) || num<=0)
            return false;

        //状态列表
        int i = 0;
        if (int.TryParse(pp[2], out i))
            buffIds.Add(i);
        else
        {
            if (!StringUtil.TryParse(pp[2].Split('|'), ref buffIds))
                return false;
        }

        //作用对象
        if (pp.Length >3 && int.TryParse(pp[3], out i))
            targetType = (enBuffTargetType)i;

        //释放者
        if (pp.Length > 4 && int.TryParse(pp[4], out i))
            sourceType = (enBuffTargetType)i;

        return true;
    }
    public override void PreLoad()
    {
        for (int i = 0; i < buffIds.Count; ++i)
        {
            BuffCfg.ProLoad(buffIds[i]);
        }
    }
}

public class BuffTriggerFlag : Buff
{
    public BuffTriggerFlagCfg ExCfg { get { return (BuffTriggerFlagCfg)m_cfg.exCfg; } }
    public Dictionary<int, int> m_counter = new Dictionary<int, int>();
    int m_observer;
    
   
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        
       
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("主动事件触发状态不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }
        //监听技能事件
        m_observer = m_parent.Add(MSG_ROLE.SOURCE_ADD_FLAG, OnEvent);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
        m_counter.Clear();
    }

   
    void OnEvent(object p, object p2, object p3, EventObserver ob)
    {
        Role target = (Role)p;
        string flag= (string)p2;
        int num = (int)p3;
        if (flag != ExCfg.flag)
            return;

        int poolId = this.Id;
        int parentId = m_parent.Id;
        int targetId = target.Id;

        //作用对象
        Role buffTarget = this.GetRole(ExCfg.targetType, target);
        if (buffTarget == null)
            return;
        BuffPart buffPart = buffTarget.BuffPart;

        //释放者
        Role buffSource = this.GetRole(ExCfg.sourceType, target);
        if (buffSource == null)
            return;

        int oldCount = m_counter.Get(target.Id);
        int count = oldCount + num;
        while (count >=ExCfg.num)
        {
            count -= ExCfg.num;
            for (int i = 0; i < ExCfg.buffIds.Count; ++i)
            {
                buffPart.AddBuff(ExCfg.buffIds[i], buffSource);
                if (IsUnneedHandle(poolId, m_parent, parentId, target, targetId))
                    return;
            }
        }
        if (oldCount != count)
            m_counter[target.Id] = count;
    }
    
  
}

