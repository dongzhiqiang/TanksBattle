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


public class SoundEventCfg : SkillEventCfg
{
    public int soundId= 0;
    


    public override enSkillEventType Type { get { return enSkillEventType.sound; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        
        switch (col)
        {
            case 0: if (h(ref r, "音效id", COL_WIDTH * 3)) onTip("音效id，见音效表"); return false;
            default: return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 3;
                    soundId = EditorGUI.IntField(r, soundId);
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif
   
    public override void PreLoad()
    {
        if (soundId > 0)
            SoundMgr.instance.PreLoad(soundId);
    }

    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {

        return SoundMgr.instance.Play3DSound(soundId,source.transform)!= null ;
    }


}