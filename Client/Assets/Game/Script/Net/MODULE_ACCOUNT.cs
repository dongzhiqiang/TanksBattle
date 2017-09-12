using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MODULE_ACCOUNT
{
    public const int CMD_LOGIN = 1;
    public const int CMD_CREATE_ROLE = 2;
    public const int CMD_ACTIVATE_ROLE = 3;
    public const int CMD_LOGOUT = 4;
    public const int CMD_RELOGIN = 5;
    public const int CMD_TEST_ECHO = 6;
    public const int CMD_SERVER_TIME = 7;
    public const int CMD_PING = 8;
    public const int CMD_DEMO_HERO_DATA = 9;

    public const int PUSH_FORCE_LOGOUT = -1;
    public const int PUSH_TIP_MSG = -2;
    public const int PUSH_SERVER_TIME = -3;
    public const int PUSH_PING = -4;
}

public class RESULT_CODE_ACCOUNT : RESULT_CODE
{
    public const int CHECK_TOKEN_FAIL = 1;    //一般要跳到登录界面，因为token无效的，必须重新登录
    public const int RELOGIN_FAIL = 2;    //一般只要跳回服务器选择界面
    public const int ROLE_DATA_LOST = 3;    //角色数据丢失
}


public class LoginRequestVo
{
    public string channelId;         //渠道Id
    public string userId;            //用户Id
    public string token;             //登录验证用的token
    public int serverId;             //服务器Id
    public string clientVer;         //客户端版本
    public string lang;              //客户端语言
    public string deviceModel;       //设备型号
    public string osName;            //系统名称
    public int root;                 //是否越狱
    public string macAddr;           //硬件地址
    public string network;           //联网类型
    public int screenWidth;          //屏幕宽
    public int screenHeight;         //屏幕高
}

public class ReloginRequestVo
{
    public string channelId;    //渠道Id
    public string userId;       //用户Id
    public string token;        //登录验证用的token
    public int heroId;          //主角Id
    public long lastLogin;      //最后登录时间，用于校验内存里的那个Role确实是自己离线时那个Role
    public string clientVer;         //客户端版本
    public string lang;              //客户端语言
    public string deviceModel;       //设备型号
    public string osName;            //系统名称
    public int root;                 //是否越狱
    public string macAddr;           //硬件地址
    public string network;           //联网类型
    public int screenWidth;          //屏幕宽
    public int screenHeight;         //屏幕高
}

public class CreateRoleRequestVo
{
    public string roleId;       //角色类型ID
    public string name;         //角色名
}

public class ActivateRoleRequestVo
{
    public int heroId;          //主角Id
}

public class LoginTipMsgVo
{
    public string msg;
}

public class ForceLogoutVo
{
    public const int GOTO_SVR_SELECT = 0;   //客户端收到消息后关闭连接，跳到服务器选择界面
    public const int GOTO_LOGIN_UI = 1;     //客户端收到消息后关闭连接，跳到登录界面
    public const int KEEP_CURRENT = 2;      //客户端收到消息后关闭连接，保持当前界面，界面跳转靠别的消息

    public string msg;
    public int type;
}

public class SyncServerTime
{
    public long time;
    public int tzOffset;
}
public class RoleBriefVo
{
    public string guid;
    public string name;
    public int level;
    public string roleId;
    public int heroId;
}

public class RoleListVo
{
    public List<RoleBriefVo> roleList;
}

public class GetDemoHeroDataReq
{
    public string roleId;
}