#region Header
/**
 * 名称：位置部件
 
 * 日期：2015.9.21
 * 描述：控制角色位置、方向和大小,获取骨骼等
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TranPart:RolePart
{
    public static Vector3 Default_Gravity_Speed = new Vector3(0, -20.0F, 0);//模仿重力的贴近地面时的速度
    public static Vector3 Default_Gravity = new Vector3(0, -9.8F, 0);//重力加速度
    public static Vector3 s_gravity_speed = Default_Gravity_Speed;//模仿重力，全局的

    #region Fields
    Transform m_root;
    Transform m_tranModel;
    List<TranPartCxt> m_cxts = new List<TranPartCxt>();//这里sample要保证一定顺序，以后如果删除操作有效率问题，考虑用System.Collections.Specialized.OrderedDictionary\
    List<TranPartCxt> m_removes = new List<TranPartCxt>();
    float m_lastSampleTime =0;
    
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.tran; } }
    public bool IsGrounded { get{return m_tranModel.localPosition.y<=0.01f;}}
    public Vector3 Pos
    {//这里可以认为是模型的位置，用于伤害计算
        get
        {
            //如果在浮空或者击翻状态下要调整下位置
            var behit = RSM.StateBehit;
            if (behit.IsCur&&(behit.CurStateType == enBehit.befloat || behit.CurStateType == enBehit.beFly))
            {
                return m_tranModel.position + new Vector3(0, RoleModel.Height*0.4f, 0);
            }
            else
                return m_tranModel.position;
        } }
    
    #endregion


    #region Frame   
    
     
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        m_root = RoleModel.Root;
        m_tranModel = RoleModel.Model;
        if (m_cxts.Count != 0)
        {
            Debuger.LogError("逻辑错误");
            m_cxts.Clear();
        }
        return true;
    }

    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
        m_lastSampleTime = TimeMgr.instance.logicTime;
        ResetHight();
    }

    public override void OnDestroy()
    {
        foreach (TranPartCxt c in m_cxts)
        {
            IdTypePool<TranPartCxt>.Put(c);
        }
        m_cxts.Clear();
        m_tranModel.localEulerAngles = Vector3.zero;//模型方向可能在渐变，这个时候要复位下
    }

    public override void OnUpdate() { 
        if(Parent.State != Role.enState.alive)return;
        if(!TimeMgr.instance.IsStop &&TimeMgr.instance.IsPause)
        {
            if(m_cxts.Count >0)
            {
                foreach (TranPartCxt c in m_cxts)
                {
                    IdTypePool<TranPartCxt>.Put(c);
                }
                m_cxts.Clear();
            }
            return;
        }
        Sample();
    }
    #endregion


    #region Private Methods
    void Sample()
    {
        float time = TimeMgr.instance.logicTime;
        if(time == m_lastSampleTime)
            return;
        float delta = time - m_lastSampleTime;
        m_lastSampleTime = time;
        //float delta  =Time.deltaTime;

        Vector3 offset = Vector3.zero;//有跟随类型时为位置，否则为方向速度
        Vector3 y = Vector3.zero;
        Vector3 accelerate =Vector3.zero;
        Vector3 lookDir = Vector3.zero;
        bool isAvoid = false;//是不是免疫
        TranPartCxt dirCxt = null;//决定方向的那个上下文，注意方向是不支持重叠的

        #region 1 计算位置增量或者位置，同时删除不需要的
        TranPartCxt c;
        bool offsetInvalid;
        bool dirPosInvalid;
        bool over;
        Vector3 v;
        float sp;
        bool hasPathCalc =false;//判断是不是有寻路的操作，是的话后面移动后要做防卡住的处理
        TranPartCxt touchEndCxt=null;
        //string yLog="";
        for (int i=m_cxts.Count-1;i>=0;--i)
        {
            c =m_cxts[i];
            offsetInvalid = false;
            dirPosInvalid = false;
            over = false;
            v = Vector3.zero;

            //1 计算速度变化
            sp = c.speed;
            if (c.accelerate != 0 || c.moveValidAxis == enValidAxis.vertical)
            {
                if (c.moveValidAxis == enValidAxis.vertical)
                {
                    c.speed = Mathf.Clamp(c.speed + (c.accelerate + Default_Gravity.y) * delta, c.minSpeed, c.maxSpeed);
                    //yLog += string.Format("计算出的速度:{0}", c.speed);
                }
                else
                    c.speed = c.accelerate == 0 ? c.speed : Mathf.Clamp(c.speed + c.accelerate * delta, c.minSpeed, c.maxSpeed);
                sp = (sp + c.speed)/2 ;//用平均速度，而不是最后的速度
            }
            
            //2 计算位移（可叠加）
            if (!isAvoid && c.moveType != TranPartCxt.enMove.none)
            {
                switch (c.moveType)
                {
                    case TranPartCxt.enMove.dir: {
                        v = c.MoveDir * sp * delta;
                        if(c.moveValidAxis== enValidAxis.horizontal)
                            offset += v;
                        else
                            y += v;    
                    }break;
                    case TranPartCxt.enMove.look:{
                        if(c.moveTarget == null || !c.moveTarget.IsValid()){
                            offsetInvalid = true; break;
                        }
                        v = (c.moveTarget.Get(c.moveValidAxis, m_root.position) - m_root.position).normalized * sp * delta;
                        offset += v;
                    };break;
                    case TranPartCxt.enMove.back:{
                        if(c.moveTarget == null || !c.moveTarget.IsValid()){
                            offsetInvalid = true; break;
                        }
                        v = -(c.moveTarget.Get(c.moveValidAxis, m_root.position) - m_root.position).normalized * sp * delta;
                        offset += v;
                    };break;
                    case TranPartCxt.enMove.avoid:
                    {
                        
                    };break;
                    case TranPartCxt.enMove.path:
                    {
                        if (RoleModel.RolePath.Reached)
                        {
                            offsetInvalid = true; break;
                        }
                        v = RoleModel.RolePath.CalculateOffset(m_root.position, sp, delta);
                        if (RoleModel.RolePath.Reached)
                        {
                            offsetInvalid = true; break;
                        }
                        offset += v;
                        hasPathCalc = true;
                    }; break;
                    default: Debuger.LogError("未知的移动类型{0}", c.moveType);break;
                }
                ++c.count;//偏移次数
            }

            //3 计算方向（不可叠加）,第一个(或者是follow)有方向的上下文
            if (!isAvoid && c.dirType != TranPartCxt.enDir.none && c.moveValidAxis == enValidAxis.horizontal && (dirCxt == null || c.moveType == TranPartCxt.enMove.avoid))
            {
                dirCxt = c;
                switch (c.dirType)
                {
                    case TranPartCxt.enDir.forward:lookDir =v;break;
                    case TranPartCxt.enDir.backward:lookDir =-v;break;
                    case TranPartCxt.enDir.look:{
                        if(c.dirTarget == null || !c.dirTarget.IsValid()){
                            dirPosInvalid = true; break;
                        }
                        lookDir=c.dirTarget.Get(c.dirValidAxis,m_root.position)- m_root.position;
                    };break;
                    case TranPartCxt.enDir.back:{
                        if(c.dirTarget == null || !c.dirTarget.IsValid()){
                            dirPosInvalid = true; break;
                        }
                        lookDir=m_root.position - c.dirTarget.Get(c.dirValidAxis,m_root.position);
                    };break;
                    case TranPartCxt.enDir.dir: lookDir = c.DirDir; break;
                    default: Debuger.LogError("未知的方向类型{0}", c.dirType);break;
                }
            }
                
            //4 如果是follow的，那么记下来，之后的都不能改变
            isAvoid = c.moveType == TranPartCxt.enMove.avoid;

            //5 计算是不是结束了
            over = c.duration != -1 && time - c.beginTime>=c.duration;
            if (over||offsetInvalid || dirPosInvalid)
                m_removes.Add(c);
            else if(c.IsTouchOver)//计算是不是碰到要销毁
            {
                if (touchEndCxt != null)
                    Debuger.LogError("暂时不支持同时存在两个碰到就结束的移动上下文，是不是移动事件配置出错");
                touchEndCxt = c;
            }

        }
        #endregion

        #region 2 设置叠加后的位置方向
        if (!isAvoid){
            //位置
            if(!RSM.IsLimitMove &&!RSM.IsNoCollider)
            {
                if (offset != Vector3.zero)
                {
                    Vector3 pos = m_root.position;
                    var collisionFlags = RoleModel.CC.Move(offset + s_gravity_speed * delta);

                    //碰到的话结束
                    if (touchEndCxt != null && ((collisionFlags & CollisionFlags.Sides) != 0))
                        m_removes.Add(touchEndCxt);

                    if (hasPathCalc)
                    {
                        Vector3 newPos = m_root.position;
                        Vector3 trueOffset = newPos - pos;
                        //检测是不是卡住
                        RoleModel.RolePath.CheckStuck(newPos, offset, trueOffset);
                    }
                }
                else if (!RoleModel.CC.isGrounded)
                    RoleModel.CC.Move(s_gravity_speed * delta);//CharacterController比较耗效率，即使是静止也要消耗0.06ms,这里不在地面上的时候才move
            }
            

            //模型y轴位移
            if (m_tranModel != null && RoleModel.IsShow&& !RSM.IsNoCollider)
            {
                //y存在的话，重力已经计算在上面做了，否则在这里做下
                if (y == Vector3.zero && !IsGrounded && !RSM.IsAir)
                {
                    y = s_gravity_speed * delta;
                    //yLog += string.Format("没有垂直速度，但是不在地板");
                }

                if (y != Vector3.zero)
                {
                    //yLog += string.Format("计算出的偏移：{0}",y);
                    y = m_tranModel.localPosition + y;
                    if (y.y < 0)
                    {
                        m_tranModel.localPosition = Vector3.zero;
                        //yLog += string.Format("到地板了");    
                    }
                    else
                        m_tranModel.localPosition = y;
                }

            }

            //if(!string.IsNullOrEmpty(yLog))
            //    Debug.Log(m_parent.Cfg.id+"  "+yLog);

            //3 方向
            Quaternion modelRotate = m_tranModel != null ? m_tranModel.rotation : Quaternion.identity;
            if (dirCxt != null && lookDir != Vector3.zero)
            {
                if (dirCxt.dirModelSmooth)
                {
                    Quaternion rot = m_root.rotation;
                    Quaternion toTarget = Quaternion.LookRotation(lookDir);
                    rot = Quaternion.Slerp(rot, toTarget, 10 * Time.deltaTime);
                    m_root.rotation = rot;
                }
                else
                {
                    m_root.forward = lookDir;
                }
            }

            //4 模型方向
            if (m_tranModel != null)
            {
                if (m_tranModel.localEulerAngles != Vector3.zero)
                {
                    m_tranModel.localEulerAngles = Vector3.zero;
                }
            }
        }
        #endregion
    
        //3 删除需要删除的
        if (m_removes.Count != 0)
        {
            for (int i = m_removes.Count - 1; i >= 0; --i)
            {
                RemoveCxt(m_removes[i]);
            }
            m_removes.Clear();
                
        }
    }
    #endregion

    //设置缩放，注意这里是乘以当前的缩放，而不是乘以原始缩放
    public void AddScale(Vector3 scale, float duration, float smoothBeing = 0, float smoothEnd = 0)
    {
        Debuger.Log("未实现");
    }

    //直接设置方向，注意移动中也有可能会改变方向，这个行为可能被覆盖
    //一般是和AddCxt一起用，快速调整方向
    public void SetEuler(Vector3 euler, enValidAxis validAxis = enValidAxis.horizontal)
    {
        if (validAxis == enValidAxis.horizontal)
        {
            euler.x = 0;
            euler.z = 0;
        }

        m_root.eulerAngles = euler;
    }

    //直接设置方向，注意移动中也有可能会改变方向，这个行为可能被覆盖
    //一般是和AddCxt一起用，快速调整方向
    public void SetDir(Vector3 dir)
    {
        dir.y = 0;
        if (dir == Vector3.zero)
            return;
        m_root.forward = dir;
    }
    public void ResetHight()
    {
        if (!RoleModel.IsShow)
        {
            //Debuger.LogError("隐藏中，不能设置模型的位置：{0}", m_parent.Cfg.id);
            return;
        }

        if (m_root.GetComponent<SimpleRole>().m_needResetPos)
            m_tranModel.localPosition =Vector3.zero;

    }

    //用于设置怪物在空中的位置和方向，一般情况下外部不需要调用
    public void SetModelPosAndRot(Vector3 pos,Quaternion rot)
    {
        if (!RoleModel.IsShow)
        {
            Debuger.LogError("隐藏中，不能设置模型的位置：{0}", m_parent.Cfg.id);
            return;
        }
        if(RSM.IsNoCollider)
        {
            //Debuger.LogError("没有碰撞的角色不能设置模型位置:{0}", m_parent.Cfg.id);
            return;
        }


        //root的位置
        Vector3 offset = pos -m_tranModel.position;
        offset.y =0;
        RoleModel.CC.Move(offset + s_gravity_speed * TimeMgr.instance.logicDelta);

        //model的位置
        m_tranModel.position = new Vector3(m_tranModel.position.x, pos.y, m_tranModel.position.z);

        //root方向
        Vector3 dir = rot*Vector3.forward;
        dir.y = 0;
        RoleModel.Root.forward = dir;


        //model的方向
        m_tranModel.rotation =rot;
    }


    //直接设置位置，一般是和AddCxt一起用，快速调整位置
    public void SetPos(Vector3 pos, enValidAxis validAxis = enValidAxis.horizontal)
    {
        if(validAxis == enValidAxis.horizontal)
            m_root.position = PosUtil.CaleByTerrains(pos);
        else if(validAxis == enValidAxis.vertical)
        {
            pos.x = m_root.position.x;
            pos.y = m_root.position.y;
            m_root.position = pos;
        }
        else
            Debuger.LogError("未知的类型:{0}", validAxis);
    }

    //设置速度和方向,由于支持叠加的机制，所以不能实时改变，如果遇到需要实时改变的情况，可以用setpos和setdir
    public TranPartCxt AddCxt()
    {
        
        //没有碰撞的角色不能有移动上下文
        if (RSM.IsNoCollider)
        {
            //Debuger.LogError("没有碰撞的角色不能获取移动上下文:{0}", m_parent.Cfg.id);
            return null;
        }

        TranPartCxt c = IdTypePool<TranPartCxt>.Get();
        c.beginTime = TimeMgr.instance.logicTime;
        m_cxts.Add(c);
        return c;
    }

    public void RemoveCxt(TranPartCxt c)
    {
        if (!m_cxts.Remove(c))
        {
            //Debuger.LogError("不存在这个速度上下文{0} 角色id:{1} 是不是在对象池中:{2}", c.Id, Parent.Cfg.id, c.IsInPool);
            return;
        }
        ////模型方向可能在渐变，这个时候要复位下
        //if(c.dirType != TranPartCxt.enDir.none && c.dirModelSmooth == true)
        //    m_tranModel.localEulerAngles = Vector3.zero;

        //如果是寻路的话要停止寻路
        if (c.moveType == TranPartCxt.enMove.path)
            RoleModel.RolePath.Stop();

        if (this.Parent.State == Role.enState.alive && !string.IsNullOrEmpty(c.endSkill))
        {
            CombatPart.Play(c.endSkill,c.EndSkillTarget);
        }
            


        IdTypePool<TranPartCxt>.Put(c);
    }

    public void SetPathPos(Vector3 pos)
    {
        RoleModel.RolePath.Move(pos);
    }

    public bool IsPathFinish()
    {
        return RoleModel.RolePath.Reached;
    }

    public Vector3 GetYOff(float rate)
    {
		return m_tranModel.position + Vector3.up * RoleModel.Height*rate;
    }

    public Vector3 GetRoot()
    {
        return m_root.position;
    }
}
