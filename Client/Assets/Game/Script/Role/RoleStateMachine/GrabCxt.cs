using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


 public enum enGrabPos{
     none,//不改变位置
     bone,//骨骼
     speed,//速度
     max,
 }

 public enum enGrabDir
 {
     none,//不改变方向
     bone,//骨头的方向
     max,
 }

//位置相对类型
public enum enRelativeType
{
    hero,       //相对主角  主角不动
    monster,     //相对怪物 怪物不动
}


//注意先计算方向和方向偏移，再计算位置
public class GrabPosAndDirCxt
{
    public static string[] PosTypeName = new string[] { "不改变位置", "骨骼", "速度" };
    public static string[] DirTypeName = new string[] { "不改变方向", "骨骼" };
    static AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 2f), new Keyframe(1f, 1f, 0f, 0f));

    public bool autoIfObstacle=false;//如果碰到墙或者地板，自动进入浮空状态或者倒地
    public int frame = -1;//持续帧数
    public int smoothFrame=0;//渐变帧数
    public string bone = "";
    
    //位置相关
    public enGrabPos posType= enGrabPos.none;
    public Vector3 posOffset = Vector3.zero;
    public Vector3 speedDir= Vector3.forward;//速度的方向，相对于骨骼
    public float speed =5f;

    //方向相关
    public enGrabDir dirType = enGrabDir.none;
    public Vector3 dirOffset = Vector3.zero;//欧拉角

    

    public bool expand =true;

    public bool GetPosAndRot(Transform source, ref Vector3 pos, ref Quaternion rot)
    {
        Transform t = string.IsNullOrEmpty(bone) ? source : source.Find(bone);
        if (t == null)
        {
            Debuger.LogError("{1}抓取位置上下文没有找到骨骼:{0} ", source.name,bone);
            return false;
        }

        GetPosAndRotOfBone(t, ref pos, ref rot);
        return true;
    }
    void GetPosAndRotOfBone(Transform bone, ref Vector3 pos, ref Quaternion rot)
    {
        
        //先计算相对位置和偏移，再计算方向
        if (dirOffset != Vector3.zero)
            rot = bone.rotation * Quaternion.Euler(dirOffset);
        else
            rot = bone.rotation;

        if (posOffset != Vector3.zero)
            pos = bone.position + rot * posOffset;
        else
            pos = bone.position;
        
    }

    public bool GetSpeedRot(Transform source, ref Vector3 speedDir)
    {
        Transform t = string.IsNullOrEmpty(bone) ? source : source.Find(bone);
        if (t == null)
        {
            Debuger.LogError("抓取位置上下文没有找到骨骼:{0}",bone);
            return false;
        }

        GetSpeedRotOf(t,ref speedDir);
        return true;
        
    }

    void GetSpeedRotOf(Transform bone, ref Vector3 speedDir)
    {
        speedDir = bone.rotation * Quaternion.Euler(this.speedDir) * Vector3.forward;
    }

    public void CopyFrom(GrabPosAndDirCxt cfg)
    {
        if (cfg == null) return;

        //复制值类型的属性
        Util.Copy(cfg, this);
    }

    public void OnEnter(Role source, Role target, Transform sourceBone,ref Vector3 speedDir, ref Vector3 velocity)
    {
        if (posType == enGrabPos.none){
        }
        else if (posType == enGrabPos.bone || posType == enGrabPos.speed)
        {
            TranPart tranPart = target.TranPart;
            Transform tModel = target.RoleModel.Model;
            Vector3 pos =Vector3.zero;
            Quaternion rot = Quaternion.identity;
            //计算位置和旋转
            GetPosAndRotOfBone(sourceBone, ref pos, ref rot);

            //计算位置和旋转
            if (smoothFrame > 0 )
            {
                velocity = Vector3.zero;
                float smoothVelocity = Mathf.Lerp(smoothFrame * Util.One_Frame, 0, 0);
                pos = Vector3.SmoothDamp(tModel.position, pos, ref velocity, smoothVelocity, float.MaxValue, TimeMgr.instance.logicDelta);
                rot = tModel.rotation;                   
            }
            else
            {
                if (dirType == enGrabDir.none)
                    rot = tModel.rotation;
            }

            //设置位置和旋转
            tranPart.SetModelPosAndRot(pos, rot) ; 
        }
        else
            Debuger.LogError("未知的抓取位置的类型:{0}", posType);

        //计算速度
        if (posType == enGrabPos.speed)
        {
            GetSpeedRot(source.transform, ref speedDir);
        }
    }

    //返回false 表明当前阶段结束了
    public bool OnUpdate(Role source, Role target, Transform sourceBone,  int curFrame, Vector3 speedDir, ref Vector3 velocity)
    {
        if (posType == enGrabPos.bone )
        {
            TranPart tranPart = target.TranPart;
            Transform tModel = target.RoleModel.Model;
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            //计算位置和旋转
            GetPosAndRot(source.transform, ref pos, ref rot);

            //渐变的情况
            if (smoothFrame > 0 && curFrame < smoothFrame)
            {
                float factor =curFrame/(float)smoothFrame;
                float smoothVelocity = Mathf.Lerp(smoothFrame * Util.One_Frame, 0, animationCurve.Evaluate(factor));
                
                pos = Vector3.SmoothDamp(tModel.position, pos, ref velocity, smoothVelocity, float.MaxValue, TimeMgr.instance.logicDelta);
                if (dirType != enGrabDir.none)
                    rot = Quaternion.Slerp(tModel.rotation, rot, TimeMgr.instance.logicDelta / (smoothFrame*Util.One_Frame));
                else
                    rot = tModel.rotation;

            }
            else
            {
                if (dirType == enGrabDir.none)
                    rot = tModel.rotation;
            }

            //设置位置和旋转
            tranPart.SetModelPosAndRot(pos, rot);
        }
        else if (posType == enGrabPos.speed)
        {
            TranPart tranPart = target.TranPart;
            Transform tModel = target.RoleModel.Model;
            tranPart.SetModelPosAndRot(tModel.position + speedDir * speed, tModel.rotation);
        }

        

        return !(this.frame > 0 && this.frame < curFrame);
    }
}

