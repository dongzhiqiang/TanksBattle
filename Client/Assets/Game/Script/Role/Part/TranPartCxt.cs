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


//上下文，用于外部设置位置、方向
public class TranPartCxt : IdType
{
    public enum enMove//移动类型
    {
        none,
        dir,//方向
        look,//当前点看着目标点
        back,//目标点到当前点
        avoid,//免疫，意味着位置的方向和位置将独占，不能叠加计算
        path,//寻路到目标点
    }
    public enum enDir//方向类型
    {
        none,
        forward,//和移动方向一样
        backward,//和移动方向相反
        look,//当前点看着目标点
        back,//目标点到当前点
        dir,//指定的方向
    }

    //移动相关
    public enMove moveType = enMove.none;
    public enValidAxis moveValidAxis = enValidAxis.horizontal;//有效方向
    public float speed=0;
    public float accelerate = 0;//加速度
    public float minSpeed = float.MinValue;//速度下限，用于限制加速度
    public float maxSpeed = float.MaxValue;//速度上限，用于限制加速度
    Vector3 moveDir = Vector3.zero;
    public Vector3 MoveDir { get { return moveDir; } }
    public Pos moveTarget = null; 
    
    

    //方向相关
    public enDir dirType = enDir.none;
    public enValidAxis dirValidAxis = enValidAxis.horizontal;//有效方向    
    public Pos dirTarget = null;
    Vector3 dirDir = Vector3.forward;
    public Vector3 DirDir { get { return dirDir; } }
    public bool dirModelSmooth =false;//模型是不是带渐变
            
    //持续时间相关
    public float duration=-1;

    //碰撞层设置相关
    Role colliderRole = null;
    int colliderRoleId = 0;
    enGameLayer colliderLayer = enGameLayer.max;

    //碰到结束
    public int touchOverFrame = -1;//在第几帧到移动完成之间检测是不是碰到敌人，碰到立即结束，如果填-1表明不用判断是不是碰到敌人

    //结束技能
    public string endSkill = null;
    public Role skillTarget = null;
    public int skillTargetId = 0;


    public float beginTime = 0;//这个值外部不用填，从TranPart获取的时候会初始化
    public int count = 0;//这个值外部不用填,计算过多少次

    public bool IsTouchOver
    {
        get {
            return touchOverFrame == -1 ? false : TimeMgr.instance.logicTime >= (beginTime+ touchOverFrame*Util.One_Frame);
        }
    }

    public Role EndSkillTarget
    {
        get
        {
            return skillTarget == null || skillTarget.IsUnAlive(skillTargetId) ? null : skillTarget;
        }
    }


    public void SetMoveDir(Vector3 moveDir, enValidAxis moveValidAxis = enValidAxis.horizontal)
    {
        this.moveValidAxis = moveValidAxis;
        this.moveDir = PosUtil.CalcValidAxisRef(moveValidAxis, moveDir).normalized;
    }

    public void SetDirDir(Vector3 dirDir, enValidAxis dirValidAxis = enValidAxis.horizontal)
    {
        this.dirValidAxis = dirValidAxis;
        this.dirDir = PosUtil.CalcValidAxisRef(dirValidAxis, dirDir);//方向暂时不用归一化了
    }
    
    public void SetColliderLayer(Role r,enGameLayer gameLayer)
    {
        colliderRole = r;
        colliderRoleId = r.Id;
        colliderLayer = gameLayer;
        r.RenderPart.SetLayer(gameLayer);
    }

    public void SetEndSkill(string endSkill,Role skillTarget)
    {
        this.endSkill = endSkill;
        this.skillTarget = skillTarget;
        this.skillTargetId = skillTarget == null?0:skillTarget.Id;
    }
        
    public override void OnClear()
    {
        moveType = enMove.none;
        moveValidAxis = enValidAxis.horizontal;//有效方向
        speed=0;
        accelerate = 0;//加速度
        minSpeed = float.MinValue;//速度下限，用于限制加速度
        maxSpeed = float.MaxValue;//速度上限，用于限制加速度
        moveDir = Vector3.zero;
        
        //方向相关
        dirType = enDir.none;
        dirValidAxis = enValidAxis.horizontal;//有效方向    
        dirModelSmooth =false;//模型是不是带渐变
            
        //持续时间相关
        duration=-1;
        count=0;

        touchOverFrame = -1;

        if (!string.IsNullOrEmpty(endSkill))
            endSkill = null;
        skillTarget = null;
        skillTargetId = 0;

        if (colliderLayer != enGameLayer.max&& !colliderRole.IsUnAlive(colliderRoleId)&& colliderRole.RenderPart.GetLayer()== colliderLayer)
        {   
            colliderRole.RenderPart.ResetLayer();
            colliderLayer = enGameLayer.max;
            colliderRoleId = 0;
            colliderRole = null;
        }


        if (moveTarget != null)
        {
            moveTarget.Put();
            moveTarget = null;
        }

        if (dirTarget != null)
        {
            dirTarget.Put();
            dirTarget = null;
        }

        
    }

}
    
