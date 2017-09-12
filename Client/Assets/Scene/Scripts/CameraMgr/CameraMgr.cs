using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
/*
 * *********************************************************
 * 名称：相机管理器
 
 * 日期：2015.9.7
 * 描述：主要用于相机切换
 * 这里尝试用了两种方法来计算(现在用的是第一种，因为第二种遇到了比较严重的问题，注释掉的是第二种)
 * 1.看的点、距离、相机角度渐变，相机的位置由前三样计算出来，
 *      这种方法在两个离得比较近的盯着类型镜头间切换的时候会有点不正常，达不到盯着不动的效果
 * 2.看的点、相机位置渐变，相机角度由前两样计算出来，
 *      这种方法在两个水平角相反的镜头间切换的时候会越过看的点，而不是绕着看的点
 * *********************************************************
 */

public class CameraMgr : MonoBehaviour {
    #region Fields
    //用于防止抖动
    public const float Fov_Smooth_Min = 0.001f;
    public const float Pos_Smooth_Min_Sq = 0.000001f;
    public const float Angle_Smooth_Min = 0.001f;
    public const float Distance_Smooth_Min = 0.001f;

    public static  CameraMgr instance;


    public GameObject m_prefab;

    //关卡结算界面相机的水平角，也就是相机不穿帮的参考方向
    public float m_horizontalAngle = 0;    
    
    //动态阴影的方向   
    public float m_dynamicShadowAngle =0;

    //看着的点偏移 靠近怪时看向主角与怪物之间用
    public Vector3 m_lookPosOffset = Vector3.zero;
    public Vector3 m_lastLookPosOffset = Vector3.zero;
    public float m_disRate = 1;

    //一些调试用的信息，只读
    public int m_curId=-1;
    public bool m_isDuration = false;//是不是正在渐变中
    public int m_samlpeCounter=0;
    public Transform m_tranEnemyDebug;
    public CameraInfo m_curCameraInfo;

    bool m_cache =false;
    Transform m_follow;
    Camera m_ca;
    Transform m_tran;
    Light m_dynamicRoleLight;
    ShareThing m_share = new ShareThing();//用于多次调用时的使用策略
    

    //相关辅助计算参数
    Vector3 m_lastLookPos = Vector3.zero;
    Vector3 m_newLookPos = Vector3.zero;
    Vector3 m_smoothLookPos = Vector3.zero;
    float m_smoothVelocity = 0;
    Vector3 m_curLookPosVelocity=Vector3.zero;
    //Vector3 m_curPosVelocity = Vector3.zero;
    float m_curFovVelocity = 0;
    float m_lastDistance = 0;
    float m_curDistanceVelocity = 0;
    bool m_isSampleDurationInvalid = true;
    #endregion

    #region Properties
    public bool IsCache { get { return m_cache; } }
    //当前的相机操作者
    public CameraHandle CurHandle { get{return m_share.Get<CameraHandle>();}}

    //是不是正在渐变中
    public bool IsDuration { get { return CurHandle != null && CurHandle.m_isDuation; } }

    public Vector3 HorizontalDir { get{return Vector3.ProjectOnPlane(m_tran.forward, Vector3.up).normalized;}}

    public Vector3 HorizontalRightlDir { get { return Vector3.ProjectOnPlane(m_tran.right, Vector3.up).normalized; } }

    public Camera CurCamera {get{return m_ca;}}

    public Transform Tran { get { return m_tran;} }
    
    public Vector3 LastLookPos { get{return m_lastLookPos;}}

    public Vector3 NewLookPos { get { return m_newLookPos;} }
    public Vector3 SmoothLookPos { get{return m_smoothLookPos;}}
    public bool IsSampleDurationInvalid { get { return m_isSampleDurationInvalid; } }
    public Light DynamicRoleLight { get{return m_dynamicRoleLight;}}
    #endregion

