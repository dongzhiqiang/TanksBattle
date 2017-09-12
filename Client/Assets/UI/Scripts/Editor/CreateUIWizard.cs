using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Animations;

/// <summary>
/// UI创建向导
/// </summary>
public class CreateUIWizard : EditorWindow
{
    //预制体
    private static GameObject UITempPreb;
    //二级预制
    private static GameObject UITempPreb2;
    private static GameObject UITempEmpty;
    //步骤
    private int step = 0;
    //是否high摄像机
    private bool isUIHigh = false;
    //ui名字
    private string uiName = "";
    //UI中文名字
    private string uiChineseName = "";
    //是否需要射线
    private bool needGraphicRaycaster = false;
    //是否需要动画
    private bool needAnimator = false;
    //UITemplate模板
    private string UITemp = "UITemplate";
    //模板类型 0 (一级界面)   1(二级界面 )   2(空界面)
    private int templateType;
    //加入置顶判断中，像摇杆和聊天提示这种界面不需要加入判断
    private bool mCanTop = true;
    //如果不是最顶层的界面，禁用GraphicRaycaster,注意如果界面打开多了，那么这个组件会极大影响性能，勾上这个选项后就不会了
    private bool mDisableRaycasterIfNotTop = true;
    //动态层级，如果为false那么层级就是Canvas上的层级了。动态层级从50~200，ui_hight层不支持动态层级
    private bool mAutoOrder = true;
    //如果这个面板是顶层面板，那么把场景相机关了
    private bool mDisableCameraIfOpen = true;
    //打开动画
    private string mOpenAniName = "ui_ani_open_1";
    //关闭动画
    private string mCloseAniName = "ui_ani_close_1";

    static RectTransform m_canvasRoot;
    static RectTransform m_canvasRootHight;

    public enum LayerCom
    {
        UI = 5,
        UIHight = 10
    }
    //[MenuItem("Tool/UI创建向导")]
    public static void Init()
    {
        CreateUIWizard window = (CreateUIWizard)EditorWindow.GetWindow(typeof(CreateUIWizard), true);
        window.minSize = new Vector2(500f, 300f);
        window.titleContent = new GUIContent("UI创建向导");
        window.autoRepaintOnSceneChange = true;

        m_canvasRoot = GameObject.Find("CanvasRoot").GetComponent<RectTransform>();
        m_canvasRootHight = GameObject.Find("CanvasRootHight").GetComponent<RectTransform>();

        UITempPreb = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/UITemplate/UITemplate.prefab");
        UITempPreb2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/UITemplate/UITemplateSec.prefab");
        UITempEmpty = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/UITemplate/UITemplateEmpty.prefab");
    }

