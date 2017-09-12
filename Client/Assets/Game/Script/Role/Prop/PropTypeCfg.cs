#region Header
/**
 * 名称：属性定义表
 
 * 日期：2015.11.24
 * 描述：
 *      
装备属性=初始值*(1+装备进阶属性增量系数(初值)+装备觉醒属性增量系数(初值)）+属性等级系数（角色）*装备成长属性点数*装备属性分配比例*属性值系数（role）*(1+装备进阶属性增量系数(等级)+装备觉醒属性增量系数(等级)）

角色成长属性=初始值+属性等级系数（角色）*角色属性分配比例*属性值系数（role）
角色最终属性=(装备属性+角色成长属性+宝石属性+天赋属性+宝物属性+...)*(1+装备觉醒提升百分比+宝石提升百分比+天赋提升百分比+宝物提升百分比+...）

宠物自身成长属性=宠物属性初值*(1+宠物进阶属性增量(初值)+宠物升星属性增量(初值)）+f(lv)*宠物属性点数*宠物属性分配比例*属性值系数（role）)*(1+宠物进阶属性增量(等级)+宠物升星属性增量(等级)）
宠物最终属性=(装备属性+宠物自身成长属性+宝石属性+天赋属性+宝物属性+...)*(1+装备觉醒提升百分比+宝石提升百分比+天赋提升百分比+宝物提升百分比+...）
 
怪物成长属性=初始值+属性等级系数（怪物）*怪物属性分配比例*属性值系数（monster）
怪物最终属性=(怪物自身成长属性+宝石属性+天赋属性+宝物属性+...)*(1+装备觉醒提升百分比+宝石提升百分比+天赋提升百分比+宝物提升百分比+...）*（1+关卡怪物属性修正系数/10000)+关卡怪物固定属性修正系数

减伤率=C*护甲/(护甲+B+A*f(lv)) 
元素伤害倍率=C*(元素.攻-元素.抗)/(元素.攻+A)  取值大于等于0
伤害=C*攻击*(1-减伤率)*技能伤害修正系数*技能伤害事件伤害系数*（1+元素伤害倍率)*rand(95%,105%)   取值大于等于1
暴击时伤害=非暴击时伤害*暴击伤害
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public enum enProp
{
    //战斗属性，都是float
    minFightProp = 0,
    hpMax = 1,              //生命
    atk = 2,                //攻击
    def = 3,                //护甲
    damageDef = 4,          //减免伤害
    damage = 5,             //最终伤害
    critical = 6,           //暴击几率
    criticalDef = 7,        //抗暴几率
    criticalDamage = 8,     //暴击伤害
    fire = 9,               //火
    ice = 10,               //冰
    thunder = 11,           //雷
    dark = 12,              //冥
    fireDef = 13,           //火抗
    iceDef = 14,            //冰抗
    thunderDef = 15,        //雷抗
    darkDef = 16,           //冥抗
    hpCut = 17,             //生命偷取
    damageReflect = 18,     //伤害反弹
    mpMax = 19,             //怒气
    speed = 20,             //速度
    cdCut = 21,             //冷却缩减
    shieldMax = 22,         //气力值
    maxFightProp,           //切忌 这里之前的值用于战斗属性计算，属性表都会自动设置成初始化为float，不要在这里之前添加任何属性计算的值

    hp = 30,            //整数
    mp = 31,            //整数
    shield = 32,        //整数


    //基本属性
    channelId = 40,     //字符串，主角才有，渠道ID
    userId=41,             //字符串，主角才有，用户ID
    lastLogin=42,          //整数，主角才有，最近登录时间
    loginCount=43,         //整数，主角才有，登录次数
    heroId=44,             //整数，主角才有，主角ID，必须大于0
    gmFlag=45,             //整数，主角才有，如果带有这个标记，是GM

    guid=46,               //字符串，必须有
    createTime=47,         //整数，创建时间
    roleId=48,             //字符串，角色类型ID
    name=49,               //字符串，名字

    level=50,              //整数，等级
    exp=51,                //整数，经验值
    //curWeapon=52,          //整数，当前武器索引,弃用，放到武器部件里了
    stamina=53,            //整数，体力值
    advLv=54,              //整数，等阶
    star=55,               //整数，星级
    camp=56,               //整数，阵营
    gold=57,               //整数，金币
    pet1Main=58,           //字符串, 出战宠物1主战
    pet1Sub1=59,           //字符串, 出战宠物1输出助战
    pet1Sub2=60,           //字符串，出战宠物1生存助战
    pet2Main=61,           //字符串, 出战宠物2主战
    pet2Sub1=62,           //字符串, 出战宠物2输出助战
    pet2Sub2=63,           //字符串，出战宠物2生存助战
    staminaTime=64,        //整数，给体力时间
    power=65,              //整数，战斗力
    corpsId=66,            //整数，军团ID
    arenaCoin=67,          //整数，竞技场兑换币
    pet1MRId=68,           //字符串，主战宠物1的角色类型ID
    pet2MRId=69,           //字符串，主战宠物2的角色类型ID
    diamond=70,            //整数，钻石
    robotId=71,            //整数，如果是机器人有机器人配置ID
    offline=72,            //整数，主角才有，如果是真的离线角色（就是不是掉线的），这个值是正整数，如果是掉线的角色，这个值是负整数
    lastLogout =73,        //整数，上次登出时间，这个登出不包括掉线、离线加载后的Role销毁，只是在线加载后的Role销毁
    staminaBuyNum = 74,    //整数，体力购买次数
    staminaBuyTime = 75,   //整数，体力购买时间
    vipLv = 76,            //整数，vip等级
    hornNum = 77,          //整数，世界频道聊天的喇叭数
    powerTotal = 78,       //整数，总战力
    corpsName = 79,        //字符串，公会名
    powerPets = 80,        //整数，所有宠物的战斗力，不包括主角
    maxPowerPet = 81,      //字符串，战斗力最强的神侍的GUID
    lastRankLike = 82,     //整数，排行榜上次点赞时间
    rankLikeLog = 83,      //字符串，排行榜点赞记录，是json数据，格式：{排行类型1:[点赞对象ID1,点赞对象ID2]}
    towerLevel = 84,       //整数，预言者之塔挑战层
    towerWinTime = 85,     //整数，预言者之塔最后一次胜利时间
    towerUseTime = 86,     //整数，预言者之塔挑战用时
    towerEnterNums = 87,   //整数，预言者之塔挑进入次数
    towerEnterTime = 88,   //整数，预言者之塔挑最后一次进入时间
    upEquipTime = 89,     //整数，上次升级装备时间
    upEquipNum = 90,      //整数，每日升级装备次数  
    upPetTime = 91,        //整数，上次升级神侍时间
    upPetNum = 92,      //整数，每日升级神侍次数
    max
}

public enum enPropFormat
{
    Float = 0,
    FloatRate = 1,//比率型的，读表的时候要除以10000
}

public class AddPropCxt
{
    public enProp prop;
    public LvValue value;
    public bool error = false;

    public AddPropCxt(string s)
    {
        string[] pp = s.Split('|');
        if (pp.Length < 2)
        {
            error = true;
            return;
        }


        PropTypeCfg cfg = PropTypeCfg.GetByName(pp[0]);
        if (cfg == null)
        {
            error = true;
            return;
        }
        prop = (enProp)cfg.id;

        value = new LvValue(pp[1]);
        if (value.error)
        {
            error = true;
            return;
        }
    }
}

public class PropTypeCfg
{
    public int id;//属性索引值,见enProp
    public string name = "";
    public enPropFormat format = enPropFormat.Float;//0这个属性是float，1这个属性是float并且要除以10000
    public string key = "";

    public static Dictionary<int, PropTypeCfg> m_cfgs = new Dictionary<int, PropTypeCfg>();
    public static Dictionary<string, PropTypeCfg> m_cfgsByKey = new Dictionary<string, PropTypeCfg>();
    public static Dictionary<string, PropTypeCfg> m_cfgsByName = new Dictionary<string, PropTypeCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, PropTypeCfg>("property/propType", "id");
        m_cfgsByKey.Clear();
        m_cfgsByName.Clear();
        foreach (PropTypeCfg cfg in m_cfgs.Values)
        {
            m_cfgsByKey[cfg.key] = cfg;
            m_cfgsByName[cfg.name] = cfg;
        }
            
    }

    public static PropTypeCfg Get(enProp prop)
    {
        return Get((int)prop);
    }
    public static PropTypeCfg Get(int idx)
    {
        PropTypeCfg cfg = m_cfgs.Get(idx);
        if (cfg == null)
            Debuger.LogError("属性类型不存在，请检查attribute表的属性定义表:{0}", idx);
        return cfg;
    }

    public static PropTypeCfg GetByKey(string k)
    {
        PropTypeCfg cfg = m_cfgsByKey.Get(k);
        if (cfg == null)
            Debuger.LogError("属性类型不存在，请检查attribute表的属性定义表:{0}", k);
        return cfg;
    }

    public static PropTypeCfg GetByName(string name)
    {
        PropTypeCfg cfg = m_cfgsByName.Get(name);
        if (cfg == null)
            Debuger.LogError("属性名字不存在，请检查attribute表的属性定义表:{0}", name);
        return cfg;
    }

    //加载含有PropertyTable字段的表
    public static Dictionary<TKeyType, T> Load<TKeyType, T>(string csvName, string key, string propField,float rate = 1f) where T : new()
    {
        Type type = typeof(T);
        FieldInfo propFieldInfo = type.GetField(propField, BindingFlags.Instance | BindingFlags.Public);
        if (propFieldInfo == null)
        {
            Debuger.LogError("找不到属性表字段，传进来的字段名是不是写错了：{0}", propField);
            return null;
        }
        Type propType = propFieldInfo.FieldType;
        if (propType != typeof(PropertyTable))
        {
            Debuger.LogError("属性表字段不是PropertyTable类，{0}", propField);
            return null;
        }

        T curRow;
        PropertyTable props = null;
        PropTypeCfg typeCfg;
        float v;
        return Csv.CsvUtil.Load<TKeyType, T>(csvName, key, (T row, string colName, Csv.CsvReader r, string value) =>
        {
            if (r.Col == 0)
            {
                if (props != null)//开始新的一行的时候，把上一行设置成只读
                    props.IsRead = true;

                curRow = row;
                props = new PropertyTable();
                propFieldInfo.SetValue(row, props);
            }

            //获取对应的属性类型
            if (string.IsNullOrEmpty(colName))
                return true;
            typeCfg = m_cfgsByKey.Get(colName);
            if (typeCfg == null)
                return true;

            //属性的值
            if (string.IsNullOrEmpty(value))
                v = 0;
            else if (!float.TryParse(value, out v))
            {
                Debuger.LogError("解析不成float.行:{0} 列名:{1}", r.Row, colName);
                v = 0;
            }

            //是不是万分比
            if (typeCfg.format == enPropFormat.FloatRate)
                v *= 0.0001f ;

            if (rate != 1f)
                v *= rate;

            //设置到属性表
            props.SetFloat((enProp)typeCfg.id, v);
            return true;
        });
    }

    //获取变化的属性根据最大值，如果没有对应的变化属性，那么会返回minFightProp
    public static enProp GetPropByMax(enProp propMax)
    {
        switch (propMax)
        {
            case enProp.hpMax:return enProp.hp;
            case enProp.mpMax:return enProp.mp;
            case enProp.shieldMax: return enProp.shield;
            default:return enProp.minFightProp;
        }
    }
}