    #region Mono Frame
    void Awake()
    {
        instance = this;
        Cache();

        

//美术测试模式下自动设置跟随者和镜头
#if ART_DEBUG
        //跟随者
        if (m_prefab != null&&m_ca!= null)
        {
            //if (UnityEditor.PrefabUtility.GetPrefabType(m_prefab) == UnityEditor.PrefabType.Prefab)
            if (m_prefab.transform.parent == null && GameObject.Find(m_prefab.name) == null)//如果是预制体，那么创建
                m_prefab = (GameObject)Object.Instantiate(m_prefab);

            Transform t = m_prefab.transform;
            SetFollow(t);
        }
        else
            Debuger.LogError("CameraMgr;相机找不到跟随对象，是否给相机设置跟随对象");

        //设置初始镜头
        GameObject go = GameObject.Find("CameraTriggerManage");
        if (go == null )//没有场景相机触发管理器就用
        {
            if(m_ca!=null)
                SetFollowPos(PosUtil.CaleByCamera(m_ca));
            Set(m_curCameraInfo);
        }
        else
        {
            CameraTriggerMgr mgr =go.GetComponent<CameraTriggerMgr>();
            if (m_ca != null)
                SetFollowPos(mgr.CurGroup.m_bornPos);
            mgr.CurGroup.SetGroupActive(true);
        }
#endif
    }

	// Use this for initialization
	void Start () {

	}

    void OnDestroy(){
        instance = null;
    }
    //Vector3 drawPos = Vector3.zero;
    void Update()
    {
#if UNITY_STANDALONE_WIN ||UNITY_EDITOR
        //修改时间缩放
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Time.timeScale = Time.timeScale*2f;
            if (Mathf.Abs(Time.timeScale - 1) < 0.001f)
                Time.timeScale =1;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (Time.timeScale >= 0.00001)
                Time.timeScale /= 2f;
        }

        if (Input.GetKey(KeyCode.KeypadEnter))
        {
            /*if (!UnityEditor.EditorApplication.isPaused)
                UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Pause");
            UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Step");*/
            StopCoroutine("CoStep");
            StartCoroutine("CoStep");
        }
        if (Input.GetKeyDown(KeyCode.KeypadPeriod))
        {
            //UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Pause");
            if (Time.timeScale==0)
                Time.timeScale =1f;
            else
                Time.timeScale = 0f;
        }
#endif

#if ART_DEBUG
        //寻路测试
        if (m_ca != null &&Input.GetMouseButtonUp(0))
        {
            Vector3 screenPos = Input.mousePosition;
            Ray ray = m_ca.ScreenPointToRay(screenPos);
            RaycastHit hitInfo;

            RaycastHit[] hits = Physics.RaycastAll(ray, 100, 1 << 0);// LayerMask.GetMask("Default")
            Vector3 pos = Vector3.up*10000;
            
            for(int i=0;i<hits.Length;++i){
                Transform t = hits[i].transform;
                
                if(t.name == "[TerrainObstruct]" ||(t.parent!= null &&t.parent.name == "[TerrainObstruct]")||
                (t.parent != null && t.parent.parent != null && t.parent.parent.name == "[TerrainObstruct]"))
                {
                    pos = hits[i].point;
                    break;
                }
            }
            if (pos != Vector3.up*10000)
            {
                //drawPos = pos;
                SimpleRole[] rs = Object.FindObjectsOfType<SimpleRole>();
                foreach (SimpleRole r in rs)
                {
                    r.MovePos(pos);
                }
                //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //go.transform.position = hitInfo.point;

            }
            else
                Debuger.Log("点击的点找不到地面");
            
            
        }

#endif
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(drawPos, 0.1f);
    //}
    IEnumerator CoStep()
    {
        Time.timeScale = 1f;
        yield return 0;
        Time.timeScale = 0f;
    }

	// Update is called once per frame
	void LateUpdate () {
        Sample();
	}

    #endregion

    #region Private Methods
    void Cache()
    {
        if (m_cache)return;

        m_cache = true;
        m_tran = this.transform;

        //角色实时灯
        GameObject go = GameObject.Find("MapScene/[RoleLight]");
        m_dynamicRoleLight  =go==null?null:go.GetComponent<Light>();
        if (m_dynamicRoleLight == null || (m_dynamicRoleLight.cullingMask & LayerMask.GetMask("RoleRender"))==0)
        {
            m_dynamicRoleLight = null;
#if !ART_DEBUG
            Debuger.LogError("找不到角色实时光,请检查场景");
#endif
        }
        
        m_ca = this.GetComponentInChildren<Camera>();
        if (m_ca != null)
        {
            //m_ca.AddComponentIfNoExist<GlowEffect.GlowEffect>();
            m_ca.tag = "MainCamera";
            m_ca.transform.localPosition = Vector3.zero;

#if UNITY_EDITOR
            //加上绘制调试用的脚本
            m_ca.AddComponentIfNoExist<DrawGL>();
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
                sceneView = (SceneView)(SceneView.sceneViews.Count > 0 ? SceneView.sceneViews[0] : null);
            if (sceneView != null)
            {
                Camera c = sceneView.camera;
                if (c != null)
                    c.AddComponentIfNoExist<DrawGL>();
            }
#endif
        }
        else
            Debuger.LogError("初始化时相机为空");
    }

