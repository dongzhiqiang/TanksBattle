#region Header
/**
 * 名称：重置仇恨
 
 * 日期：2016.7.28
 * 描述：
仇恨对象
将对仇恨对象的仇恨值重置为0
仇恨对象，见作用对象类型，默认为释放者
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public class BuffResetHateCfg : BuffExCfg
{
    public enBuffTargetType hateTargetType = enBuffTargetType.source;


    public override bool Init(string[] pp)
    {
       
        int i = 0;
        if (pp!= null &&pp.Length > 0 && int.TryParse(pp[0], out i))
            hateTargetType = (enBuffTargetType)i;



        return true;
    }
}

public class BuffResetHate : Buff
{
    public BuffResetHateCfg ExCfg { get { return (BuffResetHateCfg)m_cfg.exCfg; } }
    
    

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
       
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            LogError("不需要执行多次");
            return;
        }

        var hateTarget = this.GetRole(ExCfg.hateTargetType, null);
        if (hateTarget == null)
            return;
        if (!RoleMgr.instance.IsEnemy(hateTarget, this.Parent))
        {
            LogError("仇恨目标必须和自己是敌人关系,是不是仇恨目标类型填错");
            return;
        }



        var hateInfo = this.Parent.HatePart.GetHate(hateTarget);
        if (hateInfo == null || hateInfo.hate == 0)
            return;

        this.Parent.HatePart.AddHate(hateTarget, - hateInfo.hate);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        
        
    }

    
    
  
}

