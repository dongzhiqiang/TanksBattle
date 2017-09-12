using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIStaminaButton : MonoBehaviour
{
    
    public GameObject m_image;
    public GameObject m_fxSuccess;
    public GameObject m_fxFailed;
    public GameObject m_fxUiBaodianRing;
    public GameObject m_fxUiBaodianRing01;
    private UIGainStamina m_parent;
    private Dictionary<string, GameObject> m_fxRing = new Dictionary<string,GameObject>();
    private GameObject m_fxNow;
    private bool m_isPerfect;
    private TimeMgr.Timer m_timerStart;
    private TimeMgr.Timer m_timerFail;
    private TimeMgr.Timer m_timerEnd;

    void Awake()
    {
        m_fxRing["fx_ui_baodian_ring"] = m_fxUiBaodianRing;
        m_fxRing["fx_ui_baodian_ring01"] = m_fxUiBaodianRing01;
        //m_image.GetComponent<StateHandle>().AddClick(OnClick);
        m_image.GetComponent<StateHandle>().AddPointDown(OnClick);
    }

    public void SetParent(UIGainStamina parent)
    {
        m_parent = parent;
    }

    public void Clear()
    {
        m_image.GetComponent<StateHandle>().SetState(0);
        m_image.SetActive(false);
        m_fxSuccess.SetActive(false);
        m_fxFailed.SetActive(false);
        m_fxUiBaodianRing.SetActive(false);
        m_fxUiBaodianRing01.SetActive(false);
        if(m_timerStart!=null)
        {
            TimeMgr.instance.RemoveTimer(m_timerStart);
            m_timerStart = null;
        }
        if (m_timerFail != null)
        {
            TimeMgr.instance.RemoveTimer(m_timerFail);
            m_timerFail = null;
        }
        if (m_timerEnd != null)
        {
            TimeMgr.instance.RemoveTimer(m_timerEnd);
            m_timerEnd = null;
        }
    }

    public void StartFx(string fx)
    {
        m_isPerfect = false;
        m_image.SetActive(true);
        m_fxNow = m_fxRing[fx];
        if(m_fxNow==null)
        {
            Debug.LogError("找不到" + fx);
            return;
        }
        m_fxNow.SetActive(true);
        m_timerStart = TimeMgr.instance.AddTimer(m_fxNow.GetComponentInChildren<ParticleSystem>().duration, StartPerfect);
    }

    void StartPerfect()
    {
        m_timerStart = null;
        m_fxNow.SetActive(false); // or stop
        m_isPerfect = true;
        m_timerFail = TimeMgr.instance.AddTimer(0.5f, Fail);
    }

    void Fail()
    {
        m_timerFail = null;
        m_image.SetActive(false);
        m_fxFailed.SetActive(true);
        m_timerEnd = TimeMgr.instance.AddTimer(m_fxFailed.GetComponentInChildren<ParticleSystem>().duration, End);
        SoundMgr.instance.Play2DSoundAutoChannel(303);
    }

    void OnClick(PointerEventData data)
    {
        if (m_timerStart != null)
        {
            TimeMgr.instance.RemoveTimer(m_timerStart);
            m_timerStart = null;
        }
        if (m_timerFail != null)
        {
            TimeMgr.instance.RemoveTimer(m_timerFail);
            m_timerFail = null;
        }
        m_image.SetActive(false);
        m_fxSuccess.SetActive(true);
        m_timerEnd = TimeMgr.instance.AddTimer(m_fxSuccess.GetComponentInChildren<ParticleSystem>().duration, End);
        m_parent.AddPercentage(m_isPerfect);
        if(m_isPerfect)
        {
            SoundMgr.instance.Play2DSoundAutoChannel(301);
        }
        else
        {
            SoundMgr.instance.Play2DSoundAutoChannel(302);
        }
    }

    void End()
    {
        m_timerEnd = null;
        Clear();
        m_parent.PutButton(this);
    }
}