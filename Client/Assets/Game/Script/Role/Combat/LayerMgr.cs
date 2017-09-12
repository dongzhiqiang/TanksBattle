#region Header
/**
 * 名称：层管理器
 
 * 日期：2016.8.3
 * 描述：碰撞用层和渲染用层的管理
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Csv;

//游戏中可能要动态控制或者设置到的层
public enum enGameLayer
{
    obstructCollider,//挡板碰撞
    monsterCollider,//小怪碰撞
    bossCollider,//大怪碰撞
    petCollider,//宠物碰撞
    heroCollider,//主角碰撞
    heroAvoidCollider,//主角翻滚碰撞
    deadCollider,//死亡碰撞
    flyerTrigger,//飞出物
    levelTrigger,//关卡区域触发
    cameraTrigger,//相机触发
    roleRender,//角色模型渲染,受角色实时灯影响
    terrainRender,//地表渲染,为了接受角色阴影
    sceneRender,//场景实时渲染，受场景实时灯影响
    max
}
public class LayerMgr : Singleton<LayerMgr>
{
    static string[] LayerName = new string[] {
        "挡板碰撞","小怪碰撞","大怪碰撞","宠物碰撞",
        "主角碰撞","主角翻滚碰撞","死亡碰撞",
        "飞出物触发","关卡触发","镜头触发",
        "角色渲染","地板渲染","场景渲染"
    };

    static string[] LayerTag = new string[] {
        "ObstructCollider","MonsterCollider","BossCollider","PetCollider",
        "HeroCollider","HeroAvoidCollider","DeadCollider",
        "FlyerTrigger","LevelTrigger","CameraTrigger",
        "RoleRender","TerrainRender","SceneRender"
    };

    

    int[] m_layers = new int[(int)enGameLayer.max];
    Dictionary<string, enGameLayer> m_gameLayerByName = new Dictionary<string, enGameLayer>();
    Dictionary<int, enGameLayer> m_gameLayerByLayer = new Dictionary<int, enGameLayer>();

    public LayerMgr()
    {
        for(int i = 0;i<(int)enGameLayer.max;++i)
        {
            m_layers[i] = LayerMask.NameToLayer(LayerTag[i]);
            m_gameLayerByName[LayerName[i]] = (enGameLayer)i;
            m_gameLayerByLayer[m_layers[i]] = (enGameLayer)i;
        }
    }


    public void SetLayer(GameObject go,enGameLayer gameLayer)
    {
        if(gameLayer == enGameLayer.max)
        {
            Debuger.LogError("设置了无效的层:{0}",go.name);
            return;
        }

        go.layer = m_layers[(int)gameLayer];
    }

    public void SetLayer(Component comp, enGameLayer gameLayer)
    {
        SetLayer(comp.gameObject, gameLayer);
    }

    public enGameLayer GetGameLayerByName(string name)
    {
        enGameLayer l;
        if (m_gameLayerByName.TryGetValue(name, out l))
            return l;

        Debuger.LogError("找不到{0}层",name);
        return enGameLayer.max;
    }

    public enGameLayer GetGameLayerByLayer(int layer)
    {
        enGameLayer l;
        if (m_gameLayerByLayer.TryGetValue(layer, out l))
            return l;

        Debuger.LogError("找不到{0}层", layer);
        return enGameLayer.max;
    }

}
