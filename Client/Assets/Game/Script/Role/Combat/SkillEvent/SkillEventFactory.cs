#region Header
/**
 * 名称：技能事件工厂
 
 * 日期：2016.6.24
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
public enum enSkillEventType
{
    empty,          //空
    hit,            //被击
    hitFloat,       //浮空
    hitFly,         //击飞
    damage,         //伤害，带伤害数字、伤害特效
    move,           //移动，可监听摇杆
    pause,          //卡帧
    fx,             //特效
    jump,           //跳起
    skill,          //技能
    aniFx,          //动作上绑定的特效
    camera,         //推镜
    buff,           //触发状态
    timeScale,      //时间缩放
    grab,           //抓取
    qte,            //qte
    camera2,        //推镜2
    sound,          //音效
    cameraChange,   //切镜
    stopSkill,      //停止当前技能
    killRole,       //杀死角色
    createRole,     //创建角色
    sceneEvent,     //触发关卡
    bigQte,         //大qte
    grab2,          //抓取2
    max
}

public enum enSkillEventConditionType
{
    behit,//被击
    flag,//标记
    unflag,//无标记
    buff,//状态
    unBuff,//无状态
    percent,//概率
    skillTarget,//技能目标存在
    unSkillTarget,//技能目标不存在
    triggerFrame,//帧触发
    unTriggerFrame,//无帧触
    max
}

public enum enSkillEventEditorMsg
{
    none,
    openFxCreateWindow,
    openFlyerEditor,
    openBrabEditor,
}

public static class SkillEventFactory
{
    public static string[] TypeName = new string[] { "空事件", "被击", "浮空", "击飞","伤害","移动","卡帧",
            "特效","跳起","技能","动作特效","推镜","触发状态","时间缩放","抓取","qte","推镜2","音效","切镜",
            "中断技能","杀死角色","创建角色","触发关卡", "大Qte", "抓取2"};

    public const string ConditionTip = @"
条件类型
    被击，多少时间内被打过则执行当前帧,参数(时间)
    标记，该对象身上有对应标记的时候执行当前帧，参数(标记)
    无标记，同上相反
    状态,对于前置条件检查释放者是否有此状态，对于前置对象条件，检查目标对象是否有此状态，参数(状态id)
    无状态，同上相反
    概率，0~1
    有技能目标,对于前置条件检查技能目标是不是还活着，对于前置对象条件，检查对象是不是技能目标
    无技能目标
    帧触发,某帧有没有触发过，对于前置对象条件，某帧有没有对对象触发过
    无帧触
    ";

    public static string[] ConditionTypeName = new string[] { "被击", "标记", "无标记", "状态", "无状态", "概率", "有技目", "无技目", "帧触发", "无帧触" };

    static Dictionary<string,enSkillEventType> s_nameDict;
    
    

    public static SkillEventCfg Create(enSkillEventType type)
    {
        SkillEventCfg eventCfg;
        switch (type)
        {
            case enSkillEventType.empty: eventCfg = new EmptySkillEventCfg(); break;
            case enSkillEventType.hit: eventCfg = new HitSkillEventCfg(); break;
            case enSkillEventType.hitFloat: eventCfg = new HitFloatSkillEventCfg(); break;
            case enSkillEventType.hitFly: eventCfg = new HitFlySkillEventCfg(); break;
            case enSkillEventType.damage: eventCfg = new DamageEventCfg(); break;
            case enSkillEventType.move: eventCfg = new MoveEventCfg(); break;
            case enSkillEventType.pause: eventCfg = new PauseAniEventCfg(); break;
            case enSkillEventType.fx: eventCfg = new FxEventCfg(); break;
            case enSkillEventType.jump: eventCfg = new JumpEventCfg(); break;
            case enSkillEventType.skill: eventCfg = new UseSkillEventCfg(); break;
            case enSkillEventType.aniFx: eventCfg = new AniFxEventCfg(); break;
            case enSkillEventType.camera: eventCfg = new CameraEventCfg(); break;
            case enSkillEventType.buff: eventCfg = new BuffEventCfg(); break;
            case enSkillEventType.timeScale: eventCfg = new TimeScaleEventCfg(); break;
            case enSkillEventType.grab: eventCfg = new GrabEventCfg(); break;
            case enSkillEventType.qte: eventCfg = new QTEEventCfg(); break;
            case enSkillEventType.camera2: eventCfg = new Camera2EventCfg(); break;
            case enSkillEventType.sound: eventCfg = new SoundEventCfg(); break;
            case enSkillEventType.cameraChange: eventCfg = new CameraChangeEventCfg(); break;
            case enSkillEventType.stopSkill: eventCfg = new StopSkillEventCfg(); break;
            case enSkillEventType.killRole: eventCfg = new KillRoleEventCfg(); break;
            case enSkillEventType.createRole: eventCfg = new CreateRoleEventCfg(); break;
            case enSkillEventType.sceneEvent: eventCfg = new SceneEventEventCfg(); break;
            case enSkillEventType.bigQte: eventCfg = new BigQteEventCfg(); break;
            case enSkillEventType.grab2: eventCfg = new Grab2EventCfg(); break;
                
            default:
                {
                    Debuger.LogError("未知的类型，不能添加:{0}", type);
                    return null;
                }
        }
        return eventCfg;
    }

    public static SkillEventConditionCfg CreateCondition(enSkillEventConditionType type)
    {
        SkillEventConditionCfg c;
        switch (type)
        {
            case enSkillEventConditionType.behit: c = new SECBehit(); break;
            case enSkillEventConditionType.flag: c = new SECFlag(); break;
            case enSkillEventConditionType.unflag: c = new SECUnflag(); break;
            case enSkillEventConditionType.buff: c = new SECBuff(); break;
            case enSkillEventConditionType.unBuff: c = new SECUnBuff(); break;
            case enSkillEventConditionType.percent: c = new SECPercent(); break;
            case enSkillEventConditionType.skillTarget: c = new SECSkillTarget(); break;
            case enSkillEventConditionType.unSkillTarget: c = new SECUnSkillTarget(); break;
            case enSkillEventConditionType.triggerFrame: c = new SECTriggerFrame(); break;
            case enSkillEventConditionType.unTriggerFrame: c = new SECUnTriggerFrame(); break;

            default:
                {
                    Debuger.LogError("未知的类型，不能添加:{0}", type);
                    return null;
                }
        }
        return c;
    }

    public static enSkillEventType GetEventType(string typeName)
    {
        if (s_nameDict == null)
        {
            s_nameDict = new Dictionary<string, enSkillEventType>();
            for (int i = 0; i < TypeName.Length; ++i)
            {
                s_nameDict[TypeName[i]] = (enSkillEventType)i;
            }
        }
        enSkillEventType t;
        if (!s_nameDict.TryGetValue(typeName, out t))
        {
            Debuger.LogError("找不到技能事件类型:{0}", TypeName);
            return enSkillEventType.max;
        }
        return t;
    }


}
