#region Header
/**
 * 名称：技能配置
 
 * 日期：2015.12.8
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;



public class SkillCfg
{
    

    public enum enSkillAniType
    {
        ClampForever,
        Loop,
        PingPong,
    }

    public enum enMoveType
    {
        none,
        joystick,//摇杆方向
        keepMove,//摇杆方向的情况下沿着摇杆移动，没有的话保持向前移动
        findTarget,//自动寻怪 摇杆方向的情况下沿着摇杆移动，没有的话朝最近的敌人移动，没有敌人的话向前移动
        max
    }

    public enum enDirType
    {
        none,
        keepRotate,//保持旋转
        forward,//与移动方向一样
        max
    }

    public static string[] MoveTypeName = new string[] { "不移动", "摇杆", "保持移动", "自动寻怪" };
    public static string[] DirTypeName = new string[] { "不转向", "保持旋转", "移动方向" };

    //public int id;//唯一id
    public string skillId="id";
    public string name="";//技能名
    public string desc="";//描述
    public string icon="";//图标
    public int mp = 0;//消耗耐力
    public int mpNeed =-1;//使用技能需要的耐力
    public float cd = 0;//冷却时间，**受内部技能影响**
    public bool showWeapon =true;//显示武器
    public bool isInternal = false;//是不是内部技能，一个技能要做出来可能要通过释放第二个技能来实现，那么这第二个技能就属于内部技能，暂时用来计算连击，内部技能不影响连击的判断
    public string parentSkillId = "";
    


    //动作相关
    public string aniName ="";//动作名
    public enSkillAniType aniType = enSkillAniType.ClampForever;//动作的循环方式
    public float fade = 0.2f;//开始渐变
    public float duration = -1;//-1则为动作播放时间
    public bool continueIfPress = false;//按紧持续
    public AniRateCfg aniRate = new AniRateCfg();//动作帧率调整

    //位置和方向控制
    public enMoveType moveType = enMoveType.none;
    public enDirType dirType = enDirType.none;
    public float rotateSpeed = 360;//旋转速度，多少度每秒
    public float moveSpeed = 3;//移动速度
    public int beginTranFrame = 0;//开始帧
    public int endTranFrame = -1;//结束帧

    //攻击时，自动朝向相关
    public bool autoFace = true;//自动朝向
    public RangeCfg firstRange = new RangeCfg();//摇杆和主角前方有没有敌人，有则自动朝向释放
    public RangeCfg secondRange = new RangeCfg();//主角前方有没有敌人，有则自动朝向释放
    public RangeCfg thirdRange = new RangeCfg();//主角一定范围内有没有敌人，有则自动朝向释放

    //攻击范围(用于ai索敌)
    public RangeCfg attackRange = new RangeCfg();

    //技能取消相关
    public int cancelPriority =0;//取消优先级，优先级高的可以在技能的取消前帧之前、取消后帧之后切换
    public bool isAirSkill =false;//是不是空中技能，空中技能和非空中技能不能互相取笑
    public bool canCanel=true;//可以取消别人
    public bool canBeCanel = true;//可以被别人取消
    public int cancelPreFrame = 0;//-1表明使用取消技能会缓存到取消后帧之后取消
    public int cancelPostFrame = -1;//-1表明技能结束之后取消

    //技能连击相关
    public string comboSkillId = "";//连击技能
    public int comboPreFrame = 0;//这一帧之前不能取消，这一帧之后到comboPostFrame之前缓存,**受内部技能影响**
    public int comboPostFrame = -1;//多少帧之后可以连击，-1表明技能结束之后才可以连击，**受内部技能影响**
    public int comboWaitFrame =30;//技能结束之后多少时间内使用连击技能仍然可以连击
    public bool showComboIcon =true;//显示连击图标
    
    //事件表和状态相关
    public SkillEventGroupCfg eventGroup=new SkillEventGroupCfg() ;
    public List<int> skillStates = new List<int>();//技能使用期间所加的状态
    public List<int> endStates = new List<int>();//结束时状态，被取消或者自动结束时才会触发，被中断时不会
    public string endEventGroupId = string.Empty;//结束时的事件组，被取消或者自动结束时才会触发，被中断时不会
    public bool endIfCancel =false;//如果当前技能是被其他技能连击或者取消的，也执行结束事件组和结束状态
    public bool endIfBehit = false;//如果当前技能因为被击等外力中断，也执行结束事件组和结束状态
    

    string skillStatesStr = null;
    string endStatesStr =null;

    public string SkillStatesStr
    {
        get
        {
            if (skillStatesStr == null)
                skillStatesStr = StringUtil.Join<int>(skillStates);
            return skillStatesStr;
        }
        set
        {
            skillStatesStr = value;
            StringUtil.Parse(skillStatesStr, ref skillStates);
        }
    }

    public string EndStatesStr {
        get
        {
            if (endStatesStr == null)
                endStatesStr = StringUtil.Join<int>(endStates);
            return endStatesStr;
        }
        set
        {
            endStatesStr = value;
            StringUtil.Parse(endStatesStr, ref endStates);
        }
    }



    public WrapMode wrapMode
    {
        get
        {
            if (aniType == enSkillAniType.Loop)
                return WrapMode.Loop;
            else if(aniType == enSkillAniType.PingPong)
                return WrapMode.PingPong;
            else 
                return WrapMode.ClampForever;
        }
    }

    //预加载
    public void PreLoad()
    {
        eventGroup.PreLoad();
        for (int i = 0; i < skillStates.Count; ++i)
            BuffCfg.ProLoad(skillStates[i]);
        for (int i = 0; i < endStates.Count; ++i)
            BuffCfg.ProLoad(endStates[i]);
        SkillEventGroupCfg.PreLoad(endEventGroupId);
    }

    public void Reset()
    {
        //自动朝向不用判断高度
        firstRange.heightLimit = -1;
        secondRange.heightLimit = -1;
        thirdRange.heightLimit = -1;
    }
    public void CopyFrom(SkillCfg skillCfg)
    {
        if(skillCfg==null)return;

        //复制值类型的属性
        Util.Copy(skillCfg, this, BindingFlags.Public | BindingFlags.Instance);

        //复制其他
        this.aniRate.CopyFrom(skillCfg.aniRate);
        this.firstRange.CopyFrom(skillCfg.firstRange);
        this.secondRange.CopyFrom(skillCfg.secondRange);
        this.thirdRange.CopyFrom(skillCfg.thirdRange);
        this.attackRange.CopyFrom(skillCfg.attackRange);
        this.eventGroup.CopyFrom(skillCfg.eventGroup);
        skillStates.Clear();
        skillStates.AddRange(skillCfg.skillStates);
        endStates.Clear();
        endStates.AddRange(skillCfg.endStates);
        skillStatesStr = null;
        endStatesStr = null;
        
    }
}
