#region Header
/**
 * 名称：调试用ui
 
 * 日期：2015.9.25
 * 描述：
 **/
#endregion

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

public class DebugUI : SingletonMonoBehaviour<DebugUI>
{
    //页信息
    class PageInfo
    {
        public delegate void DrawPage();

        public int idx;
        public string name;
        public DrawPage fun;
        public PageInfo(int idx, string name, DrawPage fun)
        {
            this.idx = idx;
            this.name = name;
            this.fun = fun;
        }

        public void Draw()
        {
            if (fun == null)
                return;
            fun();
        }
    }

    #region Fields
    static List<PageInfo> s_pages = new List<PageInfo>();
    int curPage = 0;
    bool isShow = false;

    //用于计算帧率
    int curFrameRate = 60;
    int frameCounter = 0;
    float frameTimeCounter = 0f;
    string lastLog = "";
    string errorLog = "";
    string gmCmd = "";
    public string gmCmdResult = "";

    //用于分步调试
    public string stepDesc = "";
    public bool canStep = false;

    
    #endregion


    #region Properties



    #endregion

    #region Mono Frame
    void Start()
    {
        
    }

    void OnGUI()
    {
        //做下分辨率适配，以免控件在不同设备上太小
        float height = 640f;
        float width = Screen.width * height / Screen.height;
        float s = Screen.height / height;

        //适配字体
        using (new AutoFontSize((int)(22 * s)))
        {
            DrawAlways();
            DrawTemp();//临时测试用的东西，测试完即删
            if (GUI.Button(new Rect(50 * s, 0 * s, 90 * s, 40 * s), "TestUI"))
            {
                isShow = !isShow;
            }
            if (isShow == false)
            {
                return;
            }
            DrawGM();


            //绘制要显示的页
            string[] pageNames = new string[s_pages.Count];
            for (int i = 0; i < s_pages.Count; ++i)
            {
                pageNames[i] = s_pages[i].name;
            }
            curPage = GUI.Toolbar(new Rect((width - pageNames.Length * 120 - 100 ) * s, 35 * s, pageNames.Length * 120 * s, 50 * s), curPage, pageNames);
            s_pages[curPage].Draw();
        }
        
       

        
    }
    #endregion
   


    #region Private Methods
    
