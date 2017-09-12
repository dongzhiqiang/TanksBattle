using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//关卡编辑器
[CustomEditor(typeof(Room))]
public class SceneEditor : Editor
{

    public static string editorAssets = "Assets/AstarPathfindingProject/Editor/EditorAssets";
    const string scriptsFolder = "Assets/AstarPathfindingProject";

    public static List<SceneEditorBorn> mBornViewList = new List<SceneEditorBorn>();
    public static List<SceneEditorRefresh> mRefreshViewList = new List<SceneEditorRefresh>();
    public static List<SceneEditorCheck> mCheckViewList = new List<SceneEditorCheck>();
    public static List<SceneEditorArea> mAreaViewList = new List<SceneEditorArea>();
    public static List<SceneEditorCamera> mCameraViewList = new List<SceneEditorCamera>();
    public static List<SceneEditorPoss> mPossViewList = new List<SceneEditorPoss>();

    public static string mShowWaveGroupId = "";

    public static string[] mEveTypeDesc;
    public static string[] mActTypeDesc;

    public static Dictionary<string, ActionCfg> mAllActionCfg = null;
    public static Dictionary<string, EventCfg> mAllEventCfg = null;

    //public static EditorGUILayoutEx guiLayoutx;


    bool bOpenBorn = false;
    bool bOpenAddBorn = false;
    bool bOpenRefresh = false;
    bool bOpenAddRefresh = false;
    bool bOpenCheck = false;
    bool bOpenArea = false;
    bool bOpenCamera = false;
    bool bOpenPoss= false;

    public SceneCfg.SceneData sceneData { get; private set; }

    public void OnEnable()
    {
        SceneMgr.SceneDebug = false;
        Room room = target as Room;
        if (LevelMgr.instance.CurLevel == null)
            return;

        room.OnDrawGizmosCallback = OnDrawGizmos;
        sceneData = SceneMgr.instance.SceneData;
        if (!LevelMgr.instance.bEditorLoaded)
        {
            LoadData();
            LevelMgr.instance.bEditorLoaded = true;
        }
    }

    public void OnDisable()
    {

    }

    void LoadData()
    {
        mAllActionCfg = ActionCfgFactory.instance.GetAllActionCfg();
        mAllEventCfg = EventCfgFactory.instance.GetAllEventCfg();

        mEveTypeDesc = new string[mAllEventCfg.Keys.Count];
        mAllEventCfg.Keys.CopyTo(mEveTypeDesc, 0);

        mActTypeDesc = new string[mAllActionCfg.Keys.Count];
        mAllActionCfg.Keys.CopyTo(mActTypeDesc, 0);

        mBornViewList.Clear();
        mCheckViewList.Clear();
        mRefreshViewList.Clear();
        mAreaViewList.Clear();
        mCameraViewList.Clear();
        mPossViewList.Clear();

        //出生点
        for (int i = 0; i < sceneData.mBornList.Count; i++)
        {
            SceneEditorBorn bornView = new SceneEditorBorn();
            bornView.Init(sceneData.mBornList[i]);
            mBornViewList.Add(bornView);
        }

        //刷新点
        for (int i = 0; i < sceneData.mRefGroupList.Count; i++)
        {
            SceneEditorRefresh refreshView = new SceneEditorRefresh();
            refreshView.Init(sceneData.mRefGroupList[i]);
            mRefreshViewList.Add(refreshView);
        }

        //事件
        for (int i = 0; i < sceneData.mCheckList.Count; i++)
        {
            SceneEditorCheck checkView = new SceneEditorCheck();
            checkView.Init(sceneData.mCheckList[i]);
            mCheckViewList.Add(checkView);
        }

        //区域
        for (int i = 0; i < sceneData.mAreaList.Count; i++)
        {
            SceneEditorArea areaView = new SceneEditorArea();
            areaView.Init(sceneData.mAreaList[i]);
            mAreaViewList.Add(areaView);
        }

        for (int i = 0; i < sceneData.mCameraList.Count; i++)
        {
            SceneEditorCamera cameraView = new SceneEditorCamera();
            cameraView.Init(sceneData.mCameraList[i]);
            mCameraViewList.Add(cameraView);
        }

        //路径点
        for (int i = 0; i < sceneData.mPoss.Count; i++)
        {
            SceneEditorPoss possView = new SceneEditorPoss();
            possView.Init(sceneData.mPoss[i]);
            mPossViewList.Add(possView);
        }


    }