    //重置切换速度
    void ResetVelocity()
    {
        m_smoothVelocity = 0;
        m_curLookPosVelocity = Vector3.zero;
        //m_curPosVelocity = Vector3.zero;
        m_curFovVelocity = 0;
    }

    

    List<LinkedListNode<IShareThingHandle>> temRemoves = new List<LinkedListNode<IShareThingHandle>>();//用于计算删除的临时变量
    void Sample()
    {
        CameraHandle handle = m_share.Get<CameraHandle>();
        if (handle == null ||  m_ca == null || !m_cache || Time.unscaledDeltaTime == 0)
            return;

        //当前镜头需要跟随者才能计算的话，返回
        if (m_follow == null && handle.NeedFollow)
            return;

        CameraInfo info = handle.m_info;
        //不受战斗镜头影响的要重置下参数
        if (!info.isBattleDisturb)
        {
            m_lookPosOffset = Vector3.zero;
            m_lastLookPosOffset = Vector3.zero;
            m_disRate = 1;
        }

        //相机位置、方向、fov计算
        //直接切的情况
        if (handle.m_isDuation && handle.IsDurationInvalid)
        {
            m_isSampleDurationInvalid = true;
            ResetVelocity();
            m_smoothLookPos = m_newLookPos = m_lastLookPos = (handle.LookPos + m_lookPosOffset);
            m_lastDistance = handle.Distance * m_disRate;
            //m_lastLookPosOffset = m_lookPosOffset;
            m_tran.eulerAngles = handle.Euler;
            m_tran.position = m_lastLookPos - m_tran.forward * m_lastDistance;
//             m_tran.position = handle.Pos;
//             m_tran.forward = m_newLookPos - m_tran.position;
            m_ca.fieldOfView = handle.Fov;
        }
        //渐变的情况
        else
        {
            m_isSampleDurationInvalid = false;
            float delta = Time.unscaledDeltaTime;
            float factor =handle.m_isDuation? handle.Factor:1;

            //计算看的点
            m_newLookPos = handle.LookPos;

            //计算渐变速度
            float v1 = Mathf.Lerp(handle.m_duration, 0, info.animationCurve.Evaluate(factor));
            float v2 = 0;
            
            if (info.useDisSmooth)//渐变过程和距离渐变都有的情况
            {
                Vector3 link = m_newLookPos + m_lookPosOffset - m_lastLookPos;
                float dis = link.magnitude;
                v2= MathUtil.easeInExpo(0, info.disSmooth, Mathf.Lerp(1, 0, dis / info.disSmoothLimit)); //这里用exp函数可以让过度更平滑
            }
            m_smoothVelocity = Mathf.Max(v1, v2);


            if (Mathf.Abs(m_smoothVelocity) > Distance_Smooth_Min )//非常小的时候会有抖动，要用直接设置的方式
            {
                m_smoothLookPos = Vector3.SmoothDamp(m_lastLookPos, m_newLookPos+ m_lookPosOffset, ref m_curLookPosVelocity, m_smoothVelocity, float.MaxValue, delta);
                m_lastLookPosOffset = m_lookPosOffset;
                //与主角的距离渐变
                m_lastDistance = Mathf.SmoothDamp(m_lastDistance, handle.Distance * m_disRate, ref m_curDistanceVelocity, v1, float.MaxValue, delta);
                //角度渐变
                m_tran.rotation = Quaternion.Slerp(m_tran.rotation, handle.Rotate, (delta * factor) / v1);

            //                 //看着的点渐变
            //                 m_smoothLookPos = Vector3.SmoothDamp(m_lastLookPos, m_newLookPos, ref m_curLookPosVelocity, m_smoothVelocity, float.MaxValue, delta);
            //                 
            //                 //相机的点渐变
            //                 m_tran.position = Vector3.SmoothDamp(m_tran.position, handle.Pos, ref m_curPosVelocity, m_smoothVelocity, float.MaxValue, delta);
            //                 
            //                 //由于上面两个点都渐变了，方向不用渐变
            //                 m_tran.forward = m_smoothLookPos - m_tran.position;
            }
                else//直接设置
                {
                m_lastLookPosOffset = Vector3.Slerp(m_lastLookPosOffset, m_lookPosOffset, delta);
                m_smoothLookPos = m_newLookPos + m_lastLookPosOffset;
                m_lastDistance = handle.Distance * m_disRate;
                m_tran.rotation = handle.Rotate;
                //                 m_smoothLookPos = m_newLookPos;
                //                 m_tran.position  = handle.Pos;
                //                 m_tran.forward = m_smoothLookPos - m_tran.position;
            }

            //位置渐变,注意这里不是当前位置渐变，是看着的点渐变、距离渐变、方向渐变，然后推导出当前的位置，这样看着才是渐变的。
            m_lastLookPos = m_smoothLookPos;
            
            m_tran.position = m_smoothLookPos - m_tran.forward * m_lastDistance;
            

            //视野渐变
            float newFov = handle.Fov;
            if (Mathf.Abs(newFov - m_ca.fieldOfView) > Fov_Smooth_Min)
                m_ca.fieldOfView = Mathf.SmoothDampAngle(m_ca.fieldOfView, newFov, ref m_curFovVelocity, m_smoothVelocity, float.MaxValue, delta);

            ++m_samlpeCounter;
        }
        

        //3 计算渐变结束(要修改优先级),计算需要删除的
        LinkedListNode<IShareThingHandle> node = m_share.m_handles.First;
        CameraHandle tempHandle;
        bool needSort = false;
        do
        {
            tempHandle = node.Value as CameraHandle;
            //将置顶优先级降为不置顶优先级
            if (tempHandle.m_isDuation)
            {
                if (tempHandle.m_info.isDurationInvalid || tempHandle.m_firstDurationInvalid || //立即切
                    (Time.unscaledTime - tempHandle.m_beginTime >= tempHandle.m_duration|| handle!= tempHandle))//时间到或者不是置顶的handle
                {
                    needSort = true;
                    tempHandle.m_isDuation = false;
                    tempHandle.m_priority = tempHandle.m_info.priority;

                    if (tempHandle == handle)
                        CameraMgr.instance.m_isDuration = false;
                }
            }

            //销毁片段
            if ((tempHandle == handle && !handle.m_isDuation && info.isOverAfterDuration) ||//不永久的时间到了
                (tempHandle != handle && tempHandle.m_info.durationType == CameraInfo.enDurationType.overWhenOverlay) ||//不置顶就删除的
                (tempHandle.m_info.duration != -1 && tempHandle.CurTime >= tempHandle.m_info.duration)//时间到了
            )
                temRemoves.Add(node);
            node = node.Next;
        } while (node != null);
        if (needSort)
            m_share.Sort();
        if (temRemoves.Count != 0)
        {
            m_share.Remove(temRemoves);
            temRemoves.Clear();
        }

        
    }
    #endregion

