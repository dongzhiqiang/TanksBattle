using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

//目录贴图浏览器
public class TextureBrowser : EditorWindow
{

    public static void ShowWindow()
    {
        TextureBrowser instance = (TextureBrowser)EditorWindow.GetWindow(typeof(TextureBrowser));//很遗憾，窗口关闭的时候instance就会为null
        instance.minSize = new Vector2(750, 300.0f);
        instance.titleContent = new GUIContent("贴图浏览器");


        instance.m_infos.Clear();
        foreach (string o in Selection.assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(o);
            if (string.IsNullOrEmpty(path))
                continue;

            // 是个文件
            if (File.Exists(path))
            {
                TextureInfo info = TextureInfo.Create(path);
                if (info != null)
                    instance.m_infos.Add(info);
            }
            else
            {
                path = path.Substring(7);//把asset/去掉
                List<string> files = Util.GetAllFileList(Application.dataPath + "/" + path, "Assets/" + path + "/");
                foreach (string assetPath in files)
                {
                    TextureInfo info = TextureInfo.Create(assetPath);
                    if (info != null)
                        instance.m_infos.Add(info);
                }
            }
        }
        instance.m_sizeIdx.Clear();
        instance.m_textureTypeToggle.Clear();
        instance.m_typeToggle.Clear();
        foreach (var info in instance.m_infos)
        {
            if (instance.m_sizeIdx.ContainsKey(info.sizeMax))
                instance.m_sizeIdx[info.sizeMax] += 1;
            else
                instance.m_sizeIdx[info.sizeMax] = 1;

            instance.m_textureTypeToggle[(int)info.texImporter.textureType] = true;
            instance.m_typeToggle[(int)info.type] = true;
        }

        instance.m_sizeToggle.Clear();
        foreach (var a in instance.m_sizeIdx)
        {
            instance.m_sizeToggle[a.Key] = true;
        }

        //排序
        instance.m_infos.SortEx((TextureInfo a, TextureInfo b) => a.sizeMax.CompareTo(b.sizeMax) * -1);
    }



    public class TextureInfo
    {
        public Texture tex;
        public TextureImporter texImporter;
        public Vector2 size;
        public TextureImporterFormat type;
        public int typeOrder;
        public int sizeMax;//大的那一边
        public bool IsPOT { get { return ((int)size.x) % 2 == 0 && ((int)size.y) % 2 == 0; } }

        public static TextureInfo Create(string file)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(file) as TextureImporter;
            if (textureImporter == null)
                return null;
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(file);
            if (tex == null)
                return null;

            int maxTextureSize;
            TextureImporterFormat textureFormat;
            textureImporter.GetPlatformTextureSettings("Android", out maxTextureSize, out textureFormat);

            TextureInfo info = new TextureInfo();
            info.tex = tex;
            info.texImporter = textureImporter;
            info.type = textureFormat;
            info.size.x = tex.width;
            info.size.y = tex.height;
            info.sizeMax = tex.width >= tex.height ? tex.width : tex.height;

            //已经压缩的，排序时的优先级提高下
            if (textureFormat == TextureImporterFormat.AutomaticCompressed)
                info.typeOrder = 100;
            else if (textureFormat.ToString().StartsWith("ETC2"))
                info.typeOrder = 99;
            else if (textureFormat.ToString().StartsWith("ETC"))
                info.typeOrder = 98;
            else
                info.typeOrder = (int)textureFormat;