    string SaveData()
    {
        Room room = target as Room;
        if (LevelMgr.instance.CurLevel == null)
            return "场景mScene获取失败";

        SceneMgr.instance.SceneData.mBornList.Clear();
        SceneMgr.instance.SceneData.mCheckList.Clear();
        SceneMgr.instance.SceneData.mRefGroupList.Clear();
        SceneMgr.instance.SceneData.mAreaList.Clear();
        SceneMgr.instance.SceneData.mStoryIdList.Clear();
        SceneMgr.instance.SceneData.mGroupIdList.Clear();
        SceneMgr.instance.SceneData.mCameraList.Clear();
        SceneMgr.instance.SceneData.mPoss.Clear();

        string error = "";
        for (int i = 0; i < mBornViewList.Count; i++)
        {
            string err = mBornViewList[i].Save();
            if (err != "")
                error = string.Format("{0}{1}", error, err);
            SceneMgr.instance.SceneData.mBornList.Add(mBornViewList[i].mBornInfo);
        }

        for (int i = 0; i < mRefreshViewList.Count; i++)
        {
            string err = mRefreshViewList[i].Save();
            if (err != "")
                error = string.Format("{0}{1}", error, err);
            SceneMgr.instance.SceneData.mRefGroupList.Add(mRefreshViewList[i].mRefreshGroup);
        }

        for (int i = 0; i < mCheckViewList.Count; i++)
        {
            string err = mCheckViewList[i].Save();
            if (err != "")
                error = string.Format("{0}{1}", error, err);
            SceneMgr.instance.SceneData.mCheckList.Add(mCheckViewList[i].mCheck);
        }

        for (int i = 0; i < mAreaViewList.Count; i++)
        {
            string err = mAreaViewList[i].Save();
            if (err != "")
                error = string.Format("{0}{1}", error, err);
            SceneMgr.instance.SceneData.mAreaList.Add(mAreaViewList[i].mArea);
        }

        for (int i = 0; i < mCameraViewList.Count; i++)
        {
            string err = mCameraViewList[i].Save();
            if (err != "")
                error = string.Format("{0}{1}", error, err);
            SceneMgr.instance.SceneData.mCameraList.Add(mCameraViewList[i].mCameraCfg);
        }

        for (int i = 0; i < mPossViewList.Count; i++)
        {
            string err = mPossViewList[i].Save();
            if (err != "")
                error = string.Format("{0}{1}", error, err);
            SceneMgr.instance.SceneData.mPoss.Add(mPossViewList[i].mCheck);
        }

        //保存显示波数的刷新组缩涉及到的刷新组ID
        SceneMgr.instance.SceneData.mShowWaveGroupId = mShowWaveGroupId;
        if (!string.IsNullOrEmpty(mShowWaveGroupId))
        {
            SceneMgr.instance.SceneData.mGroupIdList.Add(SceneEditor.mShowWaveGroupId);
            SceneCfg.RefGroupCfg groupCfg = GetRefreshGroupCfg(SceneEditor.mShowWaveGroupId);
            string groupId = groupCfg.nextGroupFlag;
            while (!string.IsNullOrEmpty(groupId) && groupId != SceneEditor.mShowWaveGroupId)
            {
                SceneMgr.instance.SceneData.mGroupIdList.Add(groupId);
                groupCfg = GetRefreshGroupCfg(groupId);
                groupId = groupCfg.nextGroupFlag;
            }
        }
        
        return error;
    }

    SceneCfg.RefGroupCfg GetRefreshGroupCfg(string groupId)
    {
        foreach(SceneCfg.RefGroupCfg cfg in SceneMgr.instance.SceneData.mRefGroupList)
        {
            if (cfg.groupFlag == groupId)
                return cfg;
        }
        return null;
    }

    void RepaintSceneView()
    {
        if (!Application.isPlaying || EditorApplication.isPaused) SceneView.RepaintAll();
    }

