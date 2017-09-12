using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class RotateBoneByTarget : MonoBehaviour {

    public Transform target;
    public Transform moveBone;
    public Vector3 euler;
    
    void LateUpdate()
    {
#if !ART_DEBUG
        if (target == null && RoleMgr.instance.Hero != null)
            target = RoleMgr.instance.Hero.transform;
        
        //使用技能时就不盯着
        Role role = this.GetComponent<RoleModel>().Parent;
        if (role == null)
            return;

        if (role.RSM.CurStateType == enRoleState.combat)
            return;

        foreach(Buff buff in role.BuffPart.Buffs)
        {
            if (buff.Cfg.id == 318) //眩晕时
                return;
        }
#endif
        if (target == null || moveBone == null)
            return;

        moveBone.forward = this.transform.position - target.position;
        moveBone.Rotate(-euler);
    }

    void OnDrawGizmos()
    {
        if (moveBone == null)
            return;

#if UNITY_EDITOR
        UnityEditor.Handles.PositionHandle(moveBone.position, Quaternion.Euler(euler) * moveBone.rotation);
#endif
    }
}
