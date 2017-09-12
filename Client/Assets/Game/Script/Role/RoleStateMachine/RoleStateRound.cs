using UnityEngine;
using System.Collections;

/*
 * *********************************************************
 * 名称：包围状态
 
 * 日期：2016.6.13
 * 描述：和目标目标保持一定位置关系,有下面几种类型，可以自己指定也可以自动选择
 * 1.跑向，跑向目标，达到包围范围内时完成(如果两者距离超过跑向范围，被自动选择)
 * 2.同步，保持和目标移动方向相同，目标停下来时完成(如果目标在移动,被自动选择)
 * 3.前移，前移动到包围范围(如果两者距离超过包围范围上限，被自动选择；自动选择的情况下，如果目标突然移动，切换到同步)
 * 4.后移，后移动到包围范围(如果两者距离小于包围范围下限，被自动选择；自动选择的情况下，如果目标突然移动，切换到同步)
 * 5.左右，那么随机左右移动一次，(如果两者距离在包围范围内，被自动选择 ；自动选择的情况下，如果目标突然移动，切换到同步)
 * 6.盯着，始终原地盯着目标(不会被自动选择)
 * *********************************************************
 */

public class RoleStateRoundCxt : IdType
{
    public Role target;
    public enRoleRoundType roundType = enRoleRoundType.auto;
    public float roundRangeMin = 4;//包围范围的随机下限
    public float roundRangeMax = 7;//包围范围的随机上限
    public float runRange = 15;//跑向范围
    public float speedRate = 0.5f;//前移、后移、左右三种类型时的速度比例，注意跑向的速度不受这个值影响
    public float leftRightAngleMin = 20f;//左右移动角度的随机下限
    public float leftRightAngleMax = 40f;//左右移动角度的随机上限
    public float lookDurationMin = 2;//盯着时间的随机下限
    public float lookDurationMax = 4;//盯着时间的随机上限
}

public enum enRoleRoundType
{
    auto,
    run,//太远了，朝目标跑去
    sync,//和主角同步，同方向移动
    foward,//前进
    back,//后退
    leftRight,//左右随机移动
    stillLook,//盯着
}

public class RoleStateRound : RoleState
{
    public static string[] TypeNames = new string[] { "自动选择","跑向", "同步", "前移", "后移", "左右", "盯着" };

    #region Fields
    enRoleRoundType m_subState;
    float m_duration;
    float m_beginTime;
    Role m_target;
    int m_targetId;
    int m_curId;
    TranPartCxt m_cxt;
    float m_lastPathTime;
    float m_range;
    bool m_isAuto;
    int m_roundCxtId;
    #endregion

    #region Properties
    public override enRoleState Type { get{return enRoleState.round; }}

    public int RoundCxtId { get { return m_roundCxtId; } }
    #endregion

    #region Frame
    public RoleStateRound(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
            
    }
    //如果没有碰撞不能移动
    public override bool CanEnter() { return !RSM.IsNoCollider; }
    public override void Enter(object param)
    {
        Do(param);
    }

