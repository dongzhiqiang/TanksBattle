using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(MaterialFx), false)]
public class MaterialFxEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        MaterialFx matFx = target as MaterialFx;
        EditorGUILayout.LabelField(matFx.IsPlaying?"播放中":"已结束");

        EditorGUI.BeginChangeCheck();
        matFx.m_type = (MaterialFx.enType)EditorGUILayout.Popup("类型", (int)matFx.m_type, MaterialFx.TypeName);

        if (matFx.m_type != MaterialFx.enType.add)
            matFx.m_matIndex = EditorGUILayout.IntField("第几个材质", matFx.m_matIndex);

        if (matFx.m_type != MaterialFx.enType.modify)
            matFx.m_mat = (Material)EditorGUILayout.ObjectField("材质", matFx.m_mat, typeof(Material), false);
        else if (matFx.m_mat!=null)
            matFx.m_mat =null;

        if (matFx.m_type == MaterialFx.enType.replace)
            matFx.m_useOldTex = EditorGUILayout.Toggle("使用替换的贴图", matFx.m_useOldTex);

        matFx.m_duration = EditorGUILayout.FloatField("结束时间", matFx.m_duration);

        MaterialAni removeMatAni = null;
        for(int i=0;i<matFx.m_anis.Count;++i)
        {
            if(DrawAni(matFx.m_anis[i],i))
                removeMatAni = matFx.m_anis[i];
        }
        if (removeMatAni != null)
            matFx.m_anis.Remove(removeMatAni);

        int idx = UnityEditor.EditorGUILayout.Popup("添加渐变", -1, MaterialAni.TypeName);
        if (idx != -1)
        {
            MaterialAni matAni = new MaterialAni();
            matAni.m_type = (MaterialAni.enType)idx;
            matFx.m_anis.Add(matAni);
        }

        if (EditorGUI.EndChangeCheck())
        {
            //Debuger.Log("修改");
            EditorUtil.SetDirty(matFx);
        }
    }

    bool DrawAni(MaterialAni matAni, int idx)
    {
        bool isShow;
        bool isClick;
        EditorUtil.DrawHeaderBtn( "渐变" + MaterialAni.TypeName[(int)matAni.m_type]+" "+idx, "删除", out isShow, out isClick);
        if (isClick)
            return true;

        if (!isShow)
            return false;

        
        using (new AutoContent())
        {
            using (new AutoEditorIndentLevel())
            {
                matAni.m_type = (MaterialAni.enType)EditorGUILayout.Popup("类型", (int)matAni.m_type, MaterialAni.TypeName);
                matAni.m_propertyName = EditorGUILayout.TextField("属性名(可不填)", matAni.m_propertyName);
                if (matAni.m_type == MaterialAni.enType.Color)
                {
                    matAni.m_beginColor = EditorGUILayout.ColorField("开始", matAni.m_beginColor);
                    matAni.m_endColor = EditorGUILayout.ColorField("结束", matAni.m_endColor);
                }
                else if (matAni.m_type == MaterialAni.enType.Float)
                {
                    matAni.m_beginFloat = EditorGUILayout.FloatField("开始", matAni.m_beginFloat);
                    matAni.m_endFloat = EditorGUILayout.FloatField("结束", matAni.m_endFloat);
                }
                else if (matAni.m_type == MaterialAni.enType.ScrollingUV)
                {
                    matAni.m_uvSpeed = EditorGUILayout.Vector2Field("位移", matAni.m_uvSpeed);
                }
                else if (matAni.m_type == MaterialAni.enType.TileTexture)
                {
                    matAni.m_onceDuration = EditorGUILayout.FloatField("间隔", matAni.m_onceDuration);
                    matAni.m_rows = EditorGUILayout.IntField("行数", matAni.m_rows);
                    matAni.m_columns = EditorGUILayout.IntField("列数", matAni.m_columns);
                }
                else
                {
                    Debuger.LogError("未知的类型{0}", matAni.m_type);
                }

                if (matAni.m_type == MaterialAni.enType.Color || matAni.m_type == MaterialAni.enType.Float)
                {
                    matAni.m_beginDuration = EditorGUILayout.FloatField("开始渐变时间", matAni.m_beginDuration);
                    matAni.m_beginCurve = UnityEditor.EditorGUILayout.CurveField("开始曲线", matAni.m_beginCurve, GUILayout.Width(175), GUILayout.Height(30f));
                    matAni.m_endDuration = EditorGUILayout.FloatField("结束渐变时间", matAni.m_endDuration);
                    matAni.m_endCurve = UnityEditor.EditorGUILayout.CurveField("结束曲线", matAni.m_endCurve, GUILayout.Width(175), GUILayout.Height(30f));
                }
            }
            
        }

        

        return false;
    }
}