    #region 跟随者
    public void SetFollow(Transform follow,bool reset=false)
    {
        if (follow == null)
        {
            Debuger.LogError("不能设一个空的跟随者进来");
            return;
        }

        m_follow = follow;

        //重置下镜头和渐变速度
        if(reset&& CurHandle!= null)
        {
            CurHandle.Reset();
            ResetVelocity();
        }
    }

    public Transform GetFollow()
    {
        return m_follow;
    }

    public void SetFollowPos(Vector3 pos)
    {
        if (m_follow == null)
        {
            Debuger.LogError("不能设跟随者为空设置无效");
            return;
        }
        m_follow.position = pos;
    }

    public Vector3 GetFollowPos()
    {
        return m_follow != null ? m_follow.position : Vector3.zero;
    }
    #endregion

    //直接切
    public CameraHandle Set(CameraInfo info, int firstPriority=-1)
    {
        Cache();   
        CameraHandle handle = Add(info, true);
        if (handle.IsHandling)
        {
            m_lastLookPos = handle.LookPos + m_lookPosOffset;//要先设置下，有些类型是相对的，需要一个lookPos
            m_lastLookPosOffset = Vector3.zero;
            m_lookPosOffset = Vector3.zero;
            Sample();//马上sample下,不然这一帧可能没有sample过
        }
        return handle;  
    }

