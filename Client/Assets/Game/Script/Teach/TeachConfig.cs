using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel;

public class TeachEntry
{
    public string name;
    public string desc;

    public TeachEntry()
    {
    }

    public TeachEntry(string name, string desc)
    {
        this.name = name;
        this.desc = desc;
    }
}

public class TeachEntries
{
    public List<TeachEntry> cfgs = new List<TeachEntry>();
}

public enum TeachUIOpType
{
    [Description("无")]
    none,

    [Description("纯指示")]
    uiStatic,

    [Description("UI点击")]
    uiClick,

    [Description("UI长按")]
    uiHold,

    [Description("UI按下")]
    uiPtrDown,

    [Description("UI弹起")]
    uiPtrUp,

    [Description("UI开始拖动")]
    uiDragBegin,

    [Description("UI结束拖动")]
    uiDragEnd,

    [Description("状态控件状态")]
    stateHandle,

    [Description("全屏图片")]
    fullScreenImg,

    [Description("剧情字幕")]
    playStory,

    [Description("打开顶层窗口")]
    uiPanelOpenTop,

    [Description("关闭窗口")]
    uiPanelClose,

    [Description("显示主城界面")]
    showMainCityUI,     //仅限在主城场景

    [Description("窗口函数（异步）")]
    uiPanelFunc,        //窗口自定义函数，由外部决定何时下一步

    [Description("窗口函数（同步）")]
    uiPanelFuncSync,    //立即执行，并下一步

    [Description("显示UI节点（立即）")]
    showUINode,         //立即执行，并下一步

    [Description("隐藏UI节点（立即）")]
    hideUINode,         //立即执行，并下一步

    [Description("窗口化显示图片")]
    windowImg,          //显示在某个带标题的子窗口的图片

    [Description("全屏点击")]
    fullScreenClick,

    [Description("拖动演示")]
    uiDragDemo,

    [Description("组控件选中项")]
    stateGroup,
}

public enum TeachCircleType
{
    [Description("无")]
    none,

    [Description("圆形波纹")]
    roundWave,

    [Description("炸开特效")]
    blastFx,

    [Description("圆形闪框")]
    circleFlash,

    [Description("方形闪框")]
    squareFlash,

    [Description("炸开特效（多实例）")]
    blastFxClone,
}

public enum TeachArrowType
{
    [Description("无")]
    none,

    [Description("箭头")]
    arrow,

    [Description("手形")]
    hand,
}

public enum TeachSceneOpType
{
    [Description("无")]
    none,

    [Description("主角移动")]
    movePos,

    [Description("发出场景事件（立即）")]
    fireSceneAction,

    [Description("进入场景（立即）")]
    enterScene,

    [Description("退出场景（立即）")]
    leaveScene,

    [Description("引导数据（立即）")]
    teachData,

    [Description("自动战斗（立即）")]
    enableHeroAI,
}

public enum TeachSceneGuideType
{
    [Description("无")]
    none,

    [Description("全路线指引")]
    fullPathGuide,    
}

public enum TeachStepObj
{
    [Description("无")]
    none,

    [Description("UI操作")]
    uiOp,

    [Description("场景操作")]
    sceneOp,

    [Description("UI节点显示")]
    showUINode,     //UI节点显示（比如窗口打开，这里指activeInHierarchy）

    [Description("UI节点隐藏")]
    hideUINode,     //UI节点隐藏（比如窗口关闭，对象销毁也算吧，这里指activeInHierarchy）

    [Description("主角等级")]
    heroLevel,

    [Description("引导事件")]
    directEvent,

    [Description("引导事件(可超时)")]
    directEventEx,

    [Description("普通事件")]
    normalEvent,

    [Description("普通事件(可超时)")]
    normalEventEx,
}

public enum TeachStepSkipType
{
    [Description("无")]
    none,

    [Description("状态控件状态值")]
    stateHandleState,

    [Description("组控件选中项")]
    stateGroupCurSel,

    [Description("窗口打开且顶部")]
    uiPanelOpenTop, //UIPanel显示且在顶层

    [Description("窗口打开可不顶部")]
    uiPanelOpen,    //UIPanel显示，不用管是否在顶层

