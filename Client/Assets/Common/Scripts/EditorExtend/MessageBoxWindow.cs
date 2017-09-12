using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;

#if UNITY_EDITOR
public class MessageBoxWindow : EditorWindow
{
    private string m_message;
    private bool m_canEdit;
    private Action<string, object> m_onOk;
    private Action<string, object> m_onCancel;
    private Action<string, object> m_onCloseBtn; //如果不提供Close回调，点右上角关闭按钮时，就调用Cancel回调
    private string m_okText;
    private string m_cancelText;
    private object m_context;
    private bool m_needCloseCallback;
    private Vector2 m_scrollPos = Vector2.zero;

    public static void ShowMessageBox(string message)
    {
        ShowAsMsgBox(message);
    }

    public static void ShowAsMsgBox(string message, string title, object context, Action<string, object> onOk = null, Action<string, object> onCancel = null, string okText = null, string cancelText = null, Action<string, object> onCloseBtn = null)
    {
        ShowWindow(false, message, title, context, onOk, okText, onCancel, cancelText, onCloseBtn);
    }

    public static void ShowAsMsgBox(string message, string title, Action<string, object> onOk = null, Action<string, object> onCancel = null, string okText = null, string cancelText = null, Action<string, object> onCloseBtn = null)
    {
        ShowWindow(false, message, title, null, onOk, okText, onCancel, cancelText, onCloseBtn);
    }

    public static void ShowAsMsgBox(string message, Action<string, object> onOk = null, Action<string, object> onCancel = null, string okText = null, string cancelText = null, Action<string, object> onCloseBtn = null)
    {
        ShowWindow(false, message, null, null, onOk, okText, onCancel, cancelText, onCloseBtn);
    }

    public static void ShowAsInputBox(string message, string title, object context, Action<string, object> onOk = null, Action<string, object> onCancel = null, string okText = null, string cancelText = null, Action<string, object> onCloseBtn = null)
    {
        ShowWindow(true, message, title, context, onOk, okText, onCancel, cancelText, onCloseBtn);
    }

    public static void ShowAsInputBox(string message, string title, Action<string, object> onOk = null, Action<string, object> onCancel = null, string okText = null, string cancelText = null, Action<string, object> onCloseBtn = null)
    {
        ShowWindow(true, message, title, null, onOk, okText, onCancel, cancelText, onCloseBtn);
    }

    public static void ShowAsInputBox(string message, Action<string, object> onOk = null, Action<string, object> onCancel = null, string okText = null, string cancelText = null, Action<string, object> onCloseBtn = null)
    {
        ShowWindow(true, message, null, null, onOk, okText, onCancel, cancelText, onCloseBtn);
    }

    public static void ShowWindow(bool canEdit, string message, string title = null, object context = null, Action<string, object> onOk = null, string okText = null, Action<string, object> onCancel = null, string cancelText = null, Action<string, object> onCloseBtn = null)
    {
        MessageBoxWindow instance = (MessageBoxWindow)EditorWindow.GetWindow(typeof(MessageBoxWindow), true);//很遗憾，窗口关闭的时候instance就会为null

        instance.minSize = new Vector2(300, 150);
        instance.titleContent = new GUIContent(title == null ? (canEdit ? "请输入" : "提示") : title);
        instance.m_message = message;
        instance.m_canEdit = canEdit;
        instance.m_onOk = onOk;
        instance.m_onCancel = onCancel;
        instance.m_onCloseBtn = onCloseBtn;
        instance.m_okText = okText;
        instance.m_cancelText = cancelText;
        instance.m_context = context;
        instance.m_needCloseCallback = true;
    }
    
#region 监听
    public void Awake()
    {
        

    }

    //更新
    void Update()
    {
        
    }

    //显示和focus时初始化下
    void OnFocus()
    {
        
    }

    void OnHierarchyChange()
    {
        //Debuger.Log("当Hierarchy视图中的任何对象发生改变时调用一次");

    }

    void OnProjectChange()
    {
        //Debuger.Log("当Project视图中的资源发生改变时调用一次");
    }

    void OnInspectorUpdate()
    {
        //Debuger.Log("窗口面板的更新");
        //这里开启窗口的重绘，不然窗口信息不会刷新
        this.Repaint();
    }

    void OnSelectionChange()
    {
        //当窗口出去开启状态，并且在Hierarchy视图中选择某游戏对象时调用
        foreach (Transform t in Selection.transforms)
        {
            //有可能是多选，这里开启一个循环打印选中游戏对象的名称
           // Debuger.Log("OnSelectionChange" + t.name);
        }
    }

    void OnDestroy()
    {
        //Debuger.Log("当窗口关闭时调用");
        if (m_needCloseCallback)
        {
            if (m_onCloseBtn != null)
                m_onCloseBtn(m_message, m_context);
            else if (m_onCancel != null)
                m_onCancel(m_message, m_context);
        }
    }

#endregion
    void OnLostFocus()
    {
        Close();
        //Debuger.Log("当窗口丢失焦点时调用一次");
    }
    
    //绘制窗口时调用
    void OnGUI()
    {
        using (var scrollView = new AutoBeginScrollView(m_scrollPos))
        {
            m_scrollPos = scrollView.Scroll;

            if (m_canEdit)
            {
                m_message = EditorGUILayout.TextArea(m_message, EditorUtil.TextAreaWordWrap, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
            else
            {
                EditorGUILayout.LabelField(m_message, EditorUtil.LabelWordWrap, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
        }

        GUILayout.Space(5);

        if (m_onOk != null && m_onCancel != null)
        {
            using (new AutoBeginHorizontal())
            {
                if (GUILayout.Button(string.IsNullOrEmpty(m_okText) ? "确定" : m_okText, GUILayout.Height(24)))
                {
                    m_needCloseCallback = false;
                    Close();
                    m_onOk(m_message, m_context);
                }

                GUILayout.Space(5);

                if (GUILayout.Button(string.IsNullOrEmpty(m_cancelText) ? "取消" : m_cancelText, GUILayout.Height(24)))
                {
                    m_needCloseCallback = false;
                    Close();
                    m_onCancel(m_message, m_context);
                }
            }
        }
        else if (m_onOk != null)
        {
            if (GUILayout.Button(string.IsNullOrEmpty(m_okText) ? "确定" : m_okText, GUILayout.Height(24)))
            {
                m_needCloseCallback = false;
                Close();
                m_onOk(m_message, m_context);
            }
        }
    }

}
#endif