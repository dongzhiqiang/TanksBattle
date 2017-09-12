using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
/// <summary>
/// UI资源浏览器
/// </summary>
[CustomEditor(typeof(UIBrowser))]
public class UIBrowserEditor : Editor
{
    private UIBrowser mp;

    Vector2 previewScroll = Vector2.zero;
    //UIRoot
    private static GameObject UIRoot;

    public override void OnInspectorGUI()//重写这个方法
    {
        mp = (UIBrowser)target;
        if (mp.uiRoot == null)
            mp.uiRoot = GameObject.Find("CanvasRoot");
        if (mp.uiRootHight == null)
            mp.uiRootHight = GameObject.Find("CanvasRootHight");
        if (UIRoot == null)
            UIRoot = GameObject.Find("UIRoot");
        GetAllPrefabs();

        DrawDisplay();
    }

    #region  Private Methods
    void DrawDisplay()
    {
        UIBrowserInfo inst = UIBrowserInfo.Instance;
        using (new AutoChangeBkColor(Color.green))
            if (GUILayout.Button("打开UI创建向导", GUILayout.Height(25)))
                CreateUIWizard.Init();

        using (new AutoChangeBkColor(Color.blue))
            if (GUILayout.Button("一键保存场景中的Prefab", GUILayout.Height(25)))
                SavePrefabs();
        //自适应的显示区域 o
        using (AutoBeginScrollView a = new AutoBeginScrollView(previewScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
        {
            //改变滚动条
            previewScroll = a.Scroll;

            using (new AutoBeginHorizontal())
            {
                using (new AutoBeginVertical("PreferencesSectionBox"))
                {
                    inst.showPrefabs.AddRange(inst.hidePrefabs);
                    ShowPrefabs("场景中", Color.green, inst.showPrefabs);
                }
                using (new AutoBeginVertical("PreferencesSectionBox"))
                {
                    ShowPrefabs("未在场景", Color.red, inst.noInstPrefabs);
                }
            }
        }

    }

    void SavePrefabs()
    {
        Canvas[] objs = GameObject.Find("UIRoot").GetComponentsInChildren<Canvas>(true);
        foreach (Canvas obj in objs)
        {
            //判断GameObject是否为一个Prefab的引用
            if (obj.name.Substring(0, 2) == "UI" && PrefabUtility.GetPrefabType(obj) == PrefabType.PrefabInstance)
            {
                UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(obj);
                //替换预设  
                PrefabUtility.ReplacePrefab(obj.gameObject, parentObject, ReplacePrefabOptions.ConnectToPrefab);
                //刷新  
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
        EditorUtility.DisplayDialog("标题", "保存成功！", "OK");
    }
    void ShowPrefabs(string title, Color color, List<GameObject> list)
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = color;
        style.alignment = TextAnchor.UpperCenter;
        GUILayout.Label(title, style);
        UIBrowserInfo inst = UIBrowserInfo.Instance;
        foreach (GameObject go in list)
        {
            UIPrefabInfo uiInfo = inst.GetPrefabInfo(go.name);

            EditorGUILayout.BeginHorizontal();
            UIPanelBase pl = go.GetComponent<UIPanelBase>();
            string name;
            if (pl && !string.IsNullOrEmpty(pl.m_panelName))
                name = pl.m_panelName;
            else
                name = go.name;
            using (new AutoFontSize(13, EditorStyles.toolbarButton))
            {
                if (GUILayout.Button(name, EditorStyles.toolbarButton))//高亮找到点击的预制体
                {
                    EditorGUIUtility.PingObject(uiInfo.instanceGo != null ? uiInfo.instanceGo.GetInstanceID() : go.GetInstanceID()); //Selection.activeObject = t;
                }
            }

            if (uiInfo.isHierarchy)
            {
                if (uiInfo.isActive)
                {
                    if (GUILayout.Button(EditorGUIUtility.IconContent("PrefabNormal Icon"), EditorStyles.toolbarButton, GUILayout.Width(20)))//隐藏
                    {
                        inst.SetPrefabActive(uiInfo.name, false);
                        ChangeActive(uiInfo.name, false);
                    }
                    if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.toolbarButton, GUILayout.Width(20)))//删除
                    {
                        GameObject prefInst = inst.GetPrefabInstance(go);
                        DestroyImmediate(prefInst.gameObject);
                        inst.UpdateSceneObjs();//更新界面上的实例
                    }
                }
                else
                {
                    //EditorGUILayout.ObjectField(t, typeof(GameObject), true);
                    if (GUILayout.Button(EditorGUIUtility.IconContent("Prefab Icon"), EditorStyles.toolbarButton, GUILayout.Width(20)))//显示
                    {
                        inst.SetPrefabActive(uiInfo.name, true);
                        ChangeActive(uiInfo.name, true);
                    }
                    if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.toolbarButton, GUILayout.Width(20)))//删除
                    {
                        GameObject prefInst = inst.GetPrefabInstance(go);
                        DestroyImmediate(prefInst.gameObject);
                        inst.UpdateSceneObjs();//更新界面上的实例
                    }
                }
            }
            else
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("GameObject Icon"), EditorStyles.toolbarButton, GUILayout.Width(20)))//添加
                {
                    GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(go.gameObject);
                    RectTransform rt = obj.GetComponent<RectTransform>();
                    rt.SetParent(go.layer == LayerMask.NameToLayer("UIHight") ? mp.uiRootHight.transform : mp.uiRoot.transform, false);

                    SetRectTransform(obj);
                    obj.name = go.gameObject.name;
                    obj.gameObject.SetActive(true);

                    inst.UpdateSceneObjs();//更新界面上的实例
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4f);
        }
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
    //改变active状态
    void ChangeActive(string prebName, bool active)
    {
        UIBrowserInfo inst = UIBrowserInfo.Instance;
        foreach (Canvas t in inst.sceneCanvas)
        {
            if (t.name == prebName)
            {
                t.gameObject.SetActive(active);
                break;
            }
        }
    }

    //获取所有Prefab
    void GetAllPrefabs()
    {
        UIBrowserInfo inst = UIBrowserInfo.Instance;
        inst.UpdateSceneObjs();//更新界面上的实例
        mp.allPrefabs.Clear();
        inst.showPrefabs.Clear();
        inst.hidePrefabs.Clear();
        inst.noInstPrefabs.Clear();
        List<string> files = Util.GetAllFileList(Application.dataPath + "/UI/Resources", "Assets/UI/Resources/");
        foreach (string assetPath in files)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
                continue;
            if (prefab.GetComponent<Canvas>() == null)
                continue;
            mp.allPrefabs.Add(prefab);
            inst.AddPrefabInfo(prefab);
        } 
  
    }
    #endregion

    /// <summary>
    /// 存贮数据的类
    /// </summary>
    public class UIBrowserInfo
    {
        //单例
        static UIBrowserInfo _instance = null;
        //存储预设体信息
        public List<UIPrefabInfo> fxs = new List<UIPrefabInfo>();
        //存储UI信息
        public Dictionary<string, UIPrefabInfo> preDic = new Dictionary<string, UIPrefabInfo>();

        //场景中的UI
        public List<Canvas> sceneCanvas;
        public List<GameObject> showPrefabs = new List<GameObject>();
        public List<GameObject> hidePrefabs = new List<GameObject>();
        public List<GameObject> noInstPrefabs = new List<GameObject>();
        public static UIBrowserInfo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UIBrowserInfo();
                }
                return _instance;
            }
        }

        public UIPrefabInfo GetPrefabInfo(string prebName)
        {
            return preDic[prebName];
        }

        public void AddPrefabInfo(GameObject obj)
        {
            UIPrefabInfo info = new UIPrefabInfo();
            info.name = obj.name;
            info.isHierarchy = false;
            info.isActive = false;
            //遍历一次场景中的
            foreach (Canvas can in sceneCanvas)
            {
                UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(can.gameObject);
                string path = AssetDatabase.GetAssetPath(parentObject);
                //判断是否是同一路径
                if (path == AssetDatabase.GetAssetPath(obj))
                {
                    info.isHierarchy = true;
                    info.instanceGo = can.gameObject;
                    //找到实例的active状态
                    GameObject prefabInst = GetPrefabInstance(obj);
                    if (prefabInst && prefabInst.activeInHierarchy)
                        info.isActive = true;
                    break;
                }
            }

            preDic[obj.name] = info;

            if (info.isHierarchy)
            {
                if (info.isActive)
                    showPrefabs.Add(obj);
                else
                    hidePrefabs.Add(obj);
            }
            else
                noInstPrefabs.Add(obj);
        }

        public void SetPrefabActive(string prebName, bool active)
        {
            preDic[prebName].isActive = active;
        }

        public void UpdateSceneObjs()
        {
            sceneCanvas = new List<Canvas>();
            Canvas[] objs = UIRoot.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas obj in objs)
            {
                //判断GameObject是否为一个Prefab的引用
                if (obj.name.Substring(0, 2) == "UI" && PrefabUtility.GetPrefabType(obj) == PrefabType.PrefabInstance)
                {
                    sceneCanvas.Add(obj);
                }
            }
        }

        //根据Prefab获取其在场景中的实例
        public GameObject GetPrefabInstance(GameObject pref)
        {
            Canvas[] objs = UIRoot.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas obj in objs)
            {
                //判断GameObject是否为一个Prefab的引用
                if (obj.name.Substring(0, 2) == "UI" && PrefabUtility.GetPrefabType(obj) == PrefabType.PrefabInstance)
                {
                    UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(obj);
                    string path = AssetDatabase.GetAssetPath(parentObject);
                    //判断路径是否一致
                    if (path == AssetDatabase.GetAssetPath(pref))
                        return obj.gameObject;
                }
            }
            return null;
        }
    }

    public class UIPrefabInfo
    {
        //UI名
        public string name = "";
        //预制体是否在场景中有实例
        public bool isHierarchy = false;
        //实例是否激活
        public bool isActive = false;
        public GameObject instanceGo = null;
    }
}