public class GrabFxCxt{
    public int frame=0;//第几帧
    public string roleFxName = "";//角色特效名

    public bool expand = true;
    public void CopyFrom(GrabFxCxt cfg)
    {
        if (cfg == null) return;


        //复制值类型的属性
        Util.Copy(cfg, this);
    }
}

public class GrabCxt
{
    public int frame = -1;//持续帧数，-1则动画序列播放完成则退出抓取状态
    public bool destroyWhenEnd =false;//结束销毁被抓取角色
    public int endBuffId = -1;//结束状态
    public string endEventGroupId = string.Empty;//结束时的事件组，被取消或者自动结束时才会触发，被中断时不会
    public SimpleAnimationsCxt anis= new SimpleAnimationsCxt();//动作序列
    public List<GrabPosAndDirCxt> poss = new List<GrabPosAndDirCxt>();//位置控制列表
    public List<GrabFxCxt> fxs = new List<GrabFxCxt>();//特效控制列表
    public SkillEventGroupCfg eventGroup = new SkillEventGroupCfg();

    public void CopyFrom(GrabCxt cfg)
    {
        if (cfg == null) return;

        //复制值类型的属性
        Util.Copy(cfg, this);

        //复制引用类型的属性
        anis.CopyFrom(cfg.anis);

        eventGroup.CopyFrom(cfg.eventGroup);

        poss.Clear();
        foreach (GrabPosAndDirCxt c in cfg.poss)
        {
            poss.Add(new GrabPosAndDirCxt());
            poss[poss.Count - 1].CopyFrom(c);
        }

        fxs.Clear();
        foreach (GrabFxCxt c in cfg.fxs)
        {
            fxs.Add(new GrabFxCxt());
            fxs[fxs.Count - 1].CopyFrom(c);
        }
    }

    public void PreLoad()
    {
        if (endBuffId>0)
            BuffCfg.ProLoad(endBuffId);
        foreach (GrabFxCxt c in fxs)
        {
            RoleFxCfg.ProLoad(c.roleFxName);
        }
        SkillEventGroupCfg.PreLoad(endEventGroupId);
    }
}

public class GrabCxt2
{
    public static string[] RelativeTypeName = { "主角", "敌人"};

    public enRelativeType type = enRelativeType.hero;

    public bool isLeft = true;      //相机在角色的那一侧
    public float Role_distance;     //两个角色间的距离
    public Vector3 Camera_euler;    //角色转到相机方向的角度
    public Vector3 Role_Camera_euler;//相机转到角色方向角度
    public float Camera_distance;   //相机和角色的距离
}


