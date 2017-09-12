#region Header
/**
 * 名称：特效创建参数的相关配置
 
 * 日期：2016.1.19
 * 描述：出身位置、方向、数量、偏移值等参数
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public enum enFxCreateDir
{
    zero,//原点
    forward,//主角前方
    backward,//主角反向
    look,//主角看向目标
    back,//目标看向主角
    targetForward,//目标前方
    targetBackward,//目标反向
    max,
}


public enum enFxCreatePos
{
    source,//释放者
    target,//目标(没有的话自动找最近的敌人)
    bone,//骨骼
    matrial,//材质特效
    hitPos,//碰撞点创建
    camera,//相机
    target2, //目标
    hate,       //仇恨目标
    hateNew,    //仇恨值目标
    hateNewNotFind,//仇恨值目标(不自动查找)
    closestSame,//最近的友方
    closestEnemy,//最近的敌方
    closestNeutral,//最近的中立阵营
    parent,//主人
    hero,//主角
    max,
}



public class FxCreateCfg
{
    public static string[] DirTypeNames = new string[] { "原点", "释放者前方", "释放者反向", "释放者看向目标", "目标看向释放者", "目标前方", "目标反向"};
    public static string[] PosTypeName = new string[] { "释放者", "目标(没有的话自动找最近的敌人)", "绑定骨骼", "绑定材质", "碰撞点", "相机", "目标","仇恨目标", "仇恨值目标(没有的话自动找最近的敌人)", "仇恨值目标",
        "最近的友方", "最近的敌方", "最近的中立阵营","主人","主角" };

    public string name=""; //特效名
    public string fireFx = "";
    public string iceFx = "";
    public string thunderFx = "";
    public string darkFx = "";
    public int durationFrame = -1;//持续帧数

    public enFxCreateDir dirType = enFxCreateDir.forward;//出生方向类型
    public enFxCreatePos posType = enFxCreatePos.source;//出生位置类型
    public float dirOffset = 0;//角度偏移
    public Vector3 posOffset = Vector3.zero;//位置偏移
    public float randomBegin = 0;//随机左区间
    public float randomEnd = 0;//随机右区间

    public int num=1;//数量
    public float multiDirOffset = 0;//多个时的方向偏移，相对于上一个
    public Vector3 multiPosOffset = Vector3.zero;//多个时的位置偏移，相对于上一个
    
    public string bone = "";//没有则为根节点
    public bool follow=true;//跟随骨骼
    public bool alignSourceY = false;//高度和source对齐，一般用来使特效创建在贴地表的地方





    public static bool BindBoneAndGetPosAnDir(Transform t,Transform tSource, Transform tTarget, Vector3 hitPos, 
        enFxCreateDir dirType, enFxCreatePos posType, float dirOffset, Vector3 posOffset,
        string bone, bool follow, bool alignSourceY,out Vector3 pos,out Vector3 euler, Role targetCreate)
    {
        bool disableDirAndPos = false;
        pos = Vector3.zero;
        euler = Vector3.zero;
        

        //绑定类型的话，先绑上
        Vector3 zeroPos = Vector3.zero;
        Vector3 zeroEuler = Vector3.zero;
        switch (posType)
        {
            case enFxCreatePos.bone:
                {//绑定骨骼
                    Transform b = string.IsNullOrEmpty(bone) ? tSource : tSource.Find(bone);
                    if (b == null)
                    {
                        Debuger.LogError("特效找不到要绑定的骨骼:{0} {1}", bone, t == null ? "" : t.name);
                        return false;
                    }
                    if (follow && t != null)
                        t.SetParent(b, false);
                    zeroPos = b.position;
                    zeroEuler = b.eulerAngles;

                }; break;
            case enFxCreatePos.matrial:
                {//绑定材质
                    Transform tMesh = tSource.Find("model/body_mesh");
                    if (tMesh != null && t!= null)
                    {
                        t.SetParent(tMesh, false);
                        t.localPosition = Vector3.zero;
                        t.localEulerAngles = Vector3.zero;
                    }

                    tMesh = tSource.Find("model/weapon_mesh");
                    if (tMesh != null && t != null)
                    {
                        GameObject go2 = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately(t == null ? "" : t.name, false);
                        Transform t2 = go2.transform;
                        t2.SetParent(tMesh, false);
                        t2.localPosition = Vector3.zero;
                        t2.localEulerAngles = Vector3.zero;
                        go2.SetActive(true);//设好了父节点了，这个时候再SetAct
                    }
                    disableDirAndPos = true;
                }; break;
            case enFxCreatePos.camera:
                {//绑定相机
                    CameraMgr cm = CameraMgr.instance;
                    if (cm == null)
                    {
                        Debuger.LogError("找不到相机特效不能绑定:{0}", t == null ? "" : t.name);
                        
                        return false;
                    }
                    Transform cmTran = cm.transform;
                    if (follow&& t != null)
                        t.SetParent(cmTran, false);
                    zeroPos = cmTran.position;
                    zeroEuler = cmTran.eulerAngles;
                }; break;
        }

        //不需要设置位置和偏移的话
        if (disableDirAndPos)
            return false;

        //方向
        switch (dirType)
        {
            case enFxCreateDir.zero:
                {//原点
                    euler = zeroEuler;
                }; break;
            case enFxCreateDir.forward:
                {//主角前方
                    euler = tSource.eulerAngles;
                    
                }; break;
            case enFxCreateDir.backward:
                {//主角反向
                    euler = tSource.eulerAngles + new Vector3(0, 180f, 0);
                    
                }; break;
            case enFxCreateDir.look:
                {//主角看向目标
                    if (tTarget!= null)
                        euler = Quaternion.LookRotation(tTarget.position - tSource.position).eulerAngles;
                    else
                    {
                        euler = tSource.eulerAngles;
                        Debuger.Log("特效创建的时候目标为空,方向类型:{0}", dirType);
                    }
                        
                }; break;
            case enFxCreateDir.back:
                {//目标看向主角
                    if (tTarget != null)
                        euler = Quaternion.LookRotation(tSource.position - tTarget.position).eulerAngles;
                    else
                    {
                        euler = tSource.eulerAngles;
                        Debuger.Log("特效创建的时候目标为空,方向类型:{0}", dirType);
                    } 
                    
                }; break;
            case enFxCreateDir.targetForward:
                {//目标前方
                    if (tTarget != null)
                        euler = tTarget.eulerAngles;
                    else
                    {
                        euler = tSource.eulerAngles;
                        Debuger.Log("特效创建的时候目标为空,方向类型:{0}", dirType);
                    } 
                }; break;
            case enFxCreateDir.targetBackward:
                {//目标反向
                    if (tTarget != null)
                        euler = tTarget.eulerAngles + new Vector3(0, 180f, 0);
                    else
                    {
                        euler = tSource.eulerAngles;
                        Debuger.Log("特效创建的时候目标为空,方向类型:{0} ", dirType);
                    }
                }; break;
            default:
                {
                    Debuger.LogError("未知的类型:{0}", dirType);
                    return false;
                }
        }
        if (dirType!= enFxCreateDir.zero)//除了原点，其他都是水平方向
        {
            euler.x = 0;
            euler.z = 0;
        }
        
        if (dirOffset != 0)//方向偏移
            euler = (Quaternion.Euler(euler) * Quaternion.Euler(0, dirOffset, 0)).eulerAngles;
            

        //位置
        switch (posType)
        {
            case enFxCreatePos.source:
                {
                    pos = tSource.position;
                }; break;
            
            case enFxCreatePos.bone:
                {//绑定骨骼
                    pos = zeroPos;
                }; break;
            case enFxCreatePos.hitPos:
                {//打击点
                    pos = hitPos;
                }; break;
            case enFxCreatePos.camera:
                {//绑定相机
                    pos = zeroPos;
                }; break;
            case enFxCreatePos.target: 
            case enFxCreatePos.target2:
            case enFxCreatePos.hate: 
            case enFxCreatePos.hateNew:
            case enFxCreatePos.hateNewNotFind: 
            case enFxCreatePos.closestSame: 
            case enFxCreatePos.closestEnemy: 
            case enFxCreatePos.closestNeutral: 
            case enFxCreatePos.parent: 
            case enFxCreatePos.hero:
                {
                    Transform tTargetCreate =targetCreate.transform;
                    if (tTargetCreate != null)
                        pos = tTargetCreate.position;
                    else
                    {
                        pos = tTargetCreate.position;
                        Debuger.Log("特效创建的时候目标为空,位置类型:{0}", posType);
                    }
                }break;
        default:
                {
                    Debuger.LogError("未知的类型:{0} {1}", posType,t.name);
                    return false;
                }
        }
        if (alignSourceY)
            pos.y = tSource.position.y;
        if (posOffset != Vector3.zero)//位置偏移，暂时根据当前方向
            pos = pos+Quaternion.Euler(euler) * posOffset;
             
        

        return true;
    }

    public static GameObject Create(Role source, Role target, Vector3 hitPos,string name, enFxCreateDir dirType, enFxCreatePos posType, 
        float dirOffset, Vector3 posOffset,float randomBegin,float randomEnd,
        string bone, bool follow, int durationFrame, bool alignSourceY, Role targetCreate)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debuger.LogError("特效名为空");
            return null;
        }

        GameObject go = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately(name, false);
        if(go == null)return null;
        Transform t =go.transform;
        Transform tSource = source.transform;
        Transform tTarget = target == null?null:target.transform;

        Vector3 pos;
        Vector3 euler;

        if (BindBoneAndGetPosAnDir(t, tSource, tTarget, hitPos, dirType, posType, dirOffset, posOffset, bone, follow, alignSourceY ,out pos, out euler, targetCreate))
        {
            t.eulerAngles = euler;//设置最终方向
            if (randomEnd >= randomBegin && randomBegin > 0)//2d水平面上随机下
                pos = pos + Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0) * Vector3.forward * UnityEngine.Random.Range(randomBegin, randomEnd);
            t.position = pos;//设置最终位置 
        }
		go.SetActive(true);
        //关卡结束后销毁
        if (durationFrame == FxDestroy.Destroy_Change_Scene)
        {
            float delayDestroy = FxDestroy.GetRunTimeDelayIfExist(go);
            if (delayDestroy == -1)
                FxDestroy.Add(go, FxDestroy.Destroy_Change_Scene);
        }
        //一定时间后销毁
        else if (durationFrame > 0)
        {
            float delayDestroy = FxDestroy.GetRunTimeDelayIfExist(go);
            if (delayDestroy < 0 || durationFrame * Util.One_Frame < delayDestroy)
                FxDestroy.Add(go, durationFrame * Util.One_Frame);
        }
        
        return go;
    }

    public GameObject Create(Role source, Role target, Vector3 hitPos, enElement elem, Action<GameObject, object> a = null, object param = null)
    {
        //用于出生点的目标
        Role targetCreate = source;
        switch (posType)
        {
            case enFxCreatePos.target: targetCreate = target == null? RoleMgr.instance.GetClosestTarget(source, enSkillEventTargetType.enemy): target; break;
            case enFxCreatePos.target2: targetCreate = target; break;
            case enFxCreatePos.hate: targetCreate = source.HatePart.GetTargetLegacy(); break;
            case enFxCreatePos.hateNew: targetCreate = source.HatePart.GetTarget(); break;
            case enFxCreatePos.hateNewNotFind: targetCreate = source.HatePart.GetTargetLegacy(false); break;
            case enFxCreatePos.closestSame: targetCreate = RoleMgr.instance.GetClosestTarget(source, enSkillEventTargetType.same); break;
            case enFxCreatePos.closestEnemy: targetCreate = RoleMgr.instance.GetClosestTarget(source, enSkillEventTargetType.enemy); break;
            case enFxCreatePos.closestNeutral: targetCreate = RoleMgr.instance.GetClosestTarget(source, enSkillEventTargetType.neutral); break;
            case enFxCreatePos.parent: targetCreate = source.Parent; break;
            case enFxCreatePos.hero:{
                    Role r = RoleMgr.instance.Hero;
                    targetCreate = r != null && r.State == Role.enState.alive ? r : null;
                };break;
        }
        if (targetCreate == null)
            return null;

        //不同元素属性创建不同特效
        string mod = this.name;
        switch (elem)
        {
            case enElement.none: mod = name; break;
            case enElement.fire: mod = fireFx; break;
            case enElement.ice: mod = iceFx; break;
            case enElement.thunder: mod = thunderFx; break;
            case enElement.dark: mod = darkFx; break;
            default: Debuger.LogError("特效 未知的元素类型:{0}", elem); break;
        }
        if (string.IsNullOrEmpty(mod))
            mod = name;

        

        float dOffset = dirOffset;
        Vector3 pOffset = posOffset;
        GameObject firstGo=null;
        GameObject go;
        for(int i =0;i<num;++i){
            if (i >= 1)
            {
                dOffset +=multiDirOffset;
                pOffset += multiPosOffset;
            }
            go = Create(source, target, hitPos, mod, dirType, posType, dOffset, pOffset, randomBegin, randomEnd, bone, follow, durationFrame,alignSourceY, targetCreate);
            if (i ==0  )
                firstGo=  go;

            if(go!= null && a!=null )
                a(go, param);
        }
        return firstGo;
    }

    public void CopyFrom(FxCreateCfg cfg)
    {
       
        if (cfg == null) return;
       

        //复制值类型的属性
        Util.Copy(cfg, this, BindingFlags.Public | BindingFlags.Instance);
    }

    public void PreLoad(){
        if (!string.IsNullOrEmpty(this.name))
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(this.name);
        if (!string.IsNullOrEmpty(this.fireFx))
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(this.fireFx);
        if (!string.IsNullOrEmpty(this.iceFx))
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(this.iceFx);
        if (!string.IsNullOrEmpty(this.thunderFx))
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(this.thunderFx);
        if (!string.IsNullOrEmpty(this.darkFx))
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(this.darkFx);
    }
}
