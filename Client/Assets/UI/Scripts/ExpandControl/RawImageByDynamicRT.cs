#region Header
/**
 * 名称：CameraDynamicRenderTexture
 
 * 日期：2016.9.10
 * 描述：
 *  如果要映射的相机用的是动态生成的rendertexture,那么要处理下
 *  
 **/
#endregion

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(RawImage))]
public class RawImageByDynamicRT : MonoBehaviour
{
    public CameraDynamicRenderTexture m_dynamicRT;

    void OnEnable()
    {
        OnTextureChange(null);
        if (m_dynamicRT != null)
            m_dynamicRT.AddTextureChange(OnTextureChange);

    } 
    
    void OnTextureChange(RenderTexture rt)
    {
        if (rt == null)
        {
            this.GetComponent<RawImage>().texture = null;   
            this.GetComponent<RawImage>().color = new Color(1, 1, 1, 0);
        }
        else
        {
            this.GetComponent<RawImage>().texture = rt;
            this.GetComponent<RawImage>().color = new Color(1, 1, 1, 1);
        }
            

    }
}