
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;
using System.Collections;
using System.Collections.Generic;



public class EditorStylesWindow : EditorWindow
{
    bool bOpenArea = true;
    bool bOpenArea2 = true;
    bool bOpenArea3 = true;

    [MenuItem("Tool/小工具/编辑器控件样式参考")]
    public static void ShowWnd()
    {
        EditorStylesWindow wnd = EditorWindow.GetWindow<EditorStylesWindow>("编辑器控件样式参考", true);
        wnd.minSize = new Vector2(300.0f, 400.0f);
        wnd.autoRepaintOnSceneChange = true;
    }


    #region 各类事件监听
    public void Awake()
    {
    }

    //更新
    void Update()
    {
        this.Repaint();
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

    Vector2 pos = Vector2.zero;
    //绘制窗口时调用
    void OnGUI()
    {
        
        using (AutoBeginScrollView a = new AutoBeginScrollView(pos))
        {
            pos = a.Scroll;
            GUILayout.Label("boldLabel", EditorStyles.boldLabel);
            GUILayout.Label("centeredGreyMiniLabel", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label("colorField", EditorStyles.colorField);
            GUILayout.Label("foldout", EditorStyles.foldout);
            GUILayout.Label("foldoutPreDrop", EditorStyles.foldoutPreDrop);
            GUILayout.Label("helpBox", EditorStyles.helpBox);
            GUILayout.Label("inspectorDefaultMargins", EditorStyles.inspectorDefaultMargins);
            GUILayout.Label("inspectorFullWidthMargins", EditorStyles.inspectorFullWidthMargins);
            GUILayout.Label("label", EditorStyles.label);
            GUILayout.Label("largeLabel", EditorStyles.largeLabel);
            GUILayout.Label("layerMaskField", EditorStyles.layerMaskField);
            GUILayout.Label("miniBoldLabel", EditorStyles.miniBoldLabel);
            GUILayout.Label("miniButton", EditorStyles.miniButton);
            GUILayout.Label("miniButtonLeft", EditorStyles.miniButtonLeft);
            GUILayout.Label("miniButtonMid", EditorStyles.miniButtonMid);
            GUILayout.Label("miniButtonRight", EditorStyles.miniButtonRight);
            GUILayout.Label("miniLabel", EditorStyles.miniLabel);
            GUILayout.Label("miniTextField", EditorStyles.miniTextField);
            GUILayout.Label("numberField", EditorStyles.numberField);
            GUILayout.Label("objectField", EditorStyles.objectField);
            GUILayout.Label("objectFieldMiniThumb", EditorStyles.objectFieldMiniThumb);
            GUILayout.Label("objectFieldThumb", EditorStyles.objectFieldThumb);
            GUILayout.Label("popup", EditorStyles.popup);

            GUILayout.Label("radioButton", EditorStyles.radioButton);
            GUILayout.Label("textArea", EditorStyles.textArea);
            GUILayout.Label("textField", EditorStyles.textField);
            GUILayout.Label("toggle", EditorStyles.toggle);
            GUILayout.Label("toggleGroup", EditorStyles.toggleGroup);
            GUILayout.Label("toolbar", EditorStyles.toolbar);
            GUILayout.Label("toolbarButton", EditorStyles.toolbarButton);
            GUILayout.Label("toolbarDropDown", EditorStyles.toolbarDropDown);
            GUILayout.Label("toolbarPopup", EditorStyles.toolbarPopup);
            GUILayout.Label("toolbarTextField", EditorStyles.toolbarTextField);
            GUILayout.Label("whiteBoldLabel", EditorStyles.whiteBoldLabel);
            GUILayout.Label("whiteLabel", EditorStyles.whiteLabel);
            GUILayout.Label("whiteLargeLabel", EditorStyles.whiteLargeLabel);
            GUILayout.Label("whiteMiniLabel", EditorStyles.whiteMiniLabel);
            GUILayout.Label("wordWrappedLabel", EditorStyles.wordWrappedLabel);
            GUILayout.Label("wordWrappedMiniLabel", EditorStyles.wordWrappedMiniLabel);
            GUILayout.Label("AnimationEventBackground", "AnimationEventBackground");
            GUILayout.Label("Dopesheetkeyframe", "Dopesheetkeyframe");
            
            
            

            EditorGUILayoutEx.FadeArea fadeArea = EditorGUILayoutEx.instance.BeginFadeArea(bOpenArea, "区域块", "area", EditorStyleEx.PixelBoxStyle, EditorStyleEx.TopBoxHeaderStyle);
            bOpenArea = fadeArea.open;
            if (fadeArea.Show())
            {

                EditorGUILayoutEx.FadeArea topFadeArea = EditorGUILayoutEx.instance.BeginFadeArea(bOpenArea3, "", "areaid", EditorStyleEx.GraphBoxStyle);
                bOpenArea3 = topFadeArea.open;
                if (topFadeArea.Show())
                {
                    EditorGUILayout.HelpBox("help box", MessageType.Info);
                }
                EditorGUILayoutEx.instance.EndFadeArea();

            }
            EditorGUILayoutEx.instance.EndFadeArea();

            EditorGUILayout.TextField("", "可编辑区", EditorStyleEx.BoxHeaderStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

            using (new AutoBeginHorizontal())
            {
                GUILayout.Button("Draw", EditorStyleEx.GraphGizmoButtonStyle);

                if (GUILayout.Toggle(true, "Info", EditorStyleEx.GraphInfoButtonStyle))
                {

                }

                GUILayout.Button("Delete", EditorStyleEx.GraphDeleteButtonStyle);

                GUILayout.Button("DownArrowEdiStyle", EditorStyleEx.DownArrowStyle);
                GUILayout.Button("UpArrowEdiStyle", EditorStyleEx.UpArrowStyle);

            }
            GUILayout.Button("buttonEdiStyle", EditorStyleEx.ButtonStyle);

            GUILayout.Toggle(true, "toggleEdiStyle", EditorStyleEx.ToggleStyle);

            GUILayout.Label("labelEdiStyle", EditorStyleEx.LabelStyle);

            GUILayout.TextField("textfieldEdiStyle", EditorStyleEx.TextfieldStyle);

            GUILayout.TextArea("textareaEdiStyle", EditorStyleEx.TextareaStyle);


            GUILayout.Space(20);


            EditorGUILayoutEx.FadeArea fadeArea2 = EditorGUILayoutEx.instance.BeginFadeArea(bOpenArea2, "区域块", "area2", EditorStyleEx.PixelBoxStyle);
            bOpenArea2 = fadeArea2.open;
            if (fadeArea2.Show())
            {
                using (new AutoBeginHorizontal())
                {

                    EditorGUILayout.HelpBox("help box", MessageType.Info);
                }

            }

            GUILayout.Button("CloseButtonEdiStyle", EditorStyleEx.CloseButtonStyle);

            GUILayout.Button("SmallResetEdiStyle", EditorStyleEx.SmallResetStyle);
            GUILayout.Button("GizmoButtonEdiStyle", EditorStyleEx.GizmoButtonStyle);

            GUILayout.Label("thinHelpBox", EditorStyleEx.ThinHelpBox, GUILayout.Height(15));

            EditorGUILayoutEx.instance.EndFadeArea();

            GUILayout.Space(30);
        }

        GUILayout.Space(20);
    }


}