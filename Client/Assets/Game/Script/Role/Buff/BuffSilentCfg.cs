#region Header
/**
 * 名称：沉默
 * 作者：
 * 日期：2016.9.1
 * 描述：
无参数
使角色不能使用技能只可以移动
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuffSilentCfg : BuffExCfg
{
    public override bool Init(string[] pp)
    {
        return true;
    }
}

public class BuffSilent : Buff
{
    public BuffSilentCfg ExCfg { get { return (BuffSilentCfg)m_cfg.exCfg; } }
    
    

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit()
    {
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            LogError("不需要执行多次");
            return;
        }
        
        m_parent.RSM.AddSilent();
    }
    
    //结束
    public override void OnBuffStop(bool isClear)
    {
        if (!isClear)
        {
            m_parent.RSM.SubSilent();
        }
    }
}
