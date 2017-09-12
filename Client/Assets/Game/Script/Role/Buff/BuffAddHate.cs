#region Header
/**
 * 名称：增减仇恨
 
 * 日期：2016.7.28
 * 描述：
值,结束回退,非负限制,仇恨对象
增减自己对仇恨对象的仇恨值
结束回退，默认为1状态结束的时候把本状态对仇恨的所有增减值回退，0则不回退
非负限制，默认为1增减时或者结束回退时保证仇恨值不小于0，0则不限制
仇恨对象，见作用对象类型，默认为释放者
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public class BuffAddHateCfg : BuffExCfg
{
    public LvValue value;
    public bool endGetBack = true;
    public bool keepPositive = true;
    public enBuffTargetType hateTargetType = enBuffTargetType.source;

    public override bool Init(string[] pp)
    {
        if (pp.Length < 1)
            return false;

        value = new LvValue(pp[0]);
        if (value.error)
            return false;

        
        if (pp.Length > 1)
            endGetBack = pp[1] == "1";
        

        if (pp.Length > 2)
            keepPositive = pp[2] == "1";

        int i = 0;
        if (pp.Length > 3 && int.TryParse(pp[3], out i))
            hateTargetType = (enBuffTargetType)i;


        return true;
    }
}

public class BuffAddHate : Buff
{
    public BuffAddHateCfg ExCfg { get { return (BuffAddHateCfg)m_cfg.exCfg; } }

    Role m_hateTarget;
    int m_hateTargetId = -1;
    int m_addhate;

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        m_hateTarget = null;
        m_hateTargetId = -1;
        m_addhate = 0;
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {   
        //第一次
        if(m_count == 1)
        {
            m_hateTarget = this.GetRole(ExCfg.hateTargetType, null);
            if (m_hateTarget == null)
                return;
            if(!RoleMgr.instance.IsEnemy(m_hateTarget,this.Parent))
            {
                LogError("仇恨目标必须和自己是敌人关系,是不是仇恨目标类型填错");
                m_hateTarget = null;
                return;
            }
            m_hateTargetId = m_hateTarget == null ? -1 : m_hateTarget.Id;
        }

        if (m_hateTarget == null || m_hateTarget.IsUnAlive(m_hateTargetId))
            return;

        int hate = (int)this.GetLvValue(ExCfg.value);
        if (hate == 0)
            return;
        

        //非负限制
        if (ExCfg.keepPositive)
        {
            var hateInfo = this.Parent.HatePart.GetHate(m_hateTarget);
            if (hateInfo == null && hate < 0)//不可能为正的情况1
                return;
            
            if(hateInfo!=null)
            {
                int lastHate = hateInfo.hate;
                int ret = lastHate + hate;
                if (lastHate <= 0 && ret < 0)//不可能为正的情况2
                    return;

                hate = ret - lastHate;
            }   
        }

        this.Parent.HatePart.AddHate(m_hateTarget, hate);
        m_addhate += hate;
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (!isClear&& m_hateTarget!=null&& !m_hateTarget.IsUnAlive(m_hateTargetId)&&ExCfg.endGetBack&& m_addhate!=0)
        {
            var hateInfo = this.Parent.HatePart.GetHate(m_hateTarget);
            if(hateInfo!=null)
            {
                bool needGetBack = true;
                if (ExCfg.keepPositive)//非负限制
                {
                    int lastHate = hateInfo.hate;
                    int ret = lastHate - m_addhate;
                    if (lastHate <= 0 && ret < 0)//不可能为正的情况
                        needGetBack = false;
                    else
                        m_addhate = -(ret - lastHate);
                }


                if (needGetBack)
                    this.Parent.HatePart.AddHate(m_hateTarget, -m_addhate);
            }
        }

        m_hateTarget = null;
        m_hateTargetId = -1;
        m_addhate = 0;
    }

    
    
  
}

