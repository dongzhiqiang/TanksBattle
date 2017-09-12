using UnityEngine;
using System.Collections;

public class QteEventGroup : QteEvent
{
    public string eventGroupId = "";
    bool isReach = false;
    public override void Init()
    {
        isReach = false;
    }
    public override void Start()
    {
        if (!Application.isPlaying)
            return;

    }
    public override void Update(float time)
    {
        if (isReach)
            return;
        if (!Application.isPlaying)
            return;

        CombatMgr.instance.PlayEventGroup(RoleMgr.instance.Hero, eventGroupId, RoleMgr.instance.Hero.transform.position);
        isReach = true;
    }
    public override void Stop()
    {
        isReach = false;
    }
}
