#region Header
/**
 * 名称: 用于销毁特效，如果是对象池的对象，那么会放回对象池
 
 * 日期：2015.10.9
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FxDestroy : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Art/特效/销毁")]
    static void AddFxDestroy()
    {
        if (UnityEditor.Selection.activeGameObject != null)
        {
            FxDestroy fx = UnityEditor.Selection.activeGameObject.AddComponentIfNoExist<FxDestroy>();
            fx.m_delay=1;
            EditorUtil.SetDirty(fx);
        }
    }

#endif

    #region Fields
    public const float Destroy_Change_Scene = -2;

    public float m_delay =-1f;

    float m_beginTime;
    float m_runTimeDelay=-1;
    float m_runTimeBeginTime = -1;
    bool m_do = false;
    #endregion


    #region Properties
    float RunTimeDelay { get{return m_runTimeDelay;}set{
        m_runTimeDelay = value;
        m_runTimeBeginTime = Util.time;
    }}
    #endregion

    #region Static Methods
    //有没永久的或者运行时的延迟销毁
    public static bool HasDelay(GameObject go)
    {
        FxDestroy fd = go.GetComponent<FxDestroy>();
        if (fd == null) return false;
        return fd.m_delay !=-1 || fd.RunTimeDelay!=-1;
    }
    public static float GetRunTimeDelayIfExist(GameObject go)
    {
        FxDestroy fd = go.GetComponent<FxDestroy>();
        if(fd == null)return -1;
        return fd.RunTimeDelay;
    }
    public static void Add(GameObject go,float delay)
    {
        FxDestroy fd= go.AddComponentIfNoExist<FxDestroy>();
        if (!fd.gameObject.activeSelf)return;
        fd.RunTimeDelay = delay;
    }
    public static void DoDestroy(GameObject go,bool checkIngore =true)
    {
        if (!Application.isPlaying)
        {
            go.SetActive(false);
            return;
        }

        GameObjectPoolObject poolObject = go.GetComponent<GameObjectPoolObject>();
        if (poolObject == null)
        {
            //GameObject.Destroy(this.gameObject);
            go.SetActive(false);
            return;
        }

        if (poolObject.IsInPool)
        {
            go.SetActive(false);
            return;
        }

        //GameObjectPoolObject提供了无视这次销毁操作的接口，用于某些特殊的特效做延迟销毁
        if (checkIngore&&poolObject.m_onIngoreDestroy != null && poolObject.m_onIngoreDestroy())
        {
            return;
        }

#if !ART_DEBUG
        if(Main.instance!=null)
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Put(go);
        else
            go.SetActive(false);
#else 
        go.SetActive(false);
    

#endif
    }

    static HashSet<FxDestroy> s_runings = new HashSet<FxDestroy>();
    public static void Clear()
    {
        if (s_runings.Count == 0)
            return;

        //有时间限制或者切换场景销毁的特效都回收下
        List<FxDestroy> l = new List<FxDestroy>(s_runings);
        foreach (var d in l)
        {
            if (d.m_do || (d.m_delay == -1 && d.m_runTimeDelay == -1) )
                continue;

            DoDestroy(d.gameObject, false);
        }
        s_runings.Clear();
    }
    #endregion


    #region Mono Frame
    void Start()
    {
        
    }

    void OnEnable()
    {

        m_beginTime = Util.time;
        m_do = false;
        s_runings.Add(this);

    }
    void OnDisable()
    {
        s_runings.Remove(this);
        m_runTimeDelay = -1;//清空
        m_do = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_do)
            return;
        if ((m_delay>=0 &&Util.time - m_beginTime >= m_delay)||
            (m_runTimeDelay >= 0 && Util.time - m_runTimeBeginTime >= m_runTimeDelay))
        {
            DoDestroy(this.gameObject);
            m_do = true;
        }
            
    }
    

    #endregion


    #region Private Methods
        
    #endregion
    

    

}
