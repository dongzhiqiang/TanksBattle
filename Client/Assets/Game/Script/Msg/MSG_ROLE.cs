using UnityEngine;
using System.Collections;

/*
 * *********************************************************
 * 名称：角色相关消息
 
 * 日期：2015.1.3
 * 描述：
 * 
 * *********************************************************
 */

public class MSG_ROLE
{
    //创建，这个时候模型还没有创建，处于init状态
    public const int INIT = 1;

    //出生,模型创建完了，处于alive状态
    public const int BORN = 2;

    //死亡
    public const int DEAD = 3;

    //完全销毁
    public const int DESTROY = 4;

    //使用技能
    public const int SKILL =5;

    //攻击
    public const int HIT = 6;

    //被击
    public const int BEHIT = 7;
   
    //ai变化
    public const int AI_CHANGE = 8;

    //背包变化
    public const int ITEM_CHANGE = 9;

    //死亡效果播完
    public const int DEAD_END = 10;
    
    //网络属性同步
    public const int NET_PROP_SYNC = 12;

    //模型被销毁
    public const int DESTROY_MODEL = 13;

    //状态创建否决
    public const int BUFF_ADD_AVOID = 14;

    //技能事件否决
    public const int SKILL_EVENT_AVOID = 15;

    //网络活动属性同步
    public const int NET_ACT_PROP_SYNC = 16;

    //标记有变化
    public const int FLAG_CHANGE = 17;

    //武器显示状态改变，比如换武器或者技能需要隐藏武器
    public const int WEAPON_RENDER_CHANGE =18;

    //技能事件(被动)，执行前(在否决前执行)
    public const int TARGET_SKILL_EVENT_PRE = 19;
    
    //伤害反弹否决
    public const int DAMAGE_REFLECT_AVOID = 20;
    
    //添加状态
    public const int BUFF_ADD = 21;

    //切换武器
    public const int WEAPON_CHANGE = 22;

    //伤害计算中，增减攻击伤害
    public const int ADD_DAMAGE = 23;

    //伤害计算中，增减被击伤害
    public const int ADD_DEF_DAMAGE = 24;

    //技能事件
    public const int SOURCE_SKILL_EVENT= 25;

    //加印记
    public const int SOURCE_ADD_FLAG = 26;

    //加印记(被动)
    public const int TARGET_ADD_FLAG = 27;

    //杀死了一个敌人
    public const int KILL = 28;
	
    //装备变化
    public const int EQUIP_CHANGE = 29;

    //宠物技能变化
    public const int PET_SKILL_CHANGE = 30;

    //宠物天赋变化
    public const int PET_TALENT_CHANGE = 31;

    //出生效果播完
    public const int BORN_END = 32;

    //本人主角创建完成
    public const int HERO_CREATED = 33;

    //武器技能变化
    public const int WEAPON_SKILL_CHANGE = 34;

    //添加好友
    public const int ADD_FRIEND = 35;

    //申请加入公会
    public const int JOIN_CORPS = 36;

    //武器铭文变化
    public const int WEAPON_TALENT_CHANGE = 37;

    //运营活动属性变化
    public const int NET_OPACT_PROP_SYNC = 38;

    //参与公会建设
    public const int CORPS_BUILD = 39;

    //击飞和浮空是否要转换为被击
    public const int CHANGE_HIT_EVENT = 40;

    //获得奖励(和道具变化区分开因为升级之类的操作发生道具变化时并不需要马上刷新界面)
    public const int GET_REWARD = 41;

    //属性变化
    public const int FRESH_BASE_PROP= 42;

    //众神传属性变化
    public const int ELITELV_CHANGE = 43;    

    //神器属性变化
    public const int TREASURE_CHANGE = 44;

    //属性变化，当要监听某个属性改变的时候应该监听Prop_CHANGE+该属性的索引
    public const int PROP_CHANGE = 1000;
    
    //活动属性变化，当要监听某个属性改变的时候应该监听ACT_PROP_CHANGE+该属性的索引
    public const int ACT_PROP_CHANGE = 2000;

}
