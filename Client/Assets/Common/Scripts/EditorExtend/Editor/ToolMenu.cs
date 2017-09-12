using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Pathfinding;
using DynamicShadowProjector;
using System.Text;
using System.IO;
using Candlelight.UI;

public class ToolMenu
{
    [MenuItem("Tool/小工具/数学计算器")]
    static public void MathToolMenu()
    {
        MathToolWindow.ShowWnd();
    }

    [MenuItem("Tool/小工具/删除用户偏好数据")]
    static public void DelPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Tool/小工具/录屏")]
    static public void Capture()
    {
        if(!Application.isPlaying)
        {
            Debuger.LogError("只有游戏运行时才可以录屏");
            return;
        }

        Selection.activeGameObject = CaptureAvi.instance.gameObject;
        
    }

    //[MenuItem("Tool/测试热键 _RIGHT")]//KEYPAD0

    [MenuItem("Assets/贴图/设置UI图片格式", false, -1)]
    static public void SetSprite2DFormatMenu()
    {
        Sprite2DFormatWindow.ShowWnd();
    }

    [MenuItem("Assets/贴图/优化空图片")]
    static public void OptimizeEmptyImage()
    {

        var s = UIResMgr.instance.GetSprite("ui_tongyong_icon_transparent");
        var s2 = UIResMgr.instance.GetSprite("ui_tongyong_di_09");
        if (s == null || s2 == null)
        {
            Debuger.LogError("找不到合适的通用透明图");
            return;
        }
        List<GameObject> prefabs = new List<GameObject>();
        EditorUtil.GetAssetAtPath("UI/Resources", ref prefabs);
        foreach (var uiPrefab in prefabs)
        {
            if (uiPrefab.GetComponent<UIPanel>() == null)
                continue;


            Util.DoAllChild<Image>(uiPrefab.transform, (image) =>
            {
                if (image.sprite == null)
                {
                    if (image.GetComponent<Mask>() != null)
                        image.sprite = s2;
                    else
                        image.sprite = s;
                }

            });

            EditorUtil.SetDirty(uiPrefab);
        }
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.SaveAssets();

    }
    [MenuItem("Assets/贴图/查找图片引用")]
    static public void FindSpreiteRef()
    {
        if(Selection.activeObject == null || !(Selection.activeObject is Sprite))
        {
            Debuger.LogError("当前没有选中图片，不能查找引用");
            return;
        }

        List<string> paths = new List<string>();
        List<GameObject> prefabs = new List<GameObject>();
        EditorUtil.GetAssetAtPath("UI/Resources", ref prefabs);
        foreach (var uiPrefab in prefabs)
        {
            if (uiPrefab.GetComponent<UIPanel>() == null)
                continue;

            Util.DoAllChild<Image>(uiPrefab.transform, (image) =>
            {
                if (image.sprite == Selection.activeObject)
                    paths.Add(Util.GetGameObjectPath(image.gameObject));
            });
        }
        Debuger.Log(string.Format("引用数量：{0} \n{1}", paths.Count, string.Join("\n", paths.ToArray())));

    }
    [MenuItem("Assets/贴图/贴图浏览器", false, -1)]
    static public void ShowTextreBrower()
    {
        TextureBrowser.ShowWindow();
    }
    [MenuItem("Assets/贴图/保存图片为png", false, -1)]
    static public void SavePng()
    {
        List<string> paths = EditorUtil.GetSelectPaths();
        int totalCount = 0;

        string dirName = System.DateTime.Now.ToString("MM月dd日 HH-mm-ss");
        foreach (var path in paths)
        {
            Texture t =AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (t == null)
                continue;
            try
            {
                SaveRenderTextureToPNG(t, dirName, t.name);
            }
            catch (System.Exception err)
            {
                Debuger.LogError(err.StackTrace + ":" + err.Message);
            }
        }


    }
    public static bool SaveRenderTextureToPNG(Texture inputTex/*, Shader outputShader*/, string contents, string pngName)
    {
        RenderTexture temp = RenderTexture.GetTemporary(inputTex.width, inputTex.height, 0, RenderTextureFormat.ARGB32);
        //Material mat = new Material(outputShader);
        //Graphics.Blit(inputTex, temp, mat);
        Graphics.Blit(inputTex, temp);
        bool ret = SaveRenderTextureToPNG(temp, contents, pngName);
        RenderTexture.ReleaseTemporary(temp);
        return ret;

    }

    //将RenderTexture保存成一张png图片  
    public static bool SaveRenderTextureToPNG(RenderTexture rt, string contents, string pngName)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        byte[] bytes = png.EncodeToPNG();
        contents = Application.dataPath + "/" + contents;
        if (!Directory.Exists(contents))
            Directory.CreateDirectory(contents);
        FileStream file = File.Open(contents + "/" + pngName + ".png", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        Texture2D.DestroyImmediate(png);
        png = null;
        RenderTexture.active = prev;
        return true;

    }
    [MenuItem("Assets/贴图/去掉MinMaps", false, -1)]
    static public void SetMinMapsDisable()
    {
        List<string> paths = EditorUtil.GetSelectPaths();
        int totalCount = 0;
        HashSet<TextureImporter> set = new HashSet<TextureImporter>();
        foreach (var path in paths)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter == null)
                continue;

