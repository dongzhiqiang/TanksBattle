#region Header
/**
 * 名称：角色配置
 
 * 日期：2015.9.21
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum enBloodType
{
    none,
    small,
    big,
    building,
    npc,
    
}

public enum enRolePropType
{
    role,
    monster,
    pet,
}

public enum enRoleType
{
    min,
    hero,
    pet,
    monster,
    boss,
    special,
    box,
    trap,
}

public class RoleCfg 
{
    public string id="";
    public string name="";//名字
    public string mod="";//模型名

    public string propType = null;//角色属性计算方式，有角色、怪物、宠物
    public string propValue=null;//属性初始值，见属性表的固定属性表
    public string propDistribute = null;//属性分配比例，见属性表的属性分配比例

    public enRoleType roleType = enRoleType.monster;//角色类型，1主角 2宠物 3普通怪物 4boss 5精英 6宝箱 7陷阱
    public int addBuffType = 0;     //是否是回血回蓝的宝箱
    public int subType=0; //子类型，对于宠物(1攻击 2防御 3治疗 4控制 5辅助)

    public float behitRate = 1f;//僵直系数(越大僵直时间越长)
    public List<string> behitFxs = new List<string>();//被击特效id
    public List<int> bornBuffs = new List<int>();//出生状态
    public List<string> flags= new List<string>();//标记
    public string deadFx = "";//死亡特效，播放了死亡特效就不会播放死亡动作
    public string skillFile;//技能文件
    public string uniqueSkill= string.Empty;//怒气技能
    public string atkUpSkill = string.Empty;//普通攻击
    public List<string> skills=new List<string>() ;//技能

    public int soulNum = 1; //死后魂值
    public string bornType = "";    //默认出生方式
    public string deadType = "";    //默认死亡方式
    public string groundDeadType = "";    //默认倒地死亡方式
    public string aiType = "";  //默认ai
    public string titleBlood = string.Empty;//头顶血条类型
    public int headBloodNum =10;//右上角boss头像血条数量，角色身上有GlobalConst.FLAG_SHOW_BLOOD标记的时候才会显示右上角血条
    public string colliderLayer = null;

    //元素属性
    public enElement element = enElement.none;

    //打击属性
    public string[] hitDefType;
    public string hitDefBloodShow ;//血条上显示的图标
    
    //气力状态
    public int shieldBuff = 0;//气力状态(注意这个状态的结束状态为气绝状态),注意不要直接从这里取，从Role.RuntimeShieldBuff取
    public float shieldRate = 0f;//气力系数(默认0，越大被扣的气力值越小)。受到伤害时的气力值消耗=攻击方技能气绝值/(1+气力系数*(1+Min(10%*n,50%)))
    public float shieldTimeRate = 0f;//气绝时间系数(默认0，越大气绝时间越短)。受到伤害时的气绝状态持续时间=通用气绝时间/(1+气绝时间系数*(1+Min(10%*n,50%))
    
    public int maxStar; //最高星级
    public int maxAdvanceLevel; // 最高等阶    
    public string upgradeCostId; // 升级消耗id
    public string advanceCostId; // 进阶消耗id
    public string upstarCostId; // 升星消耗id
    public string positioning; // 定位描述
    public string icon; // 头像图标
    public float uiModScale; // UI模型缩放
    public List<string> talents = new List<string>(); // 天赋
    public List<int> petBonds = new List<int>(); //羁绊
    public int initStar; //宠物初始星级
    public int pieceItemId; //宠物碎片id
    public int pieceNum; //宠物碎片数量
    public int pieceNumReturn; //宠物碎片数量返回

    public float power; //战斗力初值

    public int behateIfBegin = 0;//创建的时候所有敌人对自己的仇恨
    public int hateIfHit = 0;//攻击别人增加自己对此人的仇恨
    public int hateIfBeHit = 0;//被别人攻击增加自己对此人的仇恨
    public int hateIfChange = 0;//仇恨切换时再多增加自己对此人的仇恨

    public int petAniGroupId  = 0; //宠物组合动作id


    enBloodType _titleBloodType;
    enRolePropType _rolePropType;
    string _hitDefBloodIcon;
    List<enHitDef> _hitDefs;
    enGameLayer _colliderLayer;

    public static string[] BloodTypeName = new string[] { "", "小血条", "大血条", "建筑血条", "NPC血条" };
    public static string[] PropTypeName = new string[] { "角色属性", "怪物属性", "宠物属性"};

    static Dictionary<string,enBloodType> s_bloodTypes= new Dictionary<string,enBloodType>();
    static Dictionary<string, enRolePropType> s_propTypes = new Dictionary<string, enRolePropType>();
    static string[] s_roleIds;
    static string[] s_roleNames;
    static Dictionary<string, RoleCfg> s_roleCfgs;
    static HashSet<string> s_preLoads = new HashSet<string>();
    static PropertyTable tem = new PropertyTable();
    static PropertyTable empty = new PropertyTable();

    public enBloodType TitleBloodType{get{return _titleBloodType;}}
    public enRolePropType RolePropType { get { return _rolePropType; } }
    public string HitDefBloodIcon { get { return _hitDefBloodIcon; } }
    public enGameLayer ColliderLayer { get { return _colliderLayer; } }


    public static string[] RoleIds
    {
        get {
            if (s_roleIds == null)
            {
                CheckInit();
                s_roleIds = new string[s_roleCfgs.Count];
                int i=0;
                foreach (RoleCfg c in s_roleCfgs.Values)
                    s_roleIds[i++] = c.id;
                Array.Sort(s_roleIds);
            }
            return s_roleIds;
        }
    }

    public static string[] RoleNames
    {
        get
        {
            if (s_roleNames == null)
            {
                s_roleNames = new string[s_roleCfgs.Count];

                int i = 0;
                foreach (RoleCfg c in s_roleCfgs.Values)
                    s_roleNames[i++] = c.id;
                Array.Sort(s_roleNames);
            }
            return s_roleNames;
        }
    }

    public static void CheckInit()
    {
        if(s_roleCfgs == null)
            Init();
    }

    public static void Init()
    {
        s_roleCfgs = Csv.CsvUtil.Load<string, RoleCfg>("role", "id");
        s_roleIds = null;
        s_roleNames = null;
        s_preLoads.Clear();
        empty.IsRead = true;

        //建立血条的索引
        for(int i =0;i<BloodTypeName.Length;++i)
            s_bloodTypes[BloodTypeName[i]] = (enBloodType)i;
        //建立角色属性类型的索引
        for (int i = 0; i < PropTypeName.Length; ++i)
            s_propTypes[PropTypeName[i]] = (enRolePropType)i;

        foreach(RoleCfg cfg in s_roleCfgs.Values){
            if(string.IsNullOrEmpty(cfg.titleBlood))
                cfg._titleBloodType = enBloodType.none;
            else if(!s_bloodTypes.TryGetValue(cfg.titleBlood,out cfg._titleBloodType))
            {
                cfg._titleBloodType = enBloodType.none;
                Debuger.LogError("角色表，{0}未知的血条类型:{1}",cfg.id,cfg.titleBlood);
            }

            if (string.IsNullOrEmpty(cfg.propType))
                cfg._rolePropType = enRolePropType.monster;
            else if (!s_propTypes.TryGetValue(cfg.propType, out cfg._rolePropType))
            {
                cfg._rolePropType = enRolePropType.monster;
                Debuger.LogError("角色表，{0}未知的属性类型:{1}", cfg.id, cfg.propType);
            }

            cfg._hitDefs = HitPropCfg.GetDefs(cfg.hitDefType);
            var hitPropCfg = HitPropCfg.Get(cfg.hitDefBloodShow);
            if (hitPropCfg != null)
                cfg._hitDefBloodIcon = hitPropCfg.icon;

            if (string.IsNullOrEmpty(cfg.colliderLayer))
                cfg._colliderLayer = enGameLayer.monsterCollider;
            else
                cfg._colliderLayer = LayerMgr.instance.GetGameLayerByName(cfg.colliderLayer);
        }
        
      
    }

    //预加载
    public static void PreLoad(string roleId)
    {
        CheckInit();
        if (string.IsNullOrEmpty(roleId))
            return;

        if(s_preLoads.Contains(roleId))
            return;

        
        if (s_preLoads.Count == 0)
        {
            //加载一些常用特效
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad("fx_humen_prj_root");
            //加载大qte用到的动作
            GameObjectPool.GetPool(GameObjectPool.enPool.Other).PreLoad("mod_camera");

            //加载所有武器，一般只有主角有
            WeaponCfg.PreLoad();

            //加载所有元素属性事件组
            ElementCfg.PreLoad();
        }


        s_preLoads.Add(roleId);
        RoleCfg cfg = Get(roleId);
        if (cfg == null)
            return;
            
        //预加载模型
        if (!string.IsNullOrEmpty(cfg.mod))
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).PreLoad(cfg.mod);

        //预加载被击特效、死亡特效
        for(int i=0;i<cfg.behitFxs.Count;++i)
            RoleFxCfg.ProLoad(cfg.behitFxs[i]);
        if (!string.IsNullOrEmpty(cfg.deadFx))
            RoleFxCfg.ProLoad(cfg.deadFx);

        //预加载出生状态
        for (int i = 0; i < cfg.bornBuffs.Count; ++i)
            BuffCfg.ProLoad(cfg.bornBuffs[i]);

        //预加载气力状态
        if(cfg.shieldBuff > 0)
            BuffCfg.ProLoad(cfg.shieldBuff);

        //预加载默认出生死亡特效
        BornCfg.PreLoad(cfg.bornType, cfg.deadType, cfg.groundDeadType);
        
        //预加载技能
        RoleSkillCfg.PreLoad(cfg.skillFile);

        //预加载动作音效
        AniSoundCfg.PreLoad(cfg.mod);

        //预加载ai
        Simple.BehaviorTree.BehaviorTreeMgrCfg.PreLoad(cfg.aiType);
    }

    public static RoleCfg Get(string roleId)
    {
        CheckInit();
        RoleCfg cfg = s_roleCfgs.Get(roleId);
        if (cfg == null)
            Debuger.LogError("角色id不存在，请检查role表:{0}", roleId);
        return cfg;
    }

    public static Dictionary<string,RoleCfg> GetAll()
    {
        return s_roleCfgs;
    }

    public static string GetHeadIcon(string roleId, bool returnDef = true)
    {
        RoleCfg cfg = string.IsNullOrEmpty(roleId) ? null : Get(roleId);
        return (cfg != null && !string.IsNullOrEmpty(cfg.icon)) ? cfg.icon : (returnDef ? ConfigValue.defRoleHead : null);
    }
    
    public string GetSkillId(enSkillType skillType,bool isAir)
    {
        if (isAir)
            return null;

        switch (skillType)
        {
            case enSkillType.atkUp: return atkUpSkill;
            case enSkillType.unique: return uniqueSkill;
            case enSkillType.skill1: return (skills.Count <= 0 ? null : skills[0]);
            case enSkillType.skill2: return (skills.Count <= 1 ? null : skills[1]);
            case enSkillType.skill3: return (skills.Count <= 2 ? null : skills[2]);
            case enSkillType.block: return null;
            default: Debuger.LogError("未知的类型{0}", skillType); return null;
        }
    }

    //打击属性
    public enHitDef GetHitDef(HitPropCfg cfg)
    {

        return _hitDefs[cfg.id - 1];
    }



    public void GetBaseProp(PropertyTable target, int lv = 1, int advLv = 1, int star = 1)
    {
    #if PROP_DEBUG
        string log= "";
#endif
        if (RolePropType == enRolePropType.monster)//怪物成长属性=初始值+属性等级系数（怪物）*怪物属性分配比例*属性值系数（monster）
        {
            //属性等级系数（角色）*角色属性分配比例*属性值系数（role）
            PropertyTable propsDistribute = string.IsNullOrEmpty(this.propDistribute) ? empty : PropDistributeCfg.Get(this.propDistribute).props;
            PropertyTable.Mul(RoleTypePropCfg.mstTypeProp, propsDistribute, target);
            PropertyTable.Mul(MonsterLvPropCfg.Get(lv).props, target, target);
            #if PROP_DEBUG
            log+=string.Format("属性等级系数（角色）*角色属性分配比例*属性值系数（role）={0}\n", target.GetFloat(enProp.hpMax));
            #endif

            //加初始值
            PropertyTable propsValue = string.IsNullOrEmpty(this.propValue) ? empty : PropValueCfg.Get(this.propValue).props;
            PropertyTable.Add(target, propsValue, target);
            #if PROP_DEBUG
            log += string.Format("+初始值={0}\n",target.GetFloat(enProp.hpMax));
            #endif
        }
        else if (this.RolePropType == enRolePropType.role) //主角成长属性=初始值+属性等级系数（角色）*角色属性分配比例*属性值系数（role）
        {
            //属性等级系数（角色）*角色属性分配比例*属性值系数（role）
            PropertyTable propsDistribute = string.IsNullOrEmpty(this.propDistribute) ? empty : PropDistributeCfg.Get(this.propDistribute).props;
            PropertyTable.Mul(RoleTypePropCfg.roleTypeProp, propsDistribute, target);
            #if PROP_DEBUG
            log += string.Format("属性等级系数（角色）*角色属性分配比例*属性值系数（role）={0}\n" , target.GetFloat(enProp.hpMax));
            #endif
            PropertyTable.Mul(RoleLvPropCfg.Get(lv).rate, target, target);
            #if PROP_DEBUG
            log += string.Format("*属性等级系数（角色）={0}\n", target.GetFloat(enProp.hpMax));
            #endif

            //加初始值
            PropertyTable propsValue = string.IsNullOrEmpty(this.propValue) ? empty :PropValueCfg.Get(this.propValue).props;
            PropertyTable.Add(target, propsValue, target);
            #if PROP_DEBUG
            log += string.Format("+初始值={0}\n",target.GetFloat(enProp.hpMax));
            #endif

            //战斗力：（角色战斗力初值+角色属性等级系数）*战斗力系数
            target.SetInt(enProp.power, (int)((power + RoleLvPropCfg.Get(lv).rate) * PropBasicCfg.instance.powerRate));
            #if PROP_DEBUG
            log += string.Format("战斗力={0}\n", target.GetFloat(enProp.power));
            #endif
        }
        else//宠物成长属性 =宠物属性初值*(1+宠物进阶属性增量(初值)+宠物升星属性增量(初值)）+属性等级系数（角色）*宠物属性点数*宠物属性分配比例*属性值系数（role）)*(1+宠物进阶属性增量(等级)+宠物升星属性增量(等级)）
        {
            PetAdvLvPropRateCfg advCfg = PetAdvLvPropRateCfg.Get(advLv);
            PetStarPropRateCfg starCfg = PetStarPropRateCfg.Get(star);

            //属性等级系数（角色）*宠物属性点数*(1+宠物进阶属性增量(等级)+宠物升星属性增量(等级)）
            PropertyTable propsDistribute = string.IsNullOrEmpty(this.propDistribute) ? empty : PropDistributeCfg.Get(this.propDistribute).props;
            float lvRate = RoleLvPropCfg.Get(lv).rate * PropBasicCfg.instance.petPoint * (1f + advCfg.lvRate + starCfg.lvRate);
            PropertyTable.Mul(RoleTypePropCfg.roleTypeProp, propsDistribute, target);
            PropertyTable.Mul(lvRate, target, target);
            #if PROP_DEBUG
            log += string.Format("属性等级系数（角色）*宠物属性点数*(1+宠物进阶属性增量(等级)+宠物升星属性增量(等级)）={0}\n", target.GetFloat(enProp.hpMax));
            #endif

            //加初始值
            float baseRate = 1f + advCfg.baseRate + starCfg.baseRate;
            #if PROP_DEBUG
            log += string.Format("(1+宠物进阶属性增量(等级)+宠物升星属性增量(等级))={0} \n", baseRate);
            #endif
            PropertyTable propsValue = string.IsNullOrEmpty(this.propValue) ? empty : PropValueCfg.Get(this.propValue).props;
            PropertyTable.Mul(baseRate, propsValue, tem);
            PropertyTable.Add(tem, target, target);
            #if PROP_DEBUG
            log += string.Format("+初始值={0} \n", target.GetFloat(enProp.hpMax));
            #endif

            //战斗力：（角色战斗力初值*(1+宠物进阶属性增量（初值）+宠物升星属性增量（初值））+宠物属性点数*角色属性等级系数*（1+宠物进阶属性增量（等级）+宠物升星属性增量（等级）））*战斗力系数
            float petPower = power * (1f + advCfg.baseRate + starCfg.baseRate);
            petPower += PropBasicCfg.instance.petPoint*RoleLvPropCfg.Get(lv).rate*(1f + advCfg.lvRate + starCfg.lvRate);
            petPower *= PropBasicCfg.instance.powerRate;
            target.SetInt(enProp.power, (int)petPower);
            #if PROP_DEBUG
            log += string.Format("战斗力={0}\n", target.GetFloat(enProp.power));
            #endif
        }
#if PROP_DEBUG
        Debuger.Log(log);
#endif
        
    }

    
    public static void GetBasePropByCfg(string roleId, PropertyTable target, int lv = 1, int advLv = 1, int star = 1)
    {
        RoleCfg cfg = Get(roleId);
        if (cfg == null)
            return;

        cfg.GetBaseProp(target, lv, advLv, star);
    }
}
