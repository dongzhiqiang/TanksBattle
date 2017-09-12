using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


//材质浏览器
public class SceneMaterialsBrowser : EditorWindow
{
    

    [MenuItem("Art/场景/场景材质浏览器", false, 9)]
    public static void ShowWindow()
    {
        SceneMaterialsBrowser instance = (SceneMaterialsBrowser)EditorWindow.GetWindow(typeof(SceneMaterialsBrowser));//很遗憾，窗口关闭的时候instance就会为null
        instance.minSize = new Vector2(750, 300.0f);
        instance.titleContent = new GUIContent("场景材质浏览器");
        instance.FreshInfos();
    }

    public class MaterialInfo
    {
        public Material mat;
        public Vector2 size;
        public List<GameObject> gos = new List<GameObject>();
        public bool expand =false;
    }
    
    List<MaterialInfo> m_infos = new List<MaterialInfo>();
    bool m_expand =false;
    bool m_order=false;//决定正序倒序
    string m_searchMat="";
    string m_searchTex = "";
    string m_searchGo = "";
    Material m_matOld;
    Material m_matNew;

    #region 监听
    public void OnEnable()
    {
        
    }

    public void OnDisable()
    {
        
    }

    public void Awake()
    {
        

    }

    //更新
    void Update()
    {
        this.Repaint();
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

   
    Vector3 scroll = Vector3.zero;
    //绘制窗口时调用
    void OnGUI()
    {
        //标题
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button(m_expand ? EditorGUIUtility.IconContent("Icon Dropdown") : new GUIContent("›"), EditorStyles.toolbarButton, GUILayout.Width(15)))
            {
                m_expand = !m_expand;
                foreach (var info in m_infos)
                {
                   info.expand =m_expand;
                }
            }
            

            GUILayout.Button("材质", EditorStyles.toolbarButton);
            if(GUILayout.Button("贴图", EditorStyles.toolbarPopup, GUILayout.Width(250)))
            {
                m_order = !m_order;
                FreshInfos();
            }
            GUILayout.Button(goTitle, EditorStyles.toolbarPopup, GUILayout.Width(400));        
        }

        //查找
        using (new AutoBeginHorizontal(EditorStyles.toolbar))
        {
            
            //材质的查找
            using (new AutoBeginHorizontal(GUILayout.ExpandWidth(true)))
            {
                m_searchMat = EditorGUILayout.TextField(GUIContent.none, m_searchMat, "ToolbarSeachTextField", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("", string.IsNullOrEmpty(m_searchMat) ? "ToolbarSeachCancelButton" : "ToolbarSeachCancelButtonEmpty"))
                {
                    m_searchMat = "";
                    //GUI.FocusControl(null);
                }
            }

            //贴图的查找
            using (new AutoBeginHorizontal(GUILayout.Width(250)))
            {
                m_searchTex = EditorGUILayout.TextField(GUIContent.none, m_searchTex, "ToolbarSeachTextField", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("",string.IsNullOrEmpty(m_searchTex)? "ToolbarSeachCancelButton":"ToolbarSeachCancelButtonEmpty"))
                {
                    m_searchTex= "";
                    //GUI.FocusControl(null);
                }
            }

            //游戏对象的查找
            using (new AutoBeginHorizontal(GUILayout.Width(400)))
            {
                m_searchGo = EditorGUILayout.TextField(GUIContent.none, m_searchGo, "ToolbarSeachTextField", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("", string.IsNullOrEmpty(m_searchGo) ? "ToolbarSeachCancelButton" : "ToolbarSeachCancelButtonEmpty"))
                {
                    m_searchGo = "";
                    //GUI.FocusControl(null);
                }
            }
        }

        using (AutoBeginScrollView s = new AutoBeginScrollView(scroll, GUILayout.ExpandHeight(true)))
        {
            scroll =s.Scroll ;
            foreach(var info in m_infos){
                if (info == null || info.mat == null)
                    continue;

                if(DrawInfo(info))
                    GUILayout.Box(GUIContent.none, "AnimationEventBackground", GUILayout.ExpandWidth(true), GUILayout.Height(2));
            }
            

        }