    [Description("UI节点显示")]
    showUINode,  //UI节点显示（这里指activeInHierarchy）

    [Description("UI节点隐藏")]
    hideUINode,  //UI节点未显示（这里指activeInHierarchy）

    [Description("引导数据")]
    teachData,

    [Description("界面函数")]
    uiPanelFunc,//界面函数
}

public enum TeachPauseMode
{
    [Description("无")]
    none,

    [Description("逻辑")]
    logic,

    [Description("完全")]
    full,
}

public class TeachStepConfig
{
    public string name = "";
    public bool force = false;
    public TeachPauseMode pauseMode = TeachPauseMode.none;
    public bool keyStep = false;
    public int maskAlpha = 200;
    public string tipMsg = "";
    public string centerTip = "";   //屏幕中间大字提示
    public bool stopPreSnd = false; //停止前面的2D声音
    public bool muteMusic = false; //背景音乐静音
    public int soundId = 0;
    public int timeoutInMS = 0;         //多少毫秒后算超时
    public bool timeoutStop = false;    //超时后，是自动下一步还是停止引导

    public TeachStepObj stepObj = TeachStepObj.none; //本步骤要完成的目标，也就是说，就算有指示UI点击啥的，不一定就点完就完成了，可能要多点几次
    public string stepObjParam = "";
    public string stepObjParam2 = "";

    public TeachUIOpType uiOpType = TeachUIOpType.none;
    public TeachCircleType circleType = TeachCircleType.none;
    public TeachArrowType arrowType = TeachArrowType.none;
    public string uiOpParam = "";
    public string uiOpParam2 = "";

    public TeachSceneOpType sceneOpType = TeachSceneOpType.none;
    public TeachSceneGuideType sceneGuideType = TeachSceneGuideType.none;
    public string sceneOpParam = "";
    public string sceneOpParam2 = "";

    public TeachStepSkipType stepSkipType = TeachStepSkipType.none;
    public string skipParam = "";
    public string skipParam2 = "";
    public bool stopIfNeedSkip = false; //是否用停止引导来代替跳过
}

public enum TeachTriggerCond
{
    [Description("无")]
    none,

    [Description("回到主城")]
    mainCity,

    [Description("回到主城（来自任意场景）")]
    mainCityFromAny,

    [Description("主角等级")]
    heroLevel,

    [Description("引导事件")]
    directEvent,

    [Description("打开窗口")]
    openPanel,

    [Description("进入场景")]
    enterScene,

    [Description("后续引导")]
    postTeach,

    [Description("普通事件")]
    normalEvent,

    [Description("主城界面置顶")]
    mainCityUITop,

    [Description("代理对象")]
    teachAgent,
}

public enum TeachAgentType
{
    [Description("技能帧")]
    skillFrame,

    [Description("关卡失败")]
    levelFail,
}

public class TeachTriggerCondData
{
    public TeachTriggerCond triggerType = TeachTriggerCond.none;
    public string triggerParam = "";
    public string triggerParam2 = "";
}

public enum TeachCheckCond
{
    [Description("无")]
    none,

    [Description("所在场景")]
    inScene,

    [Description("主角等级")]
    heroLevel,

    [Description("窗口显示")]
    panelShow,

    [Description("副本关数")]
    levelId,

    [Description("引导数据")]
    teachData,

    [Description("任务状态")]
    taskState,

    [Description("界面函数")]
    uiPanelFunc,

    [Description("图标开启状态")]
    systemIcon,

    [Description("在主城里")]
    inMainCity,

    [Description("在副本里")]
    inLevelScene,
}

public class TeachCheckCondData
{
    public TeachCheckCond checkType = TeachCheckCond.none;
    public string checkParam = "";
    public string checkParam2 = "";
}

public enum TeachBackCheckType
{
    [Description("无")]
    none,

    [Description("窗口关闭")]
    closePanel,     //参数有：窗口路径

    [Description("窗口覆盖")]
    beCovered,      //参数有：被覆盖的窗口

    [Description("控件状态")]
    stateHandle,    //参数有：控件路径，本来期望的状态

    [Description("组选中项")]
    stateGroup,     //参数有：控件路径，本来期望的选中项

