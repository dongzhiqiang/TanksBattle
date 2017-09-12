using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class UI3DView : MonoBehaviour
{
    public UIPanel m_parent;
    public Camera m_camera;
    public GameObject m_stage;
    public Light m_light;
    public DragListener m_drag;
    public RawImage m_renderImage;
    public GameObject m_model;

    private string m_modName;
    private bool m_playAnimationNow;
    private bool m_hasAnimation;
    private float m_nextPlayAnimationTime;
    private bool m_playingStartAnimation = false;
    private bool m_isLoading = false;
    private bool m_renderImageReady = false;
    private int m_loadIdx = 0;

    void Awake()
    {
        if(m_drag!=null)
        {
            m_drag.onDrag = OnDrag;
        }

        if (m_renderImage && !m_renderImageReady) //对渲染纹理初始化，否则会是一片白色
        {
            m_camera.Render();
            m_renderImageReady = true;
        }
    }

    void OnDrag(PointerEventData eventData)
    {
        m_model.transform.localEulerAngles = new Vector3(m_model.transform.localEulerAngles.x, m_model.transform.localEulerAngles.y - eventData.delta.x, m_model.transform.localEulerAngles.z);
    }

    public void ResetRotation()
    {
        if (m_model != null)
            m_model.transform.localEulerAngles = Vector3.zero;
    }

    public void SetModel(string model, float scale=1, bool playAnimationnNow=false) 
    {
        if (model == m_modName && m_isLoading)
        {
            return;
        }
        if (model == m_modName && !playAnimationnNow)
        {
            return;
        }
        if (m_modName == null && m_model != null) //初始的对象，直接销毁掉
        {
            GameObject.Destroy(m_model);
            m_model = null;
        }
        m_modName = model;
        m_playAnimationNow = playAnimationnNow;
        if(m_model != null)
        {
            m_model.transform.SetParent(null);
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Put(m_model);
            m_model = null;
            //Debug.Log("put");
        }
        m_stage.transform.localScale = new Vector3(scale, scale, scale);
        m_playingStartAnimation = false;

        

        if(!string.IsNullOrEmpty(model))
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Get(model, ++m_loadIdx, OnGet,false);
            m_isLoading = true;
        }
    }

    float Now
    {
        get
        {
            if (Application.isPlaying)
            {
                return Time.unscaledTime;
            }
            else
            {
                var span = System.DateTime.Now.TimeOfDay;
                return (float)span.TotalSeconds;
            }
        }
    }

    void OnGet(GameObject model, object idx)
    {
        //Debug.Log("get");
        m_isLoading = false;
        if (m_loadIdx != ((int)idx))
        {
            model.transform.SetParent(null);
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Put(model);
            //Debug.Log("put2");
            return;

            //Animation ani;
            //ani.Play
        }
        m_model = model;
        m_model.transform.SetParent(m_stage.transform,false);
        m_model.transform.localPosition = Vector3.zero;
        m_model.transform.localEulerAngles = Vector3.zero;
        m_model.transform.localScale = Vector3.one;

        if (m_model.GetComponent<SimpleRole>() != null)
        {
            m_hasAnimation = true;
            if(m_playAnimationNow)
            {
                //PlayStartAnimation();
                m_nextPlayAnimationTime = Now;
                AniFxMgr ani = m_model.transform.Find("model").GetComponent<AniFxMgr>();
                if (ani.GetSt("zhuchengdaiji") != null)
                    ani.Play("zhuchengdaiji", WrapMode.Loop);
                else
                    ani.Play(AniFxMgr.Ani_DaiJi, WrapMode.Loop);
                m_playingStartAnimation = false;
            }
            else
            {
                m_nextPlayAnimationTime = Now + 1;
                AniFxMgr ani = m_model.transform.Find("model").GetComponent<AniFxMgr>();
                if (ani.GetSt("zhuchengdaiji") != null)
                    ani.Play("zhuchengdaiji", WrapMode.Loop);
                else
                    ani.Play(AniFxMgr.Ani_DaiJi, WrapMode.Loop);
                m_playingStartAnimation = false;
            }
        }
        else
        {
            m_hasAnimation = false;
        }


    }

    void PlayStartAnimation()
    {
        AniFxMgr ani = m_model.transform.Find("model").GetComponent<AniFxMgr>();
        if (ani.GetSt("zhuchengxiuxian") == null)
        {
            if (ani.GetSt("xiuxiandaiji") == null)
            {
                return;
            }
            ani.Play("xiuxiandaiji", WrapMode.Once);
            m_playingStartAnimation = true;
            return;
        }
        ani.Play("zhuchengxiuxian", WrapMode.Once);
        m_playingStartAnimation = true;
        //Debug.Log("start");
    }

    void Update()
    {/*
        if (m_parent != null && m_parent.IsOpen)
        {
            if(m_parent.IsTop)
            {
                m_stage.SetActive(true);
            }
            else
            {
                m_stage.SetActive(false);
            }
        }*/

        if(m_hasAnimation && m_model != null && m_stage.activeSelf)
        {
            AniFxMgr ani = m_model.transform.Find("model").GetComponent<AniFxMgr>();
            if (m_playingStartAnimation )
            {
                if((ani.CurSt == null || ani.CurSt.enabled == false))
                {
                    //Debug.Log("start end");
                    m_nextPlayAnimationTime = Now + 60;
                    if (ani.GetSt("zhuchengdaiji") != null)
                        ani.Play("zhuchengdaiji", WrapMode.Loop);
                    else
                        ani.Play(AniFxMgr.Ani_DaiJi, WrapMode.Loop);
                    m_playingStartAnimation = false;
                }
            }
            else
            {
                if (ani.CurSt != null && ani.CurSt.enabled == false)
                {
                    if (ani.GetSt("zhuchengdaiji") != null)
                        ani.Play("zhuchengdaiji", WrapMode.Loop);
                    else
                        ani.Play(AniFxMgr.Ani_DaiJi, WrapMode.Loop);
                }
                if(Now >= m_nextPlayAnimationTime)
                {
                    PlayStartAnimation();
                }
            }
        }
    }
}