using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using DynamicShadowProjector;

//过场批量修改模型工具
public class ModuleChangeEditor : EditorWindow
{
    public static ModuleChangeEditor instance;
    
    public string moduleName = "mod_kratos_02";
    public GameObject changeModule;
    public GameObject sourceModule;
    public static void ShowWindow()
    {
        instance = (ModuleChangeEditor)EditorWindow.GetWindow(typeof(ModuleChangeEditor));
        instance.minSize = new Vector2(400, 300);
        instance.titleContent = new GUIContent("修改模型");
        instance.autoRepaintOnSceneChange = true;
    }

    void OnGUI()
    {
        //using (new AutoBeginHorizontal())
        //{
        //    director = EditorGUILayout.TextField(director, GUILayout.Width(400));
        //    if (GUILayout.Button("路径", GUILayout.Width(60)))
        //    {
        //        director = EditorPrefs.GetString("", string.Format("{0}/Scene/Resources", Application.dataPath));
        //        director = EditorUtility.OpenFilePanel("读取剧情配置文件", director, "prefab");
        //        if (string.IsNullOrEmpty(director))
        //            return;
        //    }
        //}

        GUILayout.Space(5);
        moduleName = EditorGUILayout.TextField(moduleName, GUILayout.Width(400));
        GUILayout.Space(5);
        sourceModule = (GameObject)EditorGUILayout.ObjectField("过场模型:", sourceModule, typeof(GameObject), true);
        GUILayout.Space(5);
        changeModule = (GameObject)EditorGUILayout.ObjectField("替换模型:", changeModule, typeof(GameObject), true);
        GUILayout.Space(5);
        GUILayout.Space(20);
        if (GUILayout.Button("替换", GUILayout.Width(120)))
        {
            if (string.IsNullOrEmpty(moduleName) || changeModule == null || sourceModule == null)
            {
                Debug.LogError("条件不全");
                return;
            }
            if (EditorUtility.DisplayDialog("修改", "确定修改？", "确定", "取消"))
            {
                OnChangeModel();
                return;
            }
        }
        GUILayout.Space(20);
        if (GUILayout.Button("全部过场替换", GUILayout.Width(120)))
        {
            if (string.IsNullOrEmpty(moduleName) || changeModule == null)
            {
                Debug.LogError("条件不全");
                return;
            }
            if (EditorUtility.DisplayDialog("修改", "确定修改？", "确定", "取消"))
            {
                OnChangeAllModule();
                return;
            }
        }
    }

    public static void ShowChangeEditor()
    {
        if (instance == null)
            ShowWindow();
    }

    void ChangeModel(GameObject sourceGo)
    {
        string name = "model";
        
        List<Transform> trans = new List<Transform>();
        Animation[] anis = sourceGo.GetComponentsInChildren<Animation>();
        for(int i = 0; i < anis.Length; i++)
        {
            if (anis[i].gameObject.name == name && anis[i].transform.parent.gameObject.name == moduleName)
            {
                trans.Add(anis[i].transform);
            }
        }
        foreach(Transform model in trans)
        {
            //找到model
            //根据已给的model复制出个新的
            GameObject goNew = GameObject.Instantiate(changeModule);
            goNew.name = name;
            goNew.transform.position = model.transform.position;
            goNew.transform.rotation = model.transform.rotation;
            goNew.transform.parent = model.transform.parent;

            //组件拷贝到新的上
            Component[] components = model.GetComponents<Component>();
            foreach (var comp in components)
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(comp);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(goNew);
            }
            //找阴影 设置引用
            Transform shadow = model.parent.Find("fx_shadow_dynamic/Shadow Projector");
            if (shadow != null)
            {
                DrawTargetObject draw = shadow.GetComponent<DrawTargetObject>();
                if (draw != null)
                {
                    draw.target = goNew.transform;
                }
            }
            //删除原来的
            GameObject.DestroyImmediate(model.gameObject);
        }
    }


    public void OnChangeModel()
    {

        if (sourceModule != null)
        {
            ChangeModel(sourceModule);

            UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(sourceModule);
            //替换预设  
            PrefabUtility.ReplacePrefab(sourceModule.gameObject, parentObject, ReplacePrefabOptions.ConnectToPrefab);
            //刷新  
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
      
    }

    public void OnChangeAllModule()
    {
        List<string> files = Util.GetAllFileList(Application.dataPath + "/Scene/Resources", "Assets/Scene/Resources/");
        List<GameObject> objs = new List<GameObject>();
        foreach (string assetPath in files)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
                continue;
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            ChangeModel(go);
            objs.Add(go);
            
        }

        for(int i = 0; i < objs.Count; i++)
        {
            UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(objs[i]);
            EditorUtility.SetDirty(PrefabUtility.ReplacePrefab(objs[i], parentObject, ReplacePrefabOptions.ConnectToPrefab));
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
}
