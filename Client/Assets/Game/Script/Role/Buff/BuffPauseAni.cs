#region Header
/**
 * 名称：定身
 
 * 日期：2016.9.1
 * 描述：
无参数
定身开始时动作将停留在当前的姿势不动，定身期间不会被其他行为(被击、浮空、移动等，除了死亡)中断
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class BuffPauseAniCfg : BuffExCfg
{
    public override bool Init(string[] pp)
    {
        return true;
    }
}
public class BuffPauseAni : Buff
{
    public BuffPauseAniCfg ExCfg { get { return (BuffPauseAniCfg)m_cfg.exCfg; } }
    
    

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("定身状态不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }
        m_parent.RSM.StatePauseAni.GotoState(this);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (!isClear )
            m_parent.RSM.StatePauseAni.CheckLeave(this);        
    }
    
}

