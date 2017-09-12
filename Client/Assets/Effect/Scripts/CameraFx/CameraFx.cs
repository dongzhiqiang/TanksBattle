#region Header
/**
 * 名称：相机后期特效
 
 * 日期：2015.10.23
 * 描述：
 *  这个脚本会在场景主相机上创建一个CameraFxMgr，而CameraFxMgr在OnRenderImage的时候会调用到自己游戏对象下的后期特效脚本的OnRenderImage
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;
using System.Reflection;

//[ExecuteInEditMode]
public class CameraFx: MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Art/特效/相机后期")]
    static void AddCameraFx()
    {
        if (UnityEditor.Selection.activeGameObject != null)
        {
            CameraFx comp = UnityEditor.Selection.activeGameObject.AddComponentIfNoExist<CameraFx>();
            EditorUtil.SetDirty(comp);
        }
    }
#endif
    #region Fields
    public float m_duration = -1;//总时间
    public List<CameraAni> m_anis = new List<CameraAni>();

    CameraFxMgr.CameraHandle m_handle = null;
    PostEffectsBase m_fx;//真正的特效
    MethodInfo m_method;
    bool m_isPlaying = false;
    float m_beginTime = 0;
    bool m_needDestroy = false;
    float m_destroyTime = -1;
    #endregion


    #region Properties

    #endregion


    #region Mono Frame
    void Start()
    {
        Camera ca = this.GetComponent<Camera>();
        if (ca != null && ca.enabled)
            ca.enabled = false;
    } 
    void OnEnable()
    {
#if UNITY_EDITOR
        //如果是编辑器没有运行查看要手动调用下FxDestroy的update
        if ( Application.isEditor && !Application.isPlaying)
        {
            FxDestroy fxDestroy = this.GetComponent<FxDestroy>();
            if (fxDestroy != null)
                fxDestroy.Invoke("OnEnable", 0);
        }
#endif

        
        //判断IsPreloading是因为对象池预加载的时候不需要play
        if (!GameObjectPool.IsPreloading() && !m_isPlaying)
            Play();
    }

    // Update is called once per frame
    void Update()
    {
        //判断下结束
        if(m_isPlaying && m_destroyTime!=-1 && Util.time > m_destroyTime)
        {
            FxDestroy.DoDestroy(this.gameObject, false);
            m_destroyTime = -1;
        }
            
#if UNITY_EDITOR
        //如果是编辑器没有运行查看要手动调用下FxDestroy的update
        if (m_isPlaying&&Application.isEditor && !Application.isPlaying)
        {
            FxDestroy fxDestroy = this.GetComponent<FxDestroy>();
            if (fxDestroy != null)
                fxDestroy.Invoke("Update", 0);
        }
#endif
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
#if UNITY_EDITOR
        //如果是编辑器没有运行查看要手动调用下FxDestroy的update
        if ( Application.isEditor && !Application.isPlaying)
        {
            FxDestroy fxDestroy = this.GetComponent<FxDestroy>();
            if (fxDestroy != null)
                fxDestroy.Invoke("OnDisable", 0);
        }
#endif
    }

    //由CameraFxMgr调用，外部不要调用
    public void OnUpdateCamera(RenderTexture source, RenderTexture destination)
    {
        if (m_method == null || m_fx == null)
        {
            FxDestroy.DoDestroy(this.gameObject);
            return;
        }
        foreach (CameraAni a in m_anis)
        {
            a.OnUpdateCamera(m_fx, Util.time - m_beginTime);
        }
        m_method.Invoke(m_fx, new System.Object[]{source, destination});
    }
    #endregion



    #region Private Methods
    bool IngorePoolDestroy()
    {
        float maxEndDuration = 0;
        for (int i = 0;i< m_anis.Count;++i )
        {
            CameraAni a = m_anis[i];
            if (a.m_endDuration > maxEndDuration)
            {
                maxEndDuration = a.m_endDuration;
                a.OnEnd();
            }

        }
        if (maxEndDuration == 0)
            return false;//不需要延迟销毁
        m_destroyTime = Util.time + maxEndDuration;
        //this.transform.parent = null;
        return true;
    }
    #endregion

    public void Play()
     {
        if(m_isPlaying)
        {
            Debuger.Log("还没结束就重新播放了");
            Stop();//先结束老的
        }

        //先找到对应的脚本
        m_fx = GetComponent<PostEffectsBase>();
        if (m_fx == null)
        {
            Debuger.Log("没有找到后期处理特效");
            FxDestroy.DoDestroy(this.gameObject);
            return;
        }
        GetComponent<Camera>().enabled =false;//隐藏相机，如果没有隐藏的话
        Type t = m_fx.GetType();
        m_method = t.GetMethod("OnRenderImage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (m_method == null)
        {
            Debuger.Log("没有找到OnRenderImage方法:{0}", t.Name);
            FxDestroy.DoDestroy(this.gameObject);
            return;
        }

        //添加到相机特效管理器
        Camera c = null;
#if ART_DEBUG
        c = Camera.main;
#else
        c = CameraMgr.instance==null?null:CameraMgr.instance.CurCamera;
#endif
        if (c == null)
        {
            Debuger.LogError("找不到相机");
            FxDestroy.DoDestroy(this.gameObject);
            return;
        }
        CameraFxMgr camFxMgr = c.AddComponentIfNoExist<CameraFxMgr>();
        m_handle = camFxMgr.Add(t.Name, this);

        m_isPlaying = true;
        m_beginTime = Util.time;
        m_destroyTime = -1;

        GameObjectPoolObject po = this.AddComponentIfNoExist<GameObjectPoolObject>();
        if (po != null)//为了实现延迟销毁机制，要用到对象池
            po.m_onIngoreDestroy = IngorePoolDestroy;


        foreach (CameraAni a in m_anis)
        {
            a.OnBegin(m_fx);
        }
        
    }

     public void Stop()
     {
         if (this == null) return;//可能已经被销毁
         if (!m_isPlaying)//防止死锁
             return;

         //1 必须先把这个值设置正确，不然可能造成死锁
         m_isPlaying = false;

         //Remove和stop可能会互相调用,内部已经防止死锁，这里不用判断
         m_handle.mgr.Remove(m_handle);

         //DoDestroy和stop可能会互相调用,内部已经防止死锁，这里不用判断
         FxDestroy.DoDestroy(this.gameObject);

         m_handle = null;
         foreach (CameraAni a in m_anis)
         {
             a.OnStop();
         }
     }
}
