using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;

//角色浏览器
public class RoleBrowserWindow : EditorWindow
{
    public class RoleBrowserTag
    {
        public string tag;
        public List<string> roleIds = new List<string>();

        public void Search(List<string> rs,string search){
            rs.Clear(); 
            if(string.IsNullOrEmpty(search)){
                rs.AddRange(roleIds);   
            }
            else
            {
                foreach (string s in roleIds)
                {
                    if(s.Contains(search))
                        rs.Add(s);
                }
            }


        }
         
    }
    public class RoleBrowserInfo
    {
        public List<RoleBrowserTag> tags = new List<RoleBrowserTag>();
        string[] tagNames;
        Dictionary<string,RoleBrowserTag> tagsByName = new Dictionary<string,RoleBrowserTag>();

        public string[] TagName { get { return tagNames; } }

        public static RoleBrowserInfo Load()
        {
            string json =EditorPrefs.GetString("RoleBrowserInfo","");
            RoleBrowserInfo info;
            if(string.IsNullOrEmpty(json))
                info = new RoleBrowserInfo();
            else
                info = JsonMapper.ToObject<RoleBrowserInfo>(json);
            info.Reset();
            return info;
        }

        public void Save()
        {
            EditorPrefs.SetString("RoleBrowserInfo", JsonMapper.ToJson(this, false));
            
        }

        public void Reset(){
            //如果一个也没有那么新增加历史记录
            if(tags.Count ==0){
                RoleBrowserTag t = new RoleBrowserTag();
                t.tag = "历史记录";
                tags.Add(t);
            }

            //收集信息
            tagsByName.Clear();
            List<string> l = new List<string>() { RECORD, ALLROLE };
            foreach(RoleBrowserTag t in tags){
                tagsByName[t.tag] = t;
                if(RECORD != t.tag)
                    l.Add(t.tag);
            }
            tagNames = l.ToArray();

            //全部角色是动态获取的
            RoleBrowserTag allTag = new RoleBrowserTag();
            allTag.tag = ALLROLE;
            allTag.roleIds = new List<string>(RoleCfg.RoleIds);
            tagsByName[allTag.tag] = allTag;
        }

        public RoleBrowserTag GetTag(string tag)
        {
            RoleBrowserTag t = tagsByName.Get(tag);
            if(t == null)
                Debuger.LogError("找不到tag:{0}", tag);
            return t;
        }

        public void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || Array.IndexOf(tagNames, tag) != -1)
            {
                Debuger.LogError("逻辑错误，已经存在或者命名为空:{0}", tag);
                return;
            }

