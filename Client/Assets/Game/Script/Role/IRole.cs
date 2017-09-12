#region Header
/**
 * 名称：角色类接口
 
 * 日期：2016.5.20
 * 描述：
 **/
#endregion
using UnityEngine;
using System;  
using System.Collections;
using System.Collections.Generic;


public interface IRole 
{
    
    bool IsHero { get; }
    bool IsNetRole { get; }
    bool IsLoading { get; }
    bool IsDestroying { get; }
    Role Parent {get;}
    EventNotifier Notifier{ get; }
    Role.enState State {get;}
    RoleCfg Cfg{ get; }
    bool IsShow { get; }//角色是不是显示中，如果模型还没有创建，那么算隐藏

    #region 部件
    //1 模型层(一般是内部调用，非部件一定要慎用)
    RoleModel RoleModel { get; }
    Transform transform { get; }//注意不能利用这个接口设置位置和方向，用TranPart的接口SetPos()、SetDir()
    TranPart TranPart { get; }
    AniPart AniPart { get; }
    RenderPart RenderPart { get; }
    RoleStateMachine RSM { get; }

    //2 数据层(属性、状态、仇恨）
    PropPart PropPart { get; }
    BuffPart BuffPart { get; }
    HatePart HatePart { get; }

    //3 战斗层(移动、战斗、死亡等，和RSM有对应关系，本质上是为了更好地控制RSM)
    DeadPart DeadPart { get; }
    MovePart MovePart { get; }
    CombatPart CombatPart { get; }

    //4 控制层(ai)
    AIPart AIPart { get; }

    //5 上层逻辑(装备、时装、技能、背包等等)
    ItemsPart ItemsPart { get; }
    PetsPart PetsPart { get; }
    PetSkillsPart PetSkillsPart { get; }
    TalentsPart TalentsPart { get; }
    EquipsPart EquipsPart { get; } 
    LevelsPart LevelsPart { get; }
    ActivityPart ActivityPart { get; }
    WeaponPart WeaponPart { get; }
    MailPart MailPart { get; }
    WeaponCfg FightWeapon { get; }
    SystemsPart SystemsPart { get; }
    FlamesPart FlamesPart { get; }
    SocialPart SocialPart { get; }
    RoleBornCxt RoleBornCxt { get; }
    OpActivityPart OpActivityPart { get; }
    TaskPart TaskPart { get; }
    CorpsPart CorpsPart { get; }
    #endregion

    #region 人物属性相关，这里提供比较容易获取的接口
    int GetInt(enProp prop);
    void SetInt(enProp prop, int v);
    void AddInt(enProp prop, int v);
    
    long GetLong(enProp prop);
    void SetLong(enProp prop, long v);
    void AddLong(enProp prop, long v);
    
    float GetFloat(enProp prop);
    void SetFloat(enProp prop, float v);
    void AddFloat(enProp prop, float v);
    float GetPercent(enProp prop, enProp propMax);

    string GetString(enProp prop);
    void SetString(enProp prop, string v);

    //一些特殊的属性获取
    enCamp GetCamp();//获取阵营
    int GetStamina();//获取体力

    //标记
    void SetFlag(string flag, int n = 1, bool levelTemp = true);
    void AddFlag(string flag, int n = 1, bool levelTemp = true); 
    int GetFlag(string flag);
    #endregion

    #region 消息广播相关，这里提供比较易用的接口
    //监听人物消息(出生、死亡、被击等等等等)，code见MSG_ROLE类
    int Add(int code, EventObserver.OnFire onFire, bool bindModel = true);
    int Add(int code, EventObserver.OnFire1 onFire, bool bindModel = true);
    int Add(int code, EventObserver.OnFire2 onFire, bool bindModel = true);
    int Add(int code, EventObserver.OnFire3 onFire, bool bindModel = true);
    int Add(int code, EventObserver.OnFireOb onFire, bool bindModel = true);

    //否决人物消息，注意只有特定的消息支持否决，详见具体消息实现
    int AddVote(int code, EventObserver.OnVote onVote, bool bindModel = true);

    //监听人物属性变化
    int AddPropChange(enProp prop, EventObserver.OnFire onFire);
    int AddPropChange(enProp prop, EventObserver.OnFireOb onFire);

    //广播人物消息，一般是内部部件会发送出来
    bool Fire(int code, object param = null, object param2 = null, object param3 = null);
    #endregion

    //刷新角色，一般是刷新角色配置了，身上的临时状态会被去掉，注意这个接口是临时实现，只用于重载配置等非正常流程操作
    void Refresh();

    //加载模型,创建角色的时候可以选择创不创建模型，如果不创建模型，那么要显示角色的时候手动调用下这个接口
    void Load(RoleBornCxt cxt);

    //注册部件，注意注册的顺序决定了部件的OnInit的顺序，而枚举顺序(倒序)决定了部件的OnPostInit
    void SetPart(RolePart part);
    RolePart GetPart(enPart type);

    //用于检查角色是不是已经不在alive状态
    bool IsUnAlive(int poolId);

    //预加载
    void PreLoad();

    //显示隐藏角色，注意要先用IsShow判断下，以免重复设置
    void Show(bool show);
}
