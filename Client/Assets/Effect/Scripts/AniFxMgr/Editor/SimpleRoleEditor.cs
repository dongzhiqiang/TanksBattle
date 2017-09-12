using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEditor.UI;


[CustomEditor(typeof(SimpleRole), false)]
public class SimpleRoleEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        SimpleRole r = target as SimpleRole;
        
        EditorGUI.BeginChangeCheck();
        r.m_showDebug = EditorGUILayout.Toggle("显示调试界面", r.m_showDebug);
        r.m_isEmpty = EditorGUILayout.Toggle("无模型角色", r.m_isEmpty);
        if(!r.m_isEmpty)
        {
            EditorGUILayout.Space();
            r.m_moveAniSpeed = EditorGUILayout.FloatField("跑步动作调整(防滑步)", r.m_moveAniSpeed);
            r.m_moveSpeed = EditorGUILayout.FloatField("跑步速度", r.m_moveSpeed);
            r.m_needFade = EditorGUILayout.Toggle("是否渐变", r.m_needFade);
            if (r.m_needFade)
                r.m_fade = EditorGUILayout.FloatField("所有被击渐变时间", r.m_fade);
            r.m_needResetPos = EditorGUILayout.Toggle("是否复位", r.m_needResetPos);
            EditorGUILayout.Space();
            r.m_behitDuration = EditorGUILayout.FloatField("被击时间", r.m_behitDuration);
            EditorGUILayout.Space();
            r.m_floatBehitStartSpeed = EditorGUILayout.FloatField("被击时的初速度", r.m_floatBehitStartSpeed);
            r.m_floatBehitAccelerated = EditorGUILayout.FloatField("被击时的加速度", r.m_floatBehitAccelerated);
            r.m_floatStartSpeed = EditorGUILayout.FloatField("浮空初速度", r.m_floatStartSpeed);
            r.m_floatAcceleratedUp = EditorGUILayout.FloatField("上升时的加速度", r.m_floatAcceleratedUp);
            r.m_floatAcceleratedDown = EditorGUILayout.FloatField("下落时的加速度", r.m_floatAcceleratedDown);
            r.m_floatSpeedUpLimit = EditorGUILayout.FloatField("速度上限", r.m_floatSpeedUpLimit);
            r.m_floatSpeeDownLimit = EditorGUILayout.FloatField("速度下限", r.m_floatSpeeDownLimit);
            EditorGUILayout.Space();
            r.m_flyStartSpeed = EditorGUILayout.FloatField("击飞初速度", r.m_flyStartSpeed);
            r.m_flyAcceleratedUp = EditorGUILayout.FloatField("上升时的加速度", r.m_flyAcceleratedUp);
            r.m_flyAcceleratedDown = EditorGUILayout.FloatField("下落时的加速度", r.m_flyAcceleratedDown);
            r.m_flySpeedUpLimit = EditorGUILayout.FloatField("速度上限", r.m_flySpeedUpLimit);
            r.m_flySpeeDownLimit = EditorGUILayout.FloatField("速度下限", r.m_flySpeeDownLimit);
            EditorGUILayout.Space();
            r.m_groundDuration = EditorGUILayout.FloatField("倒地时间", r.m_groundDuration);
            EditorGUILayout.Space();


            Animation ani = r.transform.Find("model").GetComponent<Animation>();
            if (ani == null)
            {
                EditorGUILayout.LabelField("找不到model的Animation，不能设置动作");
                return;
            }
            string[] aniNames = ani.GetNames();
            if (aniNames == null || aniNames.Length == 0)
            {
                EditorGUILayout.LabelField("Animation没有动作");
                return;
            }

            DrawAtk("Num4攻击键", r, r.m_num4Atk, ani, aniNames);
            DrawAtk("Num5攻击键", r, r.m_num5Atk, ani, aniNames);
            DrawAtk("Num6攻击键", r, r.m_num6Atk, ani, aniNames);
        }
        
        
        if (EditorGUI.EndChangeCheck())
        {
            //Debuger.Log("修改");
            EditorUtil.SetDirty(r);
        }
    }

    public void DrawAtk(string title,SimpleRole r, SimpleRole.AttackCxt c, Animation ani, string[] aniNames)
    {
        if (EditorUtil.DrawHeader(string.Format("{0}:{1}", title, c.aniName)))
        {
            using (new AutoContent())
            {
                int idxOld = Array.IndexOf(aniNames, c.aniName);
                if(idxOld == -1&& !string.IsNullOrEmpty(c.aniName))
                    c.aniName = "";

                int idx = UnityEditor.EditorGUILayout.Popup("动作名", idxOld, aniNames);
                if (idx != idxOld)
                    c.aniName = aniNames[idx];

                AnimationState aniSt = ani[c.aniName];
                if (aniSt == null)
                {
                    EditorGUILayout.LabelField("无动作");
                    return;
                }
                c.wrapMode = (WrapMode)EditorGUILayout.EnumPopup("循环模式", c.wrapMode);

                //不用clamp，强制切到ClampForever
                if (c.wrapMode == WrapMode.Clamp) c.wrapMode = WrapMode.ClampForever;

                //持续时间
                if (c.wrapMode == WrapMode.Loop || c.wrapMode == WrapMode.ClampForever || c.wrapMode == WrapMode.PingPong)
                {
                    c.duration = EditorGUILayout.FloatField("持续时间", c.duration);
                }
                else
                {
                    if (c.duration > 0) c.duration = 0;
                }

                //旋转
                c.canRotate = EditorGUILayout.Toggle("可旋转", c.canRotate);
                if (c.canRotate)
                {
                    c.rotateSpeed = EditorGUILayout.FloatField("旋转速度", c.rotateSpeed);
                }

                //移动
                c.canMove = EditorGUILayout.Toggle("可移动", c.canMove);
                if (c.canMove)
                {
                    c.moveSpeed = EditorGUILayout.FloatField("移动速度", c.moveSpeed);
                }
            }
        }
        
        
    }

}

