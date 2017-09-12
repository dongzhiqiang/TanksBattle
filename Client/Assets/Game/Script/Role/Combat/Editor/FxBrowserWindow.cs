using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;


//特效浏览器
public class FxBrowserWindow : EditorWindow
{
    //特效信息
    public class FxInfo
    {
        public string mod="";//这里作为唯一id用
        public string name="";
    }

    //标签信息
    public class FxTag
    {
        public string tag;
        public List<string> fxs = new List<string>();
        public void Search(List<string> rs,string search){
            rs.Clear(); 
            if(string.IsNullOrEmpty(search)){
                rs.AddRange(fxs);   
            }
            else
            {
                foreach (string s in fxs)
                {
                    if(s.Contains(search))
                        rs.Add(s);
                }
            }


        }
    }

    //存贮数据的类
    public class FxBrowserInfo
    {
        static FxBrowserInfo s_instance = null;

        public List<FxTag> tags = new List<FxTag>();
        public List<FxInfo> fxs = new List<FxInfo>();
        public int m_slider = 60;

        string[] tagNames;
        Dictionary<string, FxTag> tagsByName = new Dictionary<string, FxTag>();

        public static FxBrowserInfo Instance
        {
            get
            {
                if (s_instance == null)
                {
                    TextAsset t = AssetDatabase.LoadAssetAtPath<TextAsset>(Info_Path);
                    if (t != null && !string.IsNullOrEmpty(t.text))
                        s_instance = JsonMapper.ToObject<FxBrowserInfo>(t.text);
                    else
                        s_instance = new FxBrowserInfo();     
                    s_instance.Reset();  
                }
                return s_instance;
            }
        }

        public string[] TagName { get { return tagNames; } }

        public void Save()
        {
            System.IO.File.WriteAllText(Application.dataPath + "/../" + Info_Path, JsonMapper.ToJson(this), System.Text.Encoding.UTF8);
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.AssetDatabase.SaveAssets();
            Reset();//重置下，保证一些修改后需要重新计算的地方重新计算
        }

        public void Reset()
        { 
            //如果一个也没有那么新增加历史记录
            if (tags.Count == 0)
            {
                FxTag t = new FxTag();
                t.tag = "历史记录";
                tags.Add(t);
            }
            

            //收集信息
            tagsByName.Clear();
            List<string> l = new List<string>() { Tag_Record, Tag_All };
            foreach (FxTag t in tags)
            {
                tagsByName[t.tag] = t;
                if (Tag_Record != t.tag)
                    l.Add(t.tag);


            }
            tagNames = l.ToArray();

            //全部特效是动态获取的
            FxTag allTag = new FxTag();
            allTag.tag = Tag_All;
            foreach(FxInfo fx in fxs){
                allTag.fxs.Add(fx.mod);
            }

            
            tagsByName[allTag.tag] = allTag;
        }
        public FxTag GetTag(string tag)
        {
            FxTag t = tagsByName.Get(tag);
            if (t == null)Debuger.LogError("找不到tag:{0}", tag);
            return t;
        }

        public FxInfo GetFx(string fx)
        {
            foreach(FxInfo info in fxs)
                if(info.mod == fx)
                    return info;
            return null;
        }

        public void RemoveFx( string fx)
        {
            foreach (FxTag t in tags)
            {
                t.fxs.Remove(fx);    
            }
            foreach(FxInfo i in fxs){
                if (i.mod == fx)
                {
                    fxs.Remove(i);
                    break;
                }
            }
            
            Reset();
        }
        public void AddFx(string fx)
        {
            if (GetFx(fx) != null) { Debuger.LogError("特效浏览器，不能重复添加特效:{0}",fx);return;}
            FxInfo info =new FxInfo();
            info.mod = fx;
            fxs.Add(info);
            Reset();
        }