            RoleBrowserTag t = new RoleBrowserTag();
            t.tag = tag;
            tags.Add(t);
            Save();
            Reset();
        }

        public void RemoveTag(string tag)
        {
            if (tag == RECORD || tag == ALLROLE || Array.IndexOf(tagNames, tag) == -1)
            {
                Debuger.LogError("逻辑错误，找不到或者不能删除的:{0}",tag);
                return;
            }

            foreach (RoleBrowserTag t in tags)
            {
                if (t.tag == tag)
                {
                    tags.Remove(t);
                    break;
                }
            }
            Save();
            Reset();
        }

        public void AddTagRole(string tag,string roleId)
        {
            RoleBrowserTag t =GetTag(tag);
            if(t == null )return;
            if (t.roleIds.IndexOf(roleId)!=-1)
            {
                Debuger.LogError("逻辑错误，重复添加了角色到标签记录里:{0}", roleId);
                return;
            }
            t.roleIds.Add(roleId);
            Save();
        }

        public void RemoveTagRole(string tag, string roleId)
        {
            RoleBrowserTag t = GetTag(tag);
            if (t == null) return;
            if (t.roleIds.IndexOf(roleId) == -1)return;
            t.roleIds.Remove(roleId);
            Save();
        }
       
    }

    const string RECORD = "历史记录";
    const string ALLROLE = "全部角色";

    public RoleBrowserInfo m_info;
    public Action<string> m_onSel;
    public int m_slider = 60;
    string m_curTag = RECORD;
    string m_tagAdd = "新标签";
    string m_search = "";
    
    List<string> m_roles = new List<string>();


    [MenuItem("Tool/角色浏览器", false, 102)]
    public static void ShowWindow()
    {
        ShowWindow(null);
    }

    public static void ShowWindow(Action<string> onSel)
    {
        RoleBrowserWindow instance = (RoleBrowserWindow)EditorWindow.GetWindow(typeof(RoleBrowserWindow),true);//很遗憾，窗口关闭的时候instance就会为null
        instance.minSize = new Vector2(400.0f, 200.0f);
        instance.titleContent = new GUIContent("角色浏览器");
        instance.autoRepaintOnSceneChange = true;
        instance.m_info = RoleBrowserInfo.Load();
        instance.m_onSel = onSel;
        instance.m_slider = EditorPrefs.GetInt("RoleBrowserSlider", 60);
    }

    
    #region 监听
    public void Awake()
    {
        

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

    
    //绘制窗口时调用
    void OnGUI()
    {
        using(new AutoBeginHorizontal())
        {
            using(new AutoBeginVertical("PreferencesSectionBox",GUILayout.Width(120))){
                DrawAddTag();
                DrawTags();
                
            }
            using (new AutoBeginVertical())
            {
                DrawRoles();
            }
        }
    }

    void DrawTags()
    {
        //GUILayout.Space(10f);
        Rect rect;
        bool isCur;
        string tag;
        for (int i = 0; i < m_info.TagName.Length; ++i)
        {
            tag = m_info.TagName[i];
            rect = GUILayoutUtility.GetRect(new GUIContent(tag), "PreferencesSection", GUILayout.ExpandWidth(true));
            isCur = (m_curTag == tag);
            if (isCur)
                GUI.Box(rect, "", "ServerUpdateChangesetOn");
            if (GUI.Toggle(rect, isCur, tag, "PreferencesSection") && !isCur)
            {
                m_search= "";
                m_curTag = tag;
                if(tag != RECORD && tag!= ALLROLE){
                    m_tagAdd = m_info.TagName[i];
                }
                m_info.GetTag(tag).Search(m_roles, m_search);
            }
                
        }
        //GUILayout.Box("", EditorStyles.label, GUILayout.ExpandHeight(true));
    }

    void DrawAddTag()
    {
        using (new AutoBeginHorizontal(EditorStyles.miniButtonMid))
        {
            m_tagAdd = GUILayout.TextField(m_tagAdd);
            bool isAdd = Array.IndexOf(m_info.TagName, m_tagAdd) == -1;//是要增加或者要删除某个标签
            if (m_tagAdd != RECORD && m_tagAdd != ALLROLE && GUILayout.Button(isAdd ? EditorGUIUtility.IconContent("Toolbar Plus") : EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.miniButtonMid, GUILayout.Width(30)))
            {
                if (isAdd)
                    m_info.AddTag(m_tagAdd);
                else
                    m_info.RemoveTag(m_tagAdd);
            }
        }
        
    }
    
    Vector2 previewScroll = Vector2.zero;
    void DrawRoles()
    {
        // Search
        EditorGUILayout.BeginHorizontal();//"Toolbar"
        string s = GUILayout.TextField(m_search, "ToolbarSeachTextField");
        if (s != m_search)
        {
            m_search =s;
            m_info.GetTag(m_curTag).Search(m_roles, m_search);
        }
        if (GUILayout.Button("", "ToolbarSeachCancelButton"))
        {
            m_search = "";
            m_info.GetTag(m_curTag).Search(m_roles, m_search);
            GUI.FocusControl(null);
        }

         
        int slider =(int)GUILayout.HorizontalSlider(m_slider,40,250,GUILayout.Width(100));
        if(slider!=m_slider){
            m_slider = slider;
            EditorPrefs.SetInt("RoleBrowserSlider", slider);
        }
        EditorGUILayout.EndHorizontal();

        
        using (AutoBeginScrollView a = new AutoBeginScrollView(previewScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
        {
            previewScroll = a.Scroll;
            int colCount = Mathf.Max((int)((this.position.width - 120) / (m_slider+150f)), 1);
            int itemWidth = (int)((this.position.width -120-30)/colCount);
            int i =0;
            RoleCfg cfg;
            foreach (string roleId in m_roles)
            {
                cfg = RoleCfg.Get(roleId);
                if (cfg == null)
                {
                    m_roles.Remove(roleId);
                    m_info.RemoveTagRole(m_curTag,roleId);
                    if(i>0)
                        EditorGUILayout.EndHorizontal();
                    break;
                }
                if (i % colCount == 0)
                {
                    if(i !=0)
                        EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }

                EditorGUILayout.BeginHorizontal("box", i == colCount - 1 ? GUILayout.ExpandWidth(true) : GUILayout.Width(itemWidth));

                Rect rt = GUILayoutUtility.GetRect(m_slider, m_slider, GUILayout.Width(m_slider), GUILayout.Height(m_slider));
                Texture2D tex =EditorUtil.GetModTexture(cfg.mod);
                //GUI.DrawTexture(rt, tex, ScaleMode.StretchToFill);
                //GUILayout.Box(tex, GUILayout.Width(m_slider), GUILayout.Height(m_slider));
                GUI.DrawTexture(rt, tex, ScaleMode.StretchToFill);

                //信息及标签编辑
                EditorGUILayout.BeginVertical();
                GUILayout.Label(roleId);
                GUILayout.Label( cfg.name);
                if (m_curTag!= ALLROLE && GUILayout.Button("删除标签"))
                {
                    if (EditorUtility.DisplayDialog("", string.Format("是否要删除{0}的{1}标签", roleId, m_curTag), "是", "否"))
                    {
                        m_roles.Remove(roleId);
                        m_info.RemoveTagRole(m_curTag, roleId);
                        if (i > 0)
                            EditorGUILayout.EndHorizontal();
                        break;
                    }
                }
                
                if (GUILayout.Button("增加其他标签", EditorStyles.popup))
                {
                    GenericMenu contextMenu = new GenericMenu();
                    string itemRoleId = roleId;
                    for(int j=0;j<m_info.TagName.Length;++j){
                        string tagName = m_info.TagName[j];
                        if(tagName==ALLROLE || tagName==RECORD)
                            continue;
                        contextMenu.AddItem(new GUIContent(tagName), false, () => m_info.AddTagRole(tagName, itemRoleId));
                        contextMenu.ShowAsContext();
                    }                    
                }

                //选中按钮
                if (m_onSel != null)
                {
                    if (GUILayout.Button("选中"))
                    {
                        RoleBrowserTag tag= m_info.GetTag(RECORD);
                        if (tag != null && tag.roleIds.IndexOf(cfg.id) == -1)
                        {
                            m_info.AddTagRole(RECORD, cfg.id);
                        }
                        m_onSel(cfg.id);
                        this.Close();
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                ++i;

            }

            if(i>0)
                EditorGUILayout.EndHorizontal();
        }
    }

}
