
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;
using System.Collections;
using System.Collections.Generic;



public abstract class MultiWindow 
{
    public MultiAdapterWindow adapterWindow;
    public int multiId;
    public static T Create<T,T2>(string title,float sizeX,float sizeY,bool wantMouseMove=false)
        where T2: MultiAdapterWindow
        where T : MultiWindow,new()

    {
        T2 wnd= (T2)EditorWindow.GetWindow(typeof(T2));//很遗憾，窗口关闭的时候instance就会为null
        wnd.minSize = new Vector2(sizeX, sizeY);
        wnd.titleContent = new GUIContent(title);
        wnd.autoRepaintOnSceneChange = true;
        wnd.wantsMouseMove = wantMouseMove;
        T multiWnd = new T();

        wnd.MultiWindow = multiWnd;
        return multiWnd;
    }

    protected string GetPrefs(string key)
    {
        return string.Format("{0}_{1}_{2}", string.IsNullOrEmpty(adapterWindow.titleContent.text)?this.GetType().Name: adapterWindow.titleContent.text, key, multiId);
    }

    //更新
    public virtual void Update() { }

    public virtual void OnEnable() { }

    public virtual void OnDisable() { }

    public virtual void OnFocus() {  }

    public virtual void OnLostFocus() { }

    public virtual void OnHierarchyChange() { }

    public virtual void OnProjectChange() { }

    public virtual void OnInspectorUpdate() { }

    public virtual void OnDestroy() { }

    public virtual void OnSelectionChange() { }

    //绘制窗口时调用
    public virtual void OnGUI() { }
    
    
}