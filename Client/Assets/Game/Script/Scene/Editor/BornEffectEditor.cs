using UnityEngine;
using UnityEditor;
using System;
using LitJson;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class BornEffectEditor : EditorWindow
{

    public int roleID;
    public BornCfg m_BornCfg;
    public BornCfg m_DeadCfg;
    public BornCfg m_GroundDeadCfg;

    public List<BornCfg> bornCfgList = new List<BornCfg>();

    bool m_infoArea = true;
    Transform m_tran;
    Vector3 m_curPos = Vector3.zero;
    Vector3 m_curDir = Vector3.one;
    string m_roleId = "";
    RoleModel m_model;


    [MenuItem("Tool/出场方式编辑器 %F5", false, 106)]
    public static void ShowWindow()
    {
        BornEffectEditor instance = (BornEffectEditor)EditorWindow.GetWindow(typeof(BornEffectEditor));
        instance.minSize = new Vector2(920, 600);
        instance.titleContent = new GUIContent("出场方式编辑器");
        instance.autoRepaintOnSceneChange = true;

    }

    void OnEnable()
    {
        roleID = 0;
        m_roleId = "";
        m_BornCfg = new BornCfg(SceneCfg.BornDeadType.Born);
        m_DeadCfg = new BornCfg(SceneCfg.BornDeadType.Dead);
        m_GroundDeadCfg = new BornCfg(SceneCfg.BornDeadType.GroundDead);

        if (DebugUI.instance != null && UIMgr.instance != null)
        {
            DebugUI.instance.unAttack = true;
            DebugUI.instance.bRunLogic = false;
            if (UIMgr.instance.Get<UILevel>().IsOpen)
                UIMgr.instance.Get<UILevel>().Close<UILevelAreaGizmos>();
        }
    }

    void OnDisable()
    {
        if (DebugUI.instance != null)
        {
            DebugUI.instance.unAttack = false;
            DebugUI.instance.bRunLogic = true;
        }
    }

    void OnGUI()
    {
        DrawToolBar();

        using (new AutoBeginHorizontal())
        {
            //左边，角色相关信息
            using (new AutoBeginVertical("PreferencesSectionBox", GUILayout.Width(220)))
            {
                using (new AutoLabelWidth(80))
                {
                    EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(true, "模型信息", "RoleInfo", EditorStyleEx.BoxStyle);
                    m_infoArea = area.open;
                    if (area.Show())
                        DrawInfo();
                    EditorGUILayoutEx.instance.EndFadeArea();

                }
            }
            using (new AutoBeginVertical("PreferencesSectionBox", GUILayout.Width(220)))
            {
                DrawBornCfg();
            }

            using (new AutoBeginVertical("PreferencesSectionBox", GUILayout.Width(220)))
            {
                DrawDeadCfg();
            }

            using (new AutoBeginVertical("PreferencesSectionBox", GUILayout.Width(220)))
            {
                DrawGroundDeadCfg();
            }
        }

   
    }

    void DrawBornCfg()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        GUILayout.Label("出场方式配置", style);
        GUILayout.Space(10);

        DrawCfg(m_BornCfg);
    }

    void DrawDeadCfg()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        GUILayout.Label("站立死亡方式配置", style);
        GUILayout.Space(10);

        DrawCfg(m_DeadCfg);
    }

    void DrawGroundDeadCfg()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        GUILayout.Label("倒地死亡方式配置", style);
        GUILayout.Space(10);

        DrawCfg(m_GroundDeadCfg);
    }

    void DrawCfg(BornCfg cfg)
    {
        using (new AutoBeginHorizontal())
        {
            cfg.typeName = EditorGUILayout.TextField("方式名", cfg.typeName);
            if (GUILayout.Button("选择"))
            {
                List<BornCfg> cfgList = new List<BornCfg>();
                foreach(BornCfg bc in BornCfg.mBornCfg.mBornCfgList)
                {
                    if (bc.type == cfg.type)
                        cfgList.Add(bc);
                }

                cfgList.Sort((x, y) => {
                    if (string.IsNullOrEmpty(x.typeName) || string.IsNullOrEmpty(y.typeName))
                        return 0;
                    int a = (short)(Convert.ToChar(x.typeName[0]));
                    int b = (short)(Convert.ToChar(y.typeName[0]));
                    return a == b ? 0 : ((a > b) ? 1 : -1);
                });
                GenericMenu selectMenu = new GenericMenu();
                for (int i = 0; i < cfgList.Count; i++)
                {
                    int type = i;

                    selectMenu.AddItem(new GUIContent(cfgList[i].typeName), false, () =>
                    {
                        if (cfg.type == SceneCfg.BornDeadType.Born)
                            m_BornCfg = cfgList[type];
                        if (cfg.type == SceneCfg.BornDeadType.Dead)
                            m_DeadCfg = cfgList[type];
                        if (cfg.type == SceneCfg.BornDeadType.GroundDead)
                            m_GroundDeadCfg = cfgList[type];
                    });
                }
                selectMenu.ShowAsContext();
            }
        }
        GUILayout.Space(5);

        cfg.modelDelay = EditorGUILayout.FloatField("模型延迟", cfg.modelDelay);
        cfg.fxDelay = EditorGUILayout.FloatField("特效延迟", cfg.fxDelay);
        
        for (int i = 0; i < cfg.aniName.Length; i++ )
            cfg.aniName[i] = EditorGUILayout.TextField("动作名", cfg.aniName[i]);

        cfg.aniDelay = EditorGUILayout.FloatField("动作延迟", cfg.aniDelay);
        cfg.pauseTime = EditorGUILayout.FloatField("逻辑停止时间", cfg.pauseTime);
        cfg.startTime = EditorGUILayout.FloatField("逻辑开启时间", cfg.startTime);
        cfg.slowStartTime = EditorGUILayout.FloatField("慢动作开始时间", cfg.slowStartTime);
        cfg.slowDurationTime = EditorGUILayout.FloatField("慢动作持续时间", cfg.slowDurationTime);
        cfg.playRate = EditorGUILayout.FloatField("播放速率", cfg.playRate);
        cfg.cameraStartTime = EditorGUILayout.FloatField("镜头开始移动", cfg.cameraStartTime);
        cfg.cameraMoveTime = EditorGUILayout.FloatField("移动时间", cfg.cameraMoveTime);
        cfg.cameraStayTime = EditorGUILayout.FloatField("停留时间", cfg.cameraStayTime);
        cfg.cameraOverDuration = EditorGUILayout.FloatField("镜头返回时间", cfg.cameraOverDuration);
        cfg.cameraOffset = EditorGUILayout.Vector3Field("镜头偏移", cfg.cameraOffset);
        cfg.cameraVerticalAngle = EditorGUILayout.FloatField("镜头高度角", cfg.cameraVerticalAngle);
        cfg.cameraHorizontalAngle = EditorGUILayout.FloatField("镜头水平角", cfg.cameraHorizontalAngle);
        cfg.cameraFOV = EditorGUILayout.FloatField("镜头视野", cfg.cameraFOV);
        cfg.bossUITime = EditorGUILayout.FloatField("boss提示时间", cfg.bossUITime);
        cfg.delayExtend = EditorGUILayout.FloatField("扩展延时", cfg.delayExtend);

        if (GUILayout.Button("编辑特效"))
        {
            Role role = RoleMgr.instance.GetRole(roleID);
            if (m_tran == null)
            {
                if (role != null)
                    m_tran = role.transform;
                else if (RoleMgr.instance.Hero != null)
                    m_tran = RoleMgr.instance.Hero.transform;
            }

            if (m_tran != null)
            {
                if (cfg.fx == null)
                    cfg.fx = new FxCreateCfg();
                FxCreateWindow.ShowWindow("特效编辑", cfg.fx, m_tran);
            }
        }

        GUILayout.Space(10);
        if (GUILayout.Button("删除当前方式"))
        {
            if (EditorUtility.DisplayDialog("删除", "确定删除", "确定", "取消"))
            {
                if (!string.IsNullOrEmpty(cfg.typeName))
                {
                    List<BornCfg> cfgList = BornCfg.mBornCfg.mBornCfgList;
                    for (int i = 0; i < cfgList.Count; i++)
                    {
                        if (cfg.typeName == cfgList[i].typeName)
                        {
                            cfgList.Remove(cfgList[i]);
                            if (cfg.type == SceneCfg.BornDeadType.Born)
                                m_BornCfg = new BornCfg(cfg.type);
                            else if(cfg.type == SceneCfg.BornDeadType.Dead)
                                m_DeadCfg = new BornCfg(cfg.type);
                            else if (cfg.type == SceneCfg.BornDeadType.GroundDead)
                                m_GroundDeadCfg = new BornCfg(cfg.type);
                        }
                    }
                }
            }
        }

        GUILayout.Space(30);
        if (GUILayout.Button("保存当前方式"))
        {
            //if (string.IsNullOrEmpty(cfg.aniName[0]) && cfg.type != SceneCfg.BornDeadType.GroundDead)
            //{
            //    EditorUtility.DisplayDialog("保存配置", "至少要配一个出生动作", "确定");
            //    return;
            //}
            if (m_BornCfg != null && !string.IsNullOrEmpty(cfg.typeName))
            {
                if (cfg.startTime < cfg.pauseTime)
                {
                    EditorUtility.DisplayDialog("保存配置", "逻辑开启停止时间设置错误", "确定");
                    return;
                }

                BornCfg bornCfg = BornCfg.GetCfg(cfg.typeName);

                for (int i = 0; i < BornCfg.mBornCfg.mBornCfgList.Count; i++)   //序列化一下动作字符串
                {
                    BornCfg.mBornCfg.mBornCfgList[i].aniNameStr = serializeAni(BornCfg.mBornCfg.mBornCfgList[i].aniName);
                }

                if (bornCfg != null)
                {
                    if (EditorUtility.DisplayDialog("保存配置", "已有重名配置，是否覆盖？", "确定", "取消"))
                    {
                        bornCfg = m_BornCfg;
                        
                        File.WriteAllText(Application.dataPath + "/Config/Resources/scene/BornCfg.json", JsonMapper.ToJson(BornCfg.mBornCfg), System.Text.Encoding.UTF8);
                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog("保存配置", "是否保存？", "确定", "取消"))
                    {
                        BornCfg.mBornCfg.mBornCfgList.Add(cfg);
                        File.WriteAllText(Application.dataPath + "/Config/Resources/scene/BornCfg.json", JsonMapper.ToJson(BornCfg.mBornCfg), System.Text.Encoding.UTF8);
                    }
                }
            }
            else
            {
                EditorUtility.DisplayDialog("保存配置", "名字不能为空", "确定");
            }
        }
        
    }

    void DrawInfo()
    {
        if (RoleMgr.instance == null)
            return;

        Role role = RoleMgr.instance.GetRole(roleID);
        m_model = role == null ? null : role.RoleModel;

        RoleModel newModel = (RoleModel)EditorGUILayout.ObjectField("角色", m_model, typeof(RoleModel), true);

        if (GUILayout.Button("移动到主角位置"))
        {
            if (RoleMgr.instance.Hero == null)
            {
                EditorUtility.DisplayDialog("", "主角为空", "确定");
                return;
            }


            m_curPos = RoleMgr.instance.Hero.TranPart.GetRoot();
            m_curDir = RoleMgr.instance.Hero.transform.eulerAngles;
            m_tran = RoleMgr.instance.Hero.transform;
            if (role != null && role.TranPart != null)
            {
                role.TranPart.SetPos(m_curPos);
                role.transform.eulerAngles = m_curDir;
            }
        }


        GUILayout.Space(40);

        if (GUILayout.Button("预览出生方式"))
        {
            if (string.IsNullOrEmpty(m_roleId))
            {
                EditorUtility.DisplayDialog("", "角色id为空，无法预览", "确定");
                return;
            }

            if (m_BornCfg == null || string.IsNullOrEmpty(m_BornCfg.typeName))
            {
                EditorUtility.DisplayDialog("", "出生配置为空，无法预览", "确定");
                return;
            }

            if (role != null && role.RSM != null && (role.RSM.CurStateType == enRoleState.born || role.RSM.CurStateType == enRoleState.dead))
            {
                EditorUtility.DisplayDialog("", "上一个效果没播完", "确定");
                return;
            }

            if (role != null && role.State == Role.enState.alive)
            {
                role.DeadPart.Handle(true);
            }

            m_BornCfg.aniNameStr = serializeAni(m_BornCfg.aniName);

            RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
            cxt.OnClear();
            cxt.Init(Util.GenerateGUID(), "", m_roleId, 1, enCamp.camp2, m_curPos, m_curDir);

            role = RoleMgr.instance.CreateRole(cxt);
            role.RSM.GotoState(enRoleState.born, new RoleStateBornCxt(m_BornCfg.typeName));
            roleID = role.Id;
        }

        GUILayout.Space(10);
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button("预览站立死亡方式"))
            {
                if (string.IsNullOrEmpty(m_roleId))
                {
                    EditorUtility.DisplayDialog("", "角色id为空，无法预览", "确定");
                    return;
                }

                if (m_DeadCfg == null || string.IsNullOrEmpty(m_DeadCfg.typeName))
                {
                    EditorUtility.DisplayDialog("", "死亡配置为空，无法预览", "确定");
                    return;
                }

                if (role != null && role.RSM != null && (role.RSM.CurStateType == enRoleState.born || role.RSM.CurStateType == enRoleState.dead))
                {
                    EditorUtility.DisplayDialog("", "上一个效果没播完", "确定");
                    return;
                }

                if (role == null || role.RSM == null)
                {
                    RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
                    cxt.Init(Util.GenerateGUID(), "", m_roleId, 1, enCamp.camp2, m_curPos, m_curDir);

                    role = RoleMgr.instance.CreateRole(cxt);
                    roleID = role.Id;
                }

                m_DeadCfg.aniNameStr = serializeAni(m_DeadCfg.aniName);

                role.RSM.GotoState(enRoleState.dead, new RoleStateDeadCxt(m_DeadCfg.typeName));
            }

            if (GUILayout.Button("预览倒地死亡方式"))
            {
                if (string.IsNullOrEmpty(m_roleId))
                {
                    EditorUtility.DisplayDialog("", "角色id为空，无法预览", "确定");
                    return;
                }

                if (m_GroundDeadCfg == null || string.IsNullOrEmpty(m_GroundDeadCfg.typeName))
                {
                    EditorUtility.DisplayDialog("", "倒地死亡配置为空，无法预览", "确定");
                    return;
                }

                if (role != null && role.RSM != null && (role.RSM.CurStateType == enRoleState.born || role.RSM.CurStateType == enRoleState.dead))
                {
                    EditorUtility.DisplayDialog("", "上一个效果没播完", "确定");
                    return;
                }

                if (role == null || role.RSM == null)
                {
                    RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
                    cxt.Init(Util.GenerateGUID(), "", m_roleId, 1, enCamp.camp2, m_curPos, m_curDir);

                    role = RoleMgr.instance.CreateRole(cxt);
                    roleID = role.Id;
                }

                if (!string.IsNullOrEmpty(m_GroundDeadCfg.aniName[0]))
                    m_GroundDeadCfg.aniNameStr = serializeAni(m_GroundDeadCfg.aniName);
                role.RSM.CheckFree();
                Room.instance.StartCoroutine(playGroundDead(role));
            }
        }
    }

    IEnumerator playGroundDead(Role role)
    {
        //yield return new WaitForSeconds(0.5f);
        role.AniPart.Play(AniFxMgr.Ani_DaoDi, WrapMode.ClampForever, 0.2f, 1f, true);
        //yield return new WaitForSeconds(0.5f);
        while (role.AniPart.CurSt.normalizedTime < 1f)
            yield return 0;
        role.RSM.GotoState(enRoleState.dead, new RoleStateDeadCxt(m_GroundDeadCfg.typeName));

    }

    void DrawToolBar()
    {
        if (RoleMgr.instance == null)
            return;

        int idx = string.IsNullOrEmpty(m_roleId) ? -1 : Array.IndexOf(RoleCfg.RoleIds, m_roleId);
        if (GUILayout.Button(idx == -1 ? "选择角色" : RoleCfg.RoleIds[idx], EditorStyles.toolbarPopup, GUILayout.Width(130)))
        {
            GenericMenu selectMenu = new GenericMenu();
            for (int i = 0; i < RoleCfg.RoleNames.Length; i++)
            {
                int type = i;
                selectMenu.AddItem(new GUIContent(RoleCfg.RoleNames[i]), false, () =>
                {
                    m_roleId = RoleCfg.RoleIds[type];
                });
            }
            selectMenu.ShowAsContext();
        }

    }

    string serializeAni(string[] aniName)
    {
        if (string.IsNullOrEmpty(aniName[0]))
        {
            //Debug.LogError("至少配一个动作名");
            return "";
        }

        string aniNameStr = "";
        for (int i = 0; i < aniName.Length; i++)
        {
            if (string.IsNullOrEmpty(aniName[i]))
                break;
            aniNameStr += string.Format("{0}:单次:-1|", aniName[i]);
        }
        return aniNameStr;
    }
}
