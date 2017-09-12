#region Header
/**
 * 名称：免疫伤害反弹
 
 * 日期：2016.3.21
 * 描述：
对敌人造成伤害时免疫其对可能自己造成的伤害反弹
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class BuffLimitMoveCfg : BuffExCfg
{
  


    public override bool Init(string[] pp)
    {
  
        return true;
    }
}
public class BuffLimitMove : Buff
{
    public BuffLimitMoveCfg ExCfg { get { return (BuffLimitMoveCfg)m_cfg.exCfg; } }

    

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {

        this.Parent.RSM.AddLimitMove();
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
       
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (isClear)
            return;
        this.Parent.RSM.SubLimitMove();
    }

    
    
  
}

