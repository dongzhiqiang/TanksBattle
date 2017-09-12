#region Header
/**
 * 名称：动作序列
 
 * 日期：2016.3.7
 * 描述：
动作序列上下文,免疫行为列表
角色进入一个播放一系列动作的行为中，这个行为的将替代待机行为
动作序列上下文,动作1:循环方式:持续时间:渐变时间|动作2:循环方式:持续时间:渐变时间|动作3:循环方式:持续时间:渐变时间
免疫行为列表，此行为能免疫某些行为,移动|战斗|被击|死亡|下落|换武器
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class BuffAniCfg : BuffExCfg
{
    public RoleStateAniCxt cxt;


    public override bool Init(string[] pp)
    {
        cxt = RoleStateAniCxt.Parse(pp[0], pp.Length < 2 ? null : pp[1]);
        if (cxt == null)
            return false;
        return true;
    }
}
public class BuffAni : Buff
{
    public BuffAniCfg ExCfg { get { return (BuffAniCfg)m_cfg.exCfg; } }
    
    

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("动作序列状态不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }
        m_parent.RSM.StateAni.Goto(ExCfg.cxt);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (!isClear )
            m_parent.RSM.StateAni.CheckLeave(ExCfg.cxt);        
    }
    
}

