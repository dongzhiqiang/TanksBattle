#region Header
/**
 * 名称：登录
 
 * 日期：2015.9.21
 * 描述：登录
 **/
#endregion
using UnityEngine;
using System.Collections;
using LitJson;
using Encrypt;
using System.Net.Sockets;
using System;

[NetModule(MODULE.MODULE_ACCOUNT)]
public class AccountHandler 
{
    #region Fields
    public const string DEF_HERO_ROLEID = "kratos";

    private const string CHANNEL_ID_KEY     = "channelId";
    private const string LOGIN_HOST_KEY     = "loginHost";
    private const string CLIENT_VER_KEY     = "clientVer";
    private const bool   NEED_REGISTER      = false;        //是否要注册，如果不要注册，那就不用输密码，账号用户注册
    private const int RECONN_MAX_SECONDS    = 15 * 60;      //最多重试多少秒后才提示重试失败
    private const float RETRY_WAITING       = 0.5f;         //重连失败后，等多久再次尝试
    private const float PING_INTERVAL       = 30.0f;        //单位秒，PING服务端的间隔
    private const float BAD_TIME_GAP        = 10.0f;        //单位秒，当两帧时差大于某个值时，就重新请求服务器时间

    bool m_isWaitServer = false;
    bool m_inAutoReconn = false;
    long m_startReconnTime = 0;  //开始重连时间，重连到一定时间后就不再重试了
    ServerInfo m_serverInfo = null;
    LoginInfo m_loginInfo = new LoginInfo();
    #endregion

    #region Properties
    public bool IsWaitServer { get{return m_isWaitServer;}}
    public LoginInfo LoginInfo { get { return m_loginInfo; } }
    public static bool IsNeedRegister { get { return NEED_REGISTER; } }
    #endregion

    public AccountHandler()
    {
        Main.instance.StartCoroutine(CoCheckTimeAndPing());
    }

    #region Net
    public void RegisterUser(string username, string password)
    {
        m_isWaitServer = true;

        Main.instance.StartCoroutine(CoRegisterUser(username, password));
    }

    public void CheckAccount(string username, string password)
    {
        m_isWaitServer = true;

        Main.instance.StartCoroutine(CoCheckAccount(username, password));    
    }

    //获取服务器列表
    public void FetchServerList()
    {
        Main.instance.StartCoroutine(CoFetchServerList());    
    }

    //连接服务器并发送登录请求
    public void ConnectServer(ServerInfo s)
    {
        if (m_isWaitServer)
            return;

        m_isWaitServer = true;
        //有服务器信息表示从服务器选择列表而来
        if (s != null)
        {
            m_serverInfo = s;
            m_inAutoReconn = false;
            //清除重发列表
            NetMgr.instance.ClearPendingMsgList();
        }
        else
        {
            m_inAutoReconn = true;

            Debuger.Log("正在尝试自动重连……");
        }

        NetMgr.instance.SetOnConnectOK(OnConnectOK);
        NetMgr.instance.SetOnConnectFail(OnConnectFail);
        NetMgr.instance.SetOnConnError(OnConnError);
        NetMgr.instance.Connect(m_serverInfo.host, m_serverInfo.port);
    }

    public void OnConnectOK()
    {
        Debuger.Log("连接服务器成功");

        //发送登录信息
        SendLogin();

        //要放后面
        m_inAutoReconn = false;
        m_startReconnTime = 0;
    }

    public void OnConnectFail()
    {
        m_isWaitServer = false;

        if (NetMgr.instance.NeedAutoRelogin && m_inAutoReconn)
        {
            if (m_startReconnTime <= 0)
                m_startReconnTime = TimeMgr.instance.GetTrueTimestamp();

            if (TimeMgr.instance.GetTrueTimestamp() - m_startReconnTime > RECONN_MAX_SECONDS)
            {
                Debuger.LogError("尝试重连服务器时间达到上限，服务器Id:{0}，服务器地址：{1}:{2}", m_serverInfo.serverId, m_serverInfo.host, m_serverInfo.port);

                m_inAutoReconn = false;
                //网络做清理
                NetMgr.instance.Close();

                UIMessageBox.Open(LanguageCfg.Get("link_error"), () => {
                    PlayerStateMachine.Instance.GotoState(enPlayerState.selectServer);
                });
            }
            else
            {
                Debuger.Log("自动重连失败，稍后再试");
                //过一段时间重试
                TimeMgr.instance.AddTimer(RETRY_WAITING, () =>
                {
                    ConnectServer(null);
                });
            }
        }
        else
        {
            Debuger.LogError("连接游戏服务器失败，服务器Id:{0}，服务器地址：{1}:{2}", m_serverInfo.serverId, m_serverInfo.host, m_serverInfo.port);
            UIMessageBox.Open(LanguageCfg.Get("link_field"), () => {});
        }
    }

