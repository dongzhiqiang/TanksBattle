#region Header
/**
 * 名称：被动印记触发
 
 * 日期：2016.4.19
 * 描述：
印记名，达到数量，状态列表,作用对象类型,释放者
当别人给角色加印记的时候

 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public class BuffBeTriggerFlagCfg : BuffExCfg
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

public class BuffBeTriggerFlag : Buff
{
    public BuffBeTriggerFlagCfg ExCfg { get { return (BuffBeTriggerFlagCfg)m_cfg.exCfg; } }
    int m_counter=0;
    int m_observer;
    
   
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        if (m_counter != 0)
        {
            Debuger.LogError("被动印记触发状态初始化时counter不为0，状态id:{0}", m_cfg.id);
            return;
        }
       
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("被动印记触发不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }
        //监听技能事件
        m_observer = m_parent.Add(MSG_ROLE.TARGET_ADD_FLAG, OnEvent);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
        m_counter = 0;
    }

   
    void OnEvent(object p, object p2, object p3, EventObserver ob)
    {
        Role source = (Role)p;
        string flag= (string)p2;
        int num = (int)p3;
        if (flag != ExCfg.flag)
            return;

        int poolId = this.Id;
        int sourceId = source.Id;
        int parentId = m_parent.Id;

        //作用对象
        Role buffTarget = this.GetRole(ExCfg.targetType, source);
        if (buffTarget == null)
            return;
        BuffPart buffPart = buffTarget.BuffPart;

        //释放者
        Role buffSource = this.GetRole(ExCfg.sourceType, source);
        if (buffSource == null)
            return;

        int count = m_counter + num;
        while (count >=ExCfg.num)
        {
            count -= ExCfg.num;
            for (int i = 0; i < ExCfg.buffIds.Count; ++i)
            {
                buffPart.AddBuff(ExCfg.buffIds[i], buffSource);
                if (IsUnneedHandle(poolId, m_parent, parentId, source, sourceId))
                    return;
            }
        }
        if (m_counter != count)
            m_counter = count;
    }
    
  
}