    [Description("引导事件")]
    directEvent,    //参数有：事件类型，事件名

    [Description("普通事件")]
    normalEvent,    //参数有：事件类型，事件ID
}

public enum TeachBackActionType
{
    [Description("无")]
    none,

    [Description("回退到某步")]
    backStepTo,   //参数有：回退到第X步

    [Description("取消播放")]
    cancelPlay,   //参数有：无
}

public class TeachBackCheckData
{
    public int fromStep = -1;
    public int toStep = -1;
    public TeachBackCheckType checkType = TeachBackCheckType.none;
    public string checkParam = "";
    public string checkParam2 = "";
    public TeachBackActionType actionType = TeachBackActionType.none;
    public string actionParam;
}

public enum TeachCancelActionType
{
    [Description("无")]
    none,

    [Description("窗口打开")]
    openPanel,

    [Description("窗口关闭")]
    closePanel,

    [Description("UI节点显示")]
    showUINode,

    [Description("UI节点隐藏")]
    hideUINode,

    [Description("状态控件状态")]
    stateHandleState,

    [Description("界面函数")]
    uiPanelFunc,//界面函数
}

public class TeachCancelActionData
{
    public TeachCancelActionType actionType = TeachCancelActionType.none;
    public string actionParam = "";
    public string actionParam2 = "";
}

public class TeachConfig
{
    #region 常量
    public const int TEACH_KEY_MIN_LEN = 1;
    public const int TEACH_KEY_MAX_LEN = 12;
    #endregion

    #region 静态变量
    private static Regex teachNameTestRegex = new Regex("^[a-z0-9]+$");
    private static string fileDir = "teach";
    private static string cfgListFileName = "teachCfgList";
    private static List<TeachConfig> configs = new List<TeachConfig>();
    private static Dictionary<string, TeachConfig> configMap = new Dictionary<string, TeachConfig>();
    #endregion

    #region 私有数据
    private bool isDirty = false;
    #endregion

    #region 串行化数据
    public string teachName = "";
    public string teachDesc = "";
    public int priority = 1;
    public int coolDown = 0;                //冷却时间，主要用于提示类引导，可能消失后又马上触发，不太好
    public string dataKey = "";
    public string postTeach = "";
    public bool stopSoundWhenFinish = false;
    public bool replayUnfinished = false;   //触发后，未播或未播完时，要不要下次放入队列重播（一般用于主城开始的引导）
    public bool canInterrupt = false;       //是否允许被更高优先级的引导打断
    public List<TeachBackCheckData> backChecks = new List<TeachBackCheckData>();
    public List<TeachCancelActionData> cancelActions = new List<TeachCancelActionData>();
    public List<TeachTriggerCondData> triggerConds = new List<TeachTriggerCondData>();
    public List<TeachCheckCondData> checkConds = new List<TeachCheckCondData>();    
    public List<TeachStepConfig> stepList = new List<TeachStepConfig>();
    #endregion

    public static List<TeachConfig> Configs
    {
        get
        {
            return configs;
        }
    }

    public bool IsDirty
    {
        get { return isDirty; }
        set { isDirty = value; }
    }

    public TeachConfig()
    {
    }

    public TeachConfig(string name = "")
    {
        teachName = name;
    }

    public void Save()
    {
        var filePath = string.Format("{0}/{1}", fileDir, teachName);
        Util.SaveJsonFile(filePath, this);

        IsDirty = false;

        SaveList();
    }

    public void Revert()
    {
        var filePath = string.Format("{0}/{1}", fileDir, teachName);
        var config = Util.LoadJsonFile<TeachConfig>(filePath);
        if (config == null)
        {
            Debuger.LogError("加载文件失败：" + filePath);
            return;
        }

        var fields = typeof(TeachConfig).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            field.SetValue(this, field.GetValue(config));
        }