    public void OnConnError(SocketError error, string info)
    {
        m_isWaitServer = false;
        NetMgr.instance.ClearWaitResponse();

        if (NetMgr.instance.NeedAutoRelogin)
        {
            //过一段时间重试
            TimeMgr.instance.AddTimer(RETRY_WAITING, () =>
            {
                m_startReconnTime = TimeMgr.instance.GetTrueTimestamp();
                ConnectServer(null);
            });
        }
        else
        {
            Debuger.LogError("连接游戏服务器失败，服务器Id:{0}，服务器地址：{1}:{2}", m_serverInfo.serverId, m_serverInfo.host, m_serverInfo.port);
            UIMessageBox.Open(LanguageCfg.Get("link_field"), () => { });
        }
    }

    //发送,登录
    public void SendLogin()
    {
        if (m_inAutoReconn && RoleMgr.instance.Hero != null)
        {
            Debuger.Log("正常尝试自动重新登录……");

            ReloginRequestVo request = new ReloginRequestVo();
            request.channelId = m_loginInfo.accountInfo.channelId;
            request.userId = m_loginInfo.accountInfo.userId;
            request.token = m_loginInfo.accountInfo.token;
            request.heroId = RoleMgr.instance.Hero.GetInt(enProp.heroId);
            request.lastLogin = RoleMgr.instance.Hero.GetInt(enProp.lastLogin);
            request.clientVer = ConfigValue.GetString(CLIENT_VER_KEY);
            request.lang = ConfigValue.clientLang;
            request.deviceModel = SystemInfo.deviceModel;
            request.osName = SystemInfo.operatingSystem;
            request.root = 0;
            request.macAddr = SystemInfo.deviceUniqueIdentifier;
            request.network = Util.GetNetworkType();
            request.screenWidth = Screen.width;
            request.screenHeight = Screen.height;
            NetMgr.instance.Send(MODULE.MODULE_ACCOUNT, MODULE_ACCOUNT.CMD_RELOGIN, request);
        }
        else
        {
            Debuger.Log("发送登录消息");

            LoginRequestVo request = new LoginRequestVo();
            request.channelId = m_loginInfo.accountInfo.channelId;
            request.userId = m_loginInfo.accountInfo.userId;
            request.token = m_loginInfo.accountInfo.token;
            request.serverId = m_serverInfo == null ? 0 : m_serverInfo.serverId;
            request.clientVer = ConfigValue.GetString(CLIENT_VER_KEY);
            request.lang = ConfigValue.clientLang;
            request.deviceModel = SystemInfo.deviceModel;
            request.osName = SystemInfo.operatingSystem;
            request.root = 0;
            request.macAddr = SystemInfo.deviceUniqueIdentifier;
            request.network = Util.GetNetworkType();
            request.screenWidth = Screen.width;
            request.screenHeight = Screen.height;
            NetMgr.instance.Send(MODULE.MODULE_ACCOUNT, MODULE_ACCOUNT.CMD_LOGIN, request);
        }        
    }

    //接收，登录
    [NetHandler(MODULE_ACCOUNT.CMD_LOGIN, true)]
    public void OnLogin(int errorCode, string errorMsg, RoleListVo info)
    {
        //登录成功了，才开启自动重连
        NetMgr.instance.NeedAutoRelogin = true;

        m_isWaitServer = false;

        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACCOUNT, errorCode, errorMsg);
            UIMessageBox.Open(errorMsg, () => {
                if (errorCode == RESULT_CODE_ACCOUNT.CHECK_TOKEN_FAIL)
                    PlayerStateMachine.Instance.GotoState(enPlayerState.login);
            });
            Debuger.LogError(errorMsg);

