#region Header
/**
 * 名称：镜头切换
 
 * 日期：2015.10.23
 * 描述：
 *  
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;
using System.Reflection;


public class CameraChange : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Art/特效/相机角度")]
    static void AddCameraChange()
    {
        if (UnityEditor.Selection.activeGameObject != null)
        {
            CameraChange comp = UnityEditor.Selection.activeGameObject.AddComponentIfNoExist<CameraChange>();
            EditorUtil.SetDirty(comp);
        }
    }
#endif
    #region Fields
    public float m_duration = -1;//总时间
    public CameraInfo m_info = new CameraInfo();

    CameraHandle m_handle = null;
    bool m_isPlaying = false;
    float m_beginTime = 0;
    bool m_needDestroy = false;
    #endregion


    #region Properties
    
    #endregion


    #region Mono Frame
    void OnEnable()
    {

        //判断IsPreloading是因为对象池预加载的时候不需要play
        if (!GameObjectPool.IsPreloading() && !m_isPlaying)
            Play();
    }

    // Update is called once per frame
    void Update()
    {
        //判断下结束
        if (m_isPlaying && !m_needDestroy && (m_duration > 0 && (Util.time - m_beginTime) > m_duration))
        {
            m_needDestroy = true;

            FxDestroy.DoDestroy(this.gameObject);
        }

    }

    void OnDisable()
    {
        Stop();

    }

    #endregion
   
     public void Play()
     {
        if(m_isPlaying)
        {
            Debuger.Log("还没结束就重新播放了");
            Stop();//先结束老的
        }

        
        if (CameraMgr.instance==null)
        {
            Debuger.Log("没有找到相机管理器");
            FxDestroy.DoDestroy(this.gameObject);
            return;
        }

        m_handle = CameraMgr.instance.Add(m_info);

        m_isPlaying = true;
        m_beginTime = Util.time;
        m_needDestroy =false;
        
        
    }

     public void Stop()
     {
         if (this == null) return;//可能已经被销毁
         if (!m_isPlaying)//防止死锁
             return;

         //1 必须先把这个值设置正确，不然可能造成死锁
         m_isPlaying = false;

         //Remove和stop可能会互相调用,内部已经防止死锁，这里不用判断
         m_handle.Release();
         m_handle = null;

         //DoDestroy和stop可能会互相调用,内部已经防止死锁，这里不用判断
         FxDestroy.DoDestroy(this.gameObject);

         
         
     }
}
