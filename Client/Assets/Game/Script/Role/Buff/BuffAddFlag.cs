#region Header
/**
 * 名称：加印记
 
 * 日期：2016.4.19
 * 描述：
印记名，数量
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public class BuffAddFlagCfg : BuffExCfg
{
    public string flag;
    public int num;

    public override bool Init(string[] pp)
    {
        if (pp.Length < 2)
            return false;

        //印记名
        flag = pp[0];
        if (string.IsNullOrEmpty(flag))
            return false;

        //数量
        if (!int.TryParse(pp[1], out num))
            return false;
        
        return true;
    }
}

public class BuffAddFlag: Buff
{
    public BuffAddFlagCfg ExCfg { get { return (BuffAddFlagCfg)m_cfg.exCfg; } }
    
    
   
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        
       
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (this.Source == null)
        {
            Debuger.Log("加印记状态source为空，加不了印记,状态id:{0}",m_cfg.id);
            return;
        }
        int poolId = this.Id;
        this.Source.Fire(MSG_ROLE.SOURCE_ADD_FLAG, this.Parent, ExCfg.flag,ExCfg.num);
        if (this.Source == null || this.IsDestroy(poolId)|| this.Parent == null)//可能这个状态被销毁了
            return;
        this.Parent.Fire(MSG_ROLE.TARGET_ADD_FLAG, this.Source, ExCfg.flag, ExCfg.num);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        
    }
  
}

