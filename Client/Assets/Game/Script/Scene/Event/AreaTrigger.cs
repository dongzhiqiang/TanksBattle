using UnityEngine;
using System.Collections;

public class AreaTriggerCxt
{
    public AreaTriggerCxt(string areaId, Role role)
    {
        this.areaId = areaId;
        this.role = role;
    }
    public string areaId = "";
    public Role role = null;
}

[RequireComponent(typeof(BoxCollider))]
public class AreaTrigger : MonoBehaviour 
{
    [System.NonSerialized]
    public string mTriggerId = "";

    void OnTriggerEnter(Collider other)
    {
        RoleModel roleModel = other.GetComponent<RoleModel>();
        if (roleModel != null && SceneEventMgr.instance.mTriggerDict.Count > 0)
        {
            AreaTriggerCxt cxt = new AreaTriggerCxt(mTriggerId, roleModel.Parent);
            EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.ENTERAREA, cxt);
        }
    }

    void OnTriggerExit(Collider other)
    {
        RoleModel roleModel = other.GetComponent<RoleModel>();
        if (roleModel != null && SceneEventMgr.instance.mTriggerDict.Count > 0)
        {
            AreaTriggerCxt cxt = new AreaTriggerCxt(mTriggerId, roleModel.Parent);
            EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.LEAVEAREA, cxt);
        }
    }
}
