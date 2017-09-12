using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public  class MaterialFx : MonoBehaviour {
    public enum enType
    {
        add,//叠加一个新的材质
        replace,//替换老材质
        modify,//原有材质上修改
    }
    public static string[] TypeName = new string[]{"叠加","替换","修改"};

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Art/特效/改材质")]
    static void AddMaterialFx()
    {
        if (UnityEditor.Selection.activeGameObject != null)
        {
            MaterialFx m = UnityEditor.Selection.activeGameObject.AddComponentIfNoExist<MaterialFx>();
            EditorUtil.SetDirty(m);
        }
    }
    [UnityEditor.MenuItem("Art/特效/ScrollingUV")]
    static void AddScrollingUV()
    {
        if (UnityEditor.Selection.activeGameObject != null)
        {
            MaterialFx m = UnityEditor.Selection.activeGameObject.AddComponentIfNoExist<MaterialFx>();
            m.m_type = enType.modify;
            MaterialAni ma = new MaterialAni();
            ma.m_type = MaterialAni.enType.ScrollingUV;
            m.m_anis.Add(ma);
            EditorUtil.SetDirty(m);
        }
    }
#endif

    #region Fields
    public enType m_type = enType.add;
    public int m_matIndex = 0;//要替换或者修改的材质的索引
    public Material m_mat;//要叠加或者替换的材质    
    public bool m_useOldTex=false;//使用老材质的贴图
    public float m_duration = -1;//总时间时间
    //public bool m_playIfEnable = true;//enable的时候运行
    //public bool m_removeIfOverlay = false;//当被重叠的的时候移除
    //public int m_priority = 0;//优先级
    public List<MaterialAni> m_anis = new List<MaterialAni>();
    

    bool m_isPlaying = false;
    float m_beginTime = 0;
    bool m_needDestroy = false;
    float m_destroyTime = -1;
    MaterialMgr.MaterialHandle m_handle = null;
    #endregion

    #region Properties
    public bool IsPlaying    {get { return m_isPlaying;}}
    public bool IsNeedDestroy    {
        get
        {
            if (!m_isPlaying) return true;
            if (m_duration < 0) return false;
            return (Time.time - m_beginTime) > m_duration;
        }
    }
   
    #endregion

    #region Mono Frame

    // Update is called once per frame
    void Update()
    {
        if(!m_isPlaying )return;

        //判断下进入结束状态
        if (!m_needDestroy&&m_duration > 0 && (Time.time - m_beginTime) > m_duration)
        {
            m_needDestroy =true;

            //并不是销毁时间到就马上销毁，要延迟到所有子ani的结束渐变完成
            float maxEndDuration = 0;
            foreach (MaterialAni a in m_anis)
            {
                if (a.EndDuration > maxEndDuration)
                {
                    maxEndDuration = a.EndDuration;
                    a.OnEnd();
                }
                    
            }
            m_destroyTime = m_beginTime +maxEndDuration+m_duration;
            
        }

        //判断下进入结束状态
        if (m_needDestroy && m_destroyTime >0 &&Time.time >= m_destroyTime)
        {
            Stop();
        }
    }

    void OnEnable()
    {
        //判断IsPreloading是因为对象池预加载的时候不需要play
        if (!GameObjectPool.IsPreloading() && !m_isPlaying)//&& m_playIfEnable
        {
            Play();
        }
    }
    public bool m_isApplicationQuit = false;
    void OnApplicationQuit()
    {
        m_isApplicationQuit = true;
    }

    void OnDisable()
    {
        if (m_isApplicationQuit)
            return;
        Stop();
    }

   
    #endregion
	

    #region Frame
    //由材质管理器调用，其他类不要调用
    public void UpdateMaterial()
    {
        foreach (MaterialAni a in m_anis)
        {
            a.OnUpdateMaterial(m_handle.mat,Time.time - m_beginTime);
        }
    }

    //由材质管理器调用，其他类不要调用
    public void OnStop()
    {
        
    }
   
    
    #endregion

    #region Private Methods
    Renderer GetRender()
    {
        Renderer r = this.GetComponent<Renderer>();
        if(r!= null)
            return r;
        if (this.transform.parent != null)
            r = this.transform.parent.GetComponent<Renderer>();
        else
            return null;

        if (r == null)
            Debuger.Log("材质特效找不到对象，必须放在Render的游戏对象上或者其子节点，{0}", this.name);

        return r;
    }
    #endregion
 
    public void Play()
    {
        if(m_isPlaying)
        {
            Debuger.Log("还没结束就重新播放了");
            Stop();//先结束老的
        } 
           
        Renderer r  = GetRender();
        if (r == null)
        {
            return;
        }
            

        MaterialMgr mm = r.AddComponentIfNoExist<MaterialMgr>();
        m_handle = mm.Add(this);
        if (m_handle == null)
        {
            return;
        }
        m_beginTime = Time.time;
        m_isPlaying = true;
        m_needDestroy = false;
        m_destroyTime =-1;
        foreach(MaterialAni a in m_anis ){
            a.OnBegin(m_handle.mat);
        }
    }

    public void Stop()
    {
        if (this == null) return;//可能已经被销毁
        if (!m_isPlaying)//防止死锁
            return;
        //1 必须先把这个值设置正确，不然可能造成死锁
        m_isPlaying = false;
        m_destroyTime =-1;

        //Remove和stop可能会互相调用,内部已经防止死锁，这里不用判断
        m_handle.mgr.Remove(m_handle);
        
        m_handle = null;
        foreach (MaterialAni a in m_anis)
        {
            a.OnStop();
        }

    }
    


   
}
