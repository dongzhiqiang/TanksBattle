using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class FlyToUIFx : MonoBehaviour
{
    public float waitDown = 0f;
    public List<float> horizonSpeed = new List<float>();
    public List<float> verticalSpeed = new List<float>();
    public float G = 0.3f;
    //public float scale=50f;
    //public float fxScale = 0.1f;  
    //public AnimationCurve stageCurve1 = new AnimationCurve(new Keyframe(0, 0.2f, 1, 1), new Keyframe(0.5f, 1, 0, 0), new Keyframe(1, 0.2f, -1, -1));
    public bool isGold;
    public bool isItem;
    bool isPlaySound = false;
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


        isPlaySound = false;
        this.gameObject.SetActive(false);//如果不先隐藏，粒子的拖尾会有残影
        stage = 0;
        waitTimePoint = Time.time + 0.1f;
        m_trans = this.gameObject.transform;
        m_trans.position = m_param.posFrom;
        //waitTimePoint = 0;
        LateUpdate();
        this.gameObject.SetActive(true);
        if(m_trans.childCount>0)
        {
            m_trans.GetChild(0).gameObject.SetActive(true);
        }

        var curCam = CameraMgr.instance != null && CameraMgr.instance.CurCamera != null ? CameraMgr.instance.CurCamera : Camera.main;

        Transform new_trans = m_trans;
        new_trans.LookAt(curCam.transform.position);

        Vector3 rotation = new_trans.rotation.eulerAngles;
        rotation.x= m_trans.rotation.eulerAngles.x;
        rotation.z = m_trans.rotation.eulerAngles.z;
        m_trans.rotation = Quaternion.Euler(rotation);


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
        //gameObject.transform.position = m_param.downPos;
        this.gameObject.SetActive(false);
        //SetParticleScale(1f);
        //FxDestroy.DoDestroy(this.gameObject);
        //GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Put(this.gameObject);
        //end();
        if(isItem)
        {
#if !ART_DEBUG
            SoundMgr.instance.Play2DSound(Sound2DType.ui, 110);
#endif
        }
        else if(!isGold)
        {
#if !ART_DEBUG
            SoundMgr.instance.Play2DSound(Sound2DType.ui, 111);
#endif
        }
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
            if (Time.time > waitTimePoint)
            {
                TrailRenderer trail = GetComponentInChildren<TrailRenderer>();
                if (trail != null)
                {
                    trail.time = 0.5f;
                }
            }           
            Vector3 fromPosV3 = m_param.posFrom;
            Vector3 curPosV3 = m_trans.position;
            Vector3 destPosV3 = m_param.downPos;

            var dist = Vector3.Distance(curPosV3, destPosV3);
#if !ART_DEBUG
            if (dist < 2f && !isPlaySound)
            {
                isPlaySound = true;
                Transform tran = RoleMgr.instance.Hero.transform;
                if (LevelMgr.instance.CurLevel.roomCfg.id.StartsWith("jinbi") && isGold && !isItem)
                {
                    SoundMgr.instance.Play3DSound(202, tran);
                }
                else if (LevelMgr.instance.CurLevel.roomCfg.id.StartsWith("jinbi") && !isGold && isItem)
                {
                    SoundMgr.instance.Play3DSound(203, tran);
                }
                else
                {
                    SoundMgr.instance.Play3DSound(201, tran);
                }
            }
#endif

            if (dist < 0.001f)
            {                           
                stage = 1;
                waitTimePoint = Time.time + waitDown;
                return;
            }
          
           

            var maxDist = Vector3.Distance(fromPosV3, destPosV3);
            //var curve =stageCurve1.Evaluate(1 - dist / maxDist);
            var dir = (destPosV3 - curPosV3).normalized;
            curPosV3 += new Vector3(dir.x * horizon, dir.y * -vertical, dir.z * horizon) * Time.deltaTime;
            vertical -= G;
            var dir2 = (destPosV3 - curPosV3).normalized;
            if (Vector3.Dot(dir, dir2) < 0)
            {                
                stage = 1;
                waitTimePoint = Time.time + waitDown;            
                return;
            }
            if (Mathf.Abs(curPosV3.y - destPosV3.y) < 0.05f)
            {                
                stage = 1;
                waitTimePoint = Time.time + waitDown;
                return;
            }
            if (m_trans.position!=null&& curPosV3!=null)
                m_trans.position = curPosV3;
            //var Scale = Vector3.Distance(m_trans.position, curCam.transform.position) *scale;
            //var FxScale = Vector3.Distance(m_trans.position, curCam.transform.position) * fxScale;
            //SetParticleScale(Scale, FxScale);
        }
        else if (stage == 1)
        {
            if (Time.time >= waitTimePoint)
            {
                SetEnd();                 
                return;
            }
        }
       
        
    }

}