            NetMgr.instance.Close();//确保断开
            return;
        }

        PlayerStateMachine.Instance.GotoState(enPlayerState.selectRole, info);
    }

    [NetHandler(MODULE_ACCOUNT.CMD_RELOGIN, true)]
    public void OnRelogin(int errorCode, string errorMsg)
    {
        m_isWaitServer = false;

        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACCOUNT, errorCode, errorMsg);

            UIMessageBox.Open(errorMsg, () => {
                if (errorCode == RESULT_CODE_ACCOUNT.CHECK_TOKEN_FAIL)
                    PlayerStateMachine.Instance.GotoState(enPlayerState.login);
                else
                    PlayerStateMachine.Instance.GotoState(enPlayerState.selectServer);
            });
            Debuger.LogError(errorMsg);

            NetMgr.instance.Close();//确保断开
            return;
        }
        else
        {
            Debuger.Log("自动重新登录成功");
            UIMessage.Show(LanguageCfg.Get("link_success_auto"));
            NetMgr.instance.SendPendingMsgList();
        }
    }

    //帐号在其他地方登陆，被踢下线
    [NetHandler(MODULE_ACCOUNT.PUSH_FORCE_LOGOUT)]
    public void OnForceLogout(ForceLogoutVo info)
    {
        m_isWaitServer = false;

        //网络做清理
        NetMgr.instance.Close();

        if (info != null)
        {
            string msg = info.msg;
            int type = info.type;

            Debuger.LogError("被踢下线，原因：" + msg);

            System.Action processGoto = () => {
                switch (type)
                {
                    case ForceLogoutVo.GOTO_SVR_SELECT:
                        PlayerStateMachine.Instance.GotoState(enPlayerState.selectServer);
                        break;
                    case ForceLogoutVo.GOTO_LOGIN_UI:
                        PlayerStateMachine.Instance.GotoState(enPlayerState.login);
                        break;
                    case ForceLogoutVo.KEEP_CURRENT:
                        //不用跳，由别的消息处理函数来跳转
                        break;
                }
            };

            if (!string.IsNullOrEmpty(msg))
            {
                UIMessageBox.Open(msg, processGoto);
            }
            else
            {
                processGoto();
            }
        }
        else
        {
            Debuger.LogError("被踢下线");
        }
    }

    //发送提示消息
    [NetHandler(MODULE_ACCOUNT.PUSH_TIP_MSG)]
    public void OnLoginTipMsg(LoginTipMsgVo info)
    {
        if (!string.IsNullOrEmpty(info.msg))
            UIMessageBox.Open(info.msg, () => { });
    }

    public void SendCreateRole(string roleId, string name)
    {
        CreateRoleRequestVo request = new CreateRoleRequestVo();
        request.roleId = roleId;
        request.name = name;
        NetMgr.instance.Send(MODULE.MODULE_ACCOUNT, MODULE_ACCOUNT.CMD_CREATE_ROLE, request);

        m_isWaitServer = true;
    }

    [NetHandler(MODULE_ACCOUNT.CMD_CREATE_ROLE, true)]
    public void OnCreateRole(int errorCode, string errorMsg, RoleListVo info)
    {
        m_isWaitServer = false;

        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACCOUNT, errorCode, errorMsg);
            UIMessageBox.Open(errorMsg, () => { });
            Debuger.LogError(errorMsg);
            return;
        }

        var heroId = info.roleList[0].heroId;
        SendActivateRole(heroId);
    }

    public void SendActivateRole(int heroId)
    {
        ActivateRoleRequestVo request = new ActivateRoleRequestVo();
        request.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_ACCOUNT, MODULE_ACCOUNT.CMD_ACTIVATE_ROLE, request);
    }

    [NetHandler(MODULE_ACCOUNT.CMD_ACTIVATE_ROLE, true)]
    public void OnActivateRole(int errorCode, string errorMsg, FullRoleInfoVo info)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACCOUNT, errorCode, errorMsg);

            UIMessageBox.Open(errorMsg, () => { PlayerStateMachine.Instance.GotoState(enPlayerState.selectServer); });
            Debuger.LogError(errorMsg);

            NetMgr.instance.Close();//确保断开
            return;
        }
        PlayerStateMachine.Instance.GotoState(enPlayerState.playGame, info);
    }

    public void SendServerTimeReq()
    {
        NetMgr.instance.Send(MODULE.MODULE_ACCOUNT, MODULE_ACCOUNT.CMD_SERVER_TIME, null, false);
    }

    [NetHandler(MODULE_ACCOUNT.CMD_SERVER_TIME)]
    public void OnRetServerTime(SyncServerTime info)
    {
        OnPushServerTime(info);
    }

    [NetHandler(MODULE_ACCOUNT.PUSH_SERVER_TIME)]
    public void OnPushServerTime(SyncServerTime info)
    {
        string s1 = TimeMgr.instance.GetTrueDateTime().ToString();
        string s2 = TimeMgr.instance.GetClientDateTime().ToString();
        TimeMgr.instance.SetServerTimeInfo(info.time, info.tzOffset);
        string s3 = TimeMgr.instance.GetClientDateTime().ToString();
        string s4 = TimeMgr.instance.GetClientTimeZoneOffset().ToString();
        string s5 = TimeMgr.instance.GetServerTimeZoneOffset().ToString();
        Debuger.Log("当前真实时间：{0}    原虚拟时间：{1}    新虚拟时间：{2}    客户端与UTC时差：{3}分钟    服务端与UTC时差：{4}分钟", s1, s2, s3, s4, s5);
        
    }

    public void SendPing()
    {
        NetMgr.instance.Send(MODULE.MODULE_ACCOUNT, MODULE_ACCOUNT.CMD_PING, null, false);
    }

    [NetHandler(MODULE_ACCOUNT.CMD_PING)]
    public void OnRetPing()
    {
    }

    [NetHandler(MODULE_ACCOUNT.PUSH_PING)]
    public void OnPushPing(SyncServerTime info)
    {
        SendPing();
    }

    public void SendGetDemoHeroData(string roleId)
    {
        var req = new GetDemoHeroDataReq();
        req.roleId = roleId;
        NetMgr.instance.Send(MODULE.MODULE_ACCOUNT, MODULE_ACCOUNT.CMD_DEMO_HERO_DATA, req);
    }

    [NetHandler(MODULE_ACCOUNT.CMD_DEMO_HERO_DATA, true)]
    public void OnGetDemoHeroData(int errorCode, string errorMsg, FullRoleInfoVo info)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACCOUNT, errorCode, errorMsg);

            UIMessageBox.Open(errorMsg, () => { PlayerStateMachine.Instance.GotoState(enPlayerState.selectServer); });
            Debuger.LogError(errorMsg);

            NetMgr.instance.Close();//确保断开
            return;
        }

        //创建演示用的英雄
        RoleMgr.instance.CreateHero(info);
        //进入序章
        LevelMgr.instance.ChangeLevel(IntroductionScene.ROOM_ID);
    }
    #endregion

    #region Private Methods
    IEnumerator CoRegisterUser(string username, string password, bool isDoAutoReg = false)
    {
        //构造参数
        JsonData reqJson = new JsonData();
        reqJson["username"] = username;
        reqJson["password"] = password;
        byte[] postData = Tea16.EncryptString(reqJson.ToJson());

        //请求并等待回应
        WWW www = new WWW("http://" + ConfigValue.GetString(LOGIN_HOST_KEY) + "/register", postData);
        yield return www;

        bool keepWaitServer = false;

        try
        {
            //判断错误
            if (!string.IsNullOrEmpty(www.error))
            {
                Debuger.LogError("访问服务器失败：{0}", www.error);
                UIMessageBox.Open(LanguageCfg.Get("link_field"), () => { });
                yield break;
            }

            //转换结果
            string resultStr = Tea16.DecryptString(www.bytes);
            Debuger.Log("注册结果:{0}", resultStr);

            JsonData result = JsonMapper.ToObject(resultStr);
            int code = (int)result["code"];
            string msg = (string)result["msg"];
            if (code != 0)
            {
                UIMessageBox.Open(msg, () => { });
                yield break;
            }

            //注册成功了？
            //如果原来自动注册的，那就自动登录吧
            if (isDoAutoReg)
            {
                Main.instance.StartCoroutine(CoCheckAccount(username, password));
                keepWaitServer = true;
            }
            else
            {
                UIMessageBox.Open(LanguageCfg.Get("reg_account_success"), () =>
                {
                    UIMgr.instance.Get<UILogin>().SetUsernamePassword(username, password);
                    UIMgr.instance.Close<UIRegister>();
                });
            }
        }
        catch (JsonException e)
        {
            Debuger.LogError("解析服务器下发的数据错误：{0}", e);
            UIMessageBox.Open(LanguageCfg.Get("parse_message_error"), () => { });
        }
        finally
        {
            www.Dispose();
            m_isWaitServer = keepWaitServer;
        }
    }

    IEnumerator CoCheckAccount(string username, string password)
    {
        //构造参数
        JsonData reqJson = new JsonData();
        reqJson["username"] = username;
        reqJson["password"] = password;
        byte[] postData = Tea16.EncryptString(reqJson.ToJson());

        //请求并等待回应
        WWW www = new WWW("http://" + ConfigValue.GetString(LOGIN_HOST_KEY) + "/login", postData);
        yield return www;

        bool keepWaitServer = false;

        try
        {
            //判断错误
            if (!string.IsNullOrEmpty(www.error))
            {
                Debuger.LogError("访问服务器失败：{0}", www.error);
                UIMessageBox.Open(LanguageCfg.Get("link_field"), () => { });
                yield break;
            }

            //转换结果
            string resultStr = Tea16.DecryptString(www.bytes);
            Debuger.Log("登录结果:{0}", resultStr);

            JsonData result = JsonMapper.ToObject(resultStr);
            int code = (int)result["code"];
            string msg = (string)result["msg"];
            if (code != 0)
            {
                //如果不用注册且账号不存，那就走自动注册
                if (!NEED_REGISTER && code == 7)
                {
                    Main.instance.StartCoroutine(CoRegisterUser(username, password, true));
                    keepWaitServer = true;
                }
                else
                {
                    UIMessageBox.Open(msg, () => { });
                }
                yield break;
            }

            //账号对了
            //获取一些信息
            JsonData cxt = (JsonData)result["cxt"];
            string userId = (string)cxt["userId"];
            string token = (string)cxt["token"];
            m_loginInfo.accountInfo.channelId = ConfigValue.GetString(CHANNEL_ID_KEY);
            m_loginInfo.accountInfo.userId = userId;
            m_loginInfo.accountInfo.token = token;
           
            //跳到服务器选择态
             PlayerStateMachine.Instance.GotoState(enPlayerState.selectServer);

            //获取服务器列表  
     //       NetMgr.instance.AccountHandler.FetchServerList();
        }
        catch (JsonException e)
        {
            Debuger.LogError("解析服务器下发的数据错误：{0}", e);
            UIMessageBox.Open(LanguageCfg.Get("parse_message_error"), () => { });
        }
        finally
        {
            www.Dispose();
            m_isWaitServer = keepWaitServer;
        }
    }

    IEnumerator CoFetchServerList()
    {
        //构造参数
        JsonData reqJson = new JsonData();
        reqJson["channelId"] = m_loginInfo.accountInfo.channelId;
        reqJson["userId"] = m_loginInfo.accountInfo.userId;
        byte[] postData = Tea16.EncryptString(reqJson.ToJson());

        //请求并等待回应
        WWW www = new WWW("http://" + ConfigValue.GetString(LOGIN_HOST_KEY) + "/getServers", postData);
        yield return www;

        try
        {
            //判断错误
            if (!string.IsNullOrEmpty(www.error))
            {
                Debuger.LogError("访问服务器失败：{0}", www.error);
                UIMessageBox.Open(LanguageCfg.Get("link_field"), () => { });
                yield break;
            }

            //转换结果
            string resultStr = Tea16.DecryptString(www.bytes);
            Debuger.Log("获取结果:{0}", resultStr);

            JsonData result = JsonMapper.ToObject(resultStr);
            int code = (int)result["code"];
            string msg = (string)result["msg"];
            if (code != 0)
            {
                UIMessageBox.Open(msg, () => { });
                yield break;
            }

            //账号对了
            //获取一些信息
            JsonData cxt = (JsonData)result["cxt"];
            m_loginInfo.Init(cxt);

            UILogin2 ui = UIMgr.instance.Get<UILogin2>();
            if (ui.IsOpen)
                ui.UpdateStateUI();
        }
        catch (JsonException e)
        {
            Debuger.LogError("解析服务器下发的数据错误：{0}", e);
            UIMessageBox.Open(LanguageCfg.Get("parse_message_error"), () => { });
        }
        finally
        {
            www.Dispose();
        }
    }

    IEnumerator CoCheckTimeAndPing()
    {
        var lastClientTime = TimeMgr.instance.GetTrueDateTime();
        var lastPingTime = lastClientTime;

        while (true)
        {
            yield return 0;

            var curTime = TimeMgr.instance.GetTrueDateTime();
            var timeSpan = curTime - lastClientTime;
            lastClientTime = curTime;

            var conn = NetMgr.instance.Connector;
            if (conn == null || !conn.isActive() || m_isWaitServer || m_inAutoReconn)
                continue;

            //检测客户端时间，如果两帧变化太大，那重新请求服务器时间
            if (Math.Abs(timeSpan.TotalSeconds) > BAD_TIME_GAP)
                SendServerTimeReq();

            if (Math.Abs((curTime - lastPingTime).TotalSeconds) > PING_INTERVAL)
            {
                lastPingTime = curTime;
                SendPing();
            }                
        }
    }

    #endregion
}
