
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class RegexToolWindow : EditorWindow
{
    [MenuItem("Tool/小工具/正则测试工具")]
    public static void ShowWnd()
    {
        RegexToolWindow wnd = (RegexToolWindow)EditorWindow.GetWindow(typeof(RegexToolWindow));
        wnd.title = "正则测试工具";
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

    string str = "";
    string regex= "";
    string result = "";
    string help =
@"元字符：用来匹配一个字符
    .       匹配除换行符以外的任意字符
    \b      匹配单词的开始或结束
    \d      匹配数字
    \s      匹配任意的空白符
    \w      匹配字母或数字或下划线或汉字
    ^       匹配字符串的开始
    $       匹配字符串的结束
转移字符：如果查找元字符本身，那么加\来替换，比如\*,\.,\\
限定符:又叫重复描述字符，表示一个字符要出现的次数
    *       重复零次或更多次
    +       重复一次或更多次
    ?       重复零次或一次
    {n}     重复n次
    {n,}    重复n次或更多次
    {n,m}   重复n到m次
    |       分支匹配，比如0~255写成2[0-4]\\d|25[0-5]|[01]?\d\d?
    ()      括号内的正则为一个分组，分组可以重复使用，比如go go写成(go) \1
贪婪匹配，正则通常是匹配尽量多的字符，在限定符后面加?符号可以变成懒惰匹配
";
    //绘制窗口时调用
    void OnGUI()
    {
        using (new AutoFontSize(13, EditorStyles.helpBox))
        {
            float height = EditorStyles.helpBox.CalcHeight(new GUIContent(help), Screen.width);
            EditorGUILayout.LabelField(help, EditorStyles.helpBox, GUILayout.Height(height));
        }
        
        using (new AutoBeginHorizontal())
        {
            EditorGUILayout.PrefixLabel("字符串:");
            str = EditorGUILayout.TextArea(str,EditorStyleEx.TextAreaWordWrap);
        }
        regex = EditorGUILayout.TextField("正则:", regex);
        if (GUILayout.Button("匹配"))
        {
            Regex reg = new Regex(regex, RegexOptions.RightToLeft);
            var matchs = reg.Matches(str);
            List<string> tem = new List<string>();
            foreach (var m in matchs)
            {
                tem.Add(((Match)m).ToString());
            }
            result = string.Join("\n", tem.ToArray());
            //var match = reg.Match(str);
            //if (match.Success)
            //{
            //    result =  match.Groups[1].ToString();
            //}

        }
        //\{\w*:\w*\}
        //if (GUILayout.Button("替换"))
        //{
        //    Regex reg = new Regex(regex);
        //    int i = 0;

        //    result =reg.Replace(str, (Match m) =>
        //    {
        //        Debuger.Log(m.ToString());
        //        return "替换" + (++i);
        //    });
        //}
        result = EditorGUILayout.TextArea( result);
    }

    
    
}
