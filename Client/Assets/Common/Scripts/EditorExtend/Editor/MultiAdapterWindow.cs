
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;
using System.Collections;
using System.Collections.Generic;



public class MultiAdapterWindow : EditorWindow
{
    MultiWindow multiWindow;
    public MultiWindow MultiWindow{
        get { return multiWindow;}
        set { 
            multiWindow = value;
            multiWindow.adapterWindow = this;
            multiWindow.OnEnable();
            multiWindow.OnFocus();
        }
    }

    bool hasPlay=false;
    
   
    //更新
    void Update()
    {
        //游戏退出运行的时候不希望编辑器上的东西还在
        if(!hasPlay&& Application.isPlaying)
            hasPlay = true;
        if (hasPlay && !Application.isPlaying)
        {
            hasPlay = false;
            multiWindow = null;
        }
            

        if (multiWindow!=null)
            multiWindow.Update();
        if (autoRepaintOnSceneChange)
            this.Repaint();
    }

    void OnEnable(){
        if (multiWindow!=null)
            multiWindow.OnEnable();
    }

    void OnDisable()
    {
        if (multiWindow != null) multiWindow.OnDisable();
    }
    void OnFocus()
    {
        if (multiWindow != null) multiWindow.OnFocus();
    }

    void OnLostFocus()
    {
        if (multiWindow != null) multiWindow.OnLostFocus();
    }

    void OnHierarchyChange()
    {
        if (multiWindow != null) multiWindow.OnHierarchyChange();
    }

    void OnProjectChange()
    {
        if (multiWindow != null) multiWindow.OnProjectChange();
    }

    void OnInspectorUpdate()
    {
        if (multiWindow != null) multiWindow.OnInspectorUpdate();
    }

    void OnDestroy()
    {
        if (multiWindow != null) multiWindow.OnDestroy();
    }
    

    void OnSelectionChange()
    {
        if (multiWindow != null) multiWindow.OnSelectionChange();
    }

   
    //绘制窗口时调用
    void OnGUI()
    {
        if (multiWindow!=null)multiWindow.OnGUI();
    }


}