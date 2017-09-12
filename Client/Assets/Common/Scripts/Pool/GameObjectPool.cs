#region Header
/**
 * 名称：游戏对象池
 
 * 日期：2015.9.21
 * 描述：
 *      这里继承自SingletonMonoBehaviour主要为了观察还对象池的数量变化
 *      注意现在检查是不是放在对象池中的机制就是看一个游戏对象是不是隐藏的
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class GameObjectPool:MonoBehaviour,IPool
{
    public class PoolRequests
    {
        string m_path;
        ResourceRequest m_resRequest;
        bool m_isInPool;//是不是对象池里已经加载好了
        List<PoolRequest> m_requesters = new List<PoolRequest>();//请求方式1的请求者
        List<object[]> m_onGets= new List<object[]>();//请求方式2的请求者
        GameObjectPool m_pool;

        public GameObjectPool Pool { get{return m_pool;}}
        public ResourceRequest Request { get{return m_resRequest;}}

        public List<PoolRequest> PoolRequesters { get{return m_requesters;}}
        public List<object[]> OnGets { get{return m_onGets;}}
        public string Path { get { return m_path; } }
        public bool IsDone { get { return m_isInPool; } }
        public float Progress { get { return m_isInPool ? 1f : m_resRequest.progress*0.999f; } }

        public PoolRequests(GameObjectPool pool,string path, ResourceRequest resRequest)
        {
            m_path = path;
            m_resRequest = resRequest;
            m_isInPool =false;
            m_pool = pool;
        }

        public PoolRequest Add()
        {
            PoolRequest r = new PoolRequest(m_path,this);
            m_requesters.Add(r);
            return r;
        }

        public void Add( Action<GameObject, object> onGet,object param)
        {
            m_onGets.Add(new object[] { onGet, param });
        }

        public void Done(){
            m_isInPool = true;
        }
    }

    public class PoolRequest
    {
        string m_path;
        PoolRequests m_parent;
        GameObject m_asset;

        public string Path { get { return m_path; } }
        public bool IsDone { get { return m_parent==null?true:m_parent.IsDone; } }

        public float Progress { get { return m_parent == null?1f:m_parent.Progress; } }

        public GameObject Asset
        {
            get {  
                if(m_asset==null){
                    m_asset = m_parent.Pool.GetImmediately(m_path);
                }
                return m_asset;
            }
            set{
                if (m_asset != null)
                {
                    Debuger.LogError("对象池重复设置{0}",Path);
                }
                m_asset = value;
            }
        }

        public PoolRequest(string path,PoolRequests p) { 
            m_parent =p;
            m_path = path;
        }
        
    }
    public enum enPool//分下目录
    {
        Other,
        Fx,
        Role,
        max
    }


    #region Fields
    const string Postfix = "(Pool)";
    const int Postfix_Count = 6;

    static GameObjectPool[] s_pools = null;
    static bool s_isPreloading= false;
    static bool s_checkPreLoading = false;
    static bool s_isPuting = false;
    int m_counter = 0;

    public string m_debugName="";
    enPool m_poolType;
    Dictionary<string,PoolRequests> m_requests = new Dictionary<string,PoolRequests>();//正在加载中的资源
    List<string> m_removeRequests = new List<string>();//临时变量，用于参数加载中的资源
    List<GameObjectPoolObject> m_putGameObjects = new List<GameObjectPoolObject>();//临时变量,用于延迟设置父节点的列表
    Dictionary<string, LinkedList<GameObjectPoolObject>> m_pools = new Dictionary<string, LinkedList<GameObjectPoolObject>>();//对象池
    #endregion


    #region Properties
    //正在加载中的资源的数量
    public int RequestCount{get{return m_requests.Count;}}

    public bool IsDone { get{return m_requests.Count ==0;}}

    //名字
    public string Name { get { return this.name; } }

    //在池中的对象
    public int Count { get {
            int c = 0;
            foreach(var l in m_pools.Values)
            {
                c += l.Count;
            }
            return c;
        } }

    
    //总数量
    public int TotalCount { get { return m_counter; } }

    public static bool CheckPreLoading { get { return s_checkPreLoading; } set { s_checkPreLoading = value; } }
    #endregion

    #region Static Methods
    static void Cache()
    {
        if (s_pools != null &&s_pools.Length>=0)
            return;
        s_pools = new GameObjectPool[(int)enPool.max];
        for(int i =0 ;i<(int)enPool.max;++i){
            GameObject go = new GameObject(((enPool)i).ToString() + Postfix, typeof(GameObjectPool));
            s_pools[i] = go.GetComponent<GameObjectPool>();
            s_pools[i].m_poolType = (enPool)i;
            DonDestroyRoot.AddChild(go);
            PoolMgr.instance.AddPool(s_pools[i]);
        }
    }
    public static GameObjectPool GetPool(enPool type){Cache();return s_pools[(int)type];}


    //判断一个游戏对象是不是预加载中
    public static bool IsPreloading() { Cache(); return s_isPreloading; }
    #endregion

    #region Mono Frame
    void LateUpdate()
    {
        CheckPut();
        CheckRequest();
    }
    #endregion
   


    #region Private Methods
    //检查需要延迟回收的对象，见Put()函数
    void CheckPut()
    {
        if (m_putGameObjects.Count == 0)return;

        GameObjectPoolObject poolObject ;
        for (int i = 0; i < m_putGameObjects.Count; ++i)
        {
            poolObject = m_putGameObjects[i];
            if (poolObject.IsInPool)
            {
                Transform t = poolObject.transform;
                Vector3 pos = t.position;//位置要设置回去
                t.SetParent(this.transform, false);
                t.position = pos;
                t.localScale = Vector3.one;
            }
        }
        m_putGameObjects.Clear();
    }
    void CheckRequest()
    {
        if (m_requests.Count == 0)return;
       
        ResourceRequest rr;
        foreach (PoolRequests request in m_requests.Values)
        {
            rr =request.Request;
            if (rr == null)
            {
                //从加载列表中删除
                m_removeRequests.Add(request.Path);
                Debuger.LogError("对象池加载资源失败:{0}", request.Path);
                continue;
            }

            if (rr.isDone)
            {
                if (request.IsDone)
                {
                    m_removeRequests.Add(request.Path);
                    continue;
                }
                request.Done();

                //加到对象池
                if (rr.asset != null)
                {
                    PreLoad((GameObject)rr.asset, false);
                    LinkedList<GameObjectPoolObject> l = m_pools.Get(request.Path);
                    //回调
                    foreach (object[] pp in request.OnGets)
                    {
                        ((Action<GameObject, object>)pp[0])(Get(l).gameObject, pp[1]);
                    }
                }
                else
                {
                    Debuger.LogError("对象池加载资源失败:{0}", request.Path);
                }

                //从加载列表中删除
                m_removeRequests.Add(request.Path);
            }
        }


        if (m_removeRequests.Count != 0)
        {
            for (int i = 0; i < m_removeRequests.Count; ++i)
                m_requests.Remove(m_removeRequests[i]);
            m_removeRequests.Clear();
        }
    }

    

    void Put(LinkedList<GameObjectPoolObject> l, GameObjectPoolObject poolObject)
    {
        bool isPuting = s_isPuting;
        if (!s_isPuting)
        {
            s_isPuting = true;
            Transform t = poolObject.transform;
            Vector3 pos = t.position;//位置要设置回去
            t.SetParent(this.transform, false);
            t.position = pos;
            t.localScale = Vector3.one;
        }
        else
            m_putGameObjects.Add(poolObject);

        try  
        {
           
            poolObject.OnPut();//要在SetActive(false)之前，因为有些脚本OnDisable的时候也会把自己放回对象池，这个时候会造成逻辑错误
            //正在隐藏一个对象的时候可能会由OnDisable()导致绑在身上的特效的对象池回收，
            //这时候如果修改父节点为对象池会报错，先放到对象池的列表里，等下一帧再把父节点设置为对象池
            poolObject.gameObject.SetActive(false);
        }
        finally
        {
#if ART_DEBUG
            l.AddFirst(poolObject);
#else
            l.AddLast(poolObject);
#endif
            if (!isPuting && s_isPuting)
                s_isPuting = false;
        }
    }
    GameObjectPoolObject Get(LinkedList<GameObjectPoolObject> l, bool active = true)
    {
        //有可能是加载不到的资源
        if (l.Count == 0) return null;

        //要保证取出后预制体不为空,如果只剩一个那么加载多一个
        if (l.Count == 1)
            PreLoad(l.Last.Value.gameObject,false);

        GameObjectPoolObject poolObj =l.First.Value;
        GameObject go = poolObj.gameObject;
        l.RemoveFirst();
        if (active)
            go.SetActive(true);//有时候要挂到别的父节点下，这种情况下应该挂到父节点下之后才显示

        poolObj.OnGet();
#if UNITY_EDITOR 
        if (!string.IsNullOrEmpty(m_debugName) && go.name == m_debugName)
            Debuger.Log("get了对象池一个:{0}",m_debugName);
#endif


        
        return poolObj;
    }

    #endregion
    public void PreLoad(string name, bool checkPreLoading = true)
    {
        LinkedList<GameObjectPoolObject> l = m_pools.Get(name);
        //已经加载过的
        if (l != null)
            return ;

        //正在加载中的
        PoolRequests poolReqs = m_requests.Get(name);
        if (poolReqs != null)
            return;

        if (string.IsNullOrEmpty(name))
        {
            Debuger.LogError("预加载的时候传进来空的资源名");
            return;
        }
        if (checkPreLoading&&!s_checkPreLoading)
            Debuger.LogError("PutPrefab.没有在预加载期间加载资源:{0}", name);
        
        //没有加载过的
        ResourceRequest resReq =Resources.LoadAsync<GameObject>(name);
        poolReqs = new PoolRequests(this, name, resReq);
        m_requests[name] = poolReqs;

        //预加载对应的音效
#if !ART_DEBUG
        if (m_poolType == enPool.Fx)
        {
            FxSoundCfg.PreLoad(name);
        }
#endif

    }
    //复制游戏对象到对象池，注意对象池是靠对象名字识别的，如果预制体名和对象名不同可能会出现异常
    //addIfNo = true，如果有了就不要预加载了
    public void PreLoad(GameObject go,bool addIfNo = true)
    {
        if (go == null)
        {
            Debuger.LogError("预制体为空？不能放入对象池");
            return;
        }
        //删除后缀
        string name = go.name;
        int idx = name.IndexOf(" (");
        if (idx != -1)
            name = name.Substring(0, idx - 1);
        idx = name.IndexOf(" (");
        if (idx != -1)
            name = name.Substring(0, idx - 1);

        //已经有了就不用预加载了
        LinkedList<GameObjectPoolObject> l = m_pools.GetNewIfNo(name);
        if (l.Count > 0 && addIfNo)
            return;

        //增加到对象池
        s_isPreloading = true;
        GameObject goNew = GameObject.Instantiate(go);
        s_isPreloading = false;
        goNew.name = name;
        GameObjectPoolObject poolObj = goNew.AddComponentIfNoExist<GameObjectPoolObject>();
        poolObj.OnInit();
        Put(l, poolObj);
        ++m_counter;//每Instantiate一个计数下，用于检查泄露

        //预加载对应的音效
#if !ART_DEBUG
        if (m_poolType  == enPool.Fx)
        {
            FxSoundCfg.PreLoad(name);
        }
#endif
    }

    //传统的异步加载方式，可以让StartCoroutine很方便使用
    public PoolRequest Get(string name)
    {
        LinkedList<GameObjectPoolObject> l = m_pools.Get(name);
        //已经加载过的
        if (l != null)
        {
            PoolRequest r = new PoolRequest(name,null) ;
            r.Asset = Get(l).gameObject;
            return r;
        }

        //正在加载中的
        PoolRequests poolReqs = m_requests.Get(name);
        if (poolReqs!= null)
        {
            return poolReqs.Add();
        }
        if (!s_checkPreLoading)
            Debuger.LogError("Get.没有在预加载期间加载资源 :{0}", name);
        //没有加载过的
        ResourceRequest resReq = Resources.LoadAsync<GameObject>(name);
        poolReqs = new PoolRequests(this,name,resReq);
        m_requests[name] = poolReqs;
        return poolReqs.Add();
    }

    //回调形式的异步加载方式
    public void Get(string name,object param,Action<GameObject,object> onGet,bool checkPreLoading=true)
    {
        LinkedList<GameObjectPoolObject> l = m_pools.Get(name);
        //已经加载过的
        if (l != null)
        {
            onGet(Get(l).gameObject,param);
            return;
        }

        //正在加载中的
        PoolRequests poolReqs = m_requests.Get(name);
        if (poolReqs != null)
        {
            poolReqs.Add(onGet, param);
            return;
        }
        if (checkPreLoading &&!s_checkPreLoading)
            Debuger.LogError("Get2.没有在预加载期间加载资源 :{0}", name);
        //没有加载过的
        ResourceRequest resReq = Resources.LoadAsync<GameObject>(name);
        poolReqs = new PoolRequests(this,name, resReq);
        m_requests[name] = poolReqs;
        poolReqs.Add(onGet, param);
        
    }

    //对于确定有预加载的对象可以马上取
    public GameObject GetImmediately(string name,bool active = true)
    {
        LinkedList<GameObjectPoolObject> l = m_pools.Get(name);
        if (l == null)
        {
            Debuger.LogError("没有预加载、加载不了资源或者还没有加载完就获取了{0}",name);
            GameObject prefab =Resources.Load<GameObject>(name);
            if (prefab == null)
                return null;
            PreLoad(prefab);//这里直接同步加载出来
            l = m_pools.Get(name);
            
        }
        return Get(l,active).gameObject;
    }

    public void Put(GameObject go)
    {
        if (this == null)
            return;//可能已经被销毁
        if (go==null)
        {
            Debuger.LogError("往对象池里放了空对象");
            return;
        }

        LinkedList<GameObjectPoolObject> l = m_pools.Get(go.name);
        if (l == null)
        {
            Debuger.LogError("放入的时候对象池找不到列表：{0}", go.name);
            return;
        }

        GameObjectPoolObject poolObj = go.GetComponent<GameObjectPoolObject>();
        if (poolObj == null)
        {
            Debuger.LogError("找不到GameObjectPoolObject。不是对象池中的对象：{0}", name);
            return;
        }
        if (poolObj.IsInPool)
        {
            Debuger.LogError("已经对象池里，重复放入了：{0}", name);
            return;
        }

        Put(l, poolObj);
    }

    //清空到剩下一个
    public void Clear()
    {
        m_counter = 0;
        foreach (var l in m_pools.Values)
        {
            while (l.Count > 1)
            {
                GameObjectPoolObject go = l.Last.Value;
                l.RemoveLast();
                GameObject.Destroy(go.gameObject);
            }
            ++m_counter;
        }
    }
}

