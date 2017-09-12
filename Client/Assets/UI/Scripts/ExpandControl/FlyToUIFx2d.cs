using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Xft;


public class FlyToUIFx2d : MonoBehaviour
{   
    public float waitUp = 0f;
    public List<float> horizonSpeed = new List<float>();
    public List<float> verticalSpeed = new List<float>();
    //public float scale=0.07f;
    //public float fxScale = 0.07f;   
    public AnimationCurve stageCurve2 = new AnimationCurve(new Keyframe(0, 0.4f, 1, 1), new Keyframe(0.5f, 0.7f, 0, 0), new Keyframe(1, 0.7f, -1, -1));   
    private bool isChangeToUI = false;

    private float horizon;
    private float vertical;
 
    
    public class Param
    {
        public Vector3 posFrom;
        public Vector3 downPos;
        public Transform uiTrans;        
        public Action onEnd;
    }
   
    public Param FxParam { get { return m_param; } set { m_param = value; } }

    [SerializeField]
    private Param m_param;

    private Transform m_trans;
    private ParticleSystem[] m_pss;
    private   int stage;
    private float waitTimePoint;

    [ContextMenu("PlayNow")]
    public void PlayNow()
    {
        if (horizonSpeed.Count == 0 || verticalSpeed.Count == 0)
        {
            horizon = 10;
            vertical = 15;
        }
        else
        {
            int i = UnityEngine.Random.Range(0, horizonSpeed.Count);        
            horizon = horizonSpeed[i];
            vertical = verticalSpeed[i];
        }



        isChangeToUI = false;
        
        gameObject.SetActive(false);//如果不先隐藏，粒子的拖尾会有残影
        stage = 0;
        m_trans = this.gameObject.transform;
        m_trans.position = m_param.downPos;
        waitTimePoint = 0;
        LateUpdate();
        gameObject.SetActive(true);
        if (m_trans.childCount > 0)
        {
            m_trans.GetChild(0).gameObject.SetActive(true);
        }

        //SetParticleScale(scale, fxScale);
        //m_trans.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }
    
    void Start()
    {
        //PlayNow();
    }

    void OnDestroy()
    {
    }

    void OnDisable()
    {
    }

    public void SetEnd()
    {
        TrailRenderer trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.time = 0;
        }
        if (m_param.onEnd != null)
        {
            m_param.onEnd();
            m_param.onEnd = null;
        }
        this.gameObject.SetActive(false);
        //SetParticleScale(1f);
        //FxDestroy.DoDestroy(this.gameObject);
        //GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Put(this.gameObject);
        //end();
        
    }
  


    void SetParticleScale(float scale,float fxscale)
    {
        if (m_pss == null || m_pss.Length <= 0)
            m_pss = this.gameObject.GetComponentsInChildren<ParticleSystem>();

      

        for (var i = 0; i < m_pss.Length; ++i)
        {
            ParticleSystem ps = m_pss[i];
            ps.startSize = fxscale;
        }
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    void LateUpdate()
    {
        var curCam = CameraMgr.instance != null && CameraMgr.instance.CurCamera != null ? CameraMgr.instance.CurCamera : Camera.main;
        if (stage == 0)
        {
           
            Vector3 curPosV3 = m_trans.position;
            var uiCam = UIMgr.instance != null && UIMgr.instance.UICamera != null ? UIMgr.instance.UICamera : null;       
            //Vector3 posInWorld = curPosV3;
            Vector3 posInCamera = curCam.WorldToScreenPoint(curPosV3);
            if (posInCamera.x < 0)
            {
                posInCamera.x = 0;
            }
            if (posInCamera.x > Screen.width)
            {
                posInCamera.x = Screen.width;
            }
            if (posInCamera.y < 0)
            {
                posInCamera.y = 0;
            }
            if (posInCamera.y > Screen.height)
            {
                posInCamera.y = Screen.height;
            }

            Vector3 newPos = uiCam.ScreenToWorldPoint(posInCamera);
     
            //m_trans.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));

            newPos.z = m_param.uiTrans.position.z;
            curPosV3 = newPos;
            m_trans.position = curPosV3;                                   
    
            stage = 1;
            waitTimePoint = Time.time + 0.1f;
        }
        if (stage == 1)
        {
                 
            if(Time.time>waitTimePoint)
            {
                TrailRenderer trail = GetComponentInChildren<TrailRenderer>();
                if (trail != null)
                {
                    trail.time = 0.5f;
                }
            }
            Vector3 fromPosV3 = m_param.posFrom;
            Vector3 curPosV3 = m_trans.position;
            Vector3 destPosV3;
            destPosV3 = m_param.uiTrans.position;
            var dist = Vector3.Distance(curPosV3, destPosV3);
            if (dist < 0.001f)
            {               
                    waitTimePoint = Time.time + waitUp;
                    stage = 2;
                    return;
                
            }

            var maxDist = Vector3.Distance(fromPosV3, destPosV3);
            var curve = stageCurve2.Evaluate(1 - dist / maxDist);
            var dir = (destPosV3 - curPosV3).normalized;
         
                curPosV3 += new Vector3(dir.x * horizon, dir.y * vertical, dir.z * horizon) * Time.deltaTime * curve;
         

            var dir2 = (destPosV3 - curPosV3).normalized;
            if (Vector3.Dot(dir, dir2) < 0)
            {             
                    waitTimePoint = Time.time + waitUp;
                    stage = 2;
                    return;                
            }
            m_trans.position = curPosV3;           
        }
        else
        {
            if (Time.time >= waitTimePoint)
            {
                
                Vector3 curPosV3 = m_trans.position;
                Vector3 destPosV3;
                var uiCam = UIMgr.instance != null && UIMgr.instance.UICamera != null ? UIMgr.instance.UICamera : null;
                if (uiCam == null)
                {
                    destPosV3 = m_param.uiTrans.position;
                }
                else
                {
                    destPosV3 = uiCam.WorldToViewportPoint(m_param.uiTrans.position);
                    destPosV3.z = curCam.nearClipPlane * 2;
                    destPosV3 = curCam.ViewportToWorldPoint(destPosV3);
                }
                curPosV3 = destPosV3;
                SetEnd();
                return;
            }
        }
    }

}