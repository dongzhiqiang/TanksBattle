using UnityEngine;
using System.Collections;

/// <summary>
/// 系统消息
/// </summary>
public class MSG_SYSTEM {

    //系统激活
    public const int ACTIVE = 1;

    //系统红点
    public const int TIP = 2;

    //引导开始（参数：引导名）
    public const int TEACH_START = 3;

    //引导结束（参数：引导名 是否正常结束）
    public const int TEACH_END = 3;

    //窗口打开（参数：UIPanel对象）
    public const int PANEL_OPEN = 4;

    //窗口关闭（参数：UIPanel对象）
    public const int PANEL_CLOSE = 5;

    //主城UI切换到置顶
    public const int MAINCITY_UI_TOP = 6;

    //主城UI切换到非置顶
    public const int MAINCITY_UI_UNTOP = 7;
}
