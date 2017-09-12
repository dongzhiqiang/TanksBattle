#region Header
/**
 * 名称：动作绑定的特效
 
 * 日期：2015.9.29
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class AniFxGroup
{
    public string name;//对应的动作名
    public List<AniFx> fxs = new List<AniFx>();

    AniFxMgr parent;
    public string[] FxNames {
        get
        {
            string[] ss = new string[fxs.Count];
            for(int i=0;i<fxs.Count;++i){
                ss[i] = fxs[i].prefab == null ? "" : fxs[i].prefab.name;
            }
            return ss;
        }
    }

    public void CopyFrom(Transform root,AniFxGroup g)
    {
        if (g == null) return;

        //复制值类型的属性
        Util.Copy(g, this, BindingFlags.Public | BindingFlags.Instance);

        for (int i = 0; i < g.fxs.Count; ++i)
        {
            AniFx fx= new AniFx();
            if (fx.CopyFrom(root,g.fxs[i]))
                fxs.Add(fx);
        }
    }

    //初始化
    public void Init(AniFxMgr parent)
    {
        this.parent = parent;
        for (int i = 0; i < fxs.Count; ++i)
        {
            fxs[i].Init(parent);
        }
    }

    //开始播放动作
    public void Begin()
    {
        for (int i = 0; i < fxs.Count; ++i)
        {
            fxs[i].Begin();
        }
    }

    //每一帧有且只有一次被更新
    public void UpdateFrame(int frame, int loopCount, bool end, Transform root)
    {
        for (int i = 0; i < fxs.Count; ++i)
        {
            fxs[i].UpdateFrame(frame, loopCount, end, root);
        }
    }

    //结束播放动作
    public void End()
    {
        for(int i= 0;i<fxs.Count;++i){
            fxs[i].End();
        }
    }

    public void Destroy(bool checkIngoreDestroy = true)
    {
        for (int i = 0; i < fxs.Count; ++i)
        {
            fxs[i].Destroy(checkIngoreDestroy);
        }
    }

    public bool SetRuntimeCreateCallback(string aniFx,System.Action<GameObject,object> onRuntimeCreate,object onRuntimeCreateParam)
    {
        bool has =false;
        for (int i = 0; i < fxs.Count; ++i)
        {
            if(fxs[i].prefab==null)continue;
            if(fxs[i].prefab.name != aniFx)continue;
            has =true;
            fxs[i].SetRuntimeCreate(onRuntimeCreate, onRuntimeCreateParam);
        }
        return has;
    }
    
}

[System.Serializable]
public class AniFx
{
    public enum enCreateType{
        bone,//绑定某个骨骼，对应的transform下挂一个预制体
        matrial,//绑定材质，会在角色的所有SkinMeshRender下挂一个预制体
        //camera,//镜头
        max
    }
    
    public static string[] TypeName = new string[] { "绑定骨骼","材质"/*,"镜头"*/};

    #region Fields
    public GameObject prefab;
    public GameObject prefabFire;
    public GameObject prefabIce;
    public GameObject prefabThunder;
    public GameObject prefabDark;

    public enCreateType type = enCreateType.bone;

    //绑定骨骼相关
    public Transform bone;//要绑定的骨骼，如果为空那么会绑定到主角下面
    public bool follow = true;//如果跟随，那么就是放到子节点，否则就是位置相对于骨骼而已
    public Vector3 offset = Vector3.zero;//相对于主角或者绑定骨骼的位移
    public Vector3 euler = Vector3.zero;//相对于主角或者绑定骨骼的角度    

    public int beginFrame =0;
    public int endFrame = 0;
    public bool destroyIfAniEnd=true;//动作结束后销毁
    public bool loopCreate =false;//循环播放的时候是不是创建
    public System.Action<GameObject,object> onRuntimeCreate = null;
    public object onRuntimeCreateParam = null;
    public bool canHide = true;

    public bool isDrawGizmos = false;

    List<GameObject> fxs = new List<GameObject>();
    //CameraHandle handle;
    AniFxMgr parent;

    #endregion

    #region Properties
    public bool IsDrawGizmos { get{return isDrawGizmos;}set{isDrawGizmos = value;}}
    #endregion

    #region Mono Frame
    public bool CopyFrom(Transform root,AniFx aniFx)
    {
        if (aniFx == null) return true;

        //复制预制体
        prefab = aniFx.prefab;
        prefabFire = aniFx.prefabFire;
        prefabIce = aniFx.prefabIce;
        prefabThunder = aniFx.prefabThunder;
        prefabDark = aniFx.prefabDark;

        //复制骨骼
        if (aniFx.bone!= null&& aniFx.type == enCreateType.bone )
        {
            string path = "";
            bone = FindBone(root,aniFx.bone,ref path);
            if(bone == null)
                return false;
        }
        else
            bone = null;

        //复制值类型的属性
        Util.Copy(aniFx, this, BindingFlags.Public | BindingFlags.Instance);
        return true;
    }

    //初始化
    public void Init(AniFxMgr parent)
    {
        this.parent = parent;
        if (prefab != null)
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(prefab);
        if (prefabFire != null)
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(prefabFire);
        if (prefabIce != null)
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(prefabIce);
        if (prefabThunder != null)
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(prefabThunder);
        if (prefabDark != null)
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(prefabDark);
        
    }

    //开始播放动作
    public void Begin()
    {
        onRuntimeCreate= null;//运行时绑定的，刚开始一定要清空
        onRuntimeCreateParam = null;
    }

    //每一帧有且只有一次被更新
    //frame 当前帧
    //loopCount 当前第几次动作循环
    //trueframe 真实达到第几帧的
    //end 真实是不是结束
    //root 用于创建的时候计算位置用
    public void UpdateFrame(int frame,int loop,bool isEnd,Transform root)
    {
        //检查销毁
        if (endFrame > 0 && beginFrame + endFrame == frame && fxs.Count != 0)
        {
            Destroy();
        }

        if (frame != beginFrame)return;//不是这一帧
        if (prefab == null)return;
        if (loop > 0 && !loopCreate) return;//已经循环过一次就不用创建了
        if (isEnd && (destroyIfAniEnd )) return;//如果是动作结束销毁的，那么就不用创建了
#if !ART_DEBUG
        //调试
        if (onRuntimeCreate == null && canHide&&!QualityMgr.instance.roleAniFx)
            return;
#endif

        //不同元素属性创建不同特效
        GameObject curPrefab = prefab;
        switch (this.parent.RuntimeElement)
        {
            case enAniFxElement.none: curPrefab = prefab; break;
            case enAniFxElement.fire: curPrefab = prefabFire; break;
            case enAniFxElement.ice: curPrefab = prefabIce; break;
            case enAniFxElement.thunder: curPrefab = prefabThunder; break;
            case enAniFxElement.dark: curPrefab = prefabDark; break;
            default: Debuger.LogError("动作特效 未知的元素类型:{0}", this.parent.RuntimeElement); break;
        }
        if (curPrefab == null)
            curPrefab = prefab;

        //1 创建特效
        GameObject go = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately(curPrefab.name,false);
        if (go == null)
        {
            //Debuger.LogError("逻辑错误动作绑定特效加载后为空");//这里只支持预加载，不支持异步加载，没有预加载过报错
            return;
        }

        //2 设置位置和方向
        Transform t = go.transform;
        if (type == enCreateType.bone)//绑定骨骼
        {
            Transform target = GetTarget(root);
            if (follow)
            {
                t.SetParent(target,false);
                //t.localScale = Vector3.one;
                t.localPosition = offset;
                t.localEulerAngles = euler;
            }
            else
            {
                t.position = target.TransformPoint(offset);
                t.rotation = target.rotation * Quaternion.Euler(euler);
            }
        }
        else if (type == enCreateType.matrial)//绑定材质
        {
            Transform target =root.Find("model/body_mesh");
            if (target != null)
            {
                t.SetParent(target, false);
                //t.localScale = Vector3.one;
                t.localPosition = Vector3.zero;
                t.localEulerAngles = Vector3.zero;
            }

            target = root.Find("model/weapon_mesh");
            if (target != null)
            {
                GameObject go2 = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately(prefab.name, false);
                Transform t2 = go2.transform;
                t2.SetParent(target, false);
                t2.localPosition = Vector3.zero;
                t2.localEulerAngles = Vector3.zero;
                go2.SetActive(true);//设好了父节点了，这个时候再SetActive
                fxs.Add(go2);
            }
        }
        else
            Debuger.LogError("未知的类型:{0}", type);
        go.SetActive(true);//设好了父节点了，这个时候再SetActive

        //如果飞出物上没有任何销毁的脚本，那么提示下
        if (!destroyIfAniEnd&&endFrame <= 0 && !FxDestroy.HasDelay(go))
        {
            Debuger.LogError("动作特效上没有绑销毁脚本，动作上也没有指定结束帧.特效名:{0}",go.name);
        }

        //加到列表中，方便管理
        fxs.Add(go);

        

        //运行时创建回调
        if (onRuntimeCreate!=null)
            onRuntimeCreate(go,onRuntimeCreateParam);
    }


    //动作结束
    public void End()
    {
        if ((endFrame > 0||destroyIfAniEnd )&& fxs.Count != 0)
        {
            Destroy();
           
        }

        if (fxs.Count > 0)
            fxs.Clear(); 
    }

