#region Header
/**
 * 名称：耐力触发
 
 * 日期：2016.4.19
 * 描述：
耐力百分比,状态列表,作用对象,释放者
当自身耐力降到一定值以下时触发，注意是状态开始时如果已经达到条件，那么马上触发
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public class BuffTriggerMpCfg : BuffExCfg
{
    public LvValue value;
    public List<int> buffIds = new List<int>();
    public enBuffTargetType targetType = enBuffTargetType.self;
    public enBuffTargetType sourceType = enBuffTargetType.self;


    public override bool Init(string[] pp)
    {
        if (pp.Length < 2)
            return false;

        //血量百分比
        value = new LvValue(pp[0]);
        if (value.error)
            return false;

        //状态列表
        int i = 0;
        if (int.TryParse(pp[1], out i))
            buffIds.Add(i);
        else
        {
            if (!StringUtil.TryParse(pp[1].Split('|'), ref buffIds))
                return false;
        }

        //作用对象
        if (pp.Length > 2 && int.TryParse(pp[2], out i))
            targetType = (enBuffTargetType)i;
        //释放者
        if (pp.Length > 3 && int.TryParse(pp[3], out i))
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


public class BuffTriggerMp : Buff
{
    public BuffTriggerMpCfg ExCfg { get { return (BuffTriggerMpCfg)m_cfg.exCfg; } }
    bool m_trigger=false;//是不是已经触发过
    
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        m_trigger = false;

    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            LogError("不需要执行多次");
            return;
        }
    }

    //每帧更新
    public override void OnBuffUpdate() {
        Check();
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        
    }

    void Check()
    {
        float percent = Parent.GetPercent(enProp.mp, enProp.mpMax);
        float v = this.GetLvValue(ExCfg.value);

        //重置下，有时候会加回去
        if (m_trigger && percent > v)
        {
            m_trigger = false;
            return;
        }

        if (m_trigger || percent > v)
            return;
        m_trigger = true;

        int poolId = this.Id;
        int parentId = m_parent.Id;
        

        //作用对象
        Role buffTarget = this.GetRole(ExCfg.targetType,null);
        if (buffTarget == null)
            return;
        BuffPart buffPart = buffTarget.BuffPart;

        //释放者
        Role buffSource = this.GetRole(ExCfg.sourceType, null);
        if (buffSource == null)
            return;

        for (int i = 0; i < ExCfg.buffIds.Count; ++i)
        {
            buffPart.AddBuff(ExCfg.buffIds[i], buffSource);
            if (IsUnneedHandle(poolId, m_parent, parentId))
                return;
        }
    }
}