        public void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || Array.IndexOf(tagNames, tag) != -1){Debuger.LogError("逻辑错误，已经存在或者命名为空:{0}", tag);return;}
            FxTag t = new FxTag();t.tag = tag;
            tags.Add(t);
            Reset();
            
        }

        public void RemoveTag(string tag)
        {
            if (tag == Tag_Record || Array.IndexOf(tagNames, tag) == -1){Debuger.LogError("逻辑错误，找不到或者不能删除的:{0}", tag);return;}
            foreach (FxTag t in tags)
                if (t.tag == tag){tags.Remove(t);break;}
            Reset();
        }

        public void AddTagFx(string tag, string fx)
        {
            FxTag t = GetTag(tag);
            if (t == null) return;
            if (t.fxs.IndexOf(fx) != -1) { Debuger.LogError("逻辑错误，重复添加了特效到标签记录里:{0}", fx); return; }
            t.fxs.Add(fx);
            Reset();
        }

        public void RemoveTagFx(string tag, string fx)
        {
            FxTag t = GetTag(tag);
            if (t == null) return;
            if (t.fxs.IndexOf(fx) == -1) return;
            t.fxs.Remove(fx);
            Reset();
        }

        

    }

    [MenuItem("Tool/特效浏览器", false, 103)]
    public static void ShowWindow()
    {
        ShowWindow(null);
    }

    public static void ShowWindow(Action<string> onSel)
    {
        FxBrowserWindow instance = (FxBrowserWindow)EditorWindow.GetWindow(typeof(FxBrowserWindow), true);//很遗憾，窗口关闭的时候instance就会为null
        instance.minSize = new Vector2(400.0f, 200.0f);
        instance.titleContent = new GUIContent("特效浏览器");
        instance.autoRepaintOnSceneChange = true;
        instance.m_onSel = onSel;
        instance.ReSearch();
        
    }


    const float Capture_Len = 256f;
    const float RC_Len =32f;
    const string Res_Path = "Assets/Game/Script/Role/Combat/Editor/Res";
    const string Info_Path = Res_Path + "/fx_browser.json";
    const string Tag_Record= "历史记录";
    const string Tag_All= "全部特效";

    static Texture2D s_handleTL;
    static Texture2D s_handleTR;
    static Texture2D s_handleBL;
    static Texture2D s_handleBR;
    static Texture2D s_handleCT;

    public Action<string> m_onSel;
    

    int m_captureFrameCount = 0;
    string m_capturePath;
    string m_curTag = Tag_Record;
    string m_tagAdd = "新标签";
    string m_search = "";
    List<string> m_fxs = new List<string>();

    public static Texture2D HandleTL { get { if (s_handleTL == null)s_handleTL = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Script/Role/Combat/Editor/Res/handle_tl.png"); return s_handleTL; } }
    public static Texture2D HandleTR { get { if (s_handleTR == null)s_handleTR = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Script/Role/Combat/Editor/Res/handle_tr.png"); return s_handleTR; } }
    public static Texture2D HandleBL { get { if (s_handleBL == null)s_handleBL = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Script/Role/Combat/Editor/Res/handle_bl.png"); return s_handleBL; } }
    public static Texture2D HandleBR { get { if (s_handleBR == null)s_handleBR = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Script/Role/Combat/Editor/Res/handle_br.png"); return s_handleBR; } }
    public static Texture2D HandleCT { get { if (s_handleCT == null)s_handleCT = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Script/Role/Combat/Editor/Res/handle_center.png"); return s_handleCT; } }
    #region 监听
    void Awake()
    {
        

    }
    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI; 
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.RepaintAll();  
    }  

    //更新
    void Update()
    {
        
    }

    //显示和focus时初始化下
    void OnFocus()
    {
        
    }

    void OnLostFocus()
    {
        //Debuger.Log("当窗口丢失焦点时调用一次");
    }

    void OnHierarchyChange()
    {
        //Debuger.Log("当Hierarchy视图中的任何对象发生改变时调用一次");

    }

    void OnProjectChange()
    {
        //Debuger.Log("当Project视图中的资源发生改变时调用一次");
        this.Repaint();
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
    }

    #endregion

    public void ReSearch(){
        FxBrowserInfo.Instance.GetTag(m_curTag).Search(m_fxs, m_search);
    }
    //绘制窗口时调用
    void OnGUI()
    {
        if (FxBrowserInfo.Instance == null)return;
        
        using (new AutoBeginHorizontal())
        {
            using (new AutoBeginVertical("PreferencesSectionBox", GUILayout.Width(120)))
            {
                DrawAddTag();
                DrawTags();

            }
            using (new AutoBeginVertical())
            {
                DrawFxs();
            }
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        //显示截图辅助线
        if (string.IsNullOrEmpty(m_capturePath))//如果在截图期间则不显示
        {
            using (new AutoBeginHandles())
            {
                float len = Capture_Len;
                //绘制截图界面
                float left = Screen.width / 2f - len / 2f;
                float top = Screen.height / 2f - len / 2f;

                Vector2 ct = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Rect captureRect = new Rect(left, top, len, len);
                Rect handleRCTL = new Rect(left, top, RC_Len, RC_Len);
                Rect handleRCTR = new Rect(left + len - RC_Len, top, RC_Len, RC_Len);
                Rect handleRCBL = new Rect(left, top + len - RC_Len, RC_Len, RC_Len);
                Rect handleRCBR = new Rect(left + len - RC_Len, top + len - RC_Len, RC_Len, RC_Len);
                Rect handleRCCT = new Rect(ct.x - RC_Len / 2f, ct.y - RC_Len / 2f, RC_Len, RC_Len);

                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0f, 0.4f, 1f, 0.2f);
                GUI.Box(captureRect, "");
                GUI.backgroundColor = oldColor;

                //draw handles
                GUI.DrawTexture(handleRCTL, HandleTL);
                GUI.DrawTexture(handleRCTR, HandleTR);
                GUI.DrawTexture(handleRCBL, HandleBL);
                GUI.DrawTexture(handleRCBR, HandleBR);
                GUI.DrawTexture(handleRCCT, HandleCT);
            }
        }
        
        //需要截图则截图
        if (!string.IsNullOrEmpty(m_capturePath) )
        {
            SceneView.RepaintAll();
            if (m_captureFrameCount++ <= 2)
                return;

            SceneView.RepaintAll();
            Rect srect = GetScreenRect();
            Texture2D tex2D = new Texture2D((int)(srect.width), (int)(srect.height), TextureFormat.RGB24, false);
            tex2D.ReadPixels(srect, 0, 0);
            tex2D.Apply();
            byte[] bytes = tex2D.EncodeToPNG();
            System.IO.File.WriteAllBytes(m_capturePath, bytes);
            DestroyImmediate(tex2D);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            SceneView.RepaintAll();
            m_capturePath = null;
            this.Repaint();
        }
    }

    Rect GetScreenRect()
    {
        float left = Screen.width / 2f - Capture_Len / 2f;
        float top = Screen.height / 2f - Capture_Len / 2f;
        Rect captureRect = new Rect(left, top, Capture_Len, Capture_Len);

        if (SceneView.lastActiveSceneView == null)
            return captureRect;

        Rect crect = captureRect;
        //I DONT KNOW WHY THIS SHIT HAPPENS. IT JUST NEEDS TO CONVERT FOR ReadPixels().
        Rect srect = SceneView.lastActiveSceneView.camera.pixelRect;
        crect.y = srect.height - (crect.y + crect.height) + srect.y;
        crect.x += srect.x;
        crect.x = Mathf.CeilToInt(crect.x);
        crect.y = Mathf.CeilToInt(crect.y) + 1;
        return crect;
    }

    void Capture(string path)
    {
        m_capturePath = path;
        m_captureFrameCount = 0;
    }

    void DrawAddTag()
    {
        using (new AutoBeginHorizontal(EditorStyles.miniButtonMid))
        {
            m_tagAdd = GUILayout.TextField(m_tagAdd);
            FxBrowserInfo info = FxBrowserInfo.Instance;
            bool isAdd = Array.IndexOf(info.TagName, m_tagAdd) == -1;//是要增加或者要删除某个标签
            if (m_tagAdd != Tag_Record && m_tagAdd != Tag_All && GUILayout.Button(isAdd ? EditorGUIUtility.IconContent("Toolbar Plus") : EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.miniButtonMid, GUILayout.Width(30)))
            {
                if (isAdd)
                    info.AddTag(m_tagAdd);
                else
                    info.RemoveTag(m_tagAdd);
            }
        }

    }

    void DrawTags()
    {
        Rect rect;
        bool isCur;
        string tag;
        FxBrowserInfo info =FxBrowserInfo.Instance;
        for (int i = 0; i < info.TagName.Length; ++i)
        {
            tag = info.TagName[i];
            rect = GUILayoutUtility.GetRect(new GUIContent(tag), "PreferencesSection", GUILayout.ExpandWidth(true));
            isCur = (m_curTag == tag);
            if (isCur)
                GUI.Box(rect, "", "ServerUpdateChangesetOn");
            if (GUI.Toggle(rect, isCur, tag, "PreferencesSection") && !isCur)
            {
                m_search = "";
                m_curTag = tag;
                if (tag != Tag_Record && tag != Tag_All)
                {
                    m_tagAdd = info.TagName[i];
                }
                info.GetTag(tag).Search(m_fxs, m_search);
            }

        }
    }

    

    Vector2 previewScroll = Vector2.zero;
    void DrawFxs()
    {
        FxBrowserInfo info = FxBrowserInfo.Instance;
        // Search

        EditorGUILayout.BeginHorizontal();//"Toolbar"
        if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), EditorStyles.toolbarButton, GUILayout.Width(40)))
        {
            MessageBoxWindow.ShowAsInputBox("预制体名", (string name, object context) =>
            {
                if (string.IsNullOrEmpty(name))return;
                FxBrowserInfo.Instance.AddFx(name);
                
            }, (string groupName, object context) => { });
        }
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Duplicate"), EditorStyles.toolbarButton, GUILayout.Width(40)))
        {
            FxBrowserInfo.Instance.Save();
        }
        string s = GUILayout.TextField(m_search, "ToolbarSeachTextField");
        if (s != m_search)
        {
            m_search = s;
            info.GetTag(m_curTag).Search(m_fxs, m_search);
        }
        if (GUILayout.Button("", "ToolbarSeachCancelButton"))
        {
            m_search = "";
            info.GetTag(m_curTag).Search(m_fxs, m_search);
            GUI.FocusControl(null);
        }


        info.m_slider = (int)GUILayout.HorizontalSlider(info.m_slider, 40, 250, GUILayout.Width(100));
        
        EditorGUILayout.EndHorizontal();

        //特效显示的区域
        using (AutoBeginScrollView a = new AutoBeginScrollView(previewScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
        {
            previewScroll = a.Scroll;
            int colCount = Mathf.Max((int)((this.position.width - 120) / (info.m_slider + 150f)), 1);
            int itemWidth = (int)((this.position.width - 120 - 30) / colCount);
            int i = 0;
            FxInfo fxInfo;
            foreach (string fx in m_fxs)
            {
                fxInfo = info.GetFx(fx);
                if (fxInfo == null)
                {
                    m_fxs.Remove(fx);
                    info.RemoveTagFx(m_curTag, fx);
                    if (i > 0)
                        EditorGUILayout.EndHorizontal();
                    break;
                }
                if (i % colCount == 0)
                {
                    if (i != 0)
                        EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }

                EditorGUILayout.BeginHorizontal("box", i == colCount - 1 ? GUILayout.ExpandWidth(true) : GUILayout.Width(itemWidth));

                Rect rt = GUILayoutUtility.GetRect(info.m_slider, info.m_slider, GUILayout.Width(info.m_slider), GUILayout.Height(info.m_slider));
                Texture2D tex = GetFxTexture(fxInfo.mod);
                //GUI.DrawTexture(rt, tex, ScaleMode.StretchToFill);
                //GUILayout.Box(tex, GUILayout.Width(m_slider), GUILayout.Height(m_slider));
                GUI.DrawTexture(rt, tex, ScaleMode.StretchToFill);

                //信息及标签编辑
                EditorGUILayout.BeginVertical();
                GUILayout.Label(fxInfo.mod);
                fxInfo.name = EditorGUILayout.TextField(fxInfo.name);
                if (m_curTag != Tag_All && GUILayout.Button("删除标签"))
                {
                    if (EditorUtility.DisplayDialog("", string.Format("是否要删除{0}的{1}标签", fxInfo.name, m_curTag), "是", "否"))
                    {
                        m_fxs.Remove(fx);
                        info.RemoveTagFx(m_curTag, fx);
                        if (i > 0)
                            EditorGUILayout.EndHorizontal();
                        break;
                    }
                }

                if (GUILayout.Button("增加其他标签", EditorStyles.popup))
                {
                    GenericMenu contextMenu = new GenericMenu();
                    string itemFxId = fxInfo.mod;
                    for (int j = 0; j < info.TagName.Length; ++j)
                    {
                        string tagName = info.TagName[j];
                        if (tagName == Tag_All || tagName == Tag_Record)
                            continue;
                        contextMenu.AddItem(new GUIContent(tagName), false, () => FxBrowserInfo.Instance.AddTagFx(tagName, itemFxId));
                        contextMenu.ShowAsContext();
                    }
                }

                if (GUILayout.Button("删除"))
                {
                    if (EditorUtility.DisplayDialog("", string.Format("是否要删除{0}", fxInfo.name), "是", "否"))
                    {
                        m_fxs.Remove(fx);
                        info.RemoveFx(fx);
                        string preview = string.Format("{0}/{1}_preview.png", Res_Path, fxInfo.mod);
                        if (System.IO.File.Exists(preview))
                            System.IO.File.Delete(preview);
                        AssetDatabase.Refresh();
                        AssetDatabase.SaveAssets();
                        if (i > 0)
                            EditorGUILayout.EndHorizontal();
                        break;
                    }
                }

                if (GUILayout.Button("截图"))
                {
                    Capture(string.Format("{0}/{1}_preview.png", Res_Path, fxInfo.mod) );
                }

                //选中按钮
                if (m_onSel != null)
                {
                    if (GUILayout.Button("选中"))
                    {
                        FxTag tag =info.GetTag(Tag_Record);
                        if (tag != null && tag.fxs.IndexOf(fxInfo.mod) == -1)
                        {
                            if (tag.fxs.Count >= 10)
                                tag.fxs.RemoveAt(0);
                            info.AddTagFx(Tag_Record, fxInfo.mod);
                        }
                        m_onSel(fxInfo.mod);
                        this.Close();
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                ++i;

            }

            if (i > 0)
                EditorGUILayout.EndHorizontal();
        }
    }

    public Texture2D GetFxTexture(string name){
        Texture2D tex =AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format("{0}/{1}_preview.png", Res_Path, name));
        if (tex!=null)return tex;
        return AssetDatabase.LoadAssetAtPath<Texture2D>(Res_Path + "/no_preview.png");
    }
}
