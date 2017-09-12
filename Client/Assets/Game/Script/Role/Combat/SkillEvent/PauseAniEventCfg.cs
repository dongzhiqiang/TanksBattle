#region Header
/**
 * 名称：卡帧事件
 
 * 日期：2015.1.7
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif



public class PauseAniEventCfg : SkillEventCfg
{
    public int frame = 2;//卡多少帧


    public override enSkillEventType Type { get { return enSkillEventType.pause; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "持续帧数", COL_WIDTH * 3)) onTip("多少帧之后动作恢复播放"); return false;
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
                    frame = EditorGUI.IntField(r, GUIContent.none, frame);
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif

    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        if(frame <=0){
            Debuger.LogError("卡帧事件参数出错，持续帧数必须大于0");
        }

        //先获取武器的卡帧系数
        WeaponCfg weapon = source.FightWeapon;
        float weaponBehitRate = weapon == null ? 1 : weapon.pauseFrameRate;

        source.AniPart.AddPause(frame * Util.One_Frame * weaponBehitRate);
        return true;
    }
}