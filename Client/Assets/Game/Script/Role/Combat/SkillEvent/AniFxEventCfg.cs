#region Header
/**
 * 名称：动作特效事件
 
 * 日期：2015.1.22
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


public class AniFxEventCfg : SkillEventCfg
{
    
    public string aniFx = "";
    public string flyer = "";
    

    public override enSkillEventType Type { get { return enSkillEventType.aniFx; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        
        switch (col)
        {
            case 0: if (h(ref r, "特效", COL_WIDTH * 6)) onTip("动作上的特效名"); return false;
            case 1: if (h(ref r, "飞出物id", COL_WIDTH * 8)) onTip("飞出物可以进行弹道控制和事件计算"); return false;
            default: return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
    
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 6;
                    string[] fxNames = g.AniFxGroup==null?new string[0]:g.AniFxGroup.FxNames;
                    int idx =System.Array.IndexOf(fxNames ,aniFx);
                    int idxOld = EditorGUI.Popup(r,idx,fxNames);
                    if (idxOld != idx)
                    {
                        aniFx = fxNames[idxOld];
                    }
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 6;
                    flyer =EditorGUI.TextField(r, flyer);
                    r.x += r.width;

                    r.width = COL_WIDTH * 2;
                    if (GUI.Button(r, "编辑"))
                    {
                        FlyerCfg flyerCfg = string.IsNullOrEmpty(flyer) ? null : FlyerCfg.Get(flyer);
                        Action<string> onSel = (string flyerId) => flyer = flyerId;
                        EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.FLYER_EDITOR, flyerCfg, onSel);
                    }
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif
    
    public override void PreLoad()
    {
        if (!string.IsNullOrEmpty(flyer))
            FlyerCfg.PreLoad(flyer);
    }

    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        if (string.IsNullOrEmpty(aniFx))
        {
            Debuger.LogError("动作特效名为空");
            return false;
        }
        if (source != target)
        {
            Debuger.LogError("动作特效的作用对象只能设置为释放者本身");
            return false;
        }
        

        Skill s = eventFrame.Skill;
        if (s == null)
        {
            Debuger.LogError("动作特效事件找不到技能，必须是技能的动作特效事件才能找到动作上的特效");
            return false;
        }

        AniFxGroup fxGroup = s.AniFxGroup;
        if(fxGroup == null)
        {
            Debuger.LogError("{0}找不到动作特效,动作名:{1} 特效名:{2}", source.Cfg.id, s.Cfg.aniName, aniFx);
            return false;
        }

        if (!fxGroup.SetRuntimeCreateCallback(aniFx,OnLoad, new object[] { source,eventFrame.EventGroup.ParentSkill}))
        {
            Debuger.LogError("{0}找不到动作特效2,动作名:{1} 特效名:{2}", source.Cfg.id, s.Cfg.aniName, aniFx);
            return false;
        }

        return true;
    }

    void OnLoad(GameObject go, object param)
    {
        object[] pp = (object[])param;
        Role source = (Role)pp[0];
        Skill parentSkill = (Skill)pp[1];
        Flyer.Add(go, flyer, source, source, parentSkill);

    }
}