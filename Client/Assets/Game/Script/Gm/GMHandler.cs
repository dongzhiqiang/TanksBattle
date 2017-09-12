using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[NetModule(MODULE.MODULE_GM)]
public class GMHandler
{
    #region Fields
    private int m_gmCmdHistoryPos = int.MaxValue;
    private List<string> m_gmCmdHistory = new List<string>();
    #endregion


    #region Properties
    #endregion


    #region Net
    //发送,gm命令
    public void SendProcessGMCmd(string msg)
    {
        if(string.IsNullOrEmpty(msg))
            return;

        int oldIndex = m_gmCmdHistory.FindIndex((s) => { return s == msg; });
        if (oldIndex >= 0)
            m_gmCmdHistory.RemoveAt(oldIndex);
        m_gmCmdHistoryPos = int.MaxValue;
        m_gmCmdHistory.Add(msg);

        //先处理本地的
        if (ProcessLocal(msg))
            return;

        DebugUI.instance.gmCmdResult = "";

        //本地没有处理再发给服务器
        ProcessGmCmdVo request = new ProcessGmCmdVo();
        request.msg = msg;
        NetMgr.instance.Send(MODULE.MODULE_GM, MODULE_GM.CMD_PROCESS_CM_CMD, request); 
    }

    //接收，gm命令
    [NetHandler(MODULE_GM.CMD_PROCESS_CM_CMD)]
    public void OnProcessGMCmd()
    {
        DebugUI.instance.gmCmdResult = "指令执行成功";//info.reqString + (info.result ? "成功" : "失败");
    }


    #endregion

    bool ProcessLocal(string msg)
    {
        var reg = new Regex(@"\s+");
        string[] args = reg.Split(msg);

        //先过滤下本地GM命令
        switch (args[0].ToLower())//注意下面的case常量都要输入小写
        {
            case "addmonster": OnAddMonster(args); return true;
            case "setprop": OnSetProp(args); return true;
            case "addbuff": OnAddBuff(args); return true;
            case "removebuff": OnRemoveBuff(args); return true;
            case "sendmsg": OnSendNetMsg(args); return true;
            case "element": OnElement(args); return true;
            case "joystickcheck": OnJoystickCheck(args); return true;
            default:return false;
        }
    }


    void OnAddMonster(string[] ss)
    {
        //找出怪物id
        if (ss.Length <= 1)
        {
            Debuger.LogError("参数出错");
            return;
        }
        string roleId = ss[1];

        RoleCfg cfg = RoleCfg.Get(roleId);

        string aiType = cfg.aiType;
        if (ss.Length > 2 && ss[2] == "0")
            aiType = AIPart.NoneAI;

        SceneCfg.BornInfo bornInfo = SceneMgr.instance.GetNewBornInfo();
        if (bornInfo == null)
        {
            Debuger.LogError("找不到出生位置");
            return;
        }
        RoleCfg.PreLoad(roleId);//战斗相关的资源都是预加载的所以这里要预加载下
        RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
        cxt.level = Room.instance.roomCfg.levelLv;
        cxt.OnClear();
        cxt.Init(Util.GenerateGUID(), "", roleId, 1, enCamp.camp2, bornInfo.mPosition, bornInfo.mEulerAngles, cfg.bornType, cfg.deadType, cfg.groundDeadType, aiType);
        RoleMgr.instance.CreateRole(cxt);
        UIMessage.Show("创建怪物成功");
    }

    void OnSetProp(string[] ss)
    {
        //找出怪物id
        if (ss.Length < 3)
        {
            Debuger.LogError("参数个数太少");
            return;
        }

        int prop;
        float value;
        int poolId = -1;
        if (!int.TryParse(ss[1], out prop) || !float.TryParse(ss[2], out value))
        {
            Debuger.LogError("参数解析出错");
            return;
        }
        if (ss.Length <= 3 || !int.TryParse(ss[3], out poolId))
            poolId = -1;

        Role hero = poolId == -1?RoleMgr.instance.Hero:RoleMgr.instance.GetRole(poolId);
        if (hero == null || hero.State != Role.enState.alive)
        {
            UIMessage.Show("不能设置，没有找到角色或者角色不在存活状态");
            return;
        }
            

        if (prop > (int)enProp.minFightProp && prop < (int)enProp.maxFightProp)
        {
            hero.SetFloat((enProp)prop, value);
            UIMessage.Show("设置属性成功");
        }
        else if (prop > (int)enProp.maxFightProp && prop < (int)enProp.channelId)
        {
            hero.SetInt((enProp)prop, (int)value);
            UIMessage.Show("设置属性成功");
        }
        else
        {
            Debuger.LogError("枚举值不允许");
        }        
    }

