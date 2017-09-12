using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BigQteEventCfg : SkillEventCfg
{
    public static string[] SkillIdxName = new string[] { "第一列技能", "第二列技能" };

    public int skillsIdx = 0;
    public int sourceHp = 100;
    public int targetHp = 100;

    public override enSkillEventType Type { get { return enSkillEventType.bigQte; } }

#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "释放技能", COL_WIDTH * 3)) onTip("对不同敌人释放不同技能"); return false;
            case 1: if (h(ref r, "释放血量限制", COL_WIDTH * 4)) onTip("低于此百分比才可释放"); return false;
            case 2: if (h(ref r, "受击血量限制", COL_WIDTH * 4)) onTip("低于此百分比才可释放"); return false;
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
                    int newIdx = EditorGUI.Popup(r, skillsIdx, SkillIdxName);
                    if (newIdx != skillsIdx)
                        skillsIdx = newIdx;
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 4;
                    sourceHp = EditorGUI.IntField(r, GUIContent.none, sourceHp);
                    r.x += r.width;

                }; return false;
            case 2:
                {
                    r.width = COL_WIDTH * 4;
                    targetHp = EditorGUI.IntField(r, GUIContent.none, targetHp);
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif

    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {

        //if (target.GetFlag())
        QTECfg2 qteCfg = QTECfg2.Get(source.Cfg.id, target.Cfg.id);
        if (qteCfg == null)
            return false;


        //检错下
        if (qteCfg.skill == null)
        {
            Debuger.LogError("qte事件，填了qte但是没有填技能,qteId:{0}", qteCfg.id);
            return false;
        }

        int sHp = (int)(source.GetInt(enProp.hp) / source.GetFloat(enProp.hpMax)) * 100;
        int tHp = (int)(target.GetInt(enProp.hp) / target.GetFloat(enProp.hpMax)) * 100;
        if (sHp > qteCfg.sourceHp || tHp > qteCfg.targetHp)
            return false;

        //如果有多个技能，那么随机选一个
        QTESkillCfg qteSkillCfg = qteCfg.skill;
        //检错下
        if (string.IsNullOrEmpty(qteSkillCfg.skillId))
        {
            Debuger.LogError("qte事件，参数出错，技能id为空,qteId:{0}", qteCfg.id);
            return false;
        }
        BigQteCfg cfg = BigQteTableCfg.GetCfg(qteSkillCfg.skillId);
        if (cfg == null)
        {
            Debuger.LogError("bigQteCfg表里找不到" + qteSkillCfg.skillId + "技能对应的文件");
            return false;
        }
        BigQte bigQte = BigQte.Get(cfg);
        BigQte.EventCfg = qteCfg;
        return source.CombatPart.Play(qteSkillCfg.skillId, target, false, true) == CombatPart.enPlaySkill.normal;
    }
}