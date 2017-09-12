using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(CameraFx), false)]
public class CameraFxEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        
        CameraFx fx = target as CameraFx;
       
        EditorGUI.BeginChangeCheck();
        

        CameraAni removeAni = null;
        for (int i = 0; i < fx.m_anis.Count; ++i)
        {
            if (DrawAni(fx.m_anis[i], i))
                removeAni = fx.m_anis[i];
        }
        if (removeAni != null)
            fx.m_anis.Remove(removeAni);

        int idx = UnityEditor.EditorGUILayout.Popup("添加渐变", -1, CameraAni.TypeName);
        if (idx != -1)
        {
            CameraAni ani = new CameraAni();
            ani.m_type = (CameraAni.enType)idx;
            fx.m_anis.Add(ani);
        }
        if (EditorGUI.EndChangeCheck())
        {
            //Debuger.Log("修改");
            EditorUtil.SetDirty(fx);
        }
    }
    void OnEnable()
    {
        if (Application.isEditor && !EditorApplication.isPlaying)
        {
            EditorApplication.update = OnUpdate;
        }
    }
    void OnDisable()
    {
        if (Application.isEditor && !EditorApplication.isPlaying)
        {
            EditorApplication.update = OnUpdate;
        }
    }

    void OnUpdate()
    {
        //这里要重新绘制下相机
        //EditorGUIUtility.RenderGameViewCameras(new Rect(0,0,100,100),false,false);
        //SceneView.RepaintAll();
        Camera[] cs = Camera.allCameras;
        //Debuger.LogError("相机数量:{0}", cs.Length);
        foreach (Camera c in cs)
        {
            //c.RenderDontRestore();
            //c.RemoveAllCommandBuffers();
            //c.ResetAspect();
            //c.useOcclusionCulling = false;
            //c.useOcclusionCulling =true;
            EditorUtil.SetDirty(c);
        }
            
       
    }

    bool DrawAni(CameraAni ani, int idx)
    {
        bool isShow;
        bool isClick;
        EditorUtil.DrawHeaderBtn("渐变" + CameraAni.TypeName[(int)ani.m_type] + " " + idx, "删除", out isShow, out isClick);
        if (isClick)
            return true;

        if (!isShow)
            return false;


        using (new AutoContent())
        {
            using (new AutoEditorIndentLevel())
            {
                ani.m_type = (CameraAni.enType)EditorGUILayout.Popup("类型", (int)ani.m_type, CameraAni.TypeName);
                ani.m_fieldName = EditorGUILayout.TextField("字段名", ani.m_fieldName);
                if (ani.m_type == CameraAni.enType.Color)
                {
                    ani.m_beginColor = EditorGUILayout.ColorField("开始", ani.m_beginColor);
                    ani.m_endColor = EditorGUILayout.ColorField("结束", ani.m_endColor);
                }
                else if (ani.m_type == CameraAni.enType.Float)
                {
                    ani.m_beginFloat = EditorGUILayout.FloatField("开始", ani.m_beginFloat);
                    ani.m_endFloat = EditorGUILayout.FloatField("结束", ani.m_endFloat);
                }
                else
                {
                    Debuger.LogError("未知的类型{0}", ani.m_type);
                }

                ani.m_beginDuration = EditorGUILayout.FloatField("开始渐变时间", ani.m_beginDuration);
                ani.m_beginCurve = UnityEditor.EditorGUILayout.CurveField("开始曲线", ani.m_beginCurve, GUILayout.Width(175), GUILayout.Height(30f));
                ani.m_endDuration = EditorGUILayout.FloatField("结束渐变时间", ani.m_endDuration);
                ani.m_endCurve = UnityEditor.EditorGUILayout.CurveField("结束曲线", ani.m_endCurve, GUILayout.Width(175), GUILayout.Height(30f));
                
            }

        }

        return false;
    }

}