    void OnGUI()
    {
        switch (step)
        {
            case 0:
                DrawLabel(20, Color.white, "欢迎来到UI创建向导", true);
                GUILayout.Space(10f);
                if (GUILayout.Button("创建一个新UI", GUILayout.Width(100), GUILayout.Height(25)))
                {
                    step++;
                }
        
                break;
            case 1:
                uiName = EditorGUILayout.TextField("请定义UI名（英文UI开头）：", uiName, GUILayout.Width(300), GUILayout.Height(20));
                uiChineseName = EditorGUILayout.TextField("填写中文：", uiChineseName, GUILayout.Width(300), GUILayout.Height(20));
                if (GUILayout.Button("继续下一步", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    if (!string.IsNullOrEmpty(uiName))
                        step++;
                }
                DrawBackBtn();
                break;
            case 2://模板
                DrawLabel(16, Color.white, "请选择模板：", false);
                EditorGUILayout.Space();
                if (GUILayout.Button("创建一级界面", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    templateType = 0;
                    step++;
                }
                if (GUILayout.Button("创建二级界面", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    templateType = 1;
                    step++;
                }
                if (GUILayout.Button("创建空界面", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    templateType = 2;
                    step++;
                }
                GUILayout.Space(10f);
                DrawBackBtn();
                break;
            case 3:
                needGraphicRaycaster = EditorGUILayout.Toggle("是否需要检测碰撞？", needGraphicRaycaster, GUILayout.Width(300), GUILayout.Height(30));
                needAnimator = EditorGUILayout.Toggle("是否需要Animator动画？", needAnimator, GUILayout.Width(300), GUILayout.Height(30));
                isUIHigh = EditorGUILayout.Toggle("是否需要添加到高层级UI？", isUIHigh, GUILayout.Width(300), GUILayout.Height(30));
                if (GUILayout.Button("继续下一步", GUILayout.Width(80), GUILayout.Height(25)))
                    step++;
                DrawBackBtn();
                break;
            case 4:
                DrawLabel(16, Color.white, "请选择需要的UIPanel功能：", false);
                mCanTop = EditorGUILayout.Toggle("需加入置顶判断", mCanTop, GUILayout.Width(300), GUILayout.Height(30));
                mDisableRaycasterIfNotTop = EditorGUILayout.Toggle("不是最顶层可禁用Raycaster", mDisableRaycasterIfNotTop, GUILayout.Width(300), GUILayout.Height(30));
                if(templateType ==1)//二级界面 动态层级不选
                    EditorGUILayout.Toggle("动态层级", false, GUILayout.Width(300), GUILayout.Height(30));
                else
                    mAutoOrder = EditorGUILayout.Toggle("动态层级", mAutoOrder, GUILayout.Width(300), GUILayout.Height(30));
                mDisableCameraIfOpen = EditorGUILayout.Toggle("顶层面板关闭场景相机", mDisableCameraIfOpen, GUILayout.Width(300), GUILayout.Height(30));

                if (GUILayout.Button("继续下一步", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    step++;
                }
                DrawBackBtn();
                break;
            case 5:
                if (GUILayout.Button("创建", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    FinalCreate();
                    EditorUtility.DisplayDialog("标题", "恭喜！创建成功！", "OK");
                    step++;
                }
                DrawBackBtn();
                break;
            case 6:
                if (GUILayout.Button("回到主页", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    step = 0;
                }
                if (GUILayout.Button("再见", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    //关闭窗口
                    this.Close();
                }
                break;
            default:
                this.Close();
                break;
        }
    }
    //后退按钮
    void DrawBackBtn()
    {
        if (GUILayout.Button("返回上一步", GUILayout.Width(80), GUILayout.Height(25)))
        {
            step--;
        }
    }

    void DrawLabel(int fontSize, Color col, string labelContent, bool needCenter)
    {
        GUIStyle sty = new GUIStyle();
        sty.fontSize = fontSize;
        sty.normal.textColor = col;
        if (needCenter)
            sty.alignment = TextAnchor.UpperCenter;
        GUILayout.Label(labelContent, sty);
    }
    //开始创建
    void FinalCreate()
    {
        GameObject obj;
        if (templateType == 0)
            obj = (GameObject)Instantiate(UITempPreb);
        else if (templateType == 1)
            obj = (GameObject)Instantiate(UITempPreb2);
        else
            obj = (GameObject)Instantiate(UITempEmpty);
        RectTransform rt = obj.GetComponent<RectTransform>();
        obj.name = uiName;
        if (needGraphicRaycaster)
            obj.AddComponent<GraphicRaycaster>();
        if (needAnimator)
        {
            Animator anim = obj.AddComponent<Animator>();
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            RuntimeAnimatorController animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/UI/Animations/ui_ani.controller");
            anim.runtimeAnimatorController = animatorController;
        }
        //添加UIPanelBase
        UIPanelBase uBase = obj.AddComponent<UIPanelBase>();
        uBase.m_panelName = uiChineseName;
        uBase.m_canTop = mCanTop;
        uBase.m_disableRaycasterIfNotTop = mDisableRaycasterIfNotTop;
        uBase.m_autoOrder = mAutoOrder;
        uBase.m_disableCameraIfOpen = mDisableCameraIfOpen;
        if (needAnimator)
        {
            uBase.m_openAniName = mOpenAniName;
            uBase.m_closeAniName = mCloseAniName;
        }
        rt.SetParent(isUIHigh == true ? m_canvasRootHight : m_canvasRoot, false);
        obj.layer = isUIHigh == true ? (int)LayerCom.UIHight : (int)LayerCom.UI;

        //创建预制体
        PrefabUtility.CreatePrefab("Assets/UI/Resources/" + obj.name + ".prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
        EditorUtility.SetDirty(obj);

        GameObject prefabParent = (GameObject)PrefabUtility.GetPrefabParent(obj);
        SetRectTransform(prefabParent);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        //刷新后再设置RectTransform
        SetRectTransform(obj);

    }

    void SetRectTransform(GameObject obj)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.localScale = Vector3.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.anchoredPosition3D = Vector3.zero;
        rt.sizeDelta = Vector2.zero;
    }
}
