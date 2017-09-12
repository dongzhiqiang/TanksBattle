using UnityEngine;
using System.Collections;
using UnityEditor;
using Xft;

[CustomEditor(typeof(XWeaponTrail))]
[CanEditMultipleObjects]
public class XWeaponTrailEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //EditorGUILayout.BeginHorizontal();

        //EditorGUILayout.BeginVertical();
        //EditorGUILayout.LabelField("Version: " + XWeaponTrail.Version);
        //EditorGUILayout.LabelField("Author: Shallway");
        //EditorGUILayout.LabelField("Email: shallwaycn@gmail.com");
        //EditorGUILayout.LabelField("Web: http://shallway.net");
        //EditorGUILayout.EndVertical();

        //EditorGUILayout.BeginVertical();
        //if (GUILayout.Button("Forum", GUILayout.Width(120), GUILayout.Height(32)))
        //{
        //    Application.OpenURL("http://shallway.net/xffect/forum/categories/x-weapontrail");
        //}

        //if (GUILayout.Button("Get more effects!", GUILayout.Width(120), GUILayout.Height(32)))
        //{
        //    Application.OpenURL("http://shallway.net/xffect/doku.php");
        //}
        //EditorGUILayout.EndVertical();

        //EditorGUILayout.EndHorizontal();

        //GUILayout.Space(10);
        //DrawDefaultInspector();

        XWeaponTrail t = this.target as XWeaponTrail;
        EditorGUI.BeginChangeCheck();
        t.MyMaterial = (Material)EditorGUILayout.ObjectField("材质", t.MyMaterial, typeof(Material), false);
        t.MyColor = EditorGUILayout.ColorField("颜色", t.MyColor);
        GUILayout.BeginHorizontal();
        {
            if (t.PointStart != null && t.PointEnd !=null)
            {
                float width = (t.PointStart.position - t.PointEnd.position).magnitude;
                EditorGUILayout.LabelField("宽度:" + width + "  ");
            }
            
            float old = UnityEditor.EditorGUIUtility.labelWidth;
            UnityEditor.EditorGUIUtility.labelWidth = 50;
            
            {
                t.PointStart = (Transform)EditorGUILayout.ObjectField("起点", t.PointStart, typeof(Transform), true);
            }
            UnityEditor.EditorGUIUtility.labelWidth= old;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            if (t.PointEnd != null && t.PointEnd != null)
            {
                float width = (t.PointStart.position - t.PointEnd.position).magnitude;
                EditorGUILayout.LabelField(" ");
            }
            
            float old = UnityEditor.EditorGUIUtility.labelWidth;
            UnityEditor.EditorGUIUtility.labelWidth = 50;
            {
                t.PointEnd = (Transform)EditorGUILayout.ObjectField("终点", t.PointEnd, typeof(Transform), true);
            }
            UnityEditor.EditorGUIUtility.labelWidth = old;
        }
        GUILayout.EndHorizontal();

        t.MaxFrame = EditorGUILayout.IntField("采样数(超过这个数则删除尾部，即越大拖尾越长)", t.MaxFrame);
        //t.Granularity = EditorGUILayout.IntField("面数(越大越平滑，一般改这个)", t.Granularity);
        t.Fps = EditorGUILayout.FloatField("帧率(越大越平滑)", t.Fps);
        t.m_destroyDelay = EditorGUILayout.FloatField(new GUIContent("延迟销毁时间",
@"默认0，不延迟
大于0，而且在用对象池的情况下(比如绑定到角色动作)将停留在原地渐变消失"), t.m_destroyDelay);

        if( GUILayout.Button("添加宽度渐变"))
        {
            t.m_fades.Add(new XWeaponTrail.WidthFade());
        }

        XWeaponTrail.WidthFade removeFade=null;
        for (int i=0;i< t.m_fades.Count;++i)
        {
            if(DrawFade(t.m_fades[i], i))
                removeFade = t.m_fades[i];
        }
        if (removeFade!=null)
        {
            t.m_fades.Remove(removeFade);
        }

        if (EditorGUI.EndChangeCheck())
        {
            UnityEditor.EditorUtility.SetDirty(t);
        }
    }

    bool DrawFade(XWeaponTrail.WidthFade f, int idx)
    {
        bool isShow;
        bool isClick;
        EditorUtil.DrawHeaderBtn("WidthFade" + idx, "删除", out isShow, out isClick);
        if (isClick)
            return true;

        if (!isShow)
            return false;


        GUILayout.BeginHorizontal();
        GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
        float old = UnityEditor.EditorGUIUtility.labelWidth;
        UnityEditor.EditorGUIUtility.labelWidth = old - 10;

        {
            UnityEditor.EditorGUI.indentLevel += 1;
            {
                f.delay = EditorGUILayout.FloatField("延迟", f.delay);
                f.beginScale = EditorGUILayout.FloatField("开始", f.beginScale);
                f.endScale = EditorGUILayout.FloatField("结束", f.endScale);
                f.duration = EditorGUILayout.FloatField("变化时间", f.duration);
                f.curve = UnityEditor.EditorGUILayout.CurveField("曲线", f.curve, GUILayout.Width(175), GUILayout.Height(30f));
            }
            UnityEditor.EditorGUI.indentLevel -=1;
        }
        GUILayout.Space(3f);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.Space(3f);
		GUILayout.EndHorizontal();

		GUILayout.Space(3f);
        UnityEditor.EditorGUIUtility.labelWidth = old;
        return false;
    }
}

