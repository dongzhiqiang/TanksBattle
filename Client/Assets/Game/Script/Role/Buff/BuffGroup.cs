#region Header
/**
 * 名称：组合
 
 * 日期：2016.2.26
 * 描述：
状态列表，是否结束删除子状态,作用对象,释放者
是否结束删除子状态，1默认删除，0则不删除
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public class BuffGroupCfg : BuffExCfg
{
    public List<int> buffIds = new List<int>();
    public bool isEndRemove = true;
    public enBuffTargetType targetType = enBuffTargetType.self;
    public enBuffTargetType sourceType = enBuffTargetType.self;
    public override bool Init(string[] pp)
    {
        if (pp.Length < 1)
            return false;
        //状态列表
        int i = 0;
        if (int.TryParse(pp[0], out i))
            buffIds.Add(i);
        else
        {
            if (!StringUtil.TryParse(pp[0].Split('|'), ref buffIds))
                return false;
        }

        //是否结束删除
        if (pp.Length > 1 )
            isEndRemove = pp[1]=="1";

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

public class BuffGroup: Buff
{
    public BuffGroupCfg ExCfg { get { return (BuffGroupCfg)m_cfg.exCfg; } }
    
    List<int> m_buffs =new List<int>();
    int m_buffTargetId;
    Role m_buffTarget;
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        
        if (m_buffs.Count != 0 || m_buffTarget!=null)
        {
            Debuger.LogError("逻辑错误，组合状态初始化的时候发现子状态没有清空:{0}", m_cfg.id);
            m_buffs.Clear();
        }
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        int poolId = this.Id;
        int parentId = m_parent.Id;

        //作用对象
        m_buffTarget = this.GetRole(ExCfg.targetType, null);
        if (m_buffTarget == null)
            return;
        BuffPart buffPart = m_buffTarget.BuffPart;
        m_buffTargetId = m_buffTarget.Id;

        //释放者
        Role buffSource = this.GetRole(ExCfg.sourceType, null);
        if (buffSource == null)
            return;

        Buff buff;
        for (int i = 0; i < ExCfg.buffIds.Count; ++i)
        {
            buff = buffPart.AddBuff(ExCfg.buffIds[i], buffSource);
            if (IsUnneedHandle(poolId, m_parent, parentId))
                return;
            if (buff!= null && ExCfg.isEndRemove)
                m_buffs.Add(buff.Id);
        }
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (!isClear)//如果是清空，那么子状态也会清空，不用再删除
        {
            if(m_buffTarget!= null && !m_buffTarget.IsUnAlive(m_buffTargetId)&& m_buffs.Count>0)
            {
                m_buffTarget.BuffPart.BuffPart.RemoveBuffByIds(m_buffs, false);
            }
        }

        m_buffTargetId = -1;
        m_buffTarget = null;
        m_buffs.Clear();
        

    }
}