    public override void OnInspectorGUI()
    {
        if (sceneData == null)
            return;


        GUILayout.Space(5);

        SceneMgr.SceneDebug = GUILayout.Toggle(SceneMgr.SceneDebug, "debug:", GUILayout.Width(200));
        GUILayout.Space(5);

        EditorGUILayoutEx.instance.ClearFadeAreaStack();

        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button("Open"))
            {
                string filename = EditorPrefs.GetString("SceneLogicEditor:ReadConfig", string.Format("{0}/Config/Resources/scene", Application.dataPath));
                string filePath = EditorUtility.OpenFilePanel("读取场景配置文件", filename, "json");
                if (string.IsNullOrEmpty(filePath))
                    return;

                string[] pathArr = filePath.Split('/');
                string path = string.Format("scene/{0}", pathArr[pathArr.Length - 1]);
                sceneData = Util.LoadJsonFile<SceneCfg.SceneData>(path);

                Room room = target as Room;
                if (LevelMgr.instance.CurLevel == null || sceneData == null)
                    return;
                sceneData.sceneName = pathArr[pathArr.Length - 1];
                SceneMgr.instance.SceneData = sceneData;

                LoadData();

            }

            string error = "";

            if (GUILayout.Button("Save"))
            {
                if (EditorUtility.DisplayDialog("保存配置", "确定要保存配置吗？", "确定", "取消"))
                {
                    Room room = target as Room;
                    if (LevelMgr.instance.CurLevel == null)
                        return;

                    error = SaveData();
                    SceneMgr.instance.SceneData.Save();
                }
                
            }

            if (GUILayout.Button("Save As"))
            {
                string filename = string.Format("{0}/Config/Resources/scene", Application.dataPath);
                filename = EditorUtility.SaveFilePanel("保存场景配置文件", filename, "sceneCfg", "json");
                if (string.IsNullOrEmpty(filename))
                    return;
                Room room = target as Room;
                if (LevelMgr.instance.CurLevel == null)
                    return;

                error = SaveData();
                string[] pathArr = filename.Split('/');
                string path = string.Format("scene/{0}", pathArr[pathArr.Length - 1]);
                string name = path.Split('.')[0];
                SceneMgr.instance.SceneData.Save(name);
            }

