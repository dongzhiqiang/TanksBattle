#region Header
/**
 * 名称：飞出物弹道配置
 
 * 日期：2016.1.19
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public enum enFlyerPathType
{
    empty,          //空
    forward,        //直线
    circle,         //环绕
    parabola,       //抛物线
    toward,         //朝向
    targetParabola, //目标抛物线
    max
}



public static class FlyerPathFactory
{
    public static string[] TypeName = new string[] { "空", "直线","环绕", "抛物线", "朝向", "目标抛物线" };

    public static FlyerPathCfg CreateCfg(enFlyerPathType type)
    {
        FlyerPathCfg eventCfg;
        switch (type)
        {
            case enFlyerPathType.empty: eventCfg = new FlyerPathCfg(); break;
            case enFlyerPathType.forward: eventCfg = new FlyerPathCfgForward(); break;
            case enFlyerPathType.circle: eventCfg = new FlyerPathCfgCircle(); break;
            case enFlyerPathType.parabola: eventCfg = new FlyerPathCfgParabola(); break;
            case enFlyerPathType.toward: eventCfg = new FlyerPathCfgToward(); break;
            case enFlyerPathType.targetParabola: eventCfg = new FlyerPathCfgTargetParabola(); break;
            default:
                {
                    Debuger.LogError("未知的类型，不能添加:{0}", type);
                    return null;
                }
        }
        return eventCfg;
    }

    public static FlyerPath Create(enFlyerPathType type)
    {
        FlyerPath path;
        switch (type)
        {
            case enFlyerPathType.empty: path = IdTypePool<FlyerPath>.Get(); break;
            case enFlyerPathType.forward: path = IdTypePool<FlyerPathForward>.Get(); break;
            case enFlyerPathType.circle: path = IdTypePool<FlyerPathCircle>.Get(); break;
            case enFlyerPathType.parabola: path = IdTypePool<FlyerPathParabola>.Get(); break;
            case enFlyerPathType.toward: path = IdTypePool<FlyerPathToward>.Get(); break;
            case enFlyerPathType.targetParabola: path = IdTypePool<FlyerPathTargetParabola>.Get(); break;
            default:
                {
                    Debuger.LogError("未知的类型，不能添加:{0}", type);
                    return null;
                }
        }
        return path;
    }
}