    public CameraHandle Still(Transform t, float moveTime, float stopTime, float fov, float overDuration = -1, bool isBattleDisturb = false)
    {
        return Still(t.position, t.forward, Vector3.zero, moveTime, stopTime, fov, 180, -1,  -1, overDuration, isBattleDisturb);
    }

    public CameraHandle Still(Vector3 refPos, Vector3 refDir, Vector3 offset, float moveTime, float stayTime,
                float fov = -1, float horizontalAngle = -1, float verticalAngle = -1, float distance = -1, float overDuration = -1, bool isBattleDisturb = false)
    {
        return    Still(refPos, refDir, offset, moveTime, stayTime,fov , horizontalAngle , verticalAngle , distance , overDuration ,0,0,0,0,Vector3.zero,null, isBattleDisturb);    
    } 

    //推镜到一个静态的位置和方向,这里offset和horizontalAngle都是相对于refDir的
    public CameraHandle Still(Vector3 refPos, Vector3 refDir, Vector3 offset, float moveTime, float stayTime,
                float fov, float horizontalAngle , float verticalAngle , float distance , float overDuration , 
                float blur, float blurDuration ,float beginSmooth,float endSmooth, Vector3 blurOffset , Transform blurTarget, bool isBattleDisturb = false)
    {
        refDir.y = 0;
        Quaternion q = Quaternion.LookRotation(refDir);

        CameraInfo curInfo =CameraMgr.instance.CurHandle.m_info;
        CameraInfo cameraInfo = new CameraInfo();
        cameraInfo.lookType = CameraInfo.enLookType.still;
        cameraInfo.refPos = refPos + q * offset;
        cameraInfo.duration = moveTime + stayTime;
        cameraInfo.horizontalAngle = horizontalAngle == -1 ? curInfo.horizontalAngle : q.eulerAngles.y+horizontalAngle;
        cameraInfo.distance = distance == -1 ? curInfo.distance : distance;
        cameraInfo.verticalAngle = verticalAngle == -1 ? curInfo.verticalAngle : verticalAngle;
        cameraInfo.fov = fov == -1 ? curInfo.fov: fov;
        cameraInfo.durationSmooth = moveTime ;
        cameraInfo.isBattleDisturb = isBattleDisturb;

        cameraInfo.blur = blur;
        cameraInfo.blurDuration = blurDuration;
        cameraInfo.blurTarget = blurTarget;
        cameraInfo.blurOffset = blurOffset;
        cameraInfo.blurBeginSmooth= beginSmooth;
        cameraInfo.blurEndSmooth= endSmooth;

        if (overDuration!=-1)
        {
            cameraInfo.useOverDuration = true;
            cameraInfo.overDuationSmooth = overDuration;
        }

        CameraHandle handle = Add(cameraInfo);
        return handle;
    }

    //复制当前镜头的信息，然后对fov等进行缩放
    public CameraHandle ScaleFov(float moveTime, float stayTime, float overDuration, float fovRate, int priority,
                float blur, float blurDuration, float beginSmooth, float endSmooth, Vector3 blurOffset, Transform blurTarget, bool isBattleDisturb = false)
    {
        CameraInfo curInfo = CameraMgr.instance.CurHandle.m_info;
        if (curInfo.lookType == CameraInfo.enLookType.followBehind)//这种类型的暂时不能计算出推镜
            return null;

        CameraInfo cameraInfo = new CameraInfo(curInfo);
        cameraInfo.duration = moveTime + stayTime;
        cameraInfo.durationSmooth = moveTime;
        cameraInfo.fov = cameraInfo.fov * fovRate;
        cameraInfo.priority = priority;
        cameraInfo.durationPriority = priority;
        cameraInfo.isBattleDisturb = isBattleDisturb;

        cameraInfo.blur = blur;
        cameraInfo.blurDuration = blurDuration;
        cameraInfo.blurTarget = blurTarget;
        cameraInfo.blurOffset = blurOffset;
        cameraInfo.blurBeginSmooth = beginSmooth;
        cameraInfo.blurEndSmooth = endSmooth;

        if (overDuration != -1)
        {
            cameraInfo.useOverDuration = true;
            cameraInfo.overDuationSmooth = overDuration;
        }
       
        CameraHandle handle = Add(cameraInfo);
        return handle;
    }
    
