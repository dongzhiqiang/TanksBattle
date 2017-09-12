#region Header
/**
 * 名称：创建角色
 
 * 日期：2016.6.28
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

//技能
public class CreateRoleEventCfg : SkillEventCfg
{
    
    public bool self= true;//是自己使用技能，还是target使用技能
    public string roleId = "";
    public float dirOffset = 0;//角度偏移
    public Vector3 posOffset = Vector3.zero;//位置偏移

    public override enSkillEventType Type { get { return enSkillEventType.createRole; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "自己", COL_WIDTH * 2)) onTip("是自己使用技能，还是target使用技能"); return false;
            case 1: if (h(ref r, "角色id", COL_WIDTH * 3)) onTip("要创建的角色"); return false;
            case 2: if (h(ref r, "角度偏移", COL_WIDTH * 3)) onTip("水平角度偏移，注意是先偏移角度再偏移位置"); return false;
            case 3: if (h(ref r, "位置偏移", COL_WIDTH * 7)) onTip("位置偏移，注意是先偏移角度再偏移位置"); return false;
            default: return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 2;
                    self = EditorGUI.Toggle(r, GUIContent.none, self);
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 3;
                    int idx = string.IsNullOrEmpty(roleId) ? -1 : System.Array.IndexOf(RoleCfg.RoleIds, roleId);
                    int newIdx = EditorGUI.Popup(r, idx, RoleCfg.RoleIds);
                    if(idx != newIdx && newIdx != -1)
                        roleId = RoleCfg.RoleIds[newIdx];
                    r.x += r.width;
                }; return false;
            case 2:
                {
                    r.width = COL_WIDTH *3;
                    dirOffset = EditorGUI.FloatField(r, GUIContent.none, dirOffset);
                    r.x += r.width;
                }; return false;
            case 3:
                {
                    r.width = COL_WIDTH * 7;
                    posOffset = EditorGUI.Vector3Field(r, GUIContent.none, posOffset);
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif

    public override void PreLoad()
    {
        if (!string.IsNullOrEmpty(roleId))
            RoleCfg.PreLoad(roleId);
    }
    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        Role r = self?source:target;

        RoleCfg cfg = RoleCfg.Get(roleId);
        if (cfg == null)
            return false;
        RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
        cxt.bornAniId = cfg.bornType;
        cxt.deadAniId = cfg.deadType;
        cxt.groundDeadAniId = cfg.groundDeadType;
        cxt.aiBehavior = cfg.aiType;
        cxt.guid = Util.GenerateGUID();
        cxt.roleId = cfg.id;
        cxt.level = r.GetInt(enProp.level);
        cxt.camp = r.GetCamp();

        Vector3 pos = r.transform.position;
        Quaternion rot = r.transform.rotation*  Quaternion.Euler(0, dirOffset, 0);
        cxt.euler = rot.eulerAngles;
        cxt.pos = pos+ rot* posOffset;

        LevelMgr.instance.CreateRole(cxt);
        //RoleMgr.instance.CreateRole(cxt);
        return true;
    }
}