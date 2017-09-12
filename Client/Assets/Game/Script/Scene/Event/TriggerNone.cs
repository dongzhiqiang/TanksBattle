using UnityEngine;
using System.Collections;

public class TriggerNone : SceneTrigger
{
    public EventCfg_None m_eventCfg;
    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_None;
    }
    public override bool bReach()
    {
        return true;
    }
    public override void OnRelease()
    {
    }
}
