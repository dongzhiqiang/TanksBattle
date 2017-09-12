#region Header
/**
 * 名称：概率
 
 * 日期：2016.4.19
 * 描述：
概率,状态列表,作用对象,释放者
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class BuffPercentCfg : BuffExCfg
{
    public LvValue value;
    public List<int> buffIds = new List<int>();
    public enBuffTargetType targetType = enBuffTargetType.self;
    public enBuffTargetType sourceType = enBuffTargetType.self;

    public override bool Init(string[] pp)
    {
        if (pp.Length < 2)
            return false;

        //概率
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

public class BuffPercent : Buff
{
    public BuffPercentCfg ExCfg { get { return (BuffPercentCfg)m_cfg.exCfg; } }
    
    
    
   
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        
       
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        bool ret = UnityEngine.Random.value <= this.GetLvValue(ExCfg.value)  ;
        if (!ret)
            return;
        
        int poolId = this.Id;
        int parentId = m_parent.Id;

        //作用对象
        Role buffTarget = this.GetRole(ExCfg.targetType, null);
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

    //结束
    public override void OnBuffStop(bool isClear) {
        
        
    }
}

