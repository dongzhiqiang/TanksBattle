#region Header
/**
 * 名称：CameraDynamicRenderTexture
 
 * 日期：2016.9.10
 * 描述：
 * 因为rendertexture一般内存占用都比较大，所以enable的时候才创建rendertexture，disable的时候释放rendertexture
 *  
 **/
#endregion

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraDynamicRenderTexture : MonoBehaviour
{
    static RenderTexture s_emptyRenderTexture;
    static HideFlags s_flag = HideFlags.DontSave;
    public static RenderTexture EmptyRenderTexture
    {
        get {
            if(s_emptyRenderTexture == null)
            {
                s_emptyRenderTexture = new RenderTexture(4, 4, 0, RenderTextureFormat.Depth);
                s_emptyRenderTexture.antiAliasing = 1;
                s_emptyRenderTexture.useMipMap = false;
                s_emptyRenderTexture.hideFlags = HideFlags.DontSave;
            }
            return s_emptyRenderTexture;
        }
    }

    Action<RenderTexture> m_onTextureChange;
    
    void OnEnable()
    {
        RenderTexture rt = new RenderTexture(1024, 1024, 16, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 1;
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.filterMode = FilterMode.Bilinear;
        rt.useMipMap = false;
        rt.hideFlags = s_flag;
        
        rt.name = string.Format("CameraDynamicRenderTexture_{0}", System.DateTime.Now.ToString());
        ChangeTexture(rt);
    } 

    void OnDisable()
    {
        ChangeTexture(null);
    }


    void ChangeTexture(RenderTexture rt)
    {
        var ca =this.GetComponent<Camera>();
        if (ca.targetTexture!= null&& ca.targetTexture.hideFlags == s_flag)
        {
            var oldRT = ca.targetTexture;
            ca.targetTexture = null;
            oldRT.Release();
            GameObject.DestroyImmediate(oldRT);
        }
        
        if(rt == null)
            ca.targetTexture = EmptyRenderTexture;
        else
            ca.targetTexture = rt;

        if(m_onTextureChange!=null)
            m_onTextureChange(rt);
    }

    public void AddTextureChange(Action<RenderTexture> cb, bool reset = false)
    {
        if (cb == null && reset)
        {
            m_onTextureChange = null;
            return;
        }

        do
        {
            if (m_onTextureChange == null ||reset)
            {
                m_onTextureChange = cb;
                break;
            }


            //如果重复添加，那么就不添加了
            Delegate[] inlist = m_onTextureChange.GetInvocationList();
            foreach (Delegate d in inlist)
            {
                if (d == cb)
                {
                    break;
                }
            }

            m_onTextureChange += cb;
        }
        while (false);
        

        Camera ca = this.GetComponent<Camera>();
        if (ca.targetTexture == null || ca.targetTexture == EmptyRenderTexture || ca.targetTexture.hideFlags!= HideFlags.DontSave)
            cb(null);
        else
            cb(ca.targetTexture);

    }
    

}