            ++totalCount;
            if (textureImporter.textureType == TextureImporterType.Advanced && textureImporter.mipmapEnabled == false)
                continue;
            set.Add(textureImporter);
        }
        if (set.Count == 0)
        {
            EditorUtility.DisplayDialog("", string.Format("共有{0}张贴图，都不需要设置minmap", totalCount), "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog("", string.Format("共有{0}张贴图，有{1}张需要设置minmap", totalCount, set.Count), "确定", "取消"))
            return;

        EditorUtility.DisplayProgressBar("Loading", string.Format("正在设置格式"), 0.0f);
        //设置格式
        foreach (TextureImporter t in set)
        {
            t.textureType = TextureImporterType.Advanced;
            t.mipmapEnabled = false;
            t.generateMipsInLinearSpace = false;
            t.isReadable = false;
            t.spriteImportMode = SpriteImportMode.None;
            t.filterMode = FilterMode.Bilinear;
            t.textureFormat = TextureImporterFormat.AutomaticCompressed;
            t.npotScale = TextureImporterNPOTScale.ToNearest;
            t.ClearPlatformTextureSettings("Android");
            t.ClearPlatformTextureSettings("iPhone");
            EditorUtil.SetDirty(t);
        }

        //根据格式重新导入下资源
        foreach (TextureImporter t in set)
            AssetDatabase.WriteImportSettingsIfDirty(t.assetPath);
        try
        {
            AssetDatabase.StartAssetEditing();
            int i = 0;
            foreach (TextureImporter t in set)
            {
                EditorUtility.DisplayProgressBar("Loading", string.Format("正在重新导出资源:第{0}个,共{1}个",i,set.Count), 0.1f+(0.9f*i)/ set.Count);
                AssetDatabase.WriteImportSettingsIfDirty(t.assetPath);
                ++i;
            }
                
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Art/场景/光照贴图缩小到512 地形贴图缩小到1024", false, 9)]
    public static void SetSceneTextureTo512()
    {
        List<string> paths = Util.GetAllFileList(Application.dataPath + "/Scene/levels", "Assets/Scene/levels/");
        int totalCount = 0;
        HashSet<TextureImporter> lightMapSet = new HashSet<TextureImporter>();
        HashSet<TextureImporter> splatSet = new HashSet<TextureImporter>();
        foreach (var path in paths)
        {
            if (path.Contains("Lightmap") )
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (textureImporter == null)
                    continue;
                ++totalCount;
                if (textureImporter.maxTextureSize <= 512)
                    continue;
                lightMapSet.Add(textureImporter);
            }
            else if(path.Contains("SplatAlpha"))
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (textureImporter == null)
                    continue;
                ++totalCount;
                if (textureImporter.maxTextureSize <= 1024)
                    continue;
                splatSet.Add(textureImporter);
            }
        }
        if (lightMapSet.Count == 0&& splatSet.Count == 0)
        {
            EditorUtility.DisplayDialog("", string.Format("共有{0}张贴图，都不需要设置大小", totalCount), "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog("", string.Format("共有{0}张贴图，有{1}张需要设置大小", totalCount, lightMapSet.Count+ splatSet.Count), "确定", "取消"))
            return;

        EditorUtility.DisplayProgressBar("Loading", string.Format("正在设置格式"), 0.0f);
        //设置格式
        foreach (TextureImporter t in lightMapSet)
        {
            t.maxTextureSize = 512;
            EditorUtil.SetDirty(t);
        }
        foreach (TextureImporter t in splatSet)
        {
            t.maxTextureSize = 1024;
            EditorUtil.SetDirty(t);
            lightMapSet.Add(t);
        }

        //根据格式重新导入下资源
        //foreach (TextureImporter t in set)
        //    AssetDatabase.WriteImportSettingsIfDirty(t.assetPath);
        try
        {
            AssetDatabase.StartAssetEditing();
            int i = 0;
            foreach (TextureImporter t in lightMapSet)
            {
                EditorUtility.DisplayProgressBar("Loading", string.Format("正在重新导出资源:第{0}个,共{1}个", i, lightMapSet.Count), 0.1f + (0.9f * i) / lightMapSet.Count);
                AssetDatabase.WriteImportSettingsIfDirty(t.assetPath);
                ++i;
            }

        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    [MenuItem("Assets/贴图/moblie粒子材质修改", false, -1)]
    static public void ChangeMoblieParticles()
    {
        List<Material> asset = new List<Material>();
        EditorUtil.GetSelectAsset<Material>(ref asset);

        foreach (var m in asset)
        {
            switch (m.shader.name)
            {
                case "Mobile/Particles/Additive":m.shader = Shader.Find("Custom/Particles/Additive");break;
                case "Mobile/Particles/Alpha Blended": m.shader = Shader.Find("Custom/Particles/Alpha Blended"); break;
                case "Mobile/Particles/Multiply": m.shader = Shader.Find("Custom/Particles/Multiply"); break;
                case "Mobile/Particles/VertexLit Blended": m.shader = Shader.Find("Custom/Particles/VertexLit Blended"); break;
            }
        }
        

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    //[MenuItem("CONTEXT/Image/转为ImageEx")]
    //static void CopyToImageEx(MenuCommand command)
    //{
    //    Image image = command.context as Image;
    //    ImageEx imageEx = image.gameObject.GetComponent<ImageEx>();
    //    if(imageEx != null)
    //        return;
    //    GameObject go =image.gameObject;
    //    GameObject go2 = new GameObject("Image", typeof(ImageEx));
    //    EditorUtility.CopySerialized(image, go2.GetComponent<ImageEx>());
    //    UnityEngine.Object.DestroyImmediate(image, true);
    //    imageEx = go.AddComponent<ImageEx>();
    //    EditorUtility.CopySerialized(go2.GetComponent<ImageEx>(), imageEx);
    //    UnityEngine.Object.DestroyImmediate(go2, true);
    //    EditorUtility.SetDirty(go);

    //}

    [MenuItem("GameObject/UI/ImageEx")]
    static public void AddImageEx()
    {
        GameObject go2 = new GameObject("image",typeof(ImageEx));
        if (Selection.activeGameObject!=null && Selection.activeGameObject.transform!= null)
        {
            go2.transform.SetParent(Selection.activeGameObject.transform,false);
            go2.transform.localPosition =Vector3.zero;
            go2.transform.localEulerAngles = Vector3.zero;
            go2.transform.localScale =Vector3.one;
            go2.layer = Selection.activeGameObject.layer;
        }
        go2.GetComponent<ImageEx>().raycastTarget = false;
        Selection.activeGameObject = go2;
        EditorUtility.SetDirty(go2);
    }

    [MenuItem("GameObject/UI/TextEx")]
    static public void AddTextEx()
    {
        GameObject go2 = new GameObject("txt", typeof(TextEx));
        if (Selection.activeGameObject != null && Selection.activeGameObject.transform != null)
        {
            go2.transform.SetParent( Selection.activeGameObject.transform,false);
            go2.transform.localPosition = Vector3.zero;
            go2.transform.localEulerAngles = Vector3.zero;
            go2.transform.localScale = Vector3.one;
            go2.layer = Selection.activeGameObject.layer;
        }
        AddTextEx(go2,"默认文本");
        Selection.activeGameObject = go2;
        EditorUtility.SetDirty(go2);
    }

    static TextEx AddTextEx(GameObject go2, string txt)
    {
        RectTransform rt = go2.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 24);
        TextEx t = go2.AddComponentIfNoExist<TextEx>();
        t.raycastTarget = false;
        t.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Font/hkhw5.TTF");
        t.fontSize = 22;
        t.color = Color.white;
        t.text = txt;
        
        return t;
    }
    static GameObject CreateUIObject(string name, GameObject parent)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.AddComponent<RectTransform>();
        gameObject.transform.SetParent(parent.transform,false);
        return gameObject;
    }

    [MenuItem("GameObject/UI/Input FieldEx")]
    static public void AddInputFieldEx()
    {
        
        GameObject gameObject = new GameObject("input");
        RectTransform t =gameObject.AddComponent<RectTransform>();
        if (Selection.activeGameObject != null && Selection.activeGameObject.transform != null)
        {
            t.SetParent(Selection.activeGameObject.transform, false);
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
            t.localScale = Vector3.one;
            gameObject.layer = Selection.activeGameObject.layer;
        }
         
        t.sizeDelta = new Vector2(160,35);
        GameObject gameObject2 = CreateUIObject("Placeholder", gameObject);
        GameObject gameObject3 = CreateUIObject("Text", gameObject);
        Image image = gameObject.AddComponent<ImageEx>();
        //image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Atlas/commonOld/ui_common_bk_02.png");
        image.type = Image.Type.Sliced;
        
        
        InputField inputField = gameObject.AddComponent<InputField>();
        Text text = AddTextEx(gameObject3,"");
        text.color = new Color(0.3f,0.3f,0.3f,1f);
        text.supportRichText = false;

        Text text2 = AddTextEx(gameObject2, "Enter text...");
        text2.fontStyle = FontStyle.Italic;
        Color color = text.color;
        color.a *= 0.5f;
        text2.color = color;

        RectTransform component = gameObject3.GetComponent<RectTransform>();
        component.anchorMin = Vector2.zero;
        component.anchorMax = Vector2.one;
        component.sizeDelta = Vector2.zero;
        component.offsetMin = new Vector2(10f, 6f);
        component.offsetMax = new Vector2(-10f, -7f);
        RectTransform component2 = gameObject2.GetComponent<RectTransform>();
        component2.anchorMin = Vector2.zero;
        component2.anchorMax = Vector2.one;
        component2.sizeDelta = Vector2.zero;
        component2.offsetMin = new Vector2(10f, 6f);
        component2.offsetMax = new Vector2(-10f, -7f);
        inputField.textComponent = text;
        inputField.placeholder = text2;
        Selection.activeGameObject = gameObject;
        EditorUtility.SetDirty(gameObject);
    }

    [MenuItem("Tool/Handle/SimpleHandle", true)]
    static bool IsAddSimpleHandle(MenuCommand command) { return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<SimpleHandle>() == null; }


    [MenuItem("Tool/Handle/SimpleHandle", false)]
    static void AddSimpleHandle(MenuCommand command)
    {
        SimpleHandle s =Selection.activeGameObject.AddComponent<SimpleHandle>();
        EditorUtility.SetDirty(Selection.activeGameObject);
    }

    [MenuItem("GameObject/UI/ButtonEx")]
    static public void AddButtonEx()
    {
        GameObject go2 = new GameObject("Button", typeof(ImageEx));
        if (Selection.activeGameObject != null && Selection.activeGameObject.transform != null)
        {
            go2.transform.SetParent( Selection.activeGameObject.transform,false);
            go2.transform.localPosition = Vector3.zero;
            go2.transform.localEulerAngles = Vector3.zero;
            go2.transform.localScale = Vector3.one;
            go2.layer = Selection.activeGameObject.layer;
        }

        AddStateButton(go2);

        Selection.activeGameObject = go2;
        EditorUtility.SetDirty(go2);
    }

    [MenuItem("Tool/Handle/StateHandle")]
    static void AddStateHandle(MenuCommand command)
    {
        StateHandle s = Selection.activeGameObject.AddComponent<StateHandle>();
        //s.transition = UnityEngine.UI.Selectable.Transition.None;
        //var nav = s.navigation;
        //nav.mode = UnityEngine.UI.Navigation.Mode.None;
        //s.navigation = nav;
        EditorUtility.SetDirty(Selection.activeGameObject);
    }

    [MenuItem("Tool/Handle/StateButton")]
    static void AddStateButton(MenuCommand command)
    {
        AddStateButton(Selection.activeGameObject);        
    }

    static void AddStateButton(GameObject go)
    {
        if(!go || go.GetComponent<StateHandle>())
            return;

        //CanvasGroup g = go.GetComponent<CanvasGroup>();
        //if(!g)
        //    g = go.AddComponent<CanvasGroup>();
        //g.interactable = true;
        //g.blocksRaycasts = true;
        //g.ignoreParentGroups = true;


        StateHandle s = go.AddComponent<StateHandle>();

        ////初始化Selectable不需要用的地方
        //s.transition = UnityEngine.UI.Selectable.Transition.None;
        //var nav = s.navigation;
        //nav.mode = UnityEngine.UI.Navigation.Mode.None;
        //s.navigation = nav;

        s.AddPublicHandle(Handle.Type.scale);
        //设置提起状态
        Handle h1 = s.m_states[0].publicHandles[0];
        s.m_states[0].isDuration = true;
        h1.m_vEnd = Vector3.one;
        h1.m_go = s.gameObject;

        //设置按下状态
        Handle h2 = s.m_states[1].publicHandles[0];
        s.m_states[1].isDuration = true;
        h2.m_vEnd = Vector3.one * 0.85f;
        h2.m_go = s.gameObject;

        //设置控制器类型为按钮
        s.m_ctrlType = StateHandle.CtrlType.button;

        EditorUtility.SetDirty(go);
    }
    [MenuItem("Tool/小工具/转换所有Button为StateButton")]
    static public void ChangeToStateButton()
    {
        GameObject[] gos = GameObject.FindObjectsOfType<GameObject>();//注意不能找到inactive的游戏对象
        Button btn;
        foreach (var go in gos)
        {
            btn = go.GetComponent<Button>();
            if(!btn)
                continue;

            UnityEngine.Object.DestroyImmediate(btn);
            AddStateButton(go);    
        }
    }
    //[MenuItem("Assets/角色/给所有角色加上center", false, -1)]
    //static public void AddCenterToPrefab()
    //{
    //    List<GameObject> prefabs = new List<GameObject>();
    //    EditorUtil.GetAssetAtPath("FBX/Resources", ref prefabs);

    //    foreach (var p in prefabs)
    //    {
    //        //这里要实例化在场景里才可以操作
    //        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(p);

    //        var model = go.transform.Find("model");
    //        if (model == null|| go.transform.Find("fx_shadow_dynamic") != null || go.transform.Find("fx_shadow") == null)
    //        {
    //            GameObject.DestroyImmediate(go);
    //            continue;
    //        }

    //        var agent = go.GetComponent<DynamicShadowAgent>();
    //        if(agent!= null)
    //            GameObject.DestroyImmediate(agent);


    //        GameObject dynamicShadow = UnityEngine.Object.Instantiate<GameObject>(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Effect/Resources/fx_shadow_dynamic.prefab"));
    //        dynamicShadow.name = "fx_shadow_dynamic";
    //        Transform t = dynamicShadow.transform;
    //        t.SetParent(go.transform, false);
    //        t.localPosition = Vector3.zero;
    //        t.localEulerAngles = Vector3.zero;
    //        t.localScale = Vector3.one;
    //        t.Find("Shadow Projector").GetComponent<DrawTargetObject>().target = model.transform;
    //        //var center = model.Find("Center");
    //        //if (center != null)
    //        //{
    //        //    GameObject.DestroyImmediate(go);
    //        //    continue;
    //        //}

    //        //center = new GameObject("Center").transform;
    //        //center.SetParent(model);
    //        //center.localEulerAngles = Vector3.zero;

    //        //var title = model.Find("Title");
    //        //if (title == null)
    //        //    center.localPosition = Vector3.up;
    //        //else
    //        //{
    //        //    Vector3 pos = title.localPosition;
    //        //    pos.y /= 2;
    //        //    center.localPosition = pos;
    //        //}


    //        UnityEditor.EditorUtility.SetDirty(PrefabUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab));
    //        GameObject.DestroyImmediate(go);
    //    }

    //    UnityEditor.AssetDatabase.Refresh();
    //    UnityEditor.AssetDatabase.SaveAssets();
    //}

    [MenuItem("Assets/角色/由模型创建预制体", false, -1)]
    static public void ChangeModelToRolePrefab()
    {
        GameObject model = Selection.activeGameObject;
        if (model == null)
        {
            Debuger.LogError("请先选中一个模型");
            return;
        }

        //拿之前的预制体和创建一个临时的对象(这里不支持覆盖，因为可能会导致绑定在动作上的特效的骨骼点丢失)
        string path = string.Format("Assets/FBX/Resources/{0}.prefab", model.name);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            EditorUtility.DisplayDialog("", string.Format("{0}已经存在不能创建",path), "确定");
            return;
        }
        GameObject tem;
        //if (prefab != null)
        //    tem = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        //else
        tem = new GameObject(model.name, typeof(CharacterController), typeof(SimpleRole), typeof(Seeker), typeof(FunnelModifier));

        //检查模型，并添加动画和需要的脚本
        if(!CheckModelAndAdd(model,tem))
        {
            GameObject.DestroyImmediate(tem);
            return;
        }

        

        //创建或者同步到预制体
        //if(prefab!= null)
        //    PrefabUtility.ReplacePrefab(tem, prefab, ReplacePrefabOptions.ConnectToPrefab);
        //else
        //{
        GameObject p=  PrefabUtility.ReplacePrefab(tem, PrefabUtility.CreateEmptyPrefab(path));//ReplacePrefabOptions.ConnectToPrefab
        //}
        GameObject.DestroyImmediate(tem);
        UnityEditor.EditorUtility.SetDirty(p);
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("", "完成", "确定");
        
    }

    static bool CheckModelAndAdd(GameObject model, GameObject tem)
    {
        //判断用哪个动画系统
        string modelPath = AssetDatabase.GetAssetPath(model);
        if(string.IsNullOrEmpty(modelPath)&&EditorUtility.DisplayDialog("", "找不到选中对象的路径，不是文件", "确定"))
            return false;
        ModelImporter modelImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (modelImporter==null && EditorUtility.DisplayDialog("", "选中对象不是模型", "确定"))
            return false;
        if (modelImporter.animationType != ModelImporterAnimationType.Legacy && EditorUtility.DisplayDialog("", "模型设置上AnimationType不是Legacy，请先设置好", "确定"))
            return false;
        
        //把模型加到临时对象下
        tem.name = model.name;
        GameObject temModel = UnityEngine.Object.Instantiate<GameObject>(model);
        Animation ani =temModel.GetComponent<Animation>();
        if (ani == null)
        {
            Debuger.LogError("逻辑异常，获取不到animation");
            return false;
        }
        temModel.name = "model";
        Transform t = temModel.transform;
        t.SetParent(tem.transform,false);
        t.localPosition = Vector3.zero;
        t.localEulerAngles = Vector3.zero;
        t.localScale= Vector3.one;
        //判断应该有的东西
        Transform tBodyMesh = t.Find("body_mesh");
        if (tBodyMesh == null && EditorUtility.DisplayDialog("", "模型上找不到body_mesh", "确定"))
            return false;
        if(tBodyMesh!=null)//修改渲染层级
            tBodyMesh.gameObject.layer =LayerMask.NameToLayer("RoleRender");
        Transform tWeaponMesh = t.Find("weapon_mesh");
        if (tWeaponMesh == null && !EditorUtility.DisplayDialog("", "模型上找不到weapon_mesh", "继续创建", "取消"))
            return false;
        if (tWeaponMesh != null)//修改渲染层级
            tWeaponMesh.gameObject.layer = LayerMask.NameToLayer("RoleRender");
        tWeaponMesh = t.Find("weapon_mesh_01");
        if (tWeaponMesh != null)//修改渲染层级
            tWeaponMesh.gameObject.layer = LayerMask.NameToLayer("RoleRender");
        tWeaponMesh = t.Find("weapon_mesh_02");
        if (tWeaponMesh != null)//修改渲染层级
            tWeaponMesh.gameObject.layer = LayerMask.NameToLayer("RoleRender");

        //创建title
        GameObject title = new GameObject("Title");
        t = title.transform;
        t.SetParent(temModel.transform, false);
        t.localPosition = Vector3.up*2;
        t.localEulerAngles = Vector3.zero;
        t.localScale = Vector3.one;
        //创建Center
        GameObject center = new GameObject("Center");
        t = center.transform;
        t.SetParent(temModel.transform, false);
        t.localPosition = Vector3.up ;
        t.localEulerAngles = Vector3.zero;
        t.localScale = Vector3.one;

        //添加阴影
        GameObject staticShadow = UnityEngine.Object.Instantiate<GameObject>(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Effect/Resources/fx_shadow.prefab"));
        staticShadow.name = "fx_shadow";
        t = staticShadow.transform;
        t.SetParent(tem.transform, false);
        t.localPosition = Vector3.zero;
        t.localEulerAngles = Vector3.zero;
        t.localScale = Vector3.one;
        GameObject dynamicShadow = UnityEngine.Object.Instantiate<GameObject>(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Effect/Resources/fx_shadow_dynamic.prefab"));
        dynamicShadow.name = "fx_shadow_dynamic";
        t = dynamicShadow.transform;
        t.SetParent(tem.transform, false);
        t.localPosition = Vector3.zero;
        t.localEulerAngles = Vector3.zero;
        t.localScale = Vector3.one;
        t.Find("Shadow Projector").GetComponent<DrawTargetObject>().target = temModel.transform;

        //找到所有动作并加到临时对象的模型上
        List<string> commonAni = new List<string>() { AniFxMgr.Ani_DaiJi, AniFxMgr.Ani_PaoBu, AniFxMgr.Ani_SiWang
            , AniFxMgr.Ani_BeiJi01, AniFxMgr.Ani_BeiJi02, AniFxMgr.Ani_FuKong01, AniFxMgr.Ani_FuKong02
            , AniFxMgr.Ani_DaoDi, AniFxMgr.Ani_JiFei, AniFxMgr.Ani_QiShen };
        string dir = System.IO.Path.GetDirectoryName(modelPath);
        string filter = "t:AnimationClip";//string.Format("n:{0}@", model.name);
        string[] guids = AssetDatabase.FindAssets(filter, new string[] { dir });
        string log ="";
        string log2 = "";
        foreach(string guid in guids){
            string aniPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject aniPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(aniPath);
            if(aniPrefab==null)
            {
                Debuger.LogError("不能从{0}获取动作信息",aniPath);
                continue;
            }
            log2 += aniPath + "\n";
            Animation a = aniPrefab.GetComponent<Animation>();
            if (a == null)
            {
                Debuger.LogError("逻辑异常，获取不到animation2,可能设置有问题");
                continue;
            }
            foreach (AnimationState st in a)
            {
                commonAni.Remove(st.name);
                log+=  st.name+"\n";
                if (ani[st.name] == null)
                {
                    ani.AddClip(st.clip, st.name);
                }

            }
        }
       // Debuger.LogError(log);
       // Debuger.LogError(log2);
        //判断有没有基本的动作
        if (commonAni.Count != 0 && !EditorUtility.DisplayDialog("", "模型上找不到通用动作，是否继续创建。" + string.Join(" ", commonAni.ToArray()), "确定","取消"))
            return false;


        //添加脚本
        temModel.AddComponentIfNoExist<AniFxMgr>();
        CharacterController cc = tem.GetComponent<CharacterController>();
        cc.center = Vector3.up * 1;

        


        EditorUtil.SetDirty(tem);
        return true;
    }

    [MenuItem("Assets/角色/添加新做的动作", false, -1)]
    static public void AddAniToRolePrefab()
    {
        GameObject model = Selection.activeGameObject;
        if (model == null)
        {
            Debuger.LogError("请先选中一个模型");
            return;
        }
        string name = model.name;
        int postfix = name.IndexOf("@");
        if(postfix!= -1)
            name = name.Substring(0,postfix);

        Debuger.Log("要找的模型名：{0}",name);
        //拿之前的预制体和创建一个临时的对象(这里不支持覆盖，因为可能会导致绑定在动作上的特效的骨骼点丢失)
        string path = string.Format("Assets/FBX/Resources/{0}.prefab", name);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("", string.Format("{0}不存在不能添加动作", path), "确定");
            return;
        }
        string modelPath = AssetDatabase.GetAssetPath(model);
        Transform mod = prefab.transform.Find("model");
        Animation ani = mod.GetComponent<Animation>();
        UnityEngine.Object.DestroyImmediate(ani,true);//删除老的animation
        ani = mod.gameObject.AddComponent<Animation>();

        ////删除掉所有老动作
        //SerializedObject so = new SerializedObject(ani);
        //so.Update();
        //SerializedProperty serializedProperty = so.FindProperty("m_Animation");
        //serializedProperty.objectReferenceValue=null;
        //SerializedProperty serializedProperty2 = so.FindProperty("m_Animations");
        //serializedProperty2.ClearArray();
        //so.ApplyModifiedProperties();
        //UnityEditor.AssetDatabase.Refresh();
        //UnityEditor.AssetDatabase.SaveAssets();

        //找到所有动作并加到临时对象的模型上
        string dir = System.IO.Path.GetDirectoryName(modelPath);
        string filter = "t:AnimationClip";//string.Format("n:{0}@", model.name);
        string[] guids = AssetDatabase.FindAssets(filter, new string[] { dir });
        foreach (string guid in guids)
        {
            string aniPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject aniPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(aniPath);
            if (aniPrefab == null)
            {
                Debuger.LogError("不能从{0}获取动作信息", aniPath);
                continue;
            }
            
            Animation a = aniPrefab.GetComponent<Animation>();
            if (a == null)
            {
                Debuger.LogError("逻辑异常，获取不到animation2,可能设置有问题");
                continue;
            }
            foreach (AnimationState st in a)
            {
                if (ani[st.name] != null)
                    continue;

                ani.AddClip(st.clip, st.name);

            }
        }

        UnityEditor.EditorUtility.SetDirty(prefab);
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("", "完成", "确定");
    }

    [MenuItem("Assets/角色/模型动作迁移", false, -1)]
    static public void ShowRolePrefabToolWindow()
    {
        RolePrefabToolWindow.ShowWnd(Selection.activeGameObject);
    }

    [MenuItem("Tool/小工具/检查UI层")]
    static public void CheckUILayer()
    {
        DirectoryInfo dir = new DirectoryInfo(System.IO.Path.Combine(Application.dataPath, "UI/Resources"));
        var uiLayer = LayerMask.NameToLayer("UI");
        var uiHightLayer = LayerMask.NameToLayer("UIHight");
        var extension = ".prefab";
        var excluded = new HashSet<string> { "UIResMgr", "UIRoot" };
        var errStrs = new List<string>();
        foreach (var file in dir.GetFiles())
        {
            if (!file.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))
                continue;
            if (excluded.Contains(System.IO.Path.GetFileNameWithoutExtension(file.Name)))
                continue;
            GameObject panelObj = AssetDatabase.LoadAssetAtPath<GameObject>(System.IO.Path.Combine("Assets/UI/Resources", file.Name));
            var panelLayer = panelObj.layer;
            var panelLayerStr = LayerMask.LayerToName(panelLayer);
            if (panelLayer != uiLayer && panelLayer != uiHightLayer)
            {
                errStrs.Add(string.Format("发现Panel层不对，名字：{0}，层次：{1}\r\n", panelObj.name, panelLayerStr));
                continue;
            }
            var childrens = panelObj.GetComponentsInChildren<Transform>(true);
            foreach (var child in childrens)
            {
                var childLayer = child.gameObject.layer;
                var childLayerStr = LayerMask.LayerToName(childLayer);
                if (childLayer != panelLayer)
                    errStrs.Add(string.Format("发现UI子控件不跟Panel同层，Panel名字：{0}，控件路径：{1}，Panel层次：{2}，控件层次：{3}\r\n", panelObj.name, Util.GetGameObjectPath(child.gameObject), panelLayerStr, childLayerStr));
            }
        }
        if (errStrs.Count > 0)
        {
            var temp = "";
            for (var i = 0; i < errStrs.Count; ++i)
            {
                var str = errStrs[i];
                temp += str;
                if ((i + 1) % 30 == 0)
                {
                    Debuger.Log(temp);
                    temp = "";
                }                    
            }
            if (temp.Length > 0)
                Debuger.Log(temp);
        }
        else
        {
            Debuger.Log("没有检查出问题");
        }
    }

    [MenuItem("Tool/小工具/校正UI层")]
    static public void CorrectUILayer()
    {
        DirectoryInfo dir = new DirectoryInfo(System.IO.Path.Combine(Application.dataPath, "UI/Resources"));
        var uiLayer = LayerMask.NameToLayer("UI");
        var uiHightLayer = LayerMask.NameToLayer("UIHight");
        var extension = ".prefab";
        var excluded = new HashSet<string> { "UIResMgr", "UIRoot" };
        var errStrs = new List<string>();
        foreach (var file in dir.GetFiles())
        {
            if (!file.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))
                continue;
            if (excluded.Contains(System.IO.Path.GetFileNameWithoutExtension(file.Name)))
                continue;
            GameObject panelGo = AssetDatabase.LoadAssetAtPath<GameObject>(System.IO.Path.Combine("Assets/UI/Resources", file.Name));
            var panelLayer = panelGo.layer;
            var panelLayerStr = LayerMask.LayerToName(panelLayer);
            if (panelLayer != uiLayer && panelLayer != uiHightLayer)
            {
                errStrs.Add(string.Format("发现Panel层不对，名字：{0}，层次：{1}，由于是Panel，不自动校正，请手动校正后再执行本命令\r\n", panelGo.name, panelLayerStr));
                continue;
            }
            var changed = false;
            var childrens = panelGo.GetComponentsInChildren<Transform>(true);
            foreach (var child in childrens)
            {
                var childLayer = child.gameObject.layer;
                var childLayerStr = LayerMask.LayerToName(childLayer);
                if (childLayer != panelLayer)
                {
                    changed = true;
                    child.gameObject.layer = panelLayer;
                    errStrs.Add(string.Format("发现UI子控件不跟Panel同层，Panel名字：{0}，控件路径：{1}，Panel层次：{2}，控件层次：{3}，已校正\r\n", panelGo.name, Util.GetGameObjectPath(child.gameObject), panelLayerStr, childLayerStr));
                }
            }
            if (changed)
                UnityEditor.EditorUtility.SetDirty(panelGo);
        }
        
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.SaveAssets();
        if (errStrs.Count > 0)
        {
            var temp = "";
            for (var i = 0; i < errStrs.Count; ++i)
            {
                var str = errStrs[i];
                temp += str;
                if ((i + 1) % 30 == 0)
                {
                    Debuger.Log(temp);
                    temp = "";
                }
            }
            if (temp.Length > 0)
                Debuger.Log(temp);
        }
        else
        {
            Debuger.Log("没有检查出问题");
        }
    }

    [MenuItem("Tool/小工具/检查Missing MonoBehaviour")]
    static public void CheckMissingMonoBehaviour()
    {
        DirectoryInfo dir = new DirectoryInfo(System.IO.Path.Combine(Application.dataPath, "UI/Resources"));
        var extension = ".prefab";
        var excluded = new HashSet<string> { "UIResMgr", "UIRoot" };
        var errStrs = new List<string>();
        foreach (var file in dir.GetFiles())
        {
            if (!file.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))
                continue;
            if (excluded.Contains(System.IO.Path.GetFileNameWithoutExtension(file.Name)))
                continue;
            GameObject panelObj = AssetDatabase.LoadAssetAtPath<GameObject>(System.IO.Path.Combine("Assets/UI/Resources", file.Name));

            var childrens = panelObj.GetComponentsInChildren<Transform>(true);
            foreach (var child in childrens)
            {
                MonoBehaviour[] bhs2 = child.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour bh in bhs2)
                {
                    if (bh == null)
                    {
                        errStrs.Add(string.Format("Panel Child Missing MonoBehaviour，Panel名字：{0}，控件路径：{1}\r\n", panelObj.name, Util.GetGameObjectPath(child.gameObject)));
                    }
                }
            }
        }
        if (errStrs.Count > 0)
        {
            var temp = "";
            for (var i = 0; i < errStrs.Count; ++i)
            {
                var str = errStrs[i];
                temp += str;
                if ((i + 1) % 30 == 0)
                {
                    Debuger.Log(temp);
                    temp = "";
                }
            }
            if (temp.Length > 0)
                Debuger.Log(temp);
        }
        else
        {
            Debuger.Log("没有检查出问题");
        }
    }

    [MenuItem("Assets/贴图/图标添加到富文本插件")]
    static public void AddItemIconsToRichText()
    {
#if !ART_DEBUG
        if (ItemCfg.m_cfgs.Count <= 0)
            ItemCfg.Init();

        var quadList = new List<HyperTextStyles.Quad>();
        quadList.Add(new HyperTextStyles.Quad(UIResMgr.instance.GetSprite("ui_tongyong_icon_jinbi"), "gold", 1.0f, -0.1f, false, "", ""));
        quadList.Add(new HyperTextStyles.Quad(UIResMgr.instance.GetSprite("ui_tongyong_icon_tili"), "stamina", 1.0f, -0.1f, false, "", ""));
        quadList.Add(new HyperTextStyles.Quad(UIResMgr.instance.GetSprite("ui_tongyong_icon_zuanshi"), "diamond", 1.0f, -0.1f, false, "", ""));
        quadList.Add(new HyperTextStyles.Quad(UIResMgr.instance.GetSprite("ui_jingji_jingbi"), "arenacoin", 1.0f, -0.1f, false, "", ""));
        quadList.Add(new HyperTextStyles.Quad(UIResMgr.instance.GetSprite("ui_tongyong_icon_xing"), "star", 1.0f, -0.1f, false, "", ""));
        foreach (var v in ItemCfg.m_cfgs.Values)
        {
            var sprite = UIResMgr.instance.GetSprite(v.icon);
            var className = v.id.ToString();
            var sizeScalar = 1.0f;
            var verticalOffset = -0.1f;
            var shouldRespectColorization = false;
            var linkId = "";
            var linkClassName = "";
            var quad = new HyperTextStyles.Quad(sprite, className, sizeScalar, verticalOffset, shouldRespectColorization, linkId, linkClassName);
            quadList.Add(quad);
        }
        var resObj = AssetDatabase.LoadAssetAtPath<HyperTextStyles>("Assets/UI/Font/htstyle1.asset");
        resObj.SetQuadStyles(quadList);
        EditorUtil.SetDirty(resObj);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
#endif
    }
}