#region Header
/**
 * 名称：NodeCfg
 
 * 日期：2016.5.13
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Simple.BehaviorTree
{
    public class NodeType
    {
        public string menuPath;
        public Type cfgType;
        public Func<Node> createFun;
        public string name;
        public string icon;
        public string desc;
        public bool checkShort;//检不检查短路，一般是持续性的行为需要检查
        

        public NodeType(string name, bool checkShort, Type cfgType, Func<Node> createFun, string icon, string menuPath,string desc)
        {
            this.name = name;
            this.menuPath = menuPath;
            this.cfgType = cfgType;
            this.createFun = createFun;
            this.icon = icon;
            this.desc = desc;
            this.checkShort = checkShort;
        }

    }

    public static class BehaviorTreeFactory
    {
        public static List<NodeType> s_types = new List<NodeType>();
        public static Dictionary<Type, NodeType> s_cfgIdx = new Dictionary<Type, NodeType>();

        static BehaviorTreeFactory()
        {
            //通用类型
            AddCommonType();

            //数学的类型
            AddMathType();

            //游戏角色的类型
            AddRoleType();

            //建立下索引
            foreach (var n in s_types)
            {
                s_cfgIdx[n.cfgType] = n;
            }
        }

        static void AddCommonType()
        {
            //组合
            s_types.Add(new NodeType("序列",false, typeof(SequenceCfg), IdTypePool<Sequence>.Get, "Composite/sequence", "Composite/序列", "按顺序执行子节点，当有子节点返回失败则终止马上返回失败，执行完所有子节点则返回成功"));
            s_types.Add(new NodeType("选择", false, typeof(SelectorCfg), IdTypePool<Selector>.Get, "Composite/selector", "Composite/选择", "按顺序执行子节点，当有子节点返回成功则终止马上返回成功，执行完所有子节点则返回失败"));
            s_types.Add(new NodeType("平行", false, typeof(ParallelCfg), IdTypePool<Parallel>.Get, "Composite/parallel", "Composite/平行", "所有子节点同时运行(相当于多个子行为树)"));
            //修饰
            s_types.Add(new NodeType("重复", false, typeof(RepeaterCfg), IdTypePool<Repeater>.Get, "Decorator/repeater", "Decorator/重复", "当执行次数超过限制就直接返回"));
            s_types.Add(new NodeType("取反", false, typeof(InverterCfg), IdTypePool<Inverter>.Get, "Decorator/inverter", "Decorator/取反", "取反"));
            s_types.Add(new NodeType("成功", false, typeof(SuccessCfg), IdTypePool<Success>.Get, "decorator", "Decorator/成功", "必定返回成功"));
            s_types.Add(new NodeType("失败", false, typeof(FailureCfg), IdTypePool<Failure>.Get, "decorator", "Decorator/失败", "必定返回失败"));
            s_types.Add(new NodeType("运行计数", false, typeof(RunningCounterCfg), IdTypePool<RunningCounter>.Get, "decorator", "Decorator/运行计数", "这个节点进入运行状态计数器加一，退出运行状态计数器减一"));
            //条件
            s_types.Add(new NodeType("自身可见", false, typeof(IsActiveSelfCfg), IdTypePool<IsActiveSelf>.Get, "conditional", "Conditional/自身可见", "如果游戏对象自身可见返回成功,否则返回失败"));
            s_types.Add(new NodeType("是否超时", false, typeof(TimeLimitCfg), IdTypePool<TimeLimit>.Get, "conditional", "Conditional/是否超时", "拿当前时间和传进来的时间比较，如果勾选了超时，那么返回成功，如果取消勾选超时，返回相反值"));
            //行为
            s_types.Add(new NodeType("等待", true, typeof(WaitCfg), IdTypePool<Wait>.Get, "action", "Action/等待", "等待一段时间，该时间内这个节点为正在运行状态"));
            s_types.Add(new NodeType("日志", false, typeof(LogCfg), IdTypePool<Log>.Get, "action", "Action/日志", "打印一段话"));
            s_types.Add(new NodeType("BehaviorHandle", false, typeof(BehaviorHandleCfg), IdTypePool<BehaviorHandle>.Get, "action", "Action/BehaviorHandle", "提供handle类控件的功能，做一段补帧动画"));
            s_types.Add(new NodeType("刷新时间", false, typeof(RefreshTimeCfg), IdTypePool<RefreshTime>.Get, "action", "Action/刷新时间", "设置当前的时间到一个值上"));
        }

        static void AddMathType()
        {
            //组合

            //修饰
            

            //条件
            s_types.Add(new NodeType("比较(浮点数)", false, typeof(CompareFloatCfg), IdTypePool<CompareFloat>.Get, "conditional", "Conditional/Math/浮点数/比较", "比较两个数,如果结果为真返回成功，如果不想结果输出可以设置结果为常量，不影响返回"));
            s_types.Add(new NodeType("比较(整数)", false, typeof(CompareIntCfg), IdTypePool<CompareInt>.Get, "conditional", "Conditional/Math/整数/比较", "比较两个数,如果结果为真返回成功，如果不想结果输出可以设置结果为常量，不影响返回"));
            s_types.Add(new NodeType("比较(布尔)", false, typeof(CompareBoolCfg), IdTypePool<CompareBool>.Get, "conditional", "Conditional/Math/布尔/比较", "比较两个数,如果结果为真返回成功，如果不想结果输出可以设置结果为常量，不影响返回"));
            s_types.Add(new NodeType("比较(字符串)", false, typeof(CompareStringCfg), IdTypePool<CompareString>.Get, "conditional", "Conditional/Math/字符串/比较", "比较两个数,如果结果为真返回成功，如果不想结果输出可以设置结果为常量，不影响返回"));
            s_types.Add(new NodeType("比较范围(浮点数)", false, typeof(CompareRangeFloatCfg), IdTypePool<CompareRangeFloat>.Get, "conditional", "Conditional/Math/浮点数/比较范围", "比较一个数在不在范围内，如果结果为真返回成功，如果不想结果输出可以设置结果为常量，不影响返回"));
            s_types.Add(new NodeType("比较范围(整数)", false, typeof(CompareRangeIntCfg), IdTypePool<CompareRangeInt>.Get, "conditional", "Conditional/Math/整数/比较范围", "比较一个数在不在范围内，如果结果为真返回成功，如果不想结果输出可以设置结果为常量，不影响返回"));

            //行为
            s_types.Add(new NodeType("赋值(浮点数)", false, typeof(SetFloatCfg), IdTypePool<SetFloat>.Get, "action", "Action/Math/浮点数/赋值", "把值赋值给变量"));
            s_types.Add(new NodeType("加(浮点数)", false, typeof(AddFloatCfg), IdTypePool<AddFloat>.Get, "action", "Action/Math/浮点数/加", "a+b = 结果"));
            s_types.Add(new NodeType("减(浮点数)", false, typeof(SubFloatCfg), IdTypePool<SubFloat>.Get, "action", "Action/Math/浮点数/减", "a-b = 结果"));
            s_types.Add(new NodeType("乘(浮点数)", false, typeof(MulFloatCfg), IdTypePool<MulFloat>.Get, "action", "Action/Math/浮点数/乘", "a*b = 结果"));
            s_types.Add(new NodeType("除(浮点数)", false, typeof(DivFloatCfg), IdTypePool<DivFloat>.Get, "action", "Action/Math/浮点数/除", "a/b = 结果"));
            s_types.Add(new NodeType("随机(浮点数)", false, typeof(RandomFloatCfg), IdTypePool<RandomFloat>.Get, "action", "Action/Math/浮点数/随机", "随机出一个数，在[最小值,最大值]之间"));
            
            s_types.Add(new NodeType("赋值(整数)", false, typeof(SetIntCfg), IdTypePool<SetInt>.Get, "action", "Action/Math/整数/赋值", "把值赋值给变量"));
            s_types.Add(new NodeType("加(整数)", false, typeof(AddIntCfg), IdTypePool<AddInt>.Get, "action", "Action/Math/整数/加", "a+b = 结果"));
            s_types.Add(new NodeType("减(整数)", false, typeof(SubIntCfg), IdTypePool<SubInt>.Get, "action", "Action/Math/整数/减", "a-b = 结果"));
            s_types.Add(new NodeType("乘(整数)", false, typeof(MulIntCfg), IdTypePool<MulInt>.Get, "action", "Action/Math/整数/乘", "a*b = 结果"));
            s_types.Add(new NodeType("除(整数)", false, typeof(DivIntCfg), IdTypePool<DivInt>.Get, "action", "Action/Math/整数/除", "a/b = 结果"));
            s_types.Add(new NodeType("随机(整数)", false, typeof(RandomIntCfg), IdTypePool<RandomInt>.Get, "action", "Action/Math/整数/随机", "随机出一个数，在[最小值,最大值]之间"));
            
            s_types.Add(new NodeType("赋值(布尔)", false, typeof(SetBoolCfg), IdTypePool<SetBool>.Get, "action", "Action/Math/布尔/赋值", "把值赋值给变量"));
            s_types.Add(new NodeType("随机(布尔)", false, typeof(RandomBoolCfg), IdTypePool<RandomBool>.Get, "action", "Action/Math/布尔/随机", "随机出一个布尔值"));
            s_types.Add(new NodeType("取反(布尔)", false, typeof(NegationBoolCfg), IdTypePool<NegationBool>.Get, "action", "Action/Math/布尔/取反", "取反"));

            s_types.Add(new NodeType("赋值(字符串)", false, typeof(SetStringCfg), IdTypePool<SetString>.Get, "action", "Action/Math/字符串/赋值", "把值赋值给变量"));
            s_types.Add(new NodeType("加(字符串)", false, typeof(AddStringCfg), IdTypePool<AddString>.Get, "action", "Action/Math/字符串/加", "a+b = 结果"));
        }

        
        static void AddRoleType()
        {
            //组合
            //修饰
            s_types.Add(new NodeType("标记计数", false, typeof(RoleFlagCounterCfg), IdTypePool<RoleFlagCounter>.Get, "decorator", "角色/Decorator/标记计数", "这个节点进入角色标记加一，退出角色标记减一"));

            //条件
            s_types.Add(new NodeType("角色状态", false, typeof(IsRoleStateCfg), IdTypePool<IsRoleState>.Get, "conditional", "角色/Conditional/角色状态", "如果角色处于填写的状态，返回成功，否则返回失败，(空闲|移动|战斗|被击|动作序列)"));
            s_types.Add(new NodeType("赋值(角色)", false, typeof(SetRoleCfg), IdTypePool<SetRole>.Get, "conditional", "角色/Conditional/赋值角色", "设置一个角色变量,如果为空返回失败，支持的定制中断：如果和上一次的角色不一样则中断"));
            s_types.Add(new NodeType("查找技能", false, typeof(FindSkillCfg), IdTypePool<FindSkill>.Get, "conditional", "角色/Conditional/查找技能", "找当前可用的技能,可以传入一个参考目标，优先使用能打到目标的最大技能，否则使用范围最大的技能，传出技能id和技能范围，如果一个技能都没有那么返回失败"));
            s_types.Add(new NodeType("比较(角色)", false, typeof(CompareRoleCfg), IdTypePool<CompareRole>.Get, "conditional", "角色/Conditional/比较角色", "比较两个角色是否相同,如果结果为真返回成功，如果不想结果输出可以设置结果为常量，不影响返回"));
            s_types.Add(new NodeType("标记存在", false, typeof(IsRoleFlagExistCfg), IdTypePool<IsRoleFlagExist>.Get, "conditional", "角色/Conditional/标记存在", "检查角色身上标记是否存在，如果结果为真返回成功，如果不想结果输出可以设置结果为常量，不影响返回"));
            s_types.Add(new NodeType("关卡标记存在", false, typeof(IsRoomFlagExistCfg), IdTypePool<IsRoomFlagExist>.Get, "action", "关卡/Conditional/关卡标记存在", "检查角色所在关卡标记是否存在，如果结果为真返回成功，如果不想结果输出可以设置结果为常量，不影响返回"));
            s_types.Add(new NodeType("查找标记角色", false, typeof(FindRoleByFlagCfg), IdTypePool<FindRoleByFlag>.Get, "conditional", "角色/Conditional/查找标记角色", "查找场景中有这个标记的角色,如果找不到返回失败"));
            s_types.Add(new NodeType("查找范围角色", false, typeof(FindRoleByRangeCfg), IdTypePool<FindRoleByRange>.Get, "conditional", "角色/Conditional/查找范围角色", "相对范围内的角色,如果找不到返回失败"));
            s_types.Add(new NodeType("手动操作超时", false, typeof(OperationLimitCfg), IdTypePool<OperationLimit>.Get, "conditional", "角色/Conditional/手动操作超时", "距离玩家上次手动操作超过一定时间，返回成功，否则返回失败"));
            s_types.Add(new NodeType("允许被击", false, typeof(AllowBeAttackCfg), IdTypePool<AllowBeAttack>.Get, "conditional", "角色/Conditional/允许被击", "当角色没有处于qte或者空中技能中时，算是允许被击，返回成功"));

            //行为
            s_types.Add(new NodeType("追踪", true, typeof(TraceCfg), IdTypePool<Trace>.Get, "action", "角色/Action/追踪", "追踪敌人，直到达到完成距离"));
            s_types.Add(new NodeType("使用技能", true, typeof(UseSkillCfg), IdTypePool<UseSkill>.Get, "action", "角色/Action/使用技能", "使用技能，如果技能使用完的时候有连击技能且在目标在技能范围内，那么将继续使用连击技能"));
            s_types.Add(new NodeType("寻路go箭头", false, typeof(PahSceneDirCfg), IdTypePool<PahSceneDir>.Get, "action", "角色/Action/寻路go箭头", "寻路到go箭头指向的点"));
            s_types.Add(new NodeType("角色距离", false, typeof(RoleDistanceCfg), IdTypePool<RoleDistance>.Get, "action", "角色/Action/角色距离", "计算两个角色间的距离，如果角色不存在则结果为0返回失败"));
            s_types.Add(new NodeType("动作序列", true, typeof(RoleAniCfg), IdTypePool<RoleAni>.Get, "action", "角色/Action/动作序列", "播放一个动作序列用来替代待机动作，同时可以设置免疫角色状态(用来防止打断自己)"));
            s_types.Add(new NodeType("加入群体", false, typeof(AddRoleGroupMgrCfg), IdTypePool<AddRoleGroupMgr>.Get, "action", "角色/Action/加入角色群体管理器", "将当前角色加入角色群体管理器中，加入之后可以获取群体攻击目标和群体包围目标"));
            s_types.Add(new NodeType("包围", true, typeof(RoundCfg), IdTypePool<Round>.Get, "action", "角色/Action/包围", "和目标目标保持一定位置关系的行为：跑、前移、后移、左右移、盯着目标,，可以自己指定也可以自动选择，详见类型说明"));
            s_types.Add(new NodeType("获取标记", false, typeof(GetRoleFlagCfg), IdTypePool<GetRoleFlag>.Get, "action", "角色/Action/获取标记", "获取角色身上标记的值"));
            s_types.Add(new NodeType("设置标记", false, typeof(SetRoleFlagCfg), IdTypePool<SetRoleFlag>.Get, "action", "角色/Action/设置标记", "设置角色身上标记的值"));
            s_types.Add(new NodeType("获取关卡标记", false, typeof(GetRoomFlagCfg), IdTypePool<GetRoomFlag>.Get, "action", "关卡/Action/获取关卡标记", "获取角色所在关卡标记的值"));
            s_types.Add(new NodeType("设置关卡标记", false, typeof(SetRoomFlagCfg), IdTypePool<SetRoomFlag>.Get, "action", "关卡/Action/设置关卡标记", "设置角色所在关卡标记的值"));
            s_types.Add(new NodeType("保持距离", true, typeof(KeepDistanceCfg), IdTypePool<KeepDistance>.Get, "action", "角色/Action/保持距离", "和目标目标保持一定位置关系的行为:如果离角色太远会跑向角色，离角色太近会向远处跑，如果是在目标前方一定角度内还会绕开"));
            s_types.Add(new NodeType("触发关卡事件", false, typeof(TriggerSceneEventCfg), IdTypePool<TriggerSceneEvent>.Get, "action", "角色/Action/触发关卡事件", "触发关卡事件"));
            s_types.Add(new NodeType("释放飞出物", false, typeof(PlayFlyerCfg), IdTypePool<PlayFlyer>.Get, "action", "角色/Action/释放飞出物", "释放飞出物"));
            s_types.Add(new NodeType("释放事件组", false, typeof(PlayEventGroupCfg), IdTypePool<PlayEventGroup>.Get, "action", "角色/Action/释放事件组", "释放事件组"));
            s_types.Add(new NodeType("巡逻", false, typeof(PatrolCfg), IdTypePool<Patrol>.Get, "action", "角色/Action/巡逻", "巡逻"));
            s_types.Add(new NodeType("与路点距离", false, typeof(GetPosDistanceCfg), IdTypePool<GetPosDistance>.Get, "action", "角色/Action/与路点距离", "获取和路径点中最近的点的距离"));
            s_types.Add(new NodeType("获取血量", false, typeof(GetHpPercentCfg), IdTypePool<GetHpPercent>.Get, "action", "角色/Action/获取血量", "获取角色血量百分比，结果从0~1"));
        }

        public static Node CreateNode(NodeCfg cfg)
        {
            NodeType type = s_cfgIdx.Get(cfg.GetType());
            if (type == null)
            {
                Debuger.LogError("未知的类型:{0}", cfg.GetType());
                return null;
            }
            return type.createFun();
        }

     

       

    }
}