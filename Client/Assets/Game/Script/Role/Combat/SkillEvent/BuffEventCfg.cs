#region Header
/**
 * 名称：帧事件
 
 * 日期：2015.9.28
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;

public class BuffParam
{
    public int id = 0;//状态id，见状态表
    public bool end = false;//技能结束的时候结束
    public float frame = -3;//持续时间，-3的话则用状态表里的时间
}

//状态
public class BuffEventCfg : SkillEventCfg
{
    static GUIStyle headStyle;
    public bool self = false;//是给自己加状态，还是给target加状态
    public List<BuffParam> buffParams = new List<BuffParam>();

    public override enSkillEventType Type { get { return enSkillEventType.buff; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r,  SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        if (col == 0)
        {
            if (h(ref r, "自己", COL_WIDTH * 2)) onTip("是给自己加状态，还是给target加状态");
        }
        else if (col == 1)
        {
            if (h(ref r, "加", COL_WIDTH * 1)) onTip("添加状态");
        }
        else
        {
            if (h(ref r, "状态" + col, COL_WIDTH * 7)) onTip(
  @"
参数1，状态id
参数2，持续帧数，-3则用配置表配置的
参数3，是否技能结束的时候结束
");
        }

        return col >= buffParams.Count+1;

    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        if (col == 0)
        {
            r.width = COL_WIDTH * 2;
            self = EditorGUI.Toggle(r, GUIContent.none, self);
            r.x += r.width;
        }
        else if (col == 1)
        {
            r.width = COL_WIDTH * 1;
            if (headStyle == null) headStyle = "OL title";
            if (GUI.Button(r, EditorGUIUtility.IconContent("Toolbar Plus More"), headStyle))
            {
                buffParams.Add(new BuffParam());
            }
            r.x += r.width;
        }
        else
        {
            BuffParam buffParam = buffParams[col - 2];

            //状态id
            r.width = COL_WIDTH * 2;
            buffParam.id = EditorGUI.IntField(r, GUIContent.none, buffParam.id);
            r.x += r.width;

            //持续帧数，-3则用配置表配置的
            r.width = COL_WIDTH * 2;
            buffParam.frame = EditorGUI.FloatField(r, GUIContent.none, buffParam.frame);
            r.x += r.width;

            //技能结束结束
            r.width = COL_WIDTH * 1;
            buffParam.end = EditorGUI.Toggle(r, GUIContent.none, buffParam.end);
            r.x += r.width;

            //删除
            r.width = COL_WIDTH * 1;
            if (GUI.Button(r, EditorGUIUtility.IconContent("TreeEditor.Trash"), headStyle))
            {
                buffParams.Remove(buffParam);
            }
            r.x += r.width;

            r.x += COL_WIDTH * 1;
        }

        return col >= buffParams.Count+1;
     
    }
    
#endif
    public override void CopyFrom(SkillEventCfg cfg)
    {
        base.CopyFrom(cfg);
        if (cfg == null) return;
        this.buffParams.Clear();
        
        BuffEventCfg buffCfg = cfg as BuffEventCfg;
        if (buffCfg == null)
        {
            Debuger.LogError("BuffEventCfg 复制的时候类型解析出错");
            return;
        }

        for (int i = 0; i < buffCfg.buffParams.Count; ++i)
        {
            BuffParam param = new BuffParam();
            Util.Copy(param, this, BindingFlags.Public | BindingFlags.Instance);
            buffParams.Add(param);
        }
    }
    public override void PreLoad()
    {
        BuffParam buffParam;
        for (int i = 0; i < buffParams.Count; ++i)
        {
            buffParam = buffParams[i];
            if (buffParam.id == 0)
                continue;

            BuffCfg.ProLoad(buffParam.id);
        }
    }

    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        
        Role r= self?source:target;
        int id = r.Id;

        BuffParam buffParam;
        Buff buff;
        for (int i = 0; i < buffParams.Count; ++i)
        {
            buffParam = buffParams[i];
            if (buffParam.id == 0)
                continue;

            buff = r.BuffPart.AddBuff(buffParam.id, source);
            if (buff == null)
                continue;

            if (r.IsDestroy(id) && r.State!= Role.enState.alive)//可能会死亡，这里要判断下
                break;

            //结束时间
            buff.Time = buffParam.frame >= 0 ? buffParam.frame * Util.One_Frame : buffParam.frame;

            //技能结束结束
            if (buffParam.end)
            {
                eventFrame.EventGroup.AddBindObj(r, buff, SkillEventGroupBindObject.enType.buff);
            }
        }
        
        return true;
    }
}