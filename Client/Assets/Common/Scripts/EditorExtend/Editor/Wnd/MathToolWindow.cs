
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;
using System.Collections;
using System.Collections.Generic;



public class MathToolWindow : EditorWindow
{
    public static void ShowWnd()
    {
        MathToolWindow wnd = (MathToolWindow)EditorWindow.GetWindow(typeof(MathToolWindow));
        wnd.title = "数学工具";
        wnd.minSize = new Vector2(300.0f, 200.0f);
        wnd.autoRepaintOnSceneChange = true;
    }


    #region 各类事件监听
    public void Awake()
    {
        

    }

    //更新
    void Update()
    {
        
    }

    void OnEnable()
    {
        //Debuger.Log("当窗口enable时调用一次");
        //初始化
        //GameObject go = Selection.activeGameObject;
    }

    void OnDisable()
    {
        //Debuger.Log("当窗口disable时调用一次");
    }
    void OnFocus()
    {
        //Debuger.Log("当窗口获得焦点时调用一次");
    }

    void OnLostFocus()
    {
        //Debuger.Log("当窗口丢失焦点时调用一次");
    }

    void OnHierarchyChange()
    {
//        Debuger.Log("当Hierarchy视图中的任何对象发生改变时调用一次");
    }

    void OnProjectChange()
    {
  //      Debuger.Log("当Project视图中的资源发生改变时调用一次");
    }

    void OnInspectorUpdate()
    {
        //Debuger.Log("窗口面板的更新");
        //这里开启窗口的重绘，不然窗口信息不会刷新
        this.Repaint();
    }

    void OnDestroy()
    {
        //Debuger.Log("当窗口关闭时调用");
    }
    #endregion

    void OnSelectionChange()
    {
        //当窗口处于开启状态，并且在Hierarchy视图中选择某游戏对象时调用
        //foreach (Transform t in Selection.transforms)
        //{
        //   //有可能是多选，这里开启一个循环打印选中游戏对象的名称
        //    Debuger.Log("OnSelectionChange" + t.name);
        //}
       
    }

    
    //绘制窗口时调用
    void OnGUI()
    {
        DrawHexColorToDec();
    }

    string hexColorStr = "";
    string decColorStr = "";
    string hexColorStr2 = "";
    string decColorStr2 = "";
    public enum enOp
    {
       op1,
       op2,
       op3,
       op4,
       op5,
       op6,
       op7,
        op8,
    }

    public static string[] OpTypeName = new string[] { "+", "-", "*", "/", "%", "^", "|","逻辑与" };

    string a = "";
    string b = "";
    enOp op = enOp.op1;
    string c = "";


    //16进制的颜色值转成10进制，#ff0000 =>255,0,0
    void DrawHexColorToDec()
    {
        EditorGUILayout.LabelField("16进制颜色值转10进制，例如#ff0000 =>255,0,0");
        using (new AutoBeginHorizontal())
        {
            hexColorStr = EditorGUILayout.TextField(hexColorStr, GUILayout.Width(150));
            if (GUILayout.Button("=>", GUILayout.Width(50)))
            {
                int idx = hexColorStr.StartsWith("#") ? 1 : 0;
                if (hexColorStr.Length == idx + 6)
                {
                    decColorStr = string.Format("{0},{1},{2}", Convert.ToInt32(hexColorStr.Substring(idx, 2), 16), Convert.ToInt32(hexColorStr.Substring(idx + 2, 2), 16), Convert.ToInt32(hexColorStr.Substring(idx + 4, 2), 16));
                }
                else
                {
                    Debuger.LogError("格式出错");
                }
            }
            decColorStr = EditorGUILayout.TextField(decColorStr, GUILayout.Width(150));
        }

        EditorGUILayout.LabelField("10进制转16进制颜色值，例如 255,0,0=>#ff0000");
        using (new AutoBeginHorizontal())
        {
            decColorStr2 = EditorGUILayout.TextField(decColorStr2, GUILayout.Width(150));
            if (GUILayout.Button("=>", GUILayout.Width(50)))
            {
                string[] tem = decColorStr2.Split(',');
                if (tem.Length == 3)
                {
                    hexColorStr2 = string.Format("{0:X}{1:X}{2:X}", Convert.ToInt32(tem[0]), Convert.ToInt32(tem[1]), Convert.ToInt32(tem[2]));
                }
                else
                {
                    Debuger.LogError("格式出错");
                }
            }
            hexColorStr2 = EditorGUILayout.TextField(hexColorStr2, GUILayout.Width(150));
        }

        EditorGUILayout.LabelField("计算无符号数操作");
        using (new AutoBeginHorizontal())
        {
            a = EditorGUILayout.TextField(a, GUILayout.Width(100));
            op = (enOp)EditorGUILayout.Popup((int)op, OpTypeName);
            b = EditorGUILayout.TextField(b, GUILayout.Width(100));
            if (GUILayout.Button("=>", GUILayout.Width(50)))
            {
                uint ua, ub;
                if (uint.TryParse(a.EndsWith("u")?a.Substring(0,a.Length-1):a, out ua) && uint.TryParse(b.EndsWith("u") ? b.Substring(0, b.Length - 1) : b, out ub))
                {
                    uint num;
                    switch (op)
                    {
                        case enOp.op1: num = ua + ub; break;
                        case enOp.op2: num = ua - ub; break;
                        case enOp.op3: num = ua * ub; break;
                        case enOp.op4: num = ua / ub; break;
                        case enOp.op5: num = ua % ub; break;
                        case enOp.op6: num = ua ^ ub; break;
                        case enOp.op7: num = ua | ub; break;
                        case enOp.op8: num = ua & ub; break;
                        default: Debug.LogErrorFormat("未知操作符:{0}", op); c = "未知操作符:" + op; return;
                    }
                    c = num.ToString();
                }
                else
                {
                    c = "格式出错";
                    Debuger.LogError("格式出错");
                }
            }
            c = EditorGUILayout.TextField(c, GUILayout.Width(150));
        }
    }

}