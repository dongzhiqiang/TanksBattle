
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;
using System.Collections;
using System.Collections.Generic;

public class EaseCurveWindow : EditorWindow
{
    [MenuItem("Tool/小工具/曲线参考")]
    public static void ShowWnd()
    {
        EaseCurveWindow wnd = EditorWindow.GetWindow<EaseCurveWindow>("曲线参考", true);
        wnd.minSize = new Vector2(600, 400.0f);
        wnd.autoRepaintOnSceneChange = true;
    }

    public class CurveInfo
    {
        public const int COUNT = 20;
        public const float UNIT = 1f / COUNT;
        public string name;
        public Rect rc;
        public Vector3[] pts = new Vector3[COUNT];

    }

    

    List<CurveInfo> curveInfs = new List<CurveInfo>();
    int lastWidth;
    int lastHeight;
    float border = 15;
    Color LineClr = new Color(1f, 1f, 1f, 1);//暗灰色
    float gridW;
    float gridH;

    void OnGUI()
    {

        if(Event.current.type == EventType.repaint&& (lastWidth!= Screen.width|| lastHeight!= Screen.height|| curveInfs.Count==0))
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            float width = Screen.width - border * 2;
            float height = Screen.height - border * 2-20;
            int row = Mathf.CeilToInt(Mathf.Sqrt((int)MathUtil.EaseType.max * height / width));
            int col = Mathf.FloorToInt((int)MathUtil.EaseType.max / row)+1;
            float itemWidth = width / col;
            float itemHeight = height / row;
            gridW = itemWidth / 2;
            gridH = itemHeight / 2;
            curveInfs.Clear();
            for (int i = 1; i < (int)MathUtil.EaseType.max; ++i)
            {
                int itemRow = (i-1) / col;
                int itemCol = (i-1) % col;
                CurveInfo info = new CurveInfo();
                curveInfs.Add(info);
                info.rc = new Rect(border + itemCol * itemWidth, border + itemRow * itemHeight, itemWidth, itemHeight);
                info.name = ((MathUtil.EaseType)i ).ToString();

                for(int j = 0; j < CurveInfo.COUNT; ++j)
                {
                    float h = 1-MathUtil.Curve((MathUtil.EaseType)i,0,1, j* CurveInfo.UNIT);

                    info.pts[j] = new Vector3(info.rc.x+ j * CurveInfo.UNIT* itemWidth, info.rc.y+h* itemHeight);
                }
            }
        }
        
      



        Color c = Handles.color;
        Handles.color = Color.green;
        foreach (var curveInfo in curveInfs)
        {
            //GUI.Box(curveInfo.rc, GUIContent.none, EditorUtil.BoxStyle(new Color(0.2f, 0.2f, 0.2f)));


            EditorGUI.LabelField(curveInfo.rc, curveInfo.name);

            Handles.DrawPolyLine(curveInfo.pts);

        }

        Handles.color = c;
    }
    
}