#endregion

#region Private Methods
    static bool TryGetPath(Transform refBone, ref string s)
    {
        //检错下
        if (refBone == null || refBone.parent == null)
            return false;

        bool isRoot = refBone.parent.GetComponent<SimpleRole>() != null;
        s = isRoot ? (refBone.name + s) : ("/" + refBone.name + s);

        if (isRoot)
            return true;
        else
            return TryGetPath(refBone.parent, ref s);

    }
    static public Transform FindBone(Transform root, Transform refBone, ref string path)
    {
        path = "";
        if (refBone == null || root == null) return null;

        //计算骨骼路径
        if (!TryGetPath(refBone, ref path))
            return null;

        return root.Find(path);
    }
#endregion

    //销毁全部
    public void Destroy(bool checkIngoreDestroy = true)
    {
        for (int i = 0; i < fxs.Count; ++i)
        {
            if (fxs[i] != null)
                FxDestroy.DoDestroy(fxs[i], checkIngoreDestroy);
        }
        fxs.Clear();
    }

    public Transform GetTarget(Transform root)
    {
        return bone == null ? root : bone;
    }

    public void SetRuntimeCreate(System.Action<GameObject,object> onRuntimeCreate ,object param){
        this.onRuntimeCreate = onRuntimeCreate;
        this.onRuntimeCreateParam =param;
        //如果已经创建出来了，那么执行
        for (int i = 0; i < fxs.Count; ++i)
        {
            if(fxs[i]==null ||!fxs[i].activeSelf)
            {
                Debuger.LogError("监听动作特效的时候特效已经播放完了:{0}",prefab.name);   
                continue;
            }
            if (onRuntimeCreate != null)
            {
                onRuntimeCreate(fxs[i],onRuntimeCreateParam);
            }
        }
    }
}