            return info;
        }
    }

    public class TextRefMaterial
    {
        public Material mat;
        public bool expand;
        public HashSet<GameObject> prefabs = new HashSet<GameObject>();
    }

    List<TextureInfo> m_infos = new List<TextureInfo>();
    SortedDictionary<int, int> m_sizeIdx = new SortedDictionary<int, int>();
    SortedDictionary<int, bool> m_sizeToggle = new SortedDictionary<int, bool>();
    SortedDictionary<int, bool> m_textureTypeToggle = new SortedDictionary<int, bool>();
    SortedDictionary<int, bool> m_typeToggle = new SortedDictionary<int, bool>();
    Dictionary<Material, TextRefMaterial> m_refMats = new Dictionary<Material, TextRefMaterial>();
    bool m_showUncompress = true;
    Vector3 m_scroll = Vector3.zero;
    int m_slider = 15;
    Texture m_texFindRef = null;


    //更新
    void Update()
    {
        this.Repaint();
    }


    void OnInspectorUpdate()
    {
        //Debuger.Log("窗口面板的更新");
        //这里开启窗口的重绘，不然窗口信息不会刷新
        this.Repaint();
    }

    //绘制窗口时调用
    void OnGUI()
    {
        //标题栏
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button("贴图名", EditorStyles.toolbarPopup))
            {
                m_infos.SortEx((TextureInfo a, TextureInfo b) => a.tex.name.CompareTo(b.tex.name));

            }
            if (GUILayout.Button("大小", EditorStyles.toolbarPopup, GUILayout.Width(100)))
            {
                m_infos.SortEx((TextureInfo a, TextureInfo b) => a.sizeMax.CompareTo(b.sizeMax) * -1);

            }
            if (GUILayout.Button("类型", EditorStyles.toolbarPopup, GUILayout.Width(100)))
            {
                m_infos.SortEx((TextureInfo a, TextureInfo b) => a.texImporter.textureType.CompareTo(b.texImporter.textureType));
            }
            if (GUILayout.Button("格式", EditorStyles.toolbarPopup, GUILayout.Width(150)))
            {
                m_infos.SortEx((TextureInfo a, TextureInfo b) => a.typeOrder.CompareTo(b.typeOrder));
            }
            if (GUILayout.Button("2的n次方", EditorStyles.toolbarPopup, GUILayout.Width(100)))
            {
                m_infos.SortEx((TextureInfo a, TextureInfo b) =>
                {
                    if (!a.IsPOT && b.IsPOT)
                        return -1;
                    else if (a.IsPOT && !b.IsPOT)
                        return 1;
                    else
                        return 0;
                });
            }



            if (GUILayout.Button("贴图", EditorStyles.toolbarPopup, GUILayout.Width(100)))
            {
                m_infos.SortEx((TextureInfo a, TextureInfo b) =>
                {
                    int sizeA = (int)(a.size.x * a.size.y);
                    int sizeB = (int)(b.size.x * b.size.y);
                    return sizeA.CompareTo(sizeB) * 1;
                });
            }

            using (new AutoBeginHorizontal(EditorStyles.toolbar, GUILayout.Width(100)))
            {
                m_slider = (int)GUILayout.HorizontalSlider(m_slider, 15, 160, GUILayout.Width(100));
            }
        }

        //贴图列表
        using (AutoBeginScrollView s = new AutoBeginScrollView(m_scroll, GUILayout.ExpandHeight(true)))
        {
            m_scroll = s.Scroll;
            foreach (var info in m_infos)
            {
                DrawInfo(info);
            }
        }

        //底部工具栏
        using (new AutoBeginVertical(EditorStyles.helpBox))
        {
            m_showUncompress = EditorGUILayout.Toggle("显示不压缩(uncompress后缀)", m_showUncompress);
            using (new AutoBeginHorizontal())
            {
                using (new AutoBeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true)))
                {
                    foreach (var a in m_sizeIdx)
                    {
                        m_sizeToggle[a.Key] = EditorGUILayout.Toggle(string.Format("{0}贴图{1}张", a.Key, a.Value), m_sizeToggle[a.Key]);
                    }
                    EditorGUILayout.LabelField(string.Format("总共{0}张", m_infos.Count));
                }

                using (new AutoBeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true)))
                {
                    foreach (var a in m_textureTypeToggle)
                    {
                        m_textureTypeToggle[a.Key] = EditorGUILayout.Toggle(((TextureImporterType)a.Key).ToString(), a.Value);
                    }
                }

                using (new AutoBeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true)))
                {
                    foreach (var a in m_typeToggle)
                    {
                        m_typeToggle[a.Key] = EditorGUILayout.Toggle(((TextureImporterFormat)a.Key).ToString(), a.Value);
                    }
                }

                using (new AutoBeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true)))
                {
                    using (new AutoBeginHorizontal())
                    {
                        m_texFindRef = (Texture)EditorGUILayout.ObjectField(m_texFindRef, typeof(Texture), false);
                        if (GUILayout.Button("查找引用", GUILayout.Width(60)))
                            FindTextureRef();
                    }

                    foreach (var m in m_refMats.Values)
                    {
                        DrawRef(m);
                    }
                    
                }
            }

        }
    }

    void DrawInfo(TextureInfo info)
    {
        //这个大小的贴图显示不显示
        if (m_sizeToggle.Get(info.sizeMax, true) == false)
            return;
        //不压缩的贴图显示不显示
        if (!m_showUncompress && info.tex.name.EndsWith("(uncompress)"))
            return;
        //贴图类型显示不显示
        if (m_textureTypeToggle.Get((int)info.texImporter.textureType, true) == false)
            return;
        //压缩类型显示不显示
        if (m_typeToggle.Get((int)info.type, true) == false)
            return;
        //绘制
        using (new AutoBeginHorizontal())
        {
            EditorGUILayout.TextField(info.tex.name, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(string.Format("{0:f0}x{1:f0}", info.size.x, info.size.y), GUILayout.Width(100));
            EditorGUILayout.LabelField(info.texImporter.textureType.ToString(), GUILayout.Width(100));
            EditorGUILayout.LabelField(info.type.ToString(), GUILayout.Width(150));
            if (info.IsPOT)
                EditorGUILayout.LabelField("是", GUILayout.Width(80));
            else
            {
                using (new AutoChangeColor(Color.red))
                    EditorGUILayout.LabelField("否", GUILayout.Width(80));
            }
            EditorGUILayout.ObjectField(info.tex, typeof(Texture), false, GUILayout.Width(160), GUILayout.Height(m_slider));

            using (new AutoEditorDisabledGroup(System.Array.IndexOf(Selection.objects, info.tex) != -1))
            {
                if (GUILayout.Button("选中", GUILayout.Width(40)))
                {
                    Selection.objects = new UnityEngine.Object[] { info.tex };
                }
            }
        }

    }

    void DrawRef(TextRefMaterial m)
    {
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button(m.expand ? EditorGUIUtility.IconContent("Icon Dropdown") : new GUIContent("›"), EditorStyles.toolbarButton, GUILayout.Width(15)))
                m.expand = !m.expand;

            EditorGUILayout.ObjectField(m.mat, typeof(Material));

            if (GUILayout.Button("场景选中"))
            {
                EditorUtil.SelectMaterial(m.mat);
            }

        }
        if (m.expand)
        {
            using (new AutoEditorIndentLevel())
            {
                foreach (var p in m.prefabs)
                {
                    EditorGUILayout.ObjectField(p, typeof(GameObject));
                }
            }
        }
    }

    void FindTextureRef()
    {
        if (m_texFindRef == null)
            return;
        m_refMats.Clear();
        EditorUtility.DisplayProgressBar("查找贴图引用", string.Format("正在收集预制体"), 0);

        List<GameObject> prefabs = new List<GameObject>();
        EditorUtil.GetAssetAtPath("Effect/Resources", ref prefabs);
        EditorUtil.GetAssetAtPath("UI/fx_ui/Resources", ref prefabs);

        EditorUtility.DisplayProgressBar("查找贴图引用", string.Format("正在查找"), 0.5f);
        foreach (var p in prefabs)
        {
            Renderer[] rs = p.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rs)
            {
                Material[] ms = r.sharedMaterials;
                if (ms == null || ms.Length == 0)
                    continue;

                foreach (Material m in ms)
                {
                    if (m == null)
                        continue;

                    if (m.mainTexture != m_texFindRef)
                        continue;


                    TextRefMaterial refMat;
                    if(!m_refMats.TryGetValue(m,out refMat))
                    {
                        refMat = new TextRefMaterial();
                        m_refMats[m] = refMat;
                        refMat.mat = m;
                    }
                    
                    if (!refMat.prefabs.Contains(p))
                        refMat.prefabs.Add(p);
                }
            }

        }
        EditorUtility.ClearProgressBar();
    }

    
}
