using UnityEngine;
using System.Collections;



public class SYSTEM_VIS_TYPE
{
    /** 默认可见 */
    public const int VISIBLE = 0;
    /** 不可见 */
    public const int INVISIBLE = 1;
    /** 开服多少天开启 */
    public const int SERVER_TIME = 2;
    /** 创建账号多少天开启 */
    public const int ACCOUNT_TIME = 3;
}

public class SYSTEM_ACTIVE_TYPE
{
    /** 默认解锁 */
    public const int ACTIVE = 0;
    /** 通关副本 */
    public const int PASS_LEVEL = 1;
    /** 等级到达 */
    public const int LEVEL = 2;
    /** 任务触发 */
    public const int QUEST = 3;
}

public class SYSTEM_OPEN_TYPE
{
    /** 时间段 */
    public const int TIME = 1;
    /** 等级 */
    public const int LEVEL = 2;
    /** 开服天数 */
    public const int SERVER_TIME = 3;
    /** 创建账号天数 */
    public const int ACCOUNT_TIME = 4;
}