        IsDirty = false;
    }

    public string Rename(string newTeachName = null)
    {
        if (teachName == newTeachName)
            return "";

        if (string.IsNullOrEmpty(newTeachName))
            return "请填写引导名字";
        if (!teachNameTestRegex.IsMatch(newTeachName))
            return "名字只能是小写字母和数字的组合";
        if (FindConfigByName(newTeachName) != null)
            return "不能跟别的引导重复名字";
        if (cfgListFileName == newTeachName)
            return "不能用这个名字";

        //删除旧文件
        var filePath = string.Format("{0}/{1}", fileDir, teachName);
        Util.RemoveJsonFile(filePath);

        configMap.Remove(teachName);
        teachName = newTeachName;
        configMap[teachName] = this;
        Save();
        return "";
    }

    public void Delete()
    {
        var filePath = string.Format("{0}/{1}", fileDir, teachName);
        Util.RemoveJsonFile(filePath);

        var idx = configs.FindIndex((e) => { return e.teachName == teachName; });
        if (idx >= 0)
            configs.RemoveAt(idx);
        configMap.Remove(teachName);

        SaveList();
    }

    public static void Init()
    {
        var filePath = string.Format("{0}/{1}", fileDir, cfgListFileName);
        TeachEntries entries = Util.LoadJsonFile<TeachEntries>(filePath);
        if (entries == null)
            entries = new TeachEntries();

        bool bNeedSaveAgain = false;
        configs.Clear();
        configMap.Clear();
        foreach (var entry in entries.cfgs)
        {
            var filePath2 = string.Format("{0}/{1}", fileDir, entry.name);
            var config = Util.LoadJsonFile<TeachConfig>(filePath2);
            if (config != null)
            {
                configs.Add(config);
                configMap.Add(config.teachName, config);
            }
            else
            {
                bNeedSaveAgain = true;
                Debuger.LogError("引导配置文件{0}不存在", entry.name);
            }
        }

        if (bNeedSaveAgain)
            SaveList();
    }

    public static TeachConfig FindConfigByName(string name)
    {
        TeachConfig cfg;
        configMap.TryGetValue(name, out cfg);
        return cfg;
    }

    public static int GetTeachPriority(string name)
    {
        TeachConfig cfg;
        configMap.TryGetValue(name, out cfg);
        return cfg == null ? 100 : cfg.priority;
    }

    public static string NewConfig(string teachName)
    {
        if (string.IsNullOrEmpty(teachName))
            return "请填写引导名字";
        if (!teachNameTestRegex.IsMatch(teachName))
            return "名字只能是小写字母和数字的组合";
        if (FindConfigByName(teachName) != null)
            return "不能跟已存在的引导重名";
        if (cfgListFileName == teachName)
            return "不能用这个名字";

        var cfg = new TeachConfig(teachName);
        //////////
        var stepCfg = new TeachStepConfig();
        stepCfg.name = "主城界面";
        stepCfg.stepObj = TeachStepObj.uiOp;
        stepCfg.uiOpType = TeachUIOpType.showMainCityUI;
        cfg.stepList.Add(stepCfg);
        //////////
        configs.Add(cfg);
        configMap.Add(teachName, cfg);
        cfg.Save();
        return "";
    }

    public static void SaveList()
    {
        var filePath = string.Format("{0}/{1}", fileDir, cfgListFileName);
        var entries = new TeachEntries();
        foreach (var cfg in configs)
        {
            entries.cfgs.Add(new TeachEntry(cfg.teachName, cfg.teachDesc));
        }
        Util.SaveJsonFile(filePath, entries);
    }

    public static bool IsTeachKeyOK(string key)
    {
        if (key == null)
            return false;
        if (key.Length < TEACH_KEY_MIN_LEN || key.Length > TEACH_KEY_MAX_LEN)
            return false;
        for (int i = 0; i < key.Length; ++i)
        {
            var ch = key[i];
            if (!(ch == '_' || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9')))
                return false;
        }
        return true;
    }

    public static string GetTeachKeyFormatDesc()
    {
        return string.Format("引导数据键格式：长度：{0}~{1}，由小写字母、数字、下划线组成", TEACH_KEY_MIN_LEN, TEACH_KEY_MAX_LEN);
    }

    public static TeachConfig FindConfigByDataKey(string key, TeachConfig excluded = null)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        foreach (var cfg in configs)
        {
            if (cfg != excluded && cfg.dataKey == key)
                return cfg;
        }
        return null;
    }
}