            if (error != "")
                EditorUtility.DisplayDialog("配置错误", error, "确定");

        }

        if (GUILayout.Button("显示主角"))
        {
            Room.instance.StartCoroutine(ViewHero());
        }

        GUILayout.Space(5);

        DrawBorn();
        DrawRefresh();
        DrawCheck();
        DrawArea();
        DrawCamera();
        DrawPoss();

        GUILayout.Space(5);

        this.Repaint();

    }

    IEnumerator ViewHero()
    {
        if (CameraTriggerMgr.instance != null && CameraTriggerMgr.instance.CurGroup != null)
        {
            Vector3 pos = CameraTriggerMgr.instance.CurGroup.m_bornPos;
            Vector3 ea = Vector3.zero;

            //创建主角
            Role hero = RoleMgr.instance.Hero;
            if (hero != null)
            {
                RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
                cxt.OnClear();
                cxt.pos = pos;
                cxt.euler = ea;
                hero.Load(cxt);
                while (hero.State != Role.enState.alive)
                {
                    yield return 0;
                }

                //设置到相机管理器
                CameraMgr.instance.SetFollow(hero.transform);
            }
            else
                Debuger.LogError("主角不存在，无法创建");
        }
        else
        {
            Debuger.Log("不创建主角场景");
        }
    }
    public void OnSceneGUI()
    {
        for (int i = 0; i < mBornViewList.Count; i++)
        {
            mBornViewList[i].OnSceneGUI();
        }

        for (int i = 0; i < mRefreshViewList.Count; i++)
        {
            mRefreshViewList[i].OnSceneGUI();
        }

        for (int i = 0; i < mCheckViewList.Count; i++)
        {
            mCheckViewList[i].OnSceneGUI();
        }

    }

    #region DrawMainArea

    void DrawBorn()
    {

        EditorGUILayoutEx.FadeArea graphsFadeArea = EditorGUILayoutEx.instance.BeginFadeArea(bOpenBorn, "出 生 点", "showGraphInspectors", EditorGUILayoutEx.defaultAreaStyle, EditorStyleEx.TopBoxHeaderStyle);
        bOpenBorn = graphsFadeArea.open;

        if (graphsFadeArea.Show())
        {
            for (int i = 0; i < mBornViewList.Count; i++)
            {
                GUILayout.Space(5);

                Color tmp1 = GUI.color;
                EditorGUILayoutEx.FadeArea topFadeArea = EditorGUILayoutEx.instance.BeginFadeArea(true, "", "bornInfo" + i, EditorStyleEx.GraphBoxStyle);

                Color tmp2 = GUI.color;
                GUI.color = tmp1;

                GUILayout.BeginHorizontal();

                mBornViewList[i].name = GUILayout.TextField(mBornViewList[i].name, EditorGUILayoutEx.defaultLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

                if (mBornViewList[i].name == "")
                {
                    mBornViewList[i].name = string.Format("出生点 {0}", i + 1);
                }

                if (GUILayout.Button("", EditorGUILayoutEx.defaultLabelStyle))
                {
                    mBornViewList[i].bOpen = !mBornViewList[i].bOpen;
                    RepaintSceneView();
                    SaveData();
                }

                bool bSceneView = GUILayout.Toggle(mBornViewList[i].bSceneView, "Draw Gizmos", EditorStyleEx.GraphGizmoButtonStyle);
                if (bSceneView != mBornViewList[i].bSceneView)
                {
                    mBornViewList[i].bSceneView = bSceneView;
                    RepaintSceneView();
                }

                if (GUILayout.Button("Delete", EditorStyleEx.GraphDeleteButtonStyle))
                {
                    mBornViewList.Remove(mBornViewList[i]);
                    return;
                }
                GUILayout.EndHorizontal();

                if (topFadeArea.Show())
                {
                    GUI.color = tmp2;

                    mBornViewList[i].OnInspectorGUI();
                }

                EditorGUILayoutEx.instance.EndFadeArea();
            }
        }

        GUILayout.Space(6);

        if (GUILayout.Button("添 加 出 生 点"))
        {
            if (RoleMgr.instance.Hero == null || RoleMgr.instance.Hero.State != Role.enState.alive)
            {
                Debug.Log("先创建主角");
                return;
            }

            SceneEditorBorn bornView = new SceneEditorBorn();
            bornView.Init();
            bornView.mBornInfo.mPosition = RoleMgr.instance.Hero.transform.position;
            bornView.mBornInfo.mEulerAngles = RoleMgr.instance.Hero.transform.eulerAngles;
            mBornViewList.Add(bornView);
            bOpenAddBorn = false;
        }

        EditorGUILayoutEx.instance.EndFadeArea();

    }

    void DrawRefresh()
    {
        EditorGUILayoutEx.FadeArea fadeArea = EditorGUILayoutEx.instance.BeginFadeArea(bOpenRefresh, "刷 新 组", "Refresh", EditorGUILayoutEx.defaultAreaStyle, EditorStyleEx.TopBoxHeaderStyle);
        bOpenRefresh = fadeArea.open;
        if (fadeArea.Show())
        {
            for (int i = 0; i < mRefreshViewList.Count; i++)
            {
                GUILayout.Space(5);

                string graphGUIDString = "refreshGroup" + i;

                Color tmp1 = GUI.color;
                EditorGUILayoutEx.FadeArea topFadeArea = EditorGUILayoutEx.instance.BeginFadeArea(mRefreshViewList[i].bOpen, "", graphGUIDString, EditorStyleEx.GraphBoxStyle);

                Color tmp2 = GUI.color;
                GUI.color = tmp1;

                using (new AutoBeginHorizontal())
                {

                    mRefreshViewList[i].groupFlag = GUILayout.TextField(mRefreshViewList[i].groupFlag, EditorGUILayoutEx.defaultLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

                    if (mRefreshViewList[i].groupFlag == "" && Event.current.type == EventType.Repaint && GUI.GetNameOfFocusedControl() != graphGUIDString)
                    {
                        mRefreshViewList[i].groupFlag = string.Format("Group{0}", i + 1);
                    }

                    if (GUILayout.Button("", EditorGUILayoutEx.defaultLabelStyle))
                    {
                        mRefreshViewList[i].bOpen = !mRefreshViewList[i].bOpen;
                        SaveData();
                    }

                    bool bSceneShow = GUILayout.Toggle(mRefreshViewList[i].bSceneShow, "Draw Refrsh", EditorStyleEx.GraphGizmoButtonStyle);
                    if (bSceneShow != mRefreshViewList[i].bSceneShow)
                    {
                        mRefreshViewList[i].bSceneShow = bSceneShow;
                        RepaintSceneView();
                    }

                    if (GUILayout.Button("Delete", EditorStyleEx.GraphDeleteButtonStyle))
                    {
                        if (EditorUtility.DisplayDialog("删除刷新组", "确定删除刷新组？", "确定", "取消"))
                        {
                            mRefreshViewList.Remove(mRefreshViewList[i]);
                            return;
                        }
                    }
                }

                GUI.color = tmp2;

                Color c = GUI.color;
                GUI.color = Color.yellow;
                if (topFadeArea.Show())
                {
                    mRefreshViewList[i].OnInspectorGUI();
                }
                if (GUILayout.Button("添 加 刷 新 点"))
                {
                    mRefreshViewList[i].AddPoint();

                }
                GUI.color = c;
                EditorGUILayoutEx.instance.EndFadeArea();

            }
        }

        //显示波数在事件里控制
        //using (new AutoBeginHorizontal())
        //{
        //    mShowWaveGroupId = EditorGUILayout.TextField("显示波数的刷新组", mShowWaveGroupId);
        //    if (GUILayout.Button("选择"))
        //    {
        //        SceneCfg.SceneData sceneData = SceneMgr.instance.SceneData;
        //        GenericMenu selectMenu = new GenericMenu();
        //        for (int i = 0; i < sceneData.mRefGroupList.Count; i++)
        //        {
        //            int type1 = i;
        //            selectMenu.AddItem(new GUIContent(sceneData.mRefGroupList[i].groupFlag), false, () =>
        //            {
        //                mShowWaveGroupId = sceneData.mRefGroupList[type1].groupFlag;
        //            });
        //        }
        //        selectMenu.ShowAsContext();
        //        return;
        //    }
        //}

        GUILayout.Space(6);
        Color c2 = GUI.color;
        GUI.color = Color.green;
        if (GUILayout.Button("添 加 刷 新 组"))
        {
            SceneEditorRefresh refreshView = new SceneEditorRefresh();
            refreshView.Init();
            mRefreshViewList.Add(refreshView);
            bOpenAddRefresh = false;
            SaveData();
        }
        GUI.color = c2;
        EditorGUILayoutEx.instance.EndFadeArea();
    }

    void DrawCheck()
    {
        EditorGUILayoutEx.FadeArea fadeArea = EditorGUILayoutEx.instance.BeginFadeArea(bOpenCheck, "事 件", "Check", EditorGUILayoutEx.defaultAreaStyle, EditorStyleEx.TopBoxHeaderStyle);
        bOpenCheck = fadeArea.open;
        if (fadeArea.Show())
        {
            for (int i = 0; i < mCheckViewList.Count; i++)
            {
                string graphGUIDString = "check" + i;

                Color tmp1 = GUI.color;
                EditorGUILayoutEx.FadeArea topFadeArea = EditorGUILayoutEx.instance.BeginFadeArea(mCheckViewList[i].bOpen, "", graphGUIDString, EditorStyleEx.GraphBoxStyle);

                Color tmp2 = GUI.color;
                GUI.color = tmp1;

                using (new AutoBeginHorizontal())
                {
                    mCheckViewList[i].checkFlag = GUILayout.TextField(mCheckViewList[i].checkFlag, EditorGUILayoutEx.defaultLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                    if (string.IsNullOrEmpty(mCheckViewList[i].checkFlag))
                    {
                        mCheckViewList[i].checkFlag = string.Format("Check{0}", i + 1);
                    }

                    if (GUILayout.Button("", EditorGUILayoutEx.defaultLabelStyle))
                    {
                        mCheckViewList[i].bOpen = !mCheckViewList[i].bOpen;
                        SaveData();
                    }

                    if (GUILayout.Button("整理", EditorGUILayoutEx.defaultLabelStyle))
                    {
                        mCheckViewList[i].SortAction();
                        return;
                    }

                    if (GUILayout.Button("Delete", EditorStyleEx.GraphDeleteButtonStyle))
                    {
                        if (EditorUtility.DisplayDialog("删除事件组", "确定删除事件组？", "确定", "取消"))
                        {
                            mCheckViewList.Remove(mCheckViewList[i]);
                            return;
                        }
                    }
                }

                if (topFadeArea.Show())
                {
                    GUI.color = tmp2;
                    mCheckViewList[i].OnInspectorGUI();
                }

                EditorGUILayoutEx.instance.EndFadeArea();
            }
        }

        GUILayout.Space(6);

        if (GUILayout.Button("添 加 事 件"))
        {
            SceneEditorCheck checkView = new SceneEditorCheck();
            checkView.Init();
            mCheckViewList.Add(checkView);
            SaveData();
        }

        EditorGUILayoutEx.instance.EndFadeArea();
    }

    void DrawArea()
    {
        EditorGUILayoutEx.FadeArea fadeArea = EditorGUILayoutEx.instance.BeginFadeArea(bOpenArea, "区 域", "Area", EditorGUILayoutEx.defaultAreaStyle, EditorStyleEx.TopBoxHeaderStyle);
        bOpenArea = fadeArea.open;
        if (fadeArea.Show())
        {
            for (int i = 0; i < mAreaViewList.Count; i++)
            {
                string graphGUIDString = "area" + i;

                Color tmp1 = GUI.color;
                EditorGUILayoutEx.FadeArea topFadeArea = EditorGUILayoutEx.instance.BeginFadeArea(mAreaViewList[i].bOpen, "", graphGUIDString, EditorStyleEx.GraphBoxStyle);

                Color tmp2 = GUI.color;
                GUI.color = tmp1;

                using (new AutoBeginHorizontal())
                {
                    mAreaViewList[i].areaFlag = GUILayout.TextField(mAreaViewList[i].areaFlag, EditorGUILayoutEx.defaultLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                    if (string.IsNullOrEmpty(mAreaViewList[i].areaFlag))
                    {
                        mAreaViewList[i].areaFlag = string.Format("Area{0}", i + 1);
                    }

                    if (GUILayout.Button("", EditorGUILayoutEx.defaultLabelStyle))
                    {
                        mAreaViewList[i].bOpen = !mAreaViewList[i].bOpen;
                        SaveData();
                    }

                    if (GUILayout.Button("Delete", EditorStyleEx.GraphDeleteButtonStyle))
                    {
                        if (EditorUtility.DisplayDialog("删除区域", "确定删除区域？", "确定", "取消"))
                        {
                            if (mAreaViewList[i].mArea.areaType == SceneCfg.AreaType.DangBan)
                            {
                                if(SceneMgr.instance.mDangbanDict.ContainsKey(mAreaViewList[i].areaFlag))
                                    SceneMgr.instance.mDangbanDict[mAreaViewList[i].areaFlag].gameObject.SetActive(false);
                            }
                            else
                                GameObject.Destroy(Room.instance.mAreaGroup.transform.FindChild(mAreaViewList[i].areaFlag).gameObject);
                            mAreaViewList.Remove(mAreaViewList[i]);
                            return;
                        }
                    }
                }

                if (topFadeArea.Show())
                {
                    GUI.color = tmp2;
                    mAreaViewList[i].OnInspectorGUI();
                }

                EditorGUILayoutEx.instance.EndFadeArea();
            }
        }
        GUILayout.Space(6);

        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button("添加区域"))
            {
                SceneEditorArea areaView = new SceneEditorArea();
                areaView.Init(SceneCfg.AreaType.Normal);
                mAreaViewList.Add(areaView);
                SaveData();
            }

            if (GUILayout.Button("添加挡板"))
            {
                SceneEditorArea areaView = new SceneEditorArea();
                areaView.Init(SceneCfg.AreaType.DangBan);
                mAreaViewList.Add(areaView);
                SaveData();
            }
        }
        

        EditorGUILayoutEx.instance.EndFadeArea();
    }

    void DrawCamera()
    {
        EditorGUILayoutEx.FadeArea fadeArea = EditorGUILayoutEx.instance.BeginFadeArea(bOpenCamera, "镜 头", "Camera", EditorGUILayoutEx.defaultAreaStyle, EditorStyleEx.TopBoxHeaderStyle);
        bOpenCamera = fadeArea.open;
        if (fadeArea.Show())
        {
            for (int i = 0; i < mCameraViewList.Count; i++)
            {
                string graphGUIDString = "camera" + i;

                Color tmp1 = GUI.color;
                EditorGUILayoutEx.FadeArea topFadeArea = EditorGUILayoutEx.instance.BeginFadeArea(mCameraViewList[i].bOpen, "", graphGUIDString, EditorStyleEx.GraphBoxStyle);

                Color tmp2 = GUI.color;
                GUI.color = tmp1;

                using (new AutoBeginHorizontal())
                {
                    mCameraViewList[i].cameraFlag = GUILayout.TextField(mCameraViewList[i].cameraFlag, EditorGUILayoutEx.defaultLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                    if (string.IsNullOrEmpty(mCameraViewList[i].cameraFlag))
                    {
                        mCameraViewList[i].cameraFlag = string.Format("Camera{0}", i + 1);
                    }

                    if (GUILayout.Button("", EditorGUILayoutEx.defaultLabelStyle))
                    {
                        mCameraViewList[i].bOpen = !mCameraViewList[i].bOpen;
                        SaveData();
                    }

                    if (GUILayout.Button("测试"))
                    {
                        Room.instance.StartCoroutine(ActionCamera.CoStartCamera(mCameraViewList[i].mCameraCfg.cameraList));
                    }

                    if (GUILayout.Button("Delete", EditorStyleEx.GraphDeleteButtonStyle))
                    {
                        if (EditorUtility.DisplayDialog("删除镜头", "确定删除镜头？", "确定", "取消"))
                        {
                            //if (mAreaViewList[i].mArea.areaType == SceneCfg.AreaType.DangBan)
                            //{
                            //    if (SceneMgr.instance.mDangbanDict.ContainsKey(mAreaViewList[i].areaFlag))
                            //        SceneMgr.instance.mDangbanDict[mAreaViewList[i].areaFlag].gameObject.SetActive(false);
                            //}
                            //else
                            //    GameObject.Destroy(Room.instance.mAreaGroup.transform.FindChild(mAreaViewList[i].areaFlag).gameObject);
                            mCameraViewList.Remove(mCameraViewList[i]);
                            return;
                        }
                    }
                }

                if (topFadeArea.Show())
                {
                    GUI.color = tmp2;
                    mCameraViewList[i].OnInspectorGUI();
                }

                EditorGUILayoutEx.instance.EndFadeArea();
            }
        }
        GUILayout.Space(6);

        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button("添加镜头"))
            {
                SceneEditorCamera cameraView = new SceneEditorCamera();
                cameraView.Init();
                mCameraViewList.Add(cameraView);
                SaveData();
            }
        }


        EditorGUILayoutEx.instance.EndFadeArea();
    }

    void DrawPoss()
    {
        Transform source = null;
        if (Application.isPlaying && RoleMgr.instance != null && RoleMgr.instance.Hero != null && RoleMgr.instance.Hero.State == Role.enState.alive)
            source = RoleMgr.instance.Hero.transform;

        EditorGUILayoutEx.FadeArea fadeArea = EditorGUILayoutEx.instance.BeginFadeArea(bOpenPoss, "路径点", "scene_editor_poss", EditorGUILayoutEx.defaultAreaStyle, EditorStyleEx.TopBoxHeaderStyle);
        bOpenPoss = fadeArea.open;
        if (fadeArea.Show())
        {
            for (int i = 0; i < mPossViewList.Count; i++)
            {
                string graphGUIDString = "scene_editor_poss" + i;

                Color tmp1 = GUI.color;
                EditorGUILayoutEx.FadeArea topFadeArea = EditorGUILayoutEx.instance.BeginFadeArea(mPossViewList[i].bOpen, "", graphGUIDString, EditorStyleEx.GraphBoxStyle);

                Color tmp2 = GUI.color;
                GUI.color = tmp1;

                using (new AutoBeginHorizontal())
                {
                    mPossViewList[i].mCheck.name = EditorGUILayout.TextField(mPossViewList[i].mCheck.name, EditorGUILayoutEx.defaultLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                   
                    if (GUILayout.Button("", EditorGUILayoutEx.defaultLabelStyle, GUILayout.ExpandWidth(true)))
                    {
                        mPossViewList[i].bOpen = !mPossViewList[i].bOpen;
                        SaveData();
                    }

                    if (source != null && GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus More"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                    {
                        mPossViewList[i].mCheck.ps.Add(source.position);
                    }

                    if (GUILayout.Button("Delete", EditorStyleEx.GraphDeleteButtonStyle, GUILayout.ExpandWidth(false)))
                    {
                        mPossViewList.Remove(mPossViewList[i]);
                    }
                }

                if (topFadeArea.Show())
                {
                    GUI.color = tmp2;
                    mPossViewList[i].OnInspectorGUI();
                }

                EditorGUILayoutEx.instance.EndFadeArea();
                
            }
        }
        GUILayout.Space(6);
        if(source!=null&&GUILayout.Button("添加路径"))
        {
            SceneEditorPoss possView = new SceneEditorPoss();
            possView.Init();
            possView.mCheck.name = GetUniqueName(mPossViewList);
            possView.mCheck.ps.Add(source.position);
            mPossViewList.Add(possView);
            SaveData();
        }

        
        EditorGUILayoutEx.instance.EndFadeArea();
    }

    public static bool Contain(List<SceneEditorPoss> poss, string name)
    {
        foreach (var c in poss)
        {
            if (c.mCheck.name == name)
                return true;
        }
        return false;
    }

    public static string GetUniqueName(List<SceneEditorPoss> poss)
    {
        int i = poss.Count + 1;
        string n = "路径点" + i;
        while (Contain(poss, n))
        {
            ++i;
            n = "路径点" + i;
        }

        return n;

    }
    #endregion
    void OnDrawGizmos()
    {
        for (int i = 0; i < mBornViewList.Count; i++)
        {
            mBornViewList[i].OnDrawGizmos();
        }

        for (int i = 0; i < mRefreshViewList.Count; i++)
        {
            mRefreshViewList[i].OnDrawGizmos();
        }

        for (int i = 0; i < mCheckViewList.Count; i++)
        {
            mCheckViewList[i].OnDrawGizmos();
        }

        for (int i = 0; i < mAreaViewList.Count; i++)
        {
            mAreaViewList[i].OnDrawGizmos();
        }

        if(bOpenPoss)
        {
            for (int i = 0; i < mPossViewList.Count; i++)
            {
                mPossViewList[i].OnDrawGizmos();
            }
        }

        if (DebugUI.bCreateMonster)
        {
            DebugUI.bCreateMonster = false;
            if (mRefreshViewList.Count <= 0 || mRefreshViewList[0].pointPosList.Count <= 0)
            {
                Debuger.LogError("第一个刷新组没有配置刷新点");
                return;
            }
            List<AiTestCfg> cfgList = DebugUI.testCfgList;

            GameObjectPool.CheckPreLoading = true;//标记下，用于检查有没有漏预加载的地方

            for (int i = 0; i < cfgList.Count; i++)
                RoleCfg.PreLoad(cfgList[i].roleId);

            Room.instance.StartCoroutine(CreateTestNpc());
        }
    }

    IEnumerator CreateTestNpc()
    {
        int resCount = GameObjectPool.GetPool(GameObjectPool.enPool.Role).RequestCount + GameObjectPool.GetPool(GameObjectPool.enPool.Other).RequestCount + GameObjectPool.GetPool(GameObjectPool.enPool.Fx).RequestCount;
        while (resCount > 0)
            yield return 0;

        GameObjectPool.CheckPreLoading = false;//标记下，用于检查有没有漏预加载的地方

        List<AiTestCfg> cfgList = DebugUI.testCfgList;
        List<SceneEditorRefresh.PointTransform> points = mRefreshViewList[0].pointPosList;

        for (int i = 0; i < cfgList.Count; i++)
        {
            if (i >= points.Count)
                yield break;

            RoleCfg.PreLoad(cfgList[i].roleId);//战斗相关的资源都是预加载的所以这里要预加载下
            RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
            RoleCfg cfg = RoleCfg.Get(cfgList[i].roleId);
            cxt.OnClear();
            cxt.Init(Util.GenerateGUID(), "", cfgList[i].roleId, 1, enCamp.camp2, points[i].pos, points[i].dir, cfg.bornType, cfg.deadType, cfg.groundDeadType, cfgList[i].aiType);
            RoleMgr.instance.CreateRole(cxt);
        }
    }

}

