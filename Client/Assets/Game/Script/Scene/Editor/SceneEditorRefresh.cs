using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class SceneEditorRefresh : SceneEditorBase
{
    public class PointTransform
    {
        public Vector3 pos;
        public Vector3 dir;
        public string name;
        public enCamp camp;
        public string ai;
        public string roleId;
        public string bornTypeId;
        public string deadTypeId;
        public string groundDeadTypeId;
        public float bornDelay;
        public int isShowBloodBar;
        public int isShowFriendBloodBar;
        public int isShowTargetBar;
        public int buffId;
        public GameObject goModule;
        public List<Vector3> pathfindingList;
        public int boxAddNum;
        public SceneCfg.BoxStateType boxAddType;
        public int boxBuffId;
        public HateCfg hate = new HateCfg();

        public PointTransform(Vector3 pos, Vector3 dir, string roleId)
        {
            this.pos = pos;
            this.dir = dir;
            camp = enCamp.camp2;
            buffId = 0;
            name = "";
            this.roleId = roleId;
            ai = "-1";
            bornTypeId = "";
            deadTypeId = "";
            groundDeadTypeId = "";
            goModule = null;
            bornDelay = 0;
            isShowBloodBar = 0;
            isShowFriendBloodBar = 0;
            isShowTargetBar = 0;
            pathfindingList = new List<Vector3>();
            boxAddNum = 0;
            boxAddType = 0;
            boxBuffId = 0;
        }
    }

    public SceneCfg.RefGroupCfg mRefreshGroup;
    public string groupNpcId = "xiaobin_01";
    public string ai = "-1";
    public enCamp camp = enCamp.camp2;
    public int buffId = 0;

    public bool bAddFlag;
    public bool bSceneShow;
    public bool bOpenSetting;

    public List<PointTransform> pointPosList = new List<PointTransform>();

    public bool bOpen { get; set; }

    public bool bShowModule { get; set; }

    public string groupFlag { get { return mRefreshGroup.groupFlag; } set { mRefreshGroup.groupFlag = value; } }

    public override void Init()
    {
        mRefreshGroup = new SceneCfg.RefGroupCfg();
        pointPosList.Clear();
        bAddFlag = true;
        bSceneShow = true;
        bOpen = false;
        bOpenSetting = false;
    }

    public void Init(SceneCfg.RefGroupCfg refresh, string newFlag = "")
    {
        mRefreshGroup = new SceneCfg.RefGroupCfg();
        mRefreshGroup.Init(refresh, newFlag);
        bAddFlag = true;
        bOpen = false;
        bSceneShow = true;

        pointPosList.Clear();

        for (int i = 0; i < mRefreshGroup.Points.Count; i++)
        {
            PointTransform point = new PointTransform(mRefreshGroup.Points[i].pos, mRefreshGroup.Points[i].dir, mRefreshGroup.Points[i].roleId);
            point.name = mRefreshGroup.Points[i].pointFlag;
            point.camp = mRefreshGroup.Points[i].camp;
            point.buffId = mRefreshGroup.Points[i].buffId;
            point.bornTypeId = mRefreshGroup.Points[i].bornTypeId;
            point.deadTypeId = mRefreshGroup.Points[i].deadTypeId;
            point.groundDeadTypeId = mRefreshGroup.Points[i].groundDeadTypeId;
            point.bornDelay = mRefreshGroup.Points[i].bornDelay;
            point.ai = mRefreshGroup.Points[i].ai;
            point.isShowBloodBar = mRefreshGroup.Points[i].isShowBloodBar;
            point.isShowFriendBloodBar = mRefreshGroup.Points[i].isShowFriendBloodBar;
            point.isShowTargetBar = mRefreshGroup.Points[i].isShowTargetBar;
            point.pathfindingList = mRefreshGroup.Points[i].pathfindingList;
            point.boxAddNum = mRefreshGroup.Points[i].boxAddNum;
            point.boxAddType = mRefreshGroup.Points[i].boxAddType;
            point.boxBuffId = mRefreshGroup.Points[i].boxBuffId;
            point.hate.CopyFrom(mRefreshGroup.Points[i].hate);
            pointPosList.Add(point);
        }
    }

    public override string Save()
    {
        string error = "";
        base.Save();

        mRefreshGroup.Points.Clear();
        for (int i = 0; i < pointPosList.Count; i++)
        {
            SceneCfg.RefPointCfg rp = new SceneCfg.RefPointCfg();
            rp.dir = pointPosList[i].dir;
            rp.pos = pointPosList[i].pos;
            rp.camp = pointPosList[i].camp;
            rp.roleId = pointPosList[i].roleId;
            rp.pointFlag = pointPosList[i].name;
            rp.ai = pointPosList[i].ai;
            rp.bornDelay = pointPosList[i].bornDelay;
            rp.bornTypeId = pointPosList[i].bornTypeId;
            rp.isShowBloodBar = pointPosList[i].isShowBloodBar;
            rp.isShowFriendBloodBar= pointPosList[i].isShowFriendBloodBar;
            rp.isShowTargetBar = pointPosList[i].isShowTargetBar;
            rp.pathfindingList = pointPosList[i].pathfindingList;
            rp.deadTypeId = pointPosList[i].deadTypeId;
            rp.groundDeadTypeId = pointPosList[i].groundDeadTypeId;
            rp.buffId = pointPosList[i].buffId;
            rp.boxAddNum = pointPosList[i].boxAddNum;
            rp.boxAddType = pointPosList[i].boxAddType;
            rp.boxBuffId = pointPosList[i].boxBuffId;
            rp.hate.CopyFrom(pointPosList[i].hate);
            mRefreshGroup.Points.Add(rp);

            if (rp.roleId == "")
                error += string.Format("刷新点{0}为空", pointPosList[i].name);
                
        }

        return error;
    }

    public void AddPoint()
    {
        PointTransform pT = new PointTransform(Vector3.zero, Vector3.up, "");
        if (RoleMgr.instance.Hero != null)
        {
            pT.pos = RoleMgr.instance.Hero.transform.position;
            pT.dir = RoleMgr.instance.Hero.transform.eulerAngles;
        }
        pointPosList.Add(pT);
        if (string.IsNullOrEmpty(pT.name))
            pT.name = string.Format("{0}-{1}", groupFlag, pointPosList.Count);
    }

    public void OnInspectorGUI()
    {

        //SceneEditor.guiLayoutx.BeginFadeArea(true, "Editor", "RefreshSettings", SceneEditor.graphBoxStyle);
        Color color1 = GUI.color;
        GUI.color = Color.green;
        using (new AutoBeginHorizontal())
        {
            groupNpcId = EditorGUILayout.TextField("组NpcId：", groupNpcId, EditorGUILayoutEx.defaultLabelStyle);

            if (GUILayout.Button("选择"))
            {
                GenericMenu selectMenu = new GenericMenu();
                for (int i = 0; i < RoleCfg.RoleNames.Length; i++)
                {
                    int type = i;
                    selectMenu.AddItem(new GUIContent(RoleCfg.RoleNames[i]), false, () =>
                    {
                        groupNpcId = RoleCfg.RoleIds[type];
                    });
                }
                selectMenu.ShowAsContext();
            }

            if (GUILayout.Button("应用"))
            {
                for (int i = 0; i < pointPosList.Count; i++)
                {
                    pointPosList[i].roleId = groupNpcId;
                }
            }
        }

        using (new AutoBeginHorizontal())
        {
            ai = EditorGUILayout.TextField("组AI", ai);
            if (GUILayout.Button("应用"))
            {
                for (int i = 0; i < pointPosList.Count; i++)
                {
                    pointPosList[i].ai = ai;
                }
            }
        }

        using (new AutoBeginHorizontal())
        {
            camp = (enCamp)EditorGUILayout.Popup("组阵营", (int)camp, SceneCfg.CampName);
            if (GUILayout.Button("应用"))
            {
                for (int i = 0; i < pointPosList.Count; i++)
                {
                    pointPosList[i].camp = camp;
                }
            }
        }

        using (new AutoBeginHorizontal())
        {
            buffId = EditorGUILayout.IntField("组出生buff", buffId, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            if (GUILayout.Button("应用"))
            {
                for (int i = 0; i < pointPosList.Count; i++)
                {
                    pointPosList[i].buffId = buffId;
                }
            }
        }

        using (new AutoBeginHorizontal())
        {
            //暂时没有刷新组的延时
            mRefreshGroup.delayTime = EditorGUILayout.FloatField("下波刷新延时", mRefreshGroup.delayTime, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            if (GUILayout.Button("应用"))
            {
                for (int i = 0; i < pointPosList.Count; i++)
                {
                    pointPosList[i].bornDelay = mRefreshGroup.delayTime;
                }
            }
        }  
        mRefreshGroup.refreshNum = EditorGUILayout.IntField("刷新波数", mRefreshGroup.refreshNum, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

        mRefreshGroup.refreshType = (SceneCfg.RefreshType)EditorGUILayout.Popup("刷新方式", (int)mRefreshGroup.refreshType, SceneCfg.RefreshTypeName);
        if (mRefreshGroup.refreshType == SceneCfg.RefreshType.RandomNum)
        {
            mRefreshGroup.pointNum = EditorGUILayout.IntField("刷怪个数", mRefreshGroup.pointNum, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
        }

        using (new AutoBeginHorizontal())
        {
            mRefreshGroup.nextGroupFlag = EditorGUILayout.TextField("组死后刷新", mRefreshGroup.nextGroupFlag);
            if (GUILayout.Button("选择"))
            {
                SceneCfg.SceneData sceneData = SceneMgr.instance.SceneData;

                GenericMenu selectMenu = new GenericMenu();
                for (int i = 0; i < sceneData.mRefGroupList.Count; i++)
                {
                    if (sceneData.mRefGroupList[i].groupFlag == mRefreshGroup.groupFlag)
                        continue;

                    int type1 = i;
                    selectMenu.AddItem(new GUIContent(sceneData.mRefGroupList[i].groupFlag), false, () =>
                    {
                        mRefreshGroup.nextGroupFlag = sceneData.mRefGroupList[type1].groupFlag;
                    });
                }
                selectMenu.ShowAsContext();
                return;
            }

        }

        mRefreshGroup.nextWaveDelay = EditorGUILayout.IntField("下组间隔时间", mRefreshGroup.nextWaveDelay);
        
        bShowModule = EditorGUILayout.Toggle("显示模型", bShowModule);

        //复制刷新组
        if (GUILayout.Button("Copy", EditorStyleEx.GraphBoxStyle, GUILayout.Width(40)))
        {
            SceneEditorRefresh refreshView = new SceneEditorRefresh();
            refreshView.Init(this.mRefreshGroup, "Group" + (SceneEditor.mRefreshViewList.Count + 1));
            SceneEditor.mRefreshViewList.Add(refreshView);
        }

        GUI.color = color1;

        //SceneEditor.guiLayoutx.EndFadeArea();

        for (int i = 0; i < pointPosList.Count; i++)
        {
            Separator();
            using (new AutoBeginHorizontal())
            {
                pointPosList[i].name = GUILayout.TextField(pointPosList[i].name, EditorGUILayoutEx.defaultLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                if (GUILayout.Button("", EditorGUILayoutEx.defaultLabelStyle))
                {
                }
                if (GUILayout.Button("Delete", EditorStyleEx.GraphDeleteButtonStyle))
                {
                    pointPosList.Remove(pointPosList[i]);
                    return;
                }
            }

            using (new AutoBeginHorizontal())
            {
                pointPosList[i].roleId = EditorGUILayout.TextField("NpcId:", pointPosList[i].roleId, EditorGUILayoutEx.defaultLabelStyle);

                if (GUILayout.Button("选择"))
                {
                    int type1 = i;
                    GenericMenu selectMenu = new GenericMenu();
                    for (int j = 0; j < RoleCfg.RoleNames.Length; j++)
                    {
                        int type2 = j;
                        selectMenu.AddItem(new GUIContent(RoleCfg.RoleNames[j]), false, () =>
                        {
                            pointPosList[type1].roleId = RoleCfg.RoleIds[type2];
                        });
                    }
                    selectMenu.ShowAsContext();
                }
            }
            if (!string.IsNullOrEmpty(pointPosList[i].roleId) && RoleCfg.Get(pointPosList[i].roleId).addBuffType > 0)
            {
                pointPosList[i].boxAddType = (SceneCfg.BoxStateType)EditorGUILayout.Popup("状态类型", (int)pointPosList[i].boxAddType, SceneCfg.BoxStateTypeName);
                pointPosList[i].boxAddNum = EditorGUILayout.IntField("状态值", pointPosList[i].boxAddNum);
                pointPosList[i].boxBuffId = EditorGUILayout.IntField("buffId", pointPosList[i].boxBuffId);
            }
            pointPosList[i].camp = (enCamp)EditorGUILayout.Popup("阵营", (int)pointPosList[i].camp, SceneCfg.CampName);
            pointPosList[i].ai = EditorGUILayout.TextField("AI", pointPosList[i].ai);
            pointPosList[i].isShowBloodBar = (EditorGUILayout.Toggle("显示血条", pointPosList[i].isShowBloodBar == 1)) ? 1 : 0;
            pointPosList[i].isShowFriendBloodBar = (EditorGUILayout.Toggle("显示友军血条", pointPosList[i].isShowFriendBloodBar == 1)) ? 1 : 0;
            pointPosList[i].isShowTargetBar = (EditorGUILayout.Toggle("目标标记", pointPosList[i].isShowTargetBar == 1)) ? 1 : 0;
            pointPosList[i].pos = EditorGUILayout.Vector3Field("位置", pointPosList[i].pos);
            pointPosList[i].dir = EditorGUILayout.Vector3Field("方向", pointPosList[i].dir);

            pointPosList[i].bornDelay = EditorGUILayout.FloatField("出场延时", pointPosList[i].bornDelay);

            using (new AutoBeginHorizontal())
            {
                pointPosList[i].bornTypeId = EditorGUILayout.TextField("出生效果", pointPosList[i].bornTypeId);

                if (GUILayout.Button("选择"))
                {
                    List<BornCfg> cfgList = new List<BornCfg>();
                    foreach (BornCfg bc in BornCfg.mBornCfg.mBornCfgList)
                    {
                        if (bc.type == SceneCfg.BornDeadType.Born)
                            cfgList.Add(bc);
                    }
                    
                    cfgList.Sort((x, y) =>
                    {
                        if (string.IsNullOrEmpty(x.typeName) || string.IsNullOrEmpty(y.typeName))
                            return 0;
                        int a = (short)(Convert.ToChar(x.typeName[0]));
                        int b = (short)(Convert.ToChar(y.typeName[0]));
                        return a == b ? 0 : ((a > b) ? 1 : -1);
                    });

                    GenericMenu selectMenu = new GenericMenu();
                    for (int idx = 0; idx < cfgList.Count; idx++)
                    {
                        int cfgIdx = idx;
                        int pointIdx = i;

                        selectMenu.AddItem(new GUIContent(cfgList[idx].typeName), false, () =>
                        {
                            pointPosList[pointIdx].bornTypeId = cfgList[cfgIdx].typeName;
                        });
                    }
                    selectMenu.ShowAsContext();
                }
            }

            using (new AutoBeginHorizontal())
            {
                pointPosList[i].deadTypeId = EditorGUILayout.TextField("死亡效果", pointPosList[i].deadTypeId);

                if (GUILayout.Button("选择"))
                {
                    List<BornCfg> cfgList = new List<BornCfg>();
                    foreach (BornCfg bc in BornCfg.mBornCfg.mBornCfgList)
                    {
                        if (bc.type == SceneCfg.BornDeadType.Dead)
                            cfgList.Add(bc);
                    }

                    cfgList.Sort((x, y) =>
                    {
                        if (string.IsNullOrEmpty(x.typeName) || string.IsNullOrEmpty(y.typeName))
                            return 0;
                        int a = (short)(Convert.ToChar(x.typeName[0]));
                        int b = (short)(Convert.ToChar(y.typeName[0]));
                        return a == b ? 0 : ((a > b) ? 1 : -1);
                    });
                    GenericMenu selectMenu = new GenericMenu();
                    for (int idx = 0; idx < cfgList.Count; idx++)
                    {
                        int cfgIdx = idx;
                        int pointIdx = i;

                        selectMenu.AddItem(new GUIContent(cfgList[idx].typeName), false, () =>
                        {
                            pointPosList[pointIdx].deadTypeId = cfgList[cfgIdx].typeName;
                        });
                    }
                    selectMenu.ShowAsContext();
                }
            }

            using (new AutoBeginHorizontal())
            {
                pointPosList[i].groundDeadTypeId = EditorGUILayout.TextField("倒地死亡效果", pointPosList[i].groundDeadTypeId);

                if (GUILayout.Button("选择"))
                {
                    List<BornCfg> cfgList = new List<BornCfg>();
                    foreach (BornCfg bc in BornCfg.mBornCfg.mBornCfgList)
                    {
                        if (bc.type == SceneCfg.BornDeadType.GroundDead)
                            cfgList.Add(bc);
                    }

                    cfgList.Sort((x, y) =>
                    {
                        if (string.IsNullOrEmpty(x.typeName) || string.IsNullOrEmpty(y.typeName))
                            return 0;
                        int a = (short)(Convert.ToChar(x.typeName[0]));
                        int b = (short)(Convert.ToChar(y.typeName[0]));
                        return a == b ? 0 : ((a > b) ? 1 : -1);
                    });

                    GenericMenu selectMenu = new GenericMenu();
                    for (int idx = 0; idx < cfgList.Count; idx++)
                    {
                        int cfgIdx = idx;
                        int pointIdx = i;

                        selectMenu.AddItem(new GUIContent(cfgList[idx].typeName), false, () =>
                        {
                            pointPosList[pointIdx].groundDeadTypeId = cfgList[cfgIdx].typeName;
                        });
                    }
                    selectMenu.ShowAsContext();
                }
            }


            pointPosList[i].buffId = EditorGUILayout.IntField("出生buff", pointPosList[i].buffId);

            pointPosList[i].hate.Draw();

            using (new AutoBeginHorizontal())
            {
                EditorGUILayout.LabelField("寻路点:", GUILayout.Width(40));
                for (int idx = 0; idx < pointPosList[i].pathfindingList.Count; idx++)
                {
                    EditorGUILayout.LabelField(string.Format("{0}-", idx + 1), GUILayout.Width(10));
                    if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(20), GUILayout.Height(16)))
                    {
                        pointPosList[i].pathfindingList.RemoveAt(idx);
                        return;
                    }
                }

                
                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus More"), GUILayout.Width(20), GUILayout.Height(16)))
                {
                    if (RoleMgr.instance.Hero == null || RoleMgr.instance.Hero.State != Role.enState.alive)
                    {
                        Debug.Log("先创建主角");
                        return;
                    }
                    else
                    {
                        pointPosList[i].pathfindingList.Add(RoleMgr.instance.Hero.transform.position);
                    }
                }                    
            }

            if (GUILayout.Button("移动到主角位置"))
            {
                Role hero = RoleMgr.instance.Hero;
                if (hero == null || hero.State != Role.enState.alive)
                {
                    Debug.Log("先创建主角");
                    return;
                }
                else
                {

                    pointPosList[i].pos = hero.transform.position;
                    pointPosList[i].dir = hero.transform.eulerAngles;
                }
            }

            if (pointPosList[i].goModule != null)
            {
                Transform t = pointPosList[i].goModule.transform;
                t.position = pointPosList[i].pos;
                t.eulerAngles = pointPosList[i].dir;
            }

            //显示模型 
            if (bShowModule)
            {
                if (pointPosList[i].goModule == null)
                {
                    RoleCfg cfg = RoleCfg.Get(pointPosList[i].roleId);
                    if (cfg != null)
                    {
                        int idx = i;
                        GameObjectPool.GetPool(GameObjectPool.enPool.Role).Get(cfg.mod, null, (GameObject go, object param) =>
                        {
                            if (go != null)
                            {
                                pointPosList[idx].goModule = go;
                                Transform t = go.transform;
                                t.position = pointPosList[idx].pos;
                                t.eulerAngles = pointPosList[idx].dir;

                                go.transform.SetParent(Room.instance.mModuleGroup.transform, false);
                            }
                        });

                    }
                }
                else
                {
                    pointPosList[i].goModule.gameObject.SetActive(true);
                }
            }
            else
            {
                if (pointPosList[i].goModule != null)
                {
                    if (pointPosList[i].goModule.gameObject.activeSelf)
                    {
                        pointPosList[i].goModule.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public void OnSceneGUI()
    {
        if (bSceneShow)
        {
            for (int i = 0; i < pointPosList.Count; i++)
            {
                if (pointPosList[i].dir == Vector3.zero)
                    continue;
                pointPosList[i].pos = Handles.PositionHandle(pointPosList[i].pos, Quaternion.LookRotation(pointPosList[i].dir));
                Handles.Label(pointPosList[i].pos, new GUIContent(pointPosList[i].name), EditorStyleEx.LabelStyle);
                if (string.IsNullOrEmpty(pointPosList[i].name))
                    pointPosList[i].name = string.Format("{0}-{1}", groupFlag, i + 1);
            }
        }

    }

    public override void OnDrawGizmos()
    {
        if (bSceneShow)
        {
            for (int i = 0; i < pointPosList.Count; i++)
            {
                if (pointPosList[i].dir == Vector3.zero)
                    continue;
                pointPosList[i].pos = Handles.PositionHandle(pointPosList[i].pos, Quaternion.LookRotation(pointPosList[i].dir));
                Handles.Label(pointPosList[i].pos, new GUIContent(pointPosList[i].name), EditorStyleEx.LabelStyle);
                if (string.IsNullOrEmpty(pointPosList[i].name))
                    pointPosList[i].name = string.Format("{0}-{1}", groupFlag, i + 1);
            }
        }

    }


}
