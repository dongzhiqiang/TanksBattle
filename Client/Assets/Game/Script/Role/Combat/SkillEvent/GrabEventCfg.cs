#region Header
/**
 * 名称：抓取事件
 
 * 日期：2016.3.9
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;


public class GrabEventCfg : SkillEventCfg
{
    public GrabCxt grabCxt = new GrabCxt();


    public override enSkillEventType Type { get { return enSkillEventType.grab; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        
        switch (col)
        {
            case 0: if (h(ref r, "抓取编辑器", COL_WIDTH * 4)) onTip("抓取编辑器，编辑被抓取者各个阶段的位置移动和动作"); return false;
            
            default: return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
    
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 4;
                    if (GUI.Button(r,"打开"))
                    {
                        RoleModel model = tran == null ? null : tran.GetComponent<RoleModel>();
                        Role role = model == null ? null : model.Parent;

                        EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.GRAB_EDITOR, role, grabCxt);
                    }
                   
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif
    public override void CopyFrom(SkillEventCfg cfg)
    {
        base.CopyFrom(cfg);
        if (cfg == null) return;
        GrabEventCfg c = cfg as GrabEventCfg;
        if (c == null)
        {
            Debuger.LogError("GrabEventCfg 复制的时候类型解析出错");
            return;
        }

        this.grabCxt.CopyFrom(c.grabCxt);
        
    }
    public override void PreLoad()
    {
        grabCxt.PreLoad();
    }

    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        BeGrabCxt cxt = IdTypePool<BeGrabCxt>.Get();
        cxt.grabCxt = grabCxt;
        cxt.GrabRole = source;
        cxt.parentSkill = eventFrame.EventGroup.ParentSkill;

        //如果目标隐藏，强制显示出来不然位置计算会有问题
        if (!target.IsShow)
            target.Show(true);

        return target.RSM.GotoState(enRoleState.beHit, cxt,false,true);
    }

    
}