    void OnAddBuff(string[] ss)
    {
        
        int id;
        int poolId = -1;
        if (ss.Length<=1 || !int.TryParse(ss[1], out id) )
        {
            Debuger.LogError("参数解析出错");
            return;
        }
        if (ss.Length <= 2 || !int.TryParse(ss[2], out poolId))
            poolId = -1;
        Role hero = poolId == -1 ? RoleMgr.instance.Hero : RoleMgr.instance.GetRole(poolId);
        if (hero == null )
        {
            UIMessage.Show("不能添加状态，没有找到角色");
            return;
        }

        Buff buff = hero.BuffPart.AddBuff(id);
        if (buff!=null)
            Debuger.Log("添加状态{0}成功", id);
        else
            Debuger.Log("添加状态{0}失败", id);
    }

    void OnRemoveBuff(string[] ss)
    {
        int id,clear=0;
        int poolId = -1;
        if (ss.Length <= 1 || !int.TryParse(ss[1], out id) || (ss.Length > 3 && !int.TryParse(ss[3], out clear)))
        {
            Debuger.LogError("参数解析出错");
            return;
        }
        if (ss.Length <= 2 || !int.TryParse(ss[2], out poolId))
            poolId = -1;
        Role hero = poolId == -1 ? RoleMgr.instance.Hero : RoleMgr.instance.GetRole(poolId);
        if (hero == null )
        {
            UIMessage.Show("不能删除状态，没有找到角色或者角色不在存活状态");
            return;
        }

        int num = hero.BuffPart.RemoveBuffByBuffId(id, clear != 0);
        Debuger.Log("删除了{0}个状态",num);
    }

    void OnSendNetMsg(string[] ss)
    {
        if (ss.Length < 4)
        {
            Debuger.LogError("格式：sendmsg 模块号 命令号 json内容");
            return;
        }
        byte module;
        if (!byte.TryParse(ss[1], out module))
        {
            Debuger.LogError("模块号必须是整数");
            return;
        }
        int command;
        if (!int.TryParse(ss[2], out command))
        {
            Debuger.LogError("命令号必须是整数");
            return;
        }

        NetMgr.instance.SendJsonString(module, command, ss[3]);
    }
    void OnElement(string[] ss)
    {
        int elem;
        if (ss.Length <= 1 || !int.TryParse(ss[1], out elem))
        {
            Debuger.LogError("参数解析出错");
            return;
        }
        NetMgr.instance.WeaponHandler.SendElementChange(elem);
    }

    void OnJoystickCheck(string[] ss)
    {
        float checkTime;
        if (ss.Length <= 1 || !float.TryParse(ss[1], out checkTime))
        {
            Debuger.LogError("参数解析出错");
            return;
        }

        var joystick = UIMgr.instance.Get<UILevel>().Get<UILevelAreaJoystick>().m_joystick;
#if UNITY_EDITOR || UNITY_STANDALONE
        joystick.m_sliderCheckTimePC = checkTime;
#else
        joystick.m_sliderCheckTime = checkTime;
#endif

        UIMessage.Show("修改摇杆翻滚判定成功");
    }

    public string GetCmdInHistory(bool up)
    {
        if (m_gmCmdHistory.Count <= 0)
            return "";

        m_gmCmdHistoryPos = Mathf.Clamp(m_gmCmdHistoryPos + (up ? -1 : 1), 0, m_gmCmdHistory.Count - 1);
        return m_gmCmdHistory[m_gmCmdHistoryPos];
    }
}
