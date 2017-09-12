#region Header
/**
 * 名称：碰撞层状态
 
 * 日期：2016.7.22
 * 描述：
层类型
修改角色碰撞层类型
层类型,小怪碰撞、大怪碰撞、宠物碰撞、主角碰撞、主角翻滚碰撞、死亡碰撞，区别见战斗文档里的碰撞层文档
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BuffColliderLayerCfg : BuffExCfg
{
    public enGameLayer layer;
    
    public override bool Init(string[] pp)
    {
        if (pp.Length < 1)
            return false;
        
        layer = LayerMgr.instance.GetGameLayerByName(pp[0]);
        if (layer == enGameLayer.max)
            return false;
        
        return true;
    }
}

public class BuffColliderLayer: Buff
{
    public BuffColliderLayerCfg ExCfg { get { return (BuffColliderLayerCfg)m_cfg.exCfg; } }
    
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("碰撞层状态不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }
        if(ExCfg.layer!= enGameLayer.max)
            this.Parent.RenderPart.SetLayer(ExCfg.layer);

            
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (!isClear&& ExCfg.layer != enGameLayer.max&& ExCfg.layer== this.Parent.RenderPart.GetLayer())//如果是清空，那么子状态也会清空，不用再删除
        {
            this.Parent.RenderPart.ResetLayer();
        }
        

    }

   
}

