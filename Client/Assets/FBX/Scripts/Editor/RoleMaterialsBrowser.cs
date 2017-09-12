using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


//角色材质浏览器
public class RoleMaterialsBrowser : EditorWindow
{
    

    [MenuItem("Art/角色/角色材质浏览器", false, 9)]
    public static void ShowWindow()
    {
        RoleMaterialsBrowser instance = (RoleMaterialsBrowser)EditorWindow.GetWindow(typeof(RoleMaterialsBrowser));//很遗憾，窗口关闭的时候instance就会为null
        instance.minSize = new Vector2(750, 300.0f);
        instance.titleContent = new GUIContent("角色材质浏览器");
        instance.FreshInfos();
    }

    public class MaterialInfo
    {
        public Material mat;
        public Texture tex;
        public Texture bump;
        public Texture mask;
        public List<GameObject> gos = new List<GameObject>();
        public bool expand =false;
    }
    
    List<MaterialInfo> m_infos = new List<MaterialInfo>();
    bool m_expand =false;
    bool m_order=false;//决定正序倒序
    string m_searchMat="";
    string m_searchTex = "";
    string m_searchGo = "";
    

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
            if(GUILayout.Button("贴图", EditorStyles.toolbarPopup, GUILayout.Width(480)))
            {
                m_order = !m_order;
                FreshInfos();
            }
            GUILayout.Button("游戏对象", EditorStyles.toolbarButton, GUILayout.Width(250));        
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
            using (new AutoBeginHorizontal(GUILayout.Width(480)))
            {
                m_searchTex = EditorGUILayout.TextField(GUIContent.none, m_searchTex, "ToolbarSeachTextField", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("",string.IsNullOrEmpty(m_searchTex)? "ToolbarSeachCancelButton":"ToolbarSeachCancelButtonEmpty"))
                {
                    m_searchTex= "";
                    //GUI.FocusControl(null);
                }
            }

            //游戏对象的查找
            using (new AutoBeginHorizontal(GUILayout.Width(250)))
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
                if(DrawInfo(info))
                    GUILayout.Box(GUIContent.none, "AnimationEventBackground", GUILayout.ExpandWidth(true), GUILayout.Height(2));
            }
            

        }

    }
    List<GameObject> gos = new List<GameObject>();
    bool DrawInfo(MaterialInfo info)
    {
        Texture tex=info.tex;
        Vector2 size = Vector2.zero;
        if (tex!= null){
            size.x = tex.width;
            size.y = tex.height;
        }
        Texture bump = info.bump;
        Texture mask = info.mask;

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
                    EditorGUILayout.ObjectField(info.mat, typeof(Material));
                    EditorGUILayout.ObjectField(info.mat == null ? null : info.mat.shader, typeof(Shader));
                }

                using (new AutoBeginVertical(GUILayout.Width(150)))
                {
                    EditorGUILayout.LabelField(string.Format("{0}x{1}", size.x, size.y), GUILayout.Width(150));
                    EditorGUILayout.ObjectField(tex, typeof(Texture), GUILayout.Width(150), GUILayout.Height(150));    
                }
                using (new AutoBeginVertical(GUILayout.Width(150)))
                {
                   
                    if (bump!=null)
                    {
                        size.x = bump.width;
                        size.y = bump.height;
                        EditorGUILayout.LabelField(string.Format("{0}x{1}", size.x, size.y), GUILayout.Width(150));
                        EditorGUILayout.ObjectField(bump, typeof(Texture), GUILayout.Width(150), GUILayout.Height(150));
                    }
                    else
                        EditorGUILayout.LabelField("", GUILayout.Width(150));

                }
                using (new AutoBeginVertical(GUILayout.Width(150)))
                {

                    if (mask != null)
                    {
                        size.x = mask.width;
                        size.y = mask.height;
                        EditorGUILayout.LabelField(string.Format("{0}x{1}", size.x, size.y), GUILayout.Width(150));
                        EditorGUILayout.ObjectField(mask, typeof(Texture), GUILayout.Width(150), GUILayout.Height(150));
                    }
                    else
                        EditorGUILayout.LabelField("", GUILayout.Width(150));

                }
                using (new AutoBeginVertical(GUILayout.Width(250)))
                {
                    foreach (var go in gos)
                    {
                        EditorGUILayout.ObjectField(go, typeof(GameObject),false);
                    }
                }
            }
            else
            {
                EditorGUILayout.ObjectField(info.mat, typeof(Material));

                EditorGUILayout.LabelField(string.Format("{0}x{1}", size.x, size.y), GUILayout.Width(450));

                if (gos.Count > 0)
                    EditorGUILayout.ObjectField(gos[0], typeof(GameObject), false, GUILayout.Width(250));
                else
                    GUILayout.Label(GUIContent.none, GUILayout.Width(250));
            }
            
        }
        return true;
    }


    void FreshInfos()
    {
        m_infos.Clear();

        string[] files = System.IO.Directory.GetFiles(Application.dataPath + "/FBX/Resources", "*.prefab", System.IO.SearchOption.AllDirectories);
        int i = Application.dataPath.Length - 6;

        foreach (var file in files)
        {
            GameObject prefab =AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(i));
            if (prefab == null)
                continue;

            Renderer[] rs = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var r in rs)
            {
                if (r == null || r.GetComponent<ParticleRenderer>() != null || r.GetComponent<ParticleSystem>() || r.enabled == false)
                    continue;

                Material[] ms = r.sharedMaterials;
                if (ms == null || ms.Length == 0)
                    continue;

                foreach (Material m in ms)
                {
                    if (m == null)
                        continue;
                    Get(m, true).gos.Add(prefab);
                }
            }
        }
        
        //排序
        m_infos.SortEx((MaterialInfo a,MaterialInfo b)=>{
            int sizeA = 0;
            int sizeB = 0;
            if(a.tex!=null)
                sizeA = a.tex.width * a.tex.height;
            if (b.tex != null)
                sizeB = b.tex.width * b.tex.height;
            
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
            info.tex = info.mat.mainTexture;
            if (info.mat.HasProperty("_BumpMap"))
                info.bump = info.mat.GetTexture("_BumpMap");

            if (info.mat.HasProperty("_Mask"))
                info.mask = info.mat.GetTexture("_Mask");
                      
            

            m_infos.Add(info);
        }

        return info;
           
    }

}
