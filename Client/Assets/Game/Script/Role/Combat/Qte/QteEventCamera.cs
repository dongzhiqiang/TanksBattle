using UnityEngine;
using System.Collections;

public class QteEventCamera : QteEvent
{
    public float beginSmooth = 0.5f;
    public float endSmooth = 0.15f;

    public Vector3 offset;

    GameObject go;

    public override void Init()
    {

    }

    public override void Start()
    {
        go = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately("fx_camera_blur_internal", false);
        CameraFx cameraFx = go.GetComponent<CameraFx>();
        
        CameraAni cameraAni = cameraFx.m_anis[0];
        cameraAni.m_beginDuration = beginSmooth;
        cameraAni.m_endDuration = endSmooth;

        RadialBlur blur = go.GetComponent<RadialBlur>();
        blur.target = RoleMgr.instance.Hero.transform;
        blur.offset = offset;

        go.SetActive(true);

    }
    public override void Update(float time)
    {
    }

    public override void Stop()
    {
        if (go != null)
            FxDestroy.Destroy(go);
    }
}
