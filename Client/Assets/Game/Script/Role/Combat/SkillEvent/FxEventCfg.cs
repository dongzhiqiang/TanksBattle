#region Header
/**
 * 名称：特效事件
 
 * 日期：2015.1.7
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


public class FxEventCfg : SkillEventCfg
{
    
    public FxCreateCfg createCfg = new FxCreateCfg();
    public string flyer = "";
    public bool useHapponPos = false;
    //public bool hitAdjust =true;


    public override enSkillEventType Type { get { return enSkillEventType.fx; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        
        switch (col)
        {
            case 0: if (h(ref r, "特效编辑器", COL_WIDTH * 8)) onTip("特效出生编辑器：用于编辑特效的出生位置、方向、数量等参数"); return false;
            case 1: if (h(ref r, "飞出物编辑器", COL_WIDTH * 8)) onTip("飞出物可以进行弹道控制和事件计算"); return false;
            case 2: if (h(ref r, "使用发生点", COL_WIDTH * 3)) onTip("使用事件组发生点做为碰撞点"); return false;
            default: return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
    
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH *6;
                    createCfg.name = EditorGUI.TextField(r, createCfg.name);
                    r.x += r.width;

                    r.width = COL_WIDTH * 2;
                    if (GUI.Button(r,"打开"))
                    {
                        EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.FX_EDITOR, string.Format("事件{0}的特效", id), createCfg, tran); 
                    }
                   
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 6;
                    flyer =EditorGUI.TextField(r, flyer);
                    r.x += r.width;

                    r.width = COL_WIDTH * 2;
                    if (GUI.Button(r, "打开"))
                    {
                        FlyerCfg flyerCfg = string.IsNullOrEmpty(flyer) ? null : FlyerCfg.Get(flyer);
                        Action<string> onSel = (string flyerId) => flyer = flyerId;
                        EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.FLYER_EDITOR, flyerCfg, onSel);
                    }
                    r.x += r.width;
                }; return false;
            case 2:
                {
                    r.width = COL_WIDTH * 3;
                    useHapponPos = EditorGUI.Toggle(r, useHapponPos);
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
        FxEventCfg fxCfg = cfg as FxEventCfg;
        if (fxCfg==null)
        {
            Debuger.LogError("FxEventCfg 复制的时候类型解析出错");
            return;
        }

        this.createCfg.CopyFrom(fxCfg.createCfg);
        
    }
    public override void PreLoad()
    {
        createCfg.PreLoad();
        if (!string.IsNullOrEmpty(flyer))
            FlyerCfg.PreLoad(flyer);
    }

    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        //为空
        if (string.IsNullOrEmpty(createCfg.name))
        {
            Debuger.LogError("特效事件，特效没有填:{0}", source.Cfg.id);
            return false;
        }

        //计算碰撞点，如果需要的话
        Vector3 hitPos =Vector3.zero;
        if (createCfg.posType == enFxCreatePos.hitPos)
        {
            if (useHapponPos)
                hitPos = eventFrame.EventGroup.HappenPos;
            else
                hitPos = GetHitPos(true, source, target, eventFrame);
        }
            

        //计算元素属性
        enElement elementType = ElementCfg.GetRoleElement(source);

        createCfg.Create(source, target, hitPos, elementType, OnLoad, new object[] { source, target , eventFrame.EventGroup.ParentSkill});
        return true;
    }

    void OnLoad(GameObject go, object param)
    {
        object[] pp = (object[])param;
        Role source = (Role)pp[0];
        Role target = (Role)pp[1];
        Skill parentSkill = (Skill)pp[2];
        Flyer.Add(go, flyer, source, target, parentSkill);

        //如果飞出物上没有任何销毁的脚本，那么提示下
        if (!FxDestroy.HasDelay(go))
        {
            Debuger.LogError("特效上没有绑销毁脚本，特效事件的特效也没有指定销毁时间.特效名:{0}", go.name);
        }
    }
}