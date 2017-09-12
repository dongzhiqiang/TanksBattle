using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIFx : MonoBehaviour
{
    public GameObject m_offset;
    public GameObject m_fx;

    private string m_fxName;
    
    //private bool m_isLoading = false;

    void ClearObj()
    {
        if (m_fx != null)
        {
            m_fx.transform.SetParent(null);
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Put(m_fx);
            m_fx = null;
        }
    }

    public void Clear()
    {
        ClearObj();
        m_fxName = "";
    }

    public void SetFx(string fx) 
    {
        if (fx == m_fxName)// && m_isLoading)
        {
            return;
        }
        if (m_fxName==null && m_fx!=null) //初始的对象，直接销毁掉
        {
            //m_fx.transform.SetParent(null);
            GameObject.Destroy(m_fx);
            m_fx = null;
        }
        m_fxName = fx;
        ClearObj();

        if(!string.IsNullOrEmpty(fx))
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Get(fx, fx, OnGet, false);
            //m_isLoading = true;
        }
    }

    void OnGet(GameObject fx, object fxName)
    {
        //Debug.Log("get");
        //m_isLoading = false;
        if (m_fxName != ((string)fxName))
        {
            fx.transform.SetParent(null);
            GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Put(fx);
            //Debug.Log("put2");
            return;

            //Animation ani;
            //ani.Play
        }
        m_fx = fx;
        m_fx.transform.SetParent(m_offset.transform, false);
        m_fx.transform.localPosition = Vector3.zero;
        m_fx.transform.localEulerAngles = Vector3.zero;
        m_fx.transform.localScale = Vector3.one;
        /*
        if(m_offset.GetComponent<UIPanelCheckSortLayer>()!=null)
        {
            GetComponentInParent<UIPanelBase>().ResetOrder();
        }
         */
    }

}