        //材质替换
        using (new AutoBeginHorizontal("AnimationEventBackground",GUILayout.Height(15)))
        {
            m_matOld = (Material)UnityEditor.EditorGUILayout.ObjectField("", m_matOld, typeof(Material), false);
            GUILayout.Label("  =>  ", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));
            m_matNew = (Material)UnityEditor.EditorGUILayout.ObjectField("", m_matNew, typeof(Material), false);
            using (new AutoEditorDisabledGroup(!(m_matOld != null && m_matNew != null && m_matOld != m_matNew)))
            {
                if (GUILayout.Button("转换", GUILayout.Width(50)))
                {
                    MaterialInfo info =Get(m_matOld);
                    if (info != null)
                    {
                        foreach (var go in info.gos)
                        {
                            Renderer render = go.GetComponent<Renderer>();
                            Material[] ms = render.sharedMaterials;
                            if (ms == null || ms.Length == 0)
                                continue;

                            Material[] msNew = new Material[ms.Length];
                            for (int i = 0; i < ms.Length; ++i)
                            {
                                if(ms[i]==null)
                                    continue;
                                if(ms[i]==m_matOld)
                                    msNew[i] = m_matNew;
                                else
                                    msNew[i] = ms[i];
                            }
                            render.sharedMaterials = msNew ;
                        }
                    }
                    FreshInfos();
                }
            }
        }
    }
    List<GameObject> gos = new List<GameObject>();
    bool DrawInfo(MaterialInfo info)
    {
        if (gos.Count > 0 && gos[0] == null)
            return false;
        Texture tex=info.mat.mainTexture ;
        Vector2 size = Vector2.zero;
        if (tex!= null){
            size.x = tex.width;
            size.y = tex.height;
        }

        //先判断要不要显示,三个搜索条件都要符合
        gos.Clear();
        gos.AddRange(info.gos);    
        if (!string.IsNullOrEmpty(m_searchMat))
        {
            if (info.mat.name.IndexOf(m_searchMat)==-1) 
                if(info.mat.shader==null || info.mat.shader.name.IndexOf(m_searchMat)==-1)
                    return false;
        }
        if (!string.IsNullOrEmpty(m_searchTex))
        {
            int i;
            if(int.TryParse(m_searchTex,out i))
            {
                if(size.x != i)
                    return false;
            }
            else
            {
                if(info.mat.mainTexture==null || info.mat.mainTexture.name.IndexOf(m_searchTex)==-1)
                    return false;
            }
        }
        if (!string.IsNullOrEmpty(m_searchGo))
        {
            foreach(var go in info.gos){
                if(go.name.IndexOf(m_searchGo) ==-1)
                    gos.Remove(go);
            }
            if(gos.Count == 0)
                return false;
        }

        //绘制
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button(info.expand ? EditorGUIUtility.IconContent("Icon Dropdown") : new GUIContent("›"), EditorStyles.toolbarButton, GUILayout.Width(15)))
                info.expand = !info.expand;
           
            if (info.expand)
            {
                using (new AutoBeginVertical(GUILayout.ExpandWidth(true)))
                {
                    using (new AutoBeginHorizontal())
                    {
                        EditorGUILayout.ObjectField(info.mat, typeof(Material));
                        if (gos.Count > 0)
                        {
                            bool active = EditorGUILayout.Toggle(gos[0].activeSelf, GUILayout.Width(20));
                            if (gos[0].activeSelf != active)
                            {
                                foreach (var go in gos)
                                {
                                    go.SetActive(active);
                                }
                            }
                        }
                            
                    }
                        

                    EditorGUILayout.ObjectField(info.mat == null ? null : info.mat.shader, typeof(Shader));
                }

                using (new AutoBeginVertical(GUILayout.Width(250)))
                {
                    EditorGUILayout.LabelField(string.Format("{0}x{1}", size.x, size.y), GUILayout.Width(210));
                    EditorGUILayout.ObjectField(tex, typeof(Texture), GUILayout.Width(210), GUILayout.Height(210));
                   
                }
                using (new AutoBeginVertical(GUILayout.Width(400)))
                {
                    foreach (var go in gos)
                    {
                        using (new AutoBeginHorizontal(GUILayout.Width(400)))
                        {
                            EditorGUILayout.ObjectField(go, typeof(GameObject), GUILayout.Width(240));
                            MeshFilter mf = go.GetComponent<MeshFilter>();
                            Mesh mesh = mf != null ? mf.sharedMesh : null;
                            EditorGUILayout.ObjectField(mesh, typeof(Mesh), GUILayout.Width(100));
                            GUILayout.Label(mesh != null ? mesh.vertexCount.ToString() : "", GUILayout.Width(50));
                        }
                    }
                }
            }
            else
            {
                using (new AutoBeginHorizontal())
                {
                    EditorGUILayout.ObjectField(info.mat, typeof(Material));
                    if (gos.Count > 0)
                    {
                        bool active = EditorGUILayout.Toggle(gos[0].activeSelf, GUILayout.Width(20));
                        if (gos[0].activeSelf != active)
                        {
                            foreach (var go in gos)
                            {
                                go.SetActive(active);
                            }
                        }
                    }
                }
                
                EditorGUILayout.LabelField(string.Format("{0}x{1}", size.x, size.y), GUILayout.Width(250));

                if (gos.Count > 0)
                {
                    using (new AutoBeginHorizontal(GUILayout.Width(400)))
                    {
                        EditorGUILayout.ObjectField(gos[0], typeof(GameObject), GUILayout.Width(240));
                        MeshFilter mf = gos[0].GetComponent<MeshFilter>();
                        Mesh mesh = mf != null ? mf.sharedMesh : null;
                        EditorGUILayout.ObjectField(mesh, typeof(Mesh), GUILayout.Width(100));
                        GUILayout.Label(mesh != null ? mesh.vertexCount.ToString() : "", GUILayout.Width(50));
                    }
                        
                }
                else
                    GUILayout.Label(GUIContent.none, GUILayout.Width(400));
            }
            
        }
        return true;
    }

    string goTitle = "";
    
    void FreshInfos()
    {
        m_infos.Clear();

        //收集数据
        int totalCount = 0;
        int totalVertexCount = 0;
        Renderer[] rs = GameObject.FindObjectsOfType<Renderer>();
        foreach (var r in rs)
        {
            if (r == null || r.GetComponent<ParticleRenderer>() != null || r.GetComponent<ParticleSystem>() || r.enabled ==false)
                continue;

            Material[]  ms =r.sharedMaterials;
            if(ms == null || ms.Length ==0)
                continue;

            foreach(Material m in ms){
                totalCount++;

                MeshFilter mf = r.gameObject.GetComponent<MeshFilter>();
                Mesh mesh = mf != null ? mf.sharedMesh : null;
                if (mesh != null)
                    totalVertexCount += mesh.vertexCount;
               
                Get(m,true).gos.Add(r.gameObject);
            }
        }
        goTitle = string.Format("游戏对象:{0}个 场景总顶点:{1}", totalCount, totalVertexCount);

        //排序
        m_infos.SortEx((MaterialInfo a,MaterialInfo b)=>{
            int sizeA = 0;
            int sizeB = 0;
            if(a.mat != null && a.mat.mainTexture!=null)
                sizeA = a.mat.mainTexture.width*a.mat.mainTexture.height;
            if (b.mat != null && b.mat.mainTexture != null)
                sizeB = b.mat.mainTexture.width * b.mat.mainTexture.height;
            return sizeA.CompareTo(sizeB) * (m_order ? 1 : -1);
        });
    }

    MaterialInfo Get(Material mat,bool newIfNoExist =false)
    {
        MaterialInfo info = null;
        foreach (var i in m_infos)
        {
            if(i.mat == mat)
                return i;
        }
        if (newIfNoExist)
        {
            info = new MaterialInfo();
            info.mat = mat;
            m_infos.Add(info);
        }

        return info;
           
    }

}
