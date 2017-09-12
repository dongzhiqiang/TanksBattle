using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class UI3DView2 : MonoBehaviour
{
    public Transform    m_leftPoint;
    public Transform    m_rightPoint;
    public Camera       m_camera;
    public RawImage     m_leftImg;
    public RawImage     m_rightImg;

    private string m_leftModelName;
    private string m_rightModelName;

    private string m_leftAniName;
    private string m_rightAniName;

    private GameObject m_leftModel;
    private GameObject m_rightModel;

    private RenderTexture m_renderTex;

    public void SetLeftModel(string modelName, float scale = 1, string aniName = null)
    {
        if (modelName == m_leftModelName && m_leftModel != null)
        {
            if (m_leftModel.GetComponent<SimpleRole>() != null && m_leftAniName != null)
            {
                AniFxMgr aniMgr = m_leftModel.GetComponentInChildren<AniFxMgr>();
                aniMgr.Play(m_leftAniName, WrapMode.Loop);
            }
            return;
        }

        m_leftModelName = modelName;
        if (m_leftModel != null)
        {
            m_leftModel.transform.SetParent(null);
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Put(m_leftModel);
            m_leftModel = null;
        }

        scale = scale < Mathf.Epsilon ? 1 : scale;
        m_leftPoint.transform.localScale = new Vector3(scale, scale, scale);
        m_leftAniName = aniName;

        if (modelName != null)
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Get(modelName, modelName, OnLoadLeftModel, false);
        }
    }

    public void SetRightModel(string modelName, float scale = 1, string aniName = null)
    {
        if (modelName == m_rightModelName && m_rightModel != null)
        {
            if (m_rightModel.GetComponent<SimpleRole>() != null && m_rightAniName != null)
            {
                AniFxMgr aniMgr = m_rightModel.GetComponentInChildren<AniFxMgr>();
                aniMgr.Play(m_rightAniName, WrapMode.Loop);
            }
            return;
        }            

        m_rightModelName = modelName;
        if (m_rightModel != null)
        {
            m_rightModel.transform.SetParent(null);
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Put(m_rightModel);
            m_rightModel = null;
        }

        scale = scale < Mathf.Epsilon ? 1 : scale;
        m_rightPoint.transform.localScale = new Vector3(scale, scale, scale);
        m_rightAniName = aniName;

        if (modelName != null)
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Get(modelName, modelName, OnLoadRightModel, false);
        }
    }

    void OnLoadLeftModel(GameObject modelObj, object modelName)
    {
        if (m_leftModelName != (string)modelName)
        {
            m_leftModel.transform.SetParent(null);
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Put(m_leftModel);
            return;
        }

        m_leftModel = modelObj;
        m_leftModel.transform.SetParent(m_leftPoint.transform, false);
        m_leftModel.transform.localPosition = Vector3.zero;
        m_leftModel.transform.localEulerAngles = Vector3.zero;
        m_leftModel.transform.localScale = Vector3.one;

        if (modelObj.GetComponent<SimpleRole>() != null && m_leftAniName != null)
        {
            AniFxMgr aniMgr = modelObj.GetComponentInChildren<AniFxMgr>();
            aniMgr.Play(m_leftAniName, WrapMode.Loop);
        }
    }

    void OnLoadRightModel(GameObject modelObj, object modelName)
    {
        if (m_rightModelName != (string)modelName)
        {
            m_rightModel.transform.SetParent(null);
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Put(m_rightModel);
            return;
        }
        m_rightModel = modelObj;
        m_rightModel.transform.SetParent(m_rightPoint.transform, false);
        m_rightModel.transform.localPosition = Vector3.zero;
        m_rightModel.transform.localEulerAngles = Vector3.zero;
        m_rightModel.transform.localScale = Vector3.one;

        if (modelObj.GetComponent<SimpleRole>() != null && m_leftAniName != null)
        {
            AniFxMgr aniMgr = modelObj.GetComponentInChildren<AniFxMgr>();
            aniMgr.Play(m_rightAniName, WrapMode.Loop);
        }
    }

    void Start()
    {

    }

    void OnEnable()
    {
        if (m_renderTex == null)
        {
            m_renderTex = new RenderTexture(1024, 512, 16, RenderTextureFormat.ARGB32);
            
            m_camera.targetTexture = m_renderTex;
            m_leftImg.texture = m_renderTex;
            m_rightImg.texture = m_renderTex;
        }        
    }

    void OnDisable()
    {
        m_camera.targetTexture = null;
        if (m_renderTex!=null)
        {
            m_renderTex.Release();
            GameObject.DestroyImmediate(m_renderTex);
            m_renderTex = null;
        }
        
        m_leftImg.texture = null;
        m_rightImg.texture = null;
    }
}