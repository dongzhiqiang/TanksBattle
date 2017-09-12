using UnityEngine;
using System.Collections;

public class MODULE_GM
{
    /** 执行指令 */
    public const int CMD_PROCESS_CM_CMD = 1;
}

public class RESULT_CODE_GM : RESULT_CODE
{
    public const int GM_WRONG_FORMAT = 1; // 指令格式错误
    public const int GM_EXECUTE_ERROR = 2; // 指令执行错误
}

public class ProcessGmCmdVo
{
    public string msg;         //指令
}