    //重新传递参数给当前状态,比如走动中换方向，使用技能时强制使用第二个技能
    public override void Do(object param)
    {
        RoleStateRoundCxt cxt = (RoleStateRoundCxt)param;
        m_roundCxtId = cxt.Id;
        //检错下
        if (cxt.target ==null || cxt.target.State != Role.enState.alive)
        {
            RSM.CheckFree();
            return;
        }

        //初始化信息
        float rangeMin = cxt.roundRangeMin;
        float rangeMax= cxt.roundRangeMax;
        if (rangeMax < rangeMin)
        {
            rangeMin = cxt.roundRangeMax;
            rangeMax = cxt.roundRangeMin;
        }
        m_range = UnityEngine.Random.Range(rangeMin, rangeMax);
        m_beginTime = TimeMgr.instance.logicTime;
        m_targetId = cxt.target.Id;
        m_target = cxt.target;
        if (m_cxt != null)
            TranPart.RemoveCxt(m_cxt);
        m_cxt = TranPart.AddCxt();
        m_curId = m_cxt.Id;
        m_cxt.speed = Parent.GetFloat(enProp.speed);
        if (m_cxt.speed == 0)
        {
            Debuger.LogError("角色速度为0，移动会卡住：{0}", this.Parent.Cfg.id);
            m_cxt.speed = 0.5f;
        }
        float dis = this.Parent.Distance(m_target);
        MovePart movePart = m_target.MovePart;
        m_duration = -1;

        //如果自动选择那么选择下
        m_isAuto = cxt.roundType == enRoleRoundType.auto;
        if (m_isAuto)
        {
            if (dis > cxt.runRange)
                m_subState = enRoleRoundType.run;
            else if (movePart.IsMoveing)
                m_subState = enRoleRoundType.sync;
            else if (dis > (rangeMax + 1f))
                m_subState = enRoleRoundType.foward;
            else if (rangeMin > 1 && dis < (rangeMin - 1f))
                m_subState = enRoleRoundType.back;
            else
                m_subState = enRoleRoundType.leftRight;
        }
        else
            m_subState = cxt.roundType;

        //太远了，朝目标跑去
        if(m_subState == enRoleRoundType.run)
        {
            m_cxt.dirType = TranPartCxt.enDir.forward;
            m_cxt.moveType = TranPartCxt.enMove.path;
            m_cxt.dirModelSmooth = true;
            TranPart.SetPathPos(m_target.transform.position);
            m_lastPathTime = TimeMgr.instance.logicTime;
            m_rsm.AniPart.Play(AniFxMgr.Ani_PaoBu, WrapMode.Loop, 0.2f, m_cxt.speed / AniPart.MoveAniSpeed, true);
        }
        else if(m_subState == enRoleRoundType.sync || m_subState == enRoleRoundType.foward || m_subState == enRoleRoundType.back || m_subState == enRoleRoundType.leftRight)
        {
            m_cxt.speed *= cxt.speedRate;
            m_cxt.dirModelSmooth = false;
            m_cxt.moveType = TranPartCxt.enMove.dir;
            m_cxt.dirType = TranPartCxt.enDir.dir;
            Vector3 look = m_target.transform.position - this.Parent.transform.position;
            look.y = 0;
            m_cxt.SetDirDir(look);
            

            if (m_subState == enRoleRoundType.sync)
            {
                if (movePart.IsMoveing)
                    m_cxt.SetMoveDir( movePart.CurDir);
                else
                    m_cxt.SetMoveDir(look);
            }
            else if (m_subState == enRoleRoundType.foward)
            {
                m_cxt.SetMoveDir(look);
                if (m_range < dis)
                    m_duration = (dis - m_range) / m_cxt.speed;
                else
                    m_duration = 0;
            }
            else if (m_subState == enRoleRoundType.back)
            {
                m_cxt.SetMoveDir(-look);
                if(m_range > dis)
                    m_duration = ( m_range- dis) / m_cxt.speed;
                else
                    m_duration = 0;
            }
            else
            {
                Vector3 link = -look;
                ////如果不在范围内，那么得修正下//Fix 不需要修正不然会有很多奇怪的问题
                //if (dis > (rangeMax + 1f))
                //    link = link.normalized * (rangeMax-0.2f);
                //else if (rangeMin > 1 && dis < (rangeMin - 1f))
                //    link = link.normalized * (rangeMin+0.2f);

                //随机下角度
                float angle = cxt.leftRightAngleMax <= cxt.leftRightAngleMin ? cxt.leftRightAngleMin : UnityEngine.Random.Range(cxt.leftRightAngleMin, cxt.leftRightAngleMax);
                Vector3 pos = m_target.transform.position + Quaternion.Euler(0, UnityEngine.Random.Range(0,2)==0?angle:-angle, 0) * link;
                Vector3 move = pos - this.Parent.transform.position;
                m_cxt.SetMoveDir(move);
                m_duration = move.magnitude/ m_cxt.speed;
            }

            //动作
            CheckPlayMoveAni(m_cxt.MoveDir, m_cxt.DirDir);
        }
        else if (m_subState == enRoleRoundType.stillLook)
        {
            m_cxt.dirModelSmooth = false;
            m_cxt.moveType = TranPartCxt.enMove.none;
            m_cxt.dirType = TranPartCxt.enDir.dir;
            m_cxt.SetDirDir(m_target.transform.position - this.Parent.transform.position);
            m_duration = cxt.lookDurationMax <= cxt.lookDurationMin ? cxt.lookDurationMin : UnityEngine.Random.Range(cxt.lookDurationMin, cxt.lookDurationMax);
            m_rsm.AniPart.Play(AniFxMgr.Ani_DaiJi, WrapMode.Loop, 0.2f, 1, true);
        }
        else
            Debuger.LogError("未知的类型:{0}", m_subState);

        //上下文不需要留着，马上回收
        cxt.Put();
    }
    
    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        return true;
    }

    public override void Leave()
    {
        if (TranPart != null)
            TranPart.RemoveCxt(m_cxt);
        m_cxt = null;
        m_roundCxtId = -1;
    }

    public override void Update()
    {
        //通用的退出判断
        if(m_target.IsUnAlive(m_targetId) //如果目标死亡
            || (m_duration != -1 && TimeMgr.instance.logicTime - m_beginTime>m_duration)//如果时间到了
            || m_cxt.IsDestroy(m_curId)////如果寻路结束了，TranPart会销毁这个上下文
            )
        {
            RSM.CheckFree();
            return;
        }

        //特定类型的判断
        Vector3 look = m_target.transform.position - this.Parent.transform.position;
        look.y = 0;
        if (m_subState == enRoleRoundType.run)
        {
            //和目标离得比较近则退出
            if (this.Parent.DistanceSq(m_target) < m_range* m_range)
            {
                RSM.CheckFree();
                return;
            }

            //每隔一定时间重新定位下主角
            if(TimeMgr.instance.logicTime- m_lastPathTime>0.7f)
            {
                m_lastPathTime = TimeMgr.instance.logicTime;
                TranPart.SetPathPos(m_target.transform.position);
            }
        }
        else if (m_subState == enRoleRoundType.sync || m_subState == enRoleRoundType.foward || m_subState == enRoleRoundType.back || m_subState == enRoleRoundType.leftRight || m_subState == enRoleRoundType.stillLook)
        {
            m_cxt.SetDirDir(look);//同步面向目标
            
            if(m_subState == enRoleRoundType.sync)
            {
                MovePart movePart = m_target.MovePart;
                //目标停下来则退出
                if (!movePart.IsMoveing)
                {
                    RSM.CheckFree();
                    return;
                }

                m_cxt.SetMoveDir(movePart.CurDir);
                CheckPlayMoveAni(m_cxt.MoveDir, m_cxt.DirDir);//4个动作播哪个要同步检测
            }
            //如果在自己移动过程中目标也移动了，那么切换到同步
            else if(m_isAuto&&(m_subState == enRoleRoundType.foward || m_subState == enRoleRoundType.back || m_subState == enRoleRoundType.leftRight)&& m_target.MovePart.IsMoveing)
            {
                m_subState = enRoleRoundType.sync;
                m_cxt.SetMoveDir(look);
                m_duration = -1;
            }
        }
    }
    #endregion

    void CheckPlayMoveAni(Vector3 move,Vector3 dir)
    {
        float angle = Vector3.Angle(move,dir);
        string aniName;
        if (angle <= 45f)
            aniName = "qianyi";
        else if (angle >= 135f)
            aniName = "houyi";
        else if(Vector3.Cross(move, dir).y>0)
            aniName = "zuoyi";
        else
            aniName = "youyi";

        //如果角色没有这个动作，转换为跑步动作
        AniPart aniPart =m_rsm.AniPart;
        float speed = 1;
        if (!aniPart.Ani.Sts.ContainsKey(aniName))
        {
            aniName = AniFxMgr.Ani_PaoBu;
            speed = m_cxt.speed / AniPart.MoveAniSpeed;
        }
            

        //如果已经在播放了
        if (aniPart.CurSt.name == aniName)
            return;

        m_rsm.AniPart.Play(aniName, WrapMode.Loop, 0.2f, speed, true);
    }
}