    public void PlayRadialBlur(float strength,Transform t,float duration,Vector3 offset,float beginSmooth,float endSmooth)
    {
        GameObject go = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately("fx_camera_blur_internal",false);
        CameraFx cameraFx =go.GetComponent<CameraFx>();
        
        
        CameraAni cameraAni =cameraFx.m_anis[0];
        cameraAni.m_endFloat = strength;
        cameraAni.m_beginDuration = beginSmooth;
        cameraAni.m_endDuration = endSmooth;

        RadialBlur blur = go.GetComponent<RadialBlur>();
        blur.target = t;
        blur.offset = offset;
        
        go.SetActive(true);
        FxDestroy.Add(go, duration);
    }

    public CameraHandle StillLook(Transform follow,float moveTime,float overDuration = -1)
    {
        CameraInfo cameraInfo = new CameraInfo();
        cameraInfo.lookType = CameraInfo.enLookType.stillLook;
        cameraInfo.durationSmooth = moveTime;
        cameraInfo.verticalAngle = m_tran.eulerAngles.x;
        cameraInfo.horizontalAngle = m_tran.eulerAngles.y;
        cameraInfo.refPos = SmoothLookPos - cameraInfo.offset;
        cameraInfo.distance = Vector3.Distance(m_tran.position, SmoothLookPos);
        if (overDuration != -1)
        {
            cameraInfo.useOverDuration = true;
            cameraInfo.overDuationSmooth = overDuration;
        }
        CameraHandle handle = Add(cameraInfo);
        handle.SetFollow(follow);
        return handle;
    }

    

    //如果durationInvalid=true则直接切
    //firstPriority !=-1的话则第一次用这个的优先级，否则用cameraInfo.durationPriority
    public CameraHandle Add(CameraInfo cameraInfo, bool firstDurationInvalid = false, int firstPriority = -1)
    {
        if (!m_cache)
            return null;

        //如果是永久镜头，那么不允许叠加多个，这里给之前那个修改下优先级返回就可以了
        LinkedListNode<IShareThingHandle> node = m_share.m_handles.First;
        CameraHandle tempHandle;
        while (node != null)
        {
            tempHandle = node.Value as CameraHandle;
            if (tempHandle.m_info == cameraInfo && !tempHandle.m_info.isOverAfterDuration)
            {
                m_share.Change(tempHandle, firstPriority == -1 ? tempHandle.m_info.durationPriority : firstPriority);
                //这里如果已经置顶了，那么就不会立即切了，重置下
                if (firstDurationInvalid || tempHandle.m_info.isDurationInvalid)
                {
                    tempHandle.Reset(firstDurationInvalid);
                }

                return tempHandle;
            }
            node = node.Next;
        }

        //创建一个新的镜头处理器
        CameraHandle handle = new CameraHandle(cameraInfo, firstDurationInvalid);
        m_share.Add(handle, firstPriority == -1 ? cameraInfo.durationPriority : firstPriority);
#if !ART_DEBUG
        if (cameraInfo.lookType == CameraInfo.enLookType.followBehind)
        {
            Role t = RoleMgr.instance.GetRoleByRoleId(cameraInfo.targetId);
            //if (t == null)
            //    Debug.LogError("创建盯住目标点相机时没有找到目标 目标id：" + cameraInfo.targetId);
            //else
            if (t != null)
            {
                Transform tran = string.IsNullOrEmpty(cameraInfo.bone) ? t.transform: t.transform.Find(cameraInfo.bone);
                if (tran == null)
                    Debug.LogError("没有找到相应骨骼点 roleid:"+cameraInfo.targetId+" 骨骼路径："+cameraInfo.bone);
                handle.SetTarget(tran);
            }
            
        }
#endif
        return handle;
    }

    public void Clear()
    {
        m_share.Clear();
    }

  
}
