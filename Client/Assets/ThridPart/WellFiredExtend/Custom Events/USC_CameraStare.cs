using UnityEngine;
using System.Collections;
using WellFired;

[USequencerFriendlyName("Camera Stare")]
[USequencerEvent("Custom/Camera Stare")]
public class USC_CameraStare : USEventBase
{
    public Transform m_target;
    public Transform m_camera;

    Vector3 m_targetPos = Vector3.zero;

    public void Update()
    {
        if (m_target != null && m_target.position != m_targetPos)
        {
            m_targetPos = m_target.position;
            m_camera.LookAt(m_targetPos);
        }
    }

    public override void FireEvent()
    {
      
    }

    public override void ProcessEvent(float deltaTime)
    {
        if (m_camera == null)
            return;

        m_camera.LookAt(m_targetPos);
    }

    public override void StopEvent()
    {
     
    }
}