    //一直显示的东西
    void DrawAlways()
    {
         //做下分辨率适配，以免控件在不同设备上太小
        float height = 640f;
        float width = Screen.width * height / Screen.height;
        float s = Screen.height / height;

        //帧率相关
        if (Time.unscaledDeltaTime < 5f)
        {
            frameTimeCounter += Time.unscaledDeltaTime;
            ++frameCounter;
        }
        if (frameTimeCounter >= 1f)
        {
            curFrameRate = frameCounter - 1;
            frameCounter = 1;
            frameTimeCounter = frameTimeCounter - 1f;
        }
        GUI.Label(new Rect(150 * s, 0 * s, 300 * s, 30 * s), "帧率:"+curFrameRate);
        if(CaptureAvi.IsExist && CaptureAvi.instance.IsCapturing())
            GUI.Label(new Rect(300 * s, 0 * s, 100 * s, 30 * s), "录屏中");
        if (Time.timeScale!=1)
            GUI.Label(new Rect(150 * s, 35 * s, 300 * s, 30 * s), "时间缩放:" + Time.timeScale);
      
       
    
    }
    void DrawGM() {
        float height = 640f;
        float width = Screen.width * height / Screen.height;
        float s = Screen.height / height;

        //提示框，显示最后一条log
        GUI.Label(new Rect(20 * s, (height - 50) * s, (width - 20) * s, 50 * s), lastLog);

        //GM命令
        GUI.SetNextControlName("gmCmd");
        gmCmd = GUI.TextField(new Rect(400 * s, 0 * s, 200 * s, 30 * s), gmCmd);
        if (GUI.Button(new Rect(610 * s, 0 * s, 90 * s, 30 * s), "GM"))
        {
            NetMgr.instance.GMHandler.SendProcessGMCmd(gmCmd);
        }
        if (Event.current.isKey && GUI.GetNameOfFocusedControl() == "gmCmd")
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Return:
                    NetMgr.instance.GMHandler.SendProcessGMCmd(gmCmd);
                    break;
                case KeyCode.UpArrow:
                    gmCmd = NetMgr.instance.GMHandler.GetCmdInHistory(true);
                    break;
                case KeyCode.DownArrow:
                    gmCmd = NetMgr.instance.GMHandler.GetCmdInHistory(false);
                    break;
            }            
        }
        if (gmCmdResult != "")
        {
            GUI.Label(new Rect(400 * s, 32 * s, 200 * s, 30 * s), gmCmdResult);
        }
        if (GUI.Button(new Rect(710 * s, 0 * s, 90 * s, 30 * s), "加怪"))
        {
            gmCmd = "addMonster xb_01";
            NetMgr.instance.GMHandler.SendProcessGMCmd(gmCmd);
        }
        if (GUI.Button(new Rect(810 * s, 0 * s, 90 * s, 30 * s), "Help"))
        {
            errorLog+= @"
增加道具:addItem itemId 数量
根据道具类型增加道具:addItemByType 道具类型id 数量
设置等级:setlevel 等级 
创建怪物:addMonster roleId ai类型(默认攻击1，不攻击0)
战斗属性:setProp 属性id 属性值 角色唯一id(
    生命1,攻击2,护甲3,减免伤害4,最终伤害5,暴击几率6,抗暴几率7,暴击伤害8,
    火9,冰10,雷11,冥12,火抗13,冰抗14,雷抗15,冥抗16,生命偷取17,伤害反弹18,
    怒气19,速度20,冷却缩减21)
增加状态:addBuff 状态id 角色唯一id
删除状态:removeBuff 状态id(-1则删除非战斗状态 -2删除所有战斗状态) 角色唯一id
修改虚拟时间:time 年 月 日 [时 [分 [秒]]] 或 time t 时 [分 [秒]]
重置金币副本:resetGoldLv [0~6，准备重置到的打过的最大难度]
重置哈迪斯副本:resethadeslv [0~6，准备重置到的打过的最大难度]
重置守护副本:resetguardlv [0~6，准备重置到的打过的最大难度]
重置所有关卡:resetlevel   重置后需重启游戏
开启关卡：openlevel num 开启关卡 num是开启的关卡数 -1表示全部开启
重置竞技场计数:resetarena
跟任意角色竞技场PK：startarena 主角ID
添加邮件：addmail [普通邮件0/附件邮件1] [发送对象支持批量]..  例:   addmail 1 12575 12576 12577
设置服务端数字属性：setsvrnumprop 属性名 数字
设置服务端字符串属性：setsvrstrprop 属性名 字符串
增加服务端数字属性：addsvrnumprop 属性名 数字
获取服务端属性：getsvrprop 属性名
修改引导数据：setteachdata 键名 数字
清空引导数据：clearteachdata
删除好友：deletefriend 被删除的heroId
打开测试模式：opentestmode
打开某个功能：openicon 功能枚举整数
切换当前武器元素属性:element 属性索引(1火 2冰 3雷 4冥)
修改摇杆翻滚判定:joystickcheck 判定时间(单位秒，建议0.1~0.5之间)
充值钻石：recharge 钻石数量
清除退公会cd：clearjoincorpscd
通关勇士试炼:finishwarrior 0
添加神器:addtreasure 神器id
开启众神传：openelitelevel id 开启众神传 id是通关的关卡id
";
            curPage = 0;//切换到日志那一页
        }
        if (GUI.Button(new Rect(910 * s, 0 * s, 90 * s, 30 * s), "重载配置")){
            CfgMgr.instance.Init();
            //主角要重新加载
            Role hero = RoleMgr.instance.Hero;
            if (hero == null)return;
            hero.Refresh();
            UIMessage.Show("重载配置成功");
        }
        if (GUI.Button(new Rect(1010 * s, 0 * s, 90 * s, 30 * s), "测试模式"))
        {
            NetMgr.instance.GMHandler.SendProcessGMCmd("opentestmode");
        }

    }


    void SystemLogCallback(string condition, string stackTrace, LogType type)
    {
        if (LogType.Log == type)
        {
            lastLog = condition;
        }
        else if (LogType.Warning == type)
        {
            //警告太多了，不提醒
        }
        else
        {
            lastLog = string.Format("Error:{1}", stackTrace,condition);
            if (errorLog.Length>50000)
                errorLog = errorLog.Substring(errorLog.Length-10000,10000);
            errorLog += stackTrace+condition;
        }
    }

    Vector2 logScroll = Vector2.zero;
    void DrawLog()
    {
        float height = 640f;
        float width = Screen.width * height / Screen.height;
        float s = Screen.height / height;

        
        GUILayout.BeginArea(new Rect(20 * s, 100* s, (width-40)  * s, (height-200) * s));
        logScroll = GUILayout.BeginScrollView(logScroll);
        int oldLabelFontSize = GUI.skin.label.fontSize;
        GUI.skin.label.fontSize = (int)(14 * s);
        //Color PreviousColor = GUI.backgroundColor;
        //GUI.backgroundColor = Color.red * 0.1f;
        GUILayout.TextField(errorLog);
        //GUI.backgroundColor = PreviousColor;
        GUI.skin.label.fontSize = oldLabelFontSize;
        GUILayout.EndScrollView();       
        GUILayout.EndArea();
       

    }

    bool hideScene = false;
    bool hideUI= false;

    public bool meshCut = false;


    CameraInfo m_info = new CameraInfo();
    void DrawPerformance()
    {
        float height = 640f;
        float width = Screen.width * height / Screen.height;
        float s = Screen.height / height;
        bool b;
        Camera ca = CameraMgr.instance != null ?CameraMgr.instance.CurCamera:null;
        
        b = GUI.Toggle(new Rect((width - 300) * s, 140 * s, 100 * s, 30 * s), hideUI, "隐藏ui");
        if (hideUI != b)
        {
            UIMgr.instance.UICamera.enabled =!b;
            hideUI = b;
        }
        
        ////bloom
        //if(ca!= null)
        //{
        //    BloomOptimized bloom = ca.GetComponent<BloomOptimized>();
        //    b = GUI.Toggle(new Rect((width - 400) * s, 140* s, 100 * s, 30 * s), bloom!= null&& bloom.enabled, "bloom");
        //    if (b == false && bloom != null)
        //        bloom.enabled = false;
        //    else if (b && bloom != null)
        //        bloom.enabled = true;
        //    else if (b && bloom == null)
        //        bloom = ca.gameObject.AddComponent<BloomOptimized>();

        //    if (b)
        //    {
        //        GUI.Label(new Rect((width - 500) * s, 180 * s, 100 * s, 30 * s), "threshold");
        //        GUI.Label(new Rect((width - 500) * s, 220 * s, 100 * s, 30 * s), "intensity");
        //        GUI.Label(new Rect((width - 500) * s, 260 * s, 100 * s, 30 * s), "blurSize");

        //        bloom.threshold = GUI.HorizontalSlider(new Rect((width - 400) * s, 180 * s, 280 * s, 30 * s), bloom.threshold,0f,1.5f);
        //        bloom.intensity = GUI.HorizontalSlider(new Rect((width - 400) * s, 220 * s, 280 * s, 30 * s), bloom.intensity, 0f, 2.5f);
        //        bloom.blurSize = GUI.HorizontalSlider(new Rect((width - 400) * s, 260 * s, 280 * s, 30 * s), bloom.blurSize, 0.25f, 5.5f);
        //    }
        //}

        ////全局雾
        //b = GUI.Toggle(new Rect((width - 500) * s, 140 * s, 100 * s, 30 * s), RenderSettings.fog, "全局雾");
        //if (RenderSettings.fog != b)
        //{
        //    RenderSettings.fog = b;
        //}

        //内存相关
#if ENABLE_PROFILER
        GUI.Label(new Rect((50) * s, 80 * s, 300 * s, 30 * s),string.Format("mono 堆大小:{0:f0}", (Profiler.GetMonoHeapSize()/1024f)/1024f));
        GUI.Label(new Rect((50) * s, 120 * s, 300 * s, 30 * s), string.Format("mono 使用到的大小:{0:f0}", (Profiler.GetMonoUsedSize() / 1024f) / 1024f));
        GUI.Label(new Rect((50) * s, 160 * s, 300 * s, 30 * s), string.Format("总内存大小:{0:f}", (Profiler.GetTotalReservedMemory() / 1024f) / 1024f));
        GUI.Label(new Rect((50) * s, 200 * s, 300 * s, 30 * s), string.Format("预留中的:{0:f0}", (Profiler.GetTotalUnusedReservedMemory() / 1024f) / 1024f));
        
#endif
        ////抗锯齿
        //GUI.Label(new Rect((width - 900) * s, 140 * s, 100 * s, 30 * s), "抗锯齿");
        //int idx= GUI.Toolbar(new Rect((width - 800) * s, 140* s, 280 * s, 30 * s), AntiToInt(QualitySettings.antiAliasing), antiStr);
        //if (idx != AntiToInt(QualitySettings.antiAliasing))
        //    QualitySettings.antiAliasing = IntToAnti(idx);

        //帧率
        //GUI.Label(new Rect((width - 900) * s, 180 * s, 100 * s, 30 * s), "帧率");
        //int idx = GUI.Toolbar(new Rect((width - 800) * s, 180 * s, 280 * s, 30 * s), FrameRateToInt(Application.targetFrameRate), frameRateStr);
        //if (idx != FrameRateToInt(Application.targetFrameRate))
        //    Application.targetFrameRate = IntToFrameRate(idx);

        if (!canStep &&GUI.Button(new Rect((width - 900) * s, 220 * s, 500 * s, 40 * s), stepDesc))
        {
           
            canStep = true;
        }

        //网络消息整齐打印
        NetCore.MessageHandle.s_prettyPrint = GUI.Toggle(new Rect((width - 900) * s, 260 * s, 300 * s, 30 * s), NetCore.MessageHandle.s_prettyPrint, "网络消息整齐打印");

        //隐藏挡板
        if (GUI.Button(new Rect((width - 900) * s, 380 * s, 100 * s, 40 * s), "隐藏挡板"))
        {
            GameObject go = GameObject.Find("room/Area");
            Renderer[] rs = go == null ? null : go.GetComponentsInChildren<Renderer>(true);
            if(rs!=null && rs.Length>0)
            {
                foreach(var r in rs)
                {
                    r.enabled = false;
                }
            }
        }
        if (GUI.Button(new Rect((width - 780) * s, 380 * s, 100 * s, 40 * s), "显示挡板"))
        {
            GameObject go = GameObject.Find("room/Area");
            Renderer[] rs = go == null ? null : go.GetComponentsInChildren<Renderer>(true);
            if (rs != null && rs.Length > 0)
            {
                foreach (var r in rs)
                {
                    r.enabled = true;
                }
            }
        }

        //隐藏场景
        //if (m_sceneBuilding == null)
        //    m_sceneBuilding = GameObject.Find("MapScene/[Building]");
        
        //if (m_sceneBuilding != null)
        //{
        //    unSceneBuilding = GUI.Toggle(new Rect((width - 900) * s, 420 * s, 200 * s, 30 * s), unSceneBuilding, "隐藏场景物件");
        //    if (!m_sceneBuilding.activeSelf != unSceneBuilding)
        //    {
        //        m_sceneBuilding.SetActive(!unSceneBuilding);
        //    }
        //}

        if (GUI.Button(new Rect((width - 900) * s, 420 * s, 300 * s, 40 * s), "打印设备参数"))
        {
            string info = "";
            info += string.Format("SystemInfo.deviceModel:{0}\n", SystemInfo.deviceModel);
            info += string.Format("SystemInfo.deviceName:{0}\n", SystemInfo.deviceName);
            info += string.Format("SystemInfo.deviceType:{0}\n", SystemInfo.deviceType);
            info += string.Format("SystemInfo.deviceUniqueIdentifier:{0}\n", SystemInfo.deviceUniqueIdentifier);
            info += string.Format("SystemInfo.graphicsDeviceID:{0}\n", SystemInfo.graphicsDeviceID);
            info += string.Format("SystemInfo.graphicsDeviceName:{0}\n", SystemInfo.graphicsDeviceName);
            info += string.Format("SystemInfo.graphicsDeviceType:{0}\n", SystemInfo.graphicsDeviceType);
            info += string.Format("SystemInfo.graphicsDeviceVendor:{0}\n", SystemInfo.graphicsDeviceVendor);
            info += string.Format("SystemInfo.graphicsDeviceVendorID:{0}\n", SystemInfo.graphicsDeviceVendorID);
            info += string.Format("SystemInfo.graphicsDeviceVersion:{0}\n", SystemInfo.graphicsDeviceVersion);
            info += string.Format("SystemInfo.graphicsMemorySize:{0}\n", SystemInfo.graphicsMemorySize);
            info += string.Format("SystemInfo.graphicsMultiThreaded:{0}\n", SystemInfo.graphicsMultiThreaded);
            info += string.Format("SystemInfo.graphicsShaderLevel:{0}\n", SystemInfo.graphicsShaderLevel);
            info += string.Format("SystemInfo.maxTextureSize:{0}\n", SystemInfo.maxTextureSize);
            info += string.Format("SystemInfo.operatingSystem:{0}\n", SystemInfo.operatingSystem);
            info += string.Format("SystemInfo.processorCount:{0}\n", SystemInfo.processorCount);
            info += string.Format("SystemInfo.processorFrequency:{0}\n", SystemInfo.processorFrequency);
            info += string.Format("SystemInfo.processorType:{0}\n", SystemInfo.processorType);
            info += string.Format("SystemInfo.supportedRenderTargetCount:{0}\n", SystemInfo.supportedRenderTargetCount);
            info += string.Format("SystemInfo.supports3DTextures:{0}\n", SystemInfo.supports3DTextures);
            info += string.Format("SystemInfo.supportsAccelerometer:{0}\n", SystemInfo.supportsAccelerometer);
            info += string.Format("SystemInfo.supportsComputeShaders:{0}\n", SystemInfo.supportsComputeShaders);
            info += string.Format("SystemInfo.supportsGyroscope:{0}\n", SystemInfo.supportsGyroscope);
            info += string.Format("SystemInfo.supportsImageEffects:{0}\n", SystemInfo.supportsImageEffects);
            info += string.Format("SystemInfo.supportsInstancing:{0}\n", SystemInfo.supportsInstancing);
            info += string.Format("SystemInfo.supportsLocationService:{0}\n", SystemInfo.supportsLocationService);
            info += string.Format("SystemInfo.supportsRawShadowDepthSampling:{0}\n", SystemInfo.supportsRawShadowDepthSampling);
            info += string.Format("SystemInfo.supportsRenderToCubemap:{0}\n", SystemInfo.supportsRenderToCubemap);
            info += string.Format("SystemInfo.supportsShadows:{0}\n", SystemInfo.supportsShadows);
            info += string.Format("SystemInfo.supportsStencil:{0}\n", SystemInfo.supportsStencil);
            info += string.Format("SystemInfo.systemMemorySize:{0}\n", SystemInfo.systemMemorySize);
            Debuger.LogError(info);

        }

        //减面
        //bool newMeshCut= GUI.Toggle(new Rect((width - 900) * s, 460 * s, 200 * s, 30 * s), meshCut, "减面");
        //if(newMeshCut!= meshCut)
        //{
        //    meshCut = newMeshCut;
        //    foreach (var r in RoleMgr.instance.Roles)
        //    {
        //        r.RenderPart.MeshCut(meshCut);
        //    }
        //}

        //if (CameraMgr.instance!= null)
        //{
        //    GUI.Label(new Rect((width - 500) * s, 320 * s, 150 * s, 30 * s), "相机高度角"+(int)m_info.verticalAngle);
        //    GUI.Label(new Rect((width - 500) * s, 360 * s, 150 * s, 30 * s), "相机距离"+(int)m_info.distance);

        //    bool change = GUI.changed;
        //    GUI.changed = false;

        //    m_info.verticalAngle = GUI.HorizontalSlider(new Rect((width - 350) * s, 320 * s, 280 * s, 30 * s), m_info.verticalAngle, 0f, 90f);
        //    m_info.distance = GUI.HorizontalSlider(new Rect((width - 350) * s, 360 * s, 280 * s, 30 * s), m_info.distance, 5f, 30f);


        //    if(GUI.changed)
        //    {
        //        CameraHandle handle = CameraMgr.instance.Set(m_info, 100);//优先级要提高点
        //    }
        //    GUI.changed = change;
        //}

        //去掉法线
        //if (GUI.Button(new Rect((width - 900) * s, 420 * s, 100 * s, 40 * s), "去掉法线"))
        //{
        //    GameObject go = GameObject.Find("MapScene/[Building]");
        //    if (go != null)
        //    {
        //        Shader b1 = Shader.Find("VacuumShaders/Terrain To Mesh/Legacy Shaders/Bumped/4 Textures");
        //        Shader b2 = Shader.Find("Mobile/Bumped Diffuse");
        //        Shader d1 = Shader.Find("VacuumShaders/Terrain To Mesh/Legacy Shaders/Diffuse/4 Textures");
        //        Shader d2 = Shader.Find("Mobile/Diffuse");

        //        Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
        //        foreach (var r in rs)
        //        {
        //            if (r == null)
        //                continue;

        //            Material[] ms = r.sharedMaterials;
        //            if (ms == null || ms.Length == 0)
        //                continue;

        //            foreach (Material m in ms)
        //            {
        //                if (m.shader == b1)
        //                    m.shader = d1;
        //                else if(m.shader == b2)
        //                    m.shader = d2;
        //            }
        //        }
        //    }
        //}
        //if (GUI.Button(new Rect((width - 780) * s, 420 * s, 100 * s, 40 * s), "加回法线"))
        //{
        //    GameObject go = GameObject.Find("MapScene/[Building]");
        //    if (go != null)
        //    {
        //        Shader b1 = Shader.Find("VacuumShaders/Terrain To Mesh/Legacy Shaders/Bumped/4 Textures");
        //        Shader b2 = Shader.Find("Mobile/Bumped Diffuse");
        //        Shader d1 = Shader.Find("VacuumShaders/Terrain To Mesh/Legacy Shaders/Diffuse/4 Textures");
        //        Shader d2 = Shader.Find("Mobile/Diffuse");

        //        Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
        //        foreach (var r in rs)
        //        {
        //            if (r == null)
        //                continue;

        //            Material[] ms = r.sharedMaterials;
        //            if (ms == null || ms.Length == 0)
        //                continue;

        //            foreach (Material m in ms)
        //            {
        //                if (m.shader == d1)
        //                    m.shader = b1;
        //                else if (m.shader == d2)
        //                    m.shader = b2;
        //            }
        //        }
        //    }
        //}


        //屏幕分辨率,没有太大作用
        GUI.Label(new Rect((width - 800) * s, 180 * s, 100 * s, 30 * s), "分辨率");
        if(GUI.Button(new Rect((width - 700) * s, 180 * s, 150 * s, 30 * s),"原始"))
            QualityMgr.instance.SetResolution(QualityMgr.instance.originalHeight);
        if (GUI.Button(new Rect((width - 550) * s, 180 * s, 150 * s, 30 * s), "720"))
            QualityMgr.instance.SetResolution(720);
        if (GUI.Button(new Rect((width - 400) * s, 180 * s, 150 * s, 30 * s), "640"))
            QualityMgr.instance.SetResolution(640);
        if (GUI.Button(new Rect((width - 250) * s, 180 * s, 150 * s, 30 * s), "400"))
            QualityMgr.instance.SetResolution(400);
        
        //if (GUI.Button(new Rect((width - 600) * s, 160 * s, 280 * s, 30 * s), "场景:echo fastest Unlit "))
        //    SetSceneShader("Custom/echoLogin/Unlit/10-Fastest");

        //if (GUI.Button(new Rect((width - 600) * s, 200 * s, 280 * s, 30 * s), "场景:Mobile/Diffuse"))
        //    SetSceneShader("Mobile/Diffuse");

        //if (GUI.Button(new Rect((width - 600) * s, 240 * s, 280 * s, 30 * s), "场景:Mobile/Bumped Diffuse"))
        //    SetSceneShader("Mobile/Bumped Diffuse");

        //if (GUI.Button(new Rect((width - 600) * s, 280 * s, 280 * s, 30 * s), "场景:Legacy/Bumped Specular"))
        //    SetSceneShader("Legacy Shaders/Bumped Specular");

        //if (GUI.Button(new Rect((width - 300) * s, 160 * s, 280 * s, 30 * s), "角色:echo fastest Unlit "))
        //    SetRoleShader("Custom/echoLogin/Unlit/10-Fastest");

        //if (GUI.Button(new Rect((width - 300) * s, 200 * s, 280 * s, 30 * s), "角色:Unlit "))
        //    SetRoleShader("Unlit/Texture");

        //if (GUI.Button(new Rect((width - 300) * s, 240 * s, 280 * s, 30 * s), "角色:Diffuse"))
        //    SetRoleShader("Mobile/Diffuse");

        //if (GUI.Button(new Rect((width - 300) * s, 280 * s, 280 * s, 30 * s), "角色:Bumped Diffuse"))
        //    SetRoleShader("Mobile/Bumped Diffuse");

        //if (GUI.Button(new Rect((width - 300) * s, 320 * s, 280 * s, 30 * s), "角色:Bumped Specular"))
        //    SetRoleShader("Mobile/Bumped Specular");
    }

    void SetSceneShader(string shaderName)
    {
        Shader shader = Shader.Find(shaderName);
        if(shader == null)
        {
            Debuger.LogError("找不到shader:{0}", shaderName);
            return;
        }

        GameObject go =GameObject.Find("MapScene");
        if (go == null)
        {
            Debuger.LogError("找不到MapScene");
            return;
        }

        Transform t =go.transform.Find("[Building]");
        if(t == null)
        {
            Debuger.LogError("找不到[Building]");
            return;
        }

        Renderer[] rs =t.GetComponentsInChildren<Renderer>();
        foreach (var r in rs)
        {
            if (r == null )
                continue;

            Material[] ms = r.sharedMaterials;
            if (ms == null || ms.Length == 0)
                continue;

            foreach (Material m in ms)
            {
                if(m.shader != shader)
                    m.shader = shader;
            }
        }
    }

    void SetRoleShader(string shaderName)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debuger.LogError("找不到shader:{0}", shaderName);
            return;
        }

        ICollection<Role> rs =RoleMgr.instance.Roles;
        foreach(var role in rs){
            if (role.Cfg.roleType == enRoleType.box || role.Cfg.roleType == enRoleType.trap)
                continue;

            Transform t = role.transform.Find("model/body_mesh");
            if(t == null)
                continue;
            Renderer r =t.GetComponent<Renderer>();
            if(r == null)
                continue;
            if(r.sharedMaterial.shader !=shader)
                r.sharedMaterial.shader =shader;
        }

    }

    //战斗相关
    public bool unAttack = false;
    public bool unDead = false;
    public bool unCD= false;
    public bool unMp= false;

    public bool debugProp = false;
    public bool debugBuff = false;
    public bool debugNotifier = false;
    public bool debugHate = false;
    public bool debugFlag= false;
    void DrawCombat()
    {
        float height = 640f;
        float width = Screen.width * height / Screen.height;
        float s = Screen.height / height;

        unAttack = GUI.Toggle(new Rect((width - 150) * s, 120 * s, 100 * s, 30 * s), unAttack, "不攻击");
        unDead = GUI.Toggle(new Rect((width - 250) * s, 120 * s, 100 * s, 30 * s), unDead, "不死");
        unMp = GUI.Toggle(new Rect((width - 150) * s, 160 * s, 100 * s, 30 * s), unMp, "不扣耐力");
        unCD = GUI.Toggle(new Rect((width - 250) * s, 160 * s, 100 * s, 30 * s), unCD, "无cd");

        debugProp = GUI.Toggle(new Rect((width - 250) * s, 200 * s, 150 * s, 30 * s), debugProp, "调试属性");
        debugBuff = GUI.Toggle(new Rect((width - 250) * s, 240 * s, 150 * s, 30 * s), debugBuff, "调试buff");
        debugNotifier = GUI.Toggle(new Rect((width - 250) * s, 280 * s, 150 * s, 30 * s), debugNotifier, "调试消息");
        debugHate = GUI.Toggle(new Rect((width - 250) * s, 320 * s, 150 * s, 30 * s), debugHate, "调试仇恨");
        debugFlag = GUI.Toggle(new Rect((width - 250) * s, 360 * s, 150 * s, 30 * s), debugFlag, "调试标记");

        //翻滚时间
//        var joystick = UIMgr.instance.Get<UILevel>().Get<UILevelAreaJoystick>().m_joystick;
//#if UNITY_EDITOR || UNITY_STANDALONE
//        GUI.Label(new Rect(20 * s, 120 * s, 180 * s, 30 * s), string.Format("摇杆翻滚判定:{0:F2}", joystick.m_sliderCheckTimePC));
//        joystick.m_sliderCheckTimePC = GUI.HorizontalSlider(new Rect(200* s, 120 * s, 400 * s, 30 * s), joystick.m_sliderCheckTimePC, 0, 1f);
//#else
//        GUI.Label(new Rect(20 * s, 120 * s, 180 * s, 30 * s), string.Format("摇杆翻滚判定:{0:F2}s", joystick.m_sliderCheckTime));
//      joystick.m_sliderCheckTime = GUI.HorizontalSlider(new Rect(200 * s, 120 * s, 400 * s, 30 * s), joystick.m_sliderCheckTime, 0, 1f);
//#endif





        //if(RoleMgr.instance.Hero != null){
        //    CombatRecord record =CombatMgr.instance.GetCombatRecord(RoleMgr.instance.Hero);
        //    GUI.Label(new Rect((width - 800) * s, 120* s, 400* s, 30 * s), string.Format("总输出:{0} 总伤害:{1} 总治疗:{2} ",record.hitDamage,record.beHitDamage,record.addHp));
        //}


    }

    //关卡相关
    public bool bRunLogic = true;
    public bool bAiTest = false;
    string flyNum = "1";
    public Vector3 FindPos = Vector3.zero;
    Vector2 scrollPos1 = Vector2.zero;
    Vector2 scrollPos2 = Vector2.zero;
    Vector2 scrollPos3 = Vector2.zero;

    public static bool bCreateMonster = false;
    public static List<AiTestCfg> testCfgList = new List<AiTestCfg>();
    string showPartId = "";
    string showGroupId = "";
    bool playBGM = true;
    bool playOtherSound = true;

    void DrawLevel()
    {
        float height = 640f;
        float width = Screen.width * height / Screen.height;
        float l = width - 900f;    //左
        float t = 100f;     //顶
        float w = 100f;      //宽
        float h = 30f;      //高
        float wSpace = 110f; //左右间隔
        float hSpace = 50f; //上下间隔
        float s = Screen.height / height;
        bool preState = bRunLogic;
        bRunLogic = GUI.Toggle(new Rect(l * s, t * s, w * s, h * s), bRunLogic, "运行关卡");
        if (preState != bRunLogic)
        {
            SceneEventMgr.instance.SetAllEventRunOrStop(bRunLogic);
            if (bRunLogic)
                UIMessage.Show("开启关卡逻辑");
            else
                UIMessage.Show("关闭关卡逻辑");
        }

        if (GUI.Button(new Rect((l + wSpace) * s, t * s, w * s, h * s), "测试关卡"))
        {
            unAttack = true;
            unDead = true;
            LevelMgr.instance.ChangeLevel("1000");
        }

        if (GUI.Button(new Rect((l + 2 * wSpace) * s, t * s, w * s, h * s), "重进关卡"))
        {

            int id = 0;
            int.TryParse(Room.instance.roomCfg.id, out id);
            if (id == 0)
                return;

            SceneEventMgr.instance.roomId = "";     //重置roomId
            if(StoryMgr.instance.IsPlaying)
                StoryMgr.instance.SkipStory();
            string roomId = Room.instance.roomCfg.id;
            if (Main.instance.isSingle || roomId == "1000")
                LevelMgr.instance.ChangeLevel(roomId);
            else
                NetMgr.instance.LevelHandler.SendEnter(roomId);
        }


        if (GUI.Button(new Rect((l + 3 * wSpace) * s, t * s, w * s, h * s), "杀死怪物"))
        {
            if (TimeMgr.instance.IsPause) return;

             List<Role> rs = new List<Role>(RoleMgr.instance.Roles);
            Role hero = RoleMgr.instance.Hero;
            if (hero.State== Role.enState.alive)
            {
                foreach (Role r in rs)
                {
                    if (r.Cfg.roleType == enRoleType.trap || r == RoleMgr.instance.GlobalEnemy)
                        continue;

                    if (r.State == Role.enState.alive && RoleMgr.instance.IsEnemy(r, hero))
                        r.DeadPart.Handle(true);
                }
            }
            
        }

        if (GUI.Button(new Rect((l + 4 * wSpace) * s, t * s, w * s, h * s), "通  关"))
        {
            int id = 0;
            int.TryParse(Room.instance.roomCfg.id, out id);
            if (id == 0)
                return;
            UIMgr.instance.CloseAll();

            LevelMgr.instance.SetWin();
             List<Role> rs = new List<Role>(RoleMgr.instance.Roles);
            foreach (Role r in rs)
            {
                if (r.Cfg.roleType == enRoleType.trap )
                    continue;

                if (r != RoleMgr.instance.Hero && r.State == Role.enState.alive)
                    r.DeadPart.Handle(true);
            }
        }
        //关闭背景音乐
        if(playBGM)
        {
            if (GUI.Button(new Rect((l + 5 * wSpace) * s, t * s, w * s, h * s), "关闭BGM"))
            {
                SoundMgr.instance.MuteBGM(true);
                playBGM = false;
            }
        }
        else
        {
            if (GUI.Button(new Rect((l + 5 * wSpace) * s, t * s, w * s, h * s), "开启BGM"))
            {
                SoundMgr.instance.MuteBGM(false);
                playBGM = true;
            }
        }
        //关闭其他音效
        if (playOtherSound)
        {
            if (GUI.Button(new Rect((l + 6 * wSpace) * s, t * s, w * s, h * s), "关闭音效"))
            {
                SoundMgr.instance.Stop2DSound(Sound2DType.other);
                SoundMgr.instance.Stop2DSound(Sound2DType.ui);
                SoundMgr.instance.muteSound = true;
                playOtherSound = false;
            }
        }
        else
        {
            if (GUI.Button(new Rect((l + 6 * wSpace) * s, t * s, w * s, h * s), "开启音效"))
            {
                SoundMgr.instance.muteSound = false;
                playOtherSound = true;
            }
        }


        //scrollPosition = GUI.BeginScrollView(new Rect(l * s, (t + hSpace) * s, 300, 200), scrollPosition, new Rect(0, 0, 550, 500));
        //GUI.Button(new Rect((l + 4 * wSpace) * s, t * s, w * s, h * s), "通  关");
        //GUI.EndScrollView();

        bool prevFlag = bAiTest;
        bAiTest = GUI.Toggle(new Rect(l * s, (t + hSpace) * s, w * s, h * s), bAiTest, "AI调试");
        float scrollW = 150f;
        float scrollF = 80f;
        float scrollSpace = 40;
        float scrollL = 40f;
        if (bAiTest)
        {
            if (prevFlag != bAiTest)
                AiTestCfg.Init();

            scrollPos1 = GUI.BeginScrollView(new Rect((l + scrollF) * s, (t + hSpace) * s, scrollW, 400), scrollPos1, new Rect(0, 0, 0, AiTestCfg.m_dictCfg.Count * 65), false, false);
            int num = 0;
            foreach (string partId in AiTestCfg.m_dictCfg.Keys)
            {
                if (GUI.Button(new Rect(scrollL, num * scrollSpace, 1.2f * w * s, h * s), partId))
                {
                    showPartId = partId;
                    showGroupId = "";
                }
                num++;
            }
            GUI.EndScrollView();
        }
        else
        {
            showPartId = "";
            showGroupId = "";
        }

        if (!string.IsNullOrEmpty(showPartId))
        {
            Dictionary<string, List<AiTestCfg>> groupDict = AiTestCfg.m_dictCfg[showPartId];

            scrollPos2 = GUI.BeginScrollView(new Rect(((l + scrollF) + scrollW) * s, (t + hSpace) * s, scrollW, 400), scrollPos2, new Rect(0, 0, 0, groupDict.Count * 65), false, false);
            int num = 0;
            foreach (string groupId in groupDict.Keys)
            {
                if (GUI.Button(new Rect(scrollL, num * scrollSpace, 1.4f * w * s, h * s), groupId))
                {
                    showGroupId = groupId;
                }
                num++;
            }
            GUI.EndScrollView();
        }

        if (!string.IsNullOrEmpty(showPartId) && !string.IsNullOrEmpty(showGroupId))
        {
            List<AiTestCfg> cfgList = AiTestCfg.m_dictCfg[showPartId][showGroupId];

            scrollPos3 = GUI.BeginScrollView(new Rect(((l + scrollF) + scrollW*2) * s, (t + hSpace) * s, scrollW, 400), scrollPos3, new Rect(0, 0, 0, cfgList.Count * 50), false, false);
            int num = 0;
            foreach (AiTestCfg cfg in cfgList)
            {
                if (GUI.Button(new Rect(scrollL, num * scrollSpace, w * s, h * s), cfg.roleId))
                {

                }
                num++;
            }
            GUI.EndScrollView();
        }

        if (!string.IsNullOrEmpty(showPartId) && !string.IsNullOrEmpty(showGroupId))
        {
            if (GUI.Button(new Rect((l + 5.5f * wSpace) * s, (t + hSpace * 2) * s, w * s, h * s), "刷  怪"))
            {
                testCfgList = AiTestCfg.m_dictCfg[showPartId][showGroupId];
                bCreateMonster = true;
            }
        }
        

        //if (GUI.Button(new Rect((width - 500) * s, 250 * s, 80 * s, 30 * s), "取点"))
        //{
        //    Role hero = RoleMgr.instance.Hero;
        //    if (hero != null)
        //        FindPos = hero.TranPart.GetRoot();
        //}

        //if (GUI.Button(new Rect((width - 500) * s, 300 * s, 80 * s, 30 * s), "指向开关"))
        //{
        //    if (SceneMgr.instance.IsDirShow)
        //        SceneMgr.instance.OverFind();
        //    else
        //        SceneMgr.instance.StartFind(FindPos);
        //}


    }
    #endregion

    public void Init()
    {
        //注册页面
        s_pages.Add(new PageInfo(s_pages.Count, "日志", DrawLog));
        s_pages.Add(new PageInfo(s_pages.Count, "性能", DrawPerformance));
        s_pages.Add(new PageInfo(s_pages.Count, "战斗", DrawCombat));
        s_pages.Add(new PageInfo(s_pages.Count, "关卡", DrawLevel));
        curPage = s_pages.Count - 1;

        Application.logMessageReceived += SystemLogCallback;
    }

    
    //临时测试用的东西，测试完即删
    public void DrawTemp()
    {
        
    }

    public static Coroutine Step(int step)
    {
        return Main.instance.StartCoroutine(instance.CoStep(string.Format("是否进入下一步，第{0}完成",step)));
    }
    public static Coroutine Step(string s)
    {
        return Main.instance.StartCoroutine(instance.CoStep( s));
    }

    IEnumerator CoStep(string stepDesc)
    {
        Debuger.Log(stepDesc + "完成,等待进入下一步");
        DebugUI.instance.canStep = false;
        DebugUI.instance.stepDesc = stepDesc;
        while (!DebugUI.instance.canStep)
        {
            yield return 1;
        }

        DebugUI.instance.canStep = false;
        yield return 0;
    }


}
