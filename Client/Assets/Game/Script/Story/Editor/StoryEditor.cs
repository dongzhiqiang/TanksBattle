using UnityEngine;
using UnityEditor;
using LitJson;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class StoryEditor : EditorWindow
{
    bool m_bTalk;
    bool m_bStory;
    bool m_bPopo;
    string m_roleId = "";

    int m_btnWidth1 = 40;
    bool groupEnabled;
    float m_Time = 10;
    Vector2 m_scroll = Vector2.zero;
    string m_Content = "";
    string m_movieFileName = "";
    string m_movieId = "";
    string m_bgmId = "";
    int m_soundId = 0;
    bool m_isLoop;
    Color m_Color = Color.white;

    string m_fileName;

    string[] m_selStrings = new string[] { "左", "右" };
    int m_localIdx = 0;   //0：左  1：右

    public StorySaveCfg m_SaveCfg;
    List<bool> m_editorAllowList = new List<bool>();

    List<StoryRoleCfg> m_roleCfgList = new List<StoryRoleCfg>();

    [MenuItem("Tool/剧情编辑器 %F6", false, 107)]
    public static void ShowWindow()
    {
        StoryEditor instance = (StoryEditor)EditorWindow.GetWindow(typeof(StoryEditor), false, "剧情编辑器");
        instance.minSize = new Vector2(600, 700);
        instance.autoRepaintOnSceneChange = true;

    }

    void OnEnable()
    {
        m_bTalk = true;
        m_SaveCfg = new StorySaveCfg();
        m_fileName = "";

        StoryRoleCfg.Init();

        foreach(StoryRoleCfg cfg in StoryRoleCfg.m_cfg.Values)
        {
            m_roleCfgList.Add(cfg);
        }
    }

    void OnDisable()
    {
    }

    void OnGUI()
    {

        EditorGUILayoutEx.instance.BeginFadeArea(true, "", "Dialogue", EditorGUILayoutEx.defaultAreaStyle, EditorStyleEx.TopBoxHeaderStyle, this.minSize.y * 0.7f);
        DrawDisplay();
        EditorGUILayoutEx.instance.EndFadeArea();
        GUILayout.Space(0.5f);

        EditorGUILayoutEx.instance.BeginFadeArea(true, "", "ToolArea", EditorGUILayoutEx.defaultAreaStyle, EditorStyleEx.TopBoxHeaderStyle, this.minSize.y * 0.05f);
        DrawToolArea();
        EditorGUILayoutEx.instance.EndFadeArea();
        GUILayout.Space(0.5f);

        EditorGUILayoutEx.instance.BeginFadeArea(true, "", "WriteArea", EditorGUILayoutEx.defaultAreaStyle, EditorStyleEx.TopBoxHeaderStyle, this.minSize.y * 0.25f);
        DrawWriteArea();
        EditorGUILayoutEx.instance.EndFadeArea();

    }

    void DrawDisplay()
    {
        
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button("载入配置"))
            {
                m_fileName = EditorPrefs.GetString("", string.Format("{0}/Config/Resources/story", Application.dataPath));
                string filePath = EditorUtility.OpenFilePanel("读取剧情配置文件", m_fileName, "json");
                if (string.IsNullOrEmpty(filePath))
                    return;

                string[] pathArr = filePath.Split('/');
                string[] pathArr2 = pathArr[pathArr.Length - 1].Split('.');
                m_fileName = pathArr2[0];
                string path = string.Format("story/{0}", m_fileName);

                m_SaveCfg = StorySaveCfg.GetCfg(m_fileName);
                if (m_SaveCfg == null)
                {
                    EditorUtility.DisplayDialog("加载配置", "配置加载失败", "确定");
                    return;
                }
                m_editorAllowList.Clear();
                for (int i = 0; i < m_SaveCfg.storyList.Count; i++)
                    m_editorAllowList.Add(false);
            }

            if (GUILayout.Button("保存配置"))
            {
                if (string.IsNullOrEmpty(m_fileName))
                {
                    EditorUtility.DisplayDialog("保存配置", "文件名不能为空", "确定");
                    return;
                }

                string path = string.Format("story/{0}", m_fileName);
                m_SaveCfg.SaveCfg(path);
            }

            if (GUILayout.Button("另存配置"))
            {
                string filename = string.Format("{0}/Config/Resources/story", Application.dataPath);
                filename = EditorUtility.SaveFilePanel("另存文件为", filename, "story_", "json");
                if (string.IsNullOrEmpty(filename))
                    return;

                string[] pathArr = filename.Split('/');
                string[] pathArr2 = pathArr[pathArr.Length - 1].Split('.');
                m_fileName = pathArr2[0];

                string path = string.Format("story/{0}", m_fileName);
                m_SaveCfg.SaveCfg(path);
            }
        }

        using(new AutoBeginHorizontal())
        {
            EditorGUILayout.LabelField("文件名: ", GUILayout.Width(80));
            EditorGUILayout.TextField(m_fileName, GUILayout.Width(300));
            if (m_SaveCfg != null)
            {
                EditorGUILayout.LabelField("快进", GUILayout.Width(50));
                //m_SaveCfg.canSpeed = EditorGUILayout.Toggle(m_SaveCfg.canSpeed, "快进", GUILayout.Width(80));
                m_SaveCfg.canSpeed = EditorGUILayout.Toggle(m_SaveCfg.canSpeed, GUILayout.Width(70));
            }
        }
        
        if (m_SaveCfg == null)
        {
            GUILayout.Label("配置为空 加载失败-----------");
            return;
        }

        using (AutoBeginScrollView a = new AutoBeginScrollView(m_scroll))
        {
            m_scroll = a.Scroll;

            for (int i = 0; i < m_SaveCfg.storyList.Count; i++)
            {
                        switch (m_SaveCfg.storyList[i].type)
        {
            case StoryType.STORY_TALK:
                DrawTalk(m_SaveCfg.storyList[i], i); break;
            case StoryType.STORY_MOVIE:
                DrawMovie(m_SaveCfg.storyList[i], i); break;
            case StoryType.STORY_POP:
                DrawPop(m_SaveCfg.storyList[i], i); break;
        }

            }

        }
    }

    void DrawWriteArea()
    {

        GUILayout.Space(10);
        if (m_bTalk || m_bPopo)
        {
            using (new AutoBeginHorizontal())
            {
                if (!string.IsNullOrEmpty(m_roleId))
                    EditorGUILayout.LabelField(StoryRoleCfg.Get(m_roleId).name, GUILayout.Width(80));
                if (GUILayout.Button("选择", GUILayout.Width(m_btnWidth1)))
                {
                    GenericMenu selectMenu = new GenericMenu();

                    for (int i = 0; i < m_roleCfgList.Count; i++)
                    {
                        int roleIdx = i;
                        selectMenu.AddItem(new GUIContent(m_roleCfgList[i].name), false, () =>
                        {
                            m_roleId = m_roleCfgList[roleIdx].id;
                        });
                    }
                    selectMenu.ShowAsContext();
                }

                GUILayout.Space(5);

                EditorGUILayout.LabelField("时间：", GUILayout.Width(m_btnWidth1));


                m_Time = EditorGUILayout.Slider(m_Time, 0, 120, GUILayout.Width(this.minSize.x * 0.5f));
                if (m_bTalk)
                {
                    GUIStyle toggleStyle = new GUIStyle(EditorStyles.toggleGroup);
                    toggleStyle.fontSize = 12;
                    m_localIdx = GUILayout.Toolbar(m_localIdx, m_selStrings, toggleStyle, GUILayout.Width(80));
                }

                EditorGUILayout.LabelField("配音id：", GUILayout.Width(50));
                m_soundId = EditorGUILayout.IntField(m_soundId, GUILayout.Width(35));
            }

            GUILayout.Space(15);

            using (new AutoBeginHorizontal())
            {
                GUILayout.Space(20);
                using (new AutoChangeBkColor(Color.white))
                {
                    GUI.skin.textField.fontSize = 13;
                    //m_Content = GUI.TextField(new Rect(15, 50, this.minSize.x - 20, this.minSize.y * 0.2f - 80), m_Content, 100);

                    GUIStyle style = new GUIStyle(EditorStyles.textArea);
                    style.fontSize = 18;
                    m_Content = EditorGUILayout.TextArea(m_Content, style, GUILayout.Width(this.minSize.x * 0.7f), GUILayout.Height(60));

                }
            }

            if (GUI.Button(new Rect(this.minSize.x - 100, 115, 85, 40), "发 送"))
            {
                SendContent(StoryType.STORY_TALK);
            }

        }
      
        if (m_bStory)
        {
            GUI.Label(new Rect(50, 10, 200, 40), string.Format("过场预置体：  {0}", m_movieFileName));
            if (!string.IsNullOrEmpty(m_movieFileName))
            {
                GUI.Label(new Rect(50, 45, 120, 40), "过场名：");
                m_movieId = GUI.TextField(new Rect(120, 45, 120, 20), m_movieId);
                m_isLoop = GUI.Toggle(new Rect(300, 45, 200, 30), m_isLoop, "是否循环");

                GUI.Label(new Rect(50, 70, 120, 40), "过场背景音：");
                m_bgmId = GUI.TextField(new Rect(120, 70, 120, 20), m_bgmId);
            }

            if (GUI.Button(new Rect(this.minSize.x - 400, 100, 125, 20), "选择剧情"))
            {
                string fileName = EditorPrefs.GetString("", string.Format("{0}/Scene/Resources", Application.dataPath));
                string filePath = EditorUtility.OpenFilePanel("选择剧情", fileName, "");
                if (string.IsNullOrEmpty(filePath))
                    return;

                m_movieId = "";
                m_isLoop = false;

                string[] pathArr = filePath.Split('/');
                string[] pathArr2 = pathArr[pathArr.Length - 1].Split('.');
                m_movieFileName = pathArr2[0];

            }

            if (GUI.Button(new Rect(this.minSize.x - 100, 115, 85, 40), "发 送"))
            {
                SendContent(StoryType.STORY_MOVIE);
            }
        }

    }

    void DrawToolArea()
    {

        GUILayout.Space(8);
        using (new AutoBeginHorizontal())
        {
            Color tmp = GUI.color;
            GUILayout.Space(40);

            if (m_bTalk)
                GUI.color = Color.green;

            EditorGUILayout.LabelField("对话：", GUILayout.Width(m_btnWidth1));
            m_bTalk = EditorGUILayout.Toggle(m_bTalk, GUILayout.Width(70));
            if (m_bTalk)
                m_bStory = false;

            GUI.color = tmp;

            GUILayout.Space(10);
            if (m_bStory)
                GUI.color = Color.green;

            EditorGUILayout.LabelField("剧情：", GUILayout.Width(m_btnWidth1));
            m_bStory = EditorGUILayout.Toggle(m_bStory, GUILayout.Width(70));
            if (m_bStory)
            {
                m_bTalk = false;
                m_bPopo = false;
            }

            GUI.color = tmp;

            GUILayout.Space(10);
            if (m_bPopo)
                GUI.color = Color.green;
            EditorGUILayout.LabelField("气泡：", GUILayout.Width(m_btnWidth1));
            m_bPopo = EditorGUILayout.Toggle(m_bPopo, GUILayout.Width(70));
            if (m_bPopo)
                m_bStory = false;

            GUI.color = tmp;
        }

    }

    void DrawTalk(StoryCfg talkInfo, int idx)
    {
        StoryTalkCfg info = talkInfo as StoryTalkCfg;
        m_editorAllowList[idx] = EditorGUILayout.BeginToggleGroup("", m_editorAllowList[idx]);

        using (new AutoBeginHorizontal())
        {
            if (info.localIdx == 1)
                GUILayout.Space(420);
            EditorGUILayout.LabelField(string.Format("{0}：", StoryRoleCfg.Get(info.roleId).name), GUILayout.Width(60));

            info.time = int.Parse(EditorGUILayout.TextField(info.time.ToString(), GUILayout.Width(40)));

            if (GUILayout.Button("删除", GUILayout.Width(40)))
            {
                RemoveCfg(idx);
                return;
            }
        }
        using (new AutoBeginHorizontal())
        {
            if (info.localIdx == 1)
                GUILayout.Space(170);
            using (new AutoChangeBkColor(Color.cyan))
            {
                GUIStyle style = new GUIStyle(EditorStyles.textArea);
                style.fontSize = 18;
                info.content = EditorGUILayout.TextArea(info.content, style, GUILayout.Width(this.minSize.x * 0.7f), GUILayout.Height(60));
            }
        }

        EditorGUILayout.EndToggleGroup();
        GUILayout.Space(10);
    }
    void DrawMovie(StoryCfg moveInfo, int idx)
    {
        m_editorAllowList[idx] = EditorGUILayout.BeginToggleGroup("", m_editorAllowList[idx]);


        StoryMovieCfg info = moveInfo as StoryMovieCfg;

        GUIStyle style = new GUIStyle(EditorStyles.label);

        using (new AutoBeginHorizontal())
        {
            Color tmp = GUI.color;
            GUILayout.Space(10);
            GUI.color = Color.yellow;

            style.fontSize = 20;
            EditorGUILayout.LabelField("播放剧情：", info.movieId, style, GUILayout.Height(50), GUILayout.Width(400));

            if (GUILayout.Button("删除", GUILayout.Width(40)))
            {
                RemoveCfg(idx);
                return;
            }

            GUI.color = tmp;
        }


        EditorGUILayout.EndToggleGroup();
        GUILayout.Space(10);

    }
    void DrawPop(StoryCfg popInfo, int idx)
    {

    }

    void AddCfg(StoryCfg cfg)
    {
        m_editorAllowList.Add(false);
        m_SaveCfg.storyList.Add(cfg);
    }

    void RemoveCfg(int idx)
    {
        m_SaveCfg.storyList.RemoveAt(idx);
        m_editorAllowList.RemoveAt(idx);
    }

    void SendContent(StoryType type)
    {
        if (type == StoryType.STORY_TALK)
        {
            if (string.IsNullOrEmpty(m_Content))
            {
                EditorUtility.DisplayDialog("错误", "内容为空", "确定");
                return;
            }
            if (string.IsNullOrEmpty(m_roleId))
            {
                EditorUtility.DisplayDialog("错误", "没有选角色", "确定");
                return;
            }

            StoryTalkCfg talk = new StoryTalkCfg();
            talk.time = (int)m_Time;
            talk.content = m_Content;
            talk.localIdx = m_localIdx;
            talk.roleId = m_roleId;
            talk.soundId = m_soundId;
            AddCfg(talk);

            m_Content = "";
            m_roleId = "";
            m_soundId = 0;
        }

        if (type == StoryType.STORY_MOVIE)
        {
            if (string.IsNullOrEmpty(m_movieFileName))
            {
                EditorUtility.DisplayDialog("", "未选中过场文件", "确定");
                return;
            }

            if (string.IsNullOrEmpty(m_movieId))
            {
                EditorUtility.DisplayDialog("", "未添加剧情ID", "确定");
                return;
            }

            int bgmId = 0;
            if (!int.TryParse(m_bgmId, out bgmId))
            {
                EditorUtility.DisplayDialog("", "背景id要填数字类型", "确定");
                return;
            }

            StoryMovieCfg movie = new StoryMovieCfg();
            movie.movieId = m_movieId;
            movie.prefab = m_movieFileName;
            movie.isLoop = m_isLoop;
            movie.soundId = bgmId;

            AddCfg(movie);

            m_movieFileName = "";
            m_isLoop = false;
            m_bgmId = "";
            m_movieId = "";
        }

        if (type == StoryType.STORY_POP)
        {

        }

        
    }
}
