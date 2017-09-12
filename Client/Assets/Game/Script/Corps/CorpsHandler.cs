using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[NetModule(MODULE.MODULE_CORPS)]
public class CorpsHandler
{
    /******************************************************* 请求消息 ***************************************************************/
    //创建公会
    public void CreateCorps(string name)
    {
        CreateCorpsReq req = new CreateCorpsReq();
        req.name = name;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_CREATE_CORPS, req);
    }
    //请求公会信息
    public void ReqCorpsData(int corpsId, bool isFirst)
    {
        CorpsDataReq req = new CorpsDataReq();
        req.corpsId = corpsId;
        req.isFirst = isFirst;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_REQ_CORPS, req);
    }
    //请求会员和申请者信息
    public void ReqCorpsMembersReqs(int corpsId, bool isInit)
    {
        CorpsMembersReq req = new CorpsMembersReq();
        req.corpsId = corpsId;
        req.isInit = isInit;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_REQ_MEMBERS, req);
    }
    //修改公会宣言
    public void ModifyCorpsDeclare(int corpsId, string declare, int heroId)
    {
        ModifyCorpsDeclareReq req = new ModifyCorpsDeclareReq();
        req.corpsId = corpsId;
        req.declare = declare;
        req.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_MODIFYDECLARE, req);
    }
    //入会申请
    public void ApplyJoinCorps(int corpsId, int heroId)
    {
        ApplyJoinCorpsReq req = new ApplyJoinCorpsReq();
        req.corpsId = corpsId;
        req.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_APPLY_JOIN, req);
    }
    //处理会员操作
    //type——1：同意入会 2：拒绝入会 3：踢出公会 4：任命职位
    public void HandleMember(int corpsId, int handler, int beHandler, string beHandlerName, int type, int opt)
    {
        HandleMemberReq req = new HandleMemberReq();
        req.corpsId = corpsId;
        req.handler = handler;
        req.beHandler = beHandler;
        req.beHandlerName = beHandlerName;
        req.type = type;
        req.option = opt;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_HANDLE_MEMBER, req);
    }
    //请求所有公会的基础信息
    public void ReqGetAllCorps(bool isFirst)
    {
        GetAllCorpsReq req = new GetAllCorpsReq();
        req.isFirst = isFirst;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_REQ_ALL_CORPS, req);
    }

    //公会设置
    public void ReqCorpsSet(int corpsId, int handler, int type, int setting, int level)
    {
        CorpsSetReq req = new CorpsSetReq();
        req.corpsId = corpsId;
        req.handler = handler;
        req.type = type;
        req.opt1 = setting;
        req.opt2 = level;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_CORPS_SET, req);
    }

    //退出公会/会长解散公会
    public void QuitCorps(int corpsId, int heroId)
    {
        ExitCorpsReq req = new ExitCorpsReq();
        req.corpsId = corpsId;
        req.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_EXIT_CORPS, req);
    }
    //请求弹劾情况信息
    public void ReqImpeachInfo(int corpsId)
    {
        ImpeachStatusReq req = new ImpeachStatusReq();
        req.corpsId = corpsId;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_REQ_IMPEACH_STATUS, req);
    }
    //发起弹劾
    public void InitiateImpeach(int corpsId, int heroId)
    {
        InitiateImpeachReq req = new InitiateImpeachReq();
        req.corpsId = corpsId;
        req.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_INITIATE_IMPEACH, req);
    }
    //同意弹劾
    public void AgreeImpeach(int corpsId, int heroId)
    {
        AgreeImpeachReq req = new AgreeImpeachReq();
        req.corpsId = corpsId;
        req.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_AGREE_IMPEACH, req);
    }
    //请求公会建设数据
    public void ReqCorpsBuildData(int corpsId)
    {
        CorpsBuildDataReq req = new CorpsBuildDataReq();
        req.corpsId = corpsId;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_CORPS_BUILD_DATA, req);
    }
    //请求建设公会
    public void ReqBuildCorps(int corpsId, int heroId, int buildId)
    {
        BuildCorpsReq req = new BuildCorpsReq();
        req.corpsId = corpsId;
        req.heroId = heroId;
        req.buildId = buildId;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_BUILD_CORPS, req);
    }
    //请求查看其他公会
    public void ReqOtherCorps(int corpsId)
    {
        OtherCorpsReq req = new OtherCorpsReq();
        req.corpsId = corpsId;
        NetMgr.instance.Send(MODULE.MODULE_CORPS, MODULE_CORPS.CMD_REQ_OTHER_CORPS, req);
    }

    /******************************************************* 数据返回 ***************************************************************/
    //创建公会返回消息
    [NetHandler(MODULE_CORPS.CMD_CREATE_CORPS)]
    public void CreateCorpsHdl(CorpsDataRes info)
    {
        if (info == null)
            return;
        CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;
        corpsPart.UpdateCorpsProps(info.corpsProps);

        UICreateCorps createUI = UIMgr.instance.Get<UICreateCorps>();
        if (createUI.IsOpen)  //关闭创建界面
            createUI.Close();

        UIMessage.ShowFlowTip("create_corps_success");
        UIMgr.instance.Get<UICorpsList>().Close();
        UICorps ui = UIMgr.instance.Open<UICorps>();
        ui.UpdateOverViewPage();

        RoleMgr.instance.Hero.Fire(MSG_ROLE.JOIN_CORPS);  //发送通知
        corpsPart.hasReqCorpsIds.Clear();
    }

    //请求公会数据返回消息
    [NetHandler(MODULE_CORPS.CMD_REQ_CORPS)]
    public void GetCorpsDataHdl(CorpsDataRes info)
    {
        if (info == null)
            return;
        CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;
        corpsPart.UpdateCorpsProps(info.corpsProps);

        //更新日志数据
        if (info.isFirst)
            corpsPart.UpdateAllLog(info.logs);

        //更新界面显示
        UICorps ui = UIMgr.instance.Get<UICorps>();
        if (ui.IsOpen)
            ui.UpdateOverViewPage();
       
    }
    //申请加入公会返回、加入公会通知、被踢出公会通知
    [NetHandler(MODULE_CORPS.CMD_APPLY_JOIN)]
    public void CorpsJoinExitResHdl(CorpsJoinExitRes resData)
    {
        if (resData == null)
            return;
        switch (resData.status)
        {
            case 0:   //申请成功
                UIMessage.ShowFlowTip("req_corps_success");
                RoleMgr.instance.Hero.CorpsPart.hasReqCorpsIds.Add(resData.option);
                UICorpsList list = UIMgr.instance.Get<UICorpsList>();
                if (list.IsOpen)
                    list.OnUpdateList(false);
            
                break;
            case 1:  //加入了公会
                //关闭公会列表
                UIMgr.instance.Close<UICorpsList>();
                UIMessage.ShowFlowTip("join_corps_success");
                CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;
                corpsPart.UpdateCorpsProps(resData.corpsProps);
                //打开公会并更新界面显示
                UICorps ui = UIMgr.instance.Open<UICorps>();
                ui.UpdateOverViewPage();
                //修改属性值
                RoleMgr.instance.Hero.SetInt(enProp.corpsId, resData.corpsProps.corpsId);

                RoleMgr.instance.Hero.Fire(MSG_ROLE.JOIN_CORPS);  //发送通知
                corpsPart.hasReqCorpsIds.Clear();
                break;
            case 2:  //被踢出了公会
                UIMessage.ShowFlowTip("be_kickout_from_corps");
                UICorps uic = UIMgr.instance.Get<UICorps>();
                if (uic.IsOpen)
                    uic.Close();
                break;
            case 3:   //已在申请列表中
                UIMessage.ShowFlowTip("has_req_already");
                RoleMgr.instance.Hero.CorpsPart.hasReqCorpsIds.Add(resData.option);
                break;
            default:
                break;
        }
    }

    //请求会员数据和申请数据返回 只返回有更新的数据
    [NetHandler(MODULE_CORPS.CMD_REQ_MEMBERS)]
    public void GetCorpsMemsReqsHdl(CorpsMembersRes info)
    {
        if (info == null)
            return;
        CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;

        if (!info.isInit)
        {
            if (info.members.Count > 0)
                corpsPart.UpdateCorpsMembers(info.members);
        }
        else   //第一次初始数据
            corpsPart.InitMemsReqs(info.members, info.reqs);

        //更新界面
        UICorps ui = UIMgr.instance.Get<UICorps>();
        if (ui.IsOpen)
        {
            ui.UpdateMembersPage();
        }
    }
    //修改公会宣言返回消息
    [NetHandler(MODULE_CORPS.CMD_MODIFYDECLARE)]
    public void ModifyCorpsDeclareHdl(ModifyCorpsDeclareRes info)
    {
        if (info == null)
            return;
        UIMessage.ShowFlowTip("modify_declare");
        UIInputBox inputBox = UIMgr.instance.Get<UIInputBox>();
        if (inputBox.IsOpen)
            inputBox.Close();

        CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;
        corpsPart.corpsInfo.props.declare = info.newDeclare;
        //更新界面显示
        UICorps ui = UIMgr.instance.Get<UICorps>();
        if (ui.IsOpen)
        {
            ui.UpdateOverViewPage();
        }
    }
    //处理会员操作返回
    [NetHandler(MODULE_CORPS.CMD_HANDLE_MEMBER)]
    public void HandleMemberHdl(HandleMemberRes resData)
    {
        if (resData == null)
            return;

        //更新数据
        CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;
        switch (resData.type)
        {
            case (int)HandlerMemberType.agree:  //同意入会
                //这里只更新申请列表，会员列表更新统一由推送消息返回才更新
                corpsPart.RemoveReqById(resData.beHandelId);
                if(resData.opt == -1)
                    UIMessage.Show("该玩家已加入其他公会");
                else
                    UIMessage.Show(resData.beHandelName + "加入公会成功");
                break;
            case (int)HandlerMemberType.refuse: //拒绝入会
                corpsPart.RemoveReqById(resData.beHandelId);
                UIMessage.Show(string.Format("拒绝{0}加入公会", resData.beHandelName));
                break;
            case (int)HandlerMemberType.kickout: //踢出公会
                corpsPart.RemoveMemberById(resData.beHandelId);
                UIMessage.Show(string.Format("将{0}踢出出了公会", resData.beHandelName));
                break;
            case (int)HandlerMemberType.appoint:  //任命职位
                //更新对方的职位
                corpsPart.UpdatePosById(resData.beHandelId, resData.opt);
                if (resData.opt == (int)CorpsPosEnum.President)  //任命他人为会长
                {
                    corpsPart.UpdatePosById(RoleMgr.instance.Hero.GetInt(enProp.heroId), (int)CorpsPosEnum.Common);
                    corpsPart.corpsInfo.props.president = resData.beHandelName;
                }
                UIMessage.Show(string.Format("任命{0}为{1}", resData.beHandelName, CorpsPosFuncCfg.Get(resData.opt).posName));
                break;
        }

        //更新界面显示
        UICorps ui = UIMgr.instance.Get<UICorps>();
        if (ui.IsOpen)
        {
            ui.UpdateMembersPage();
            if (resData.type == 4 && resData.opt == (int)CorpsPosEnum.President)
                ui.UpdateOverViewPage();
        }
    }
    //主动推送的自己公会职位变动通知
    [NetHandler(MODULE_CORPS.PUSH_CORPS_POS_CHANGE)]
    public void CorpsPosChangeHdl(CorpsPosChangeRes resData)
    {
        //更新数据
        CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;
        if (resData.pos == (int)CorpsPosEnum.President)
        {
            UIMessage.Show("会长将公会转让给你，你成为了新会长！");
            //原会长要变成会员
            corpsPart.MakeOldPreCommon();
        }
        else if (resData.pos == (int)CorpsPosEnum.Elder)
            UIMessage.Show("你被任命为长老");
        else if (resData.pos == (int)CorpsPosEnum.Common)
            UIMessage.Show("你成为了会员");

        corpsPart.personalInfo.pos = resData.pos;
        //更新界面显示
        UICorps ui = UIMgr.instance.Get<UICorps>();
        if (ui.IsOpen)
            ui.UpdateMembersPage();
    }

    //所有公会数据返回
    [NetHandler(MODULE_CORPS.CMD_REQ_ALL_CORPS)]
    public void getAllCorpsHdl(GetAllCorpsRes resData)
    {
        CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;
        corpsPart.SetCorpsList(resData.corpsList);
        //申请过的记录
        if(corpsPart.getHasReq)
        {
            corpsPart.hasReqCorpsIds = resData.hasReqs;
            corpsPart.getHasReq = false;
        }
        UICorpsList ui = UIMgr.instance.Get<UICorpsList>();
        if (ui.IsOpen)
            ui.OnUpdateList();
    }

    //公会设置返回
    [NetHandler(MODULE_CORPS.CMD_CORPS_SET)]
    public void CorpsSetHdl(CorpsSetRes resData)
    {
        if (resData == null)
            return;
        CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;
        if (resData.type == 1)  //入会设置
        {
            corpsPart.corpsInfo.props.joinSet = resData.opt1;
            corpsPart.corpsInfo.props.joinSetLevel = resData.opt2;
        }

        UIMessage.ShowFlowTip("handle_success");
        UICorpsJoinSet ui = UIMgr.instance.Get<UICorpsJoinSet>();
        //更新一下界面
        if (ui.IsOpen)
            //ui.UpdateSetting();
            ui.Close();
    }
    //通知新成员信息和新申请信息
    [NetHandler(MODULE_CORPS.PUSH_CORPS_MEMBER_REQ)]
    public void PushNewMemsRes(PushNewMemsReqDataRes resData)
    {
        if (resData == null)
            return;
        CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;
        if (resData.newMems.pos == (int)CorpsPosEnum.Req)//申请
            corpsPart.AddReq(resData.newMems);
        else
            corpsPart.AddMember(resData.newMems);

        UICorps ui = UIMgr.instance.Get<UICorps>();
        if (ui.IsOpen)
            ui.UpdateMembersPage();
    }

    //退出公会/解散公会返回
    [NetHandler(MODULE_CORPS.CMD_EXIT_CORPS)]
    public void ExitCorpsHdl(ExitCorpsRes resData)
    {
        if (resData == null)
            return;
        UIMessage.Show("您已退出公会");
        CorpsPart corpsPart = RoleMgr.instance.Hero.CorpsPart;
        corpsPart.ClearCorps();
        RoleMgr.instance.Hero.SetInt(enProp.corpsId, 0);
        UIMgr.instance.Get<UICorps>().Close();
    }
    //推送新的日志
    [NetHandler(MODULE_CORPS.PUSH_NEW_LOG)]
    public void PushNewLogHdl(PushNewLogRes resData)
    {
        if (resData == null)
            return;
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.AddLog(resData.newLog);
        UICorpsLog ui = UIMgr.instance.Get<UICorpsLog>();
        if (ui.IsOpen)
        {
            //刷新界面前先排序一下
            part.SortLogs();
            ui.UpdateLogs();
        }
    }
    //请求弹劾状态信息返回
    [NetHandler(MODULE_CORPS.CMD_REQ_IMPEACH_STATUS)]
    public void ImpeachStatusHdl(ImpeachStatusRes resData)
    {
        if (resData == null)
            return;
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.corpsInfo.impeach = resData.impeach;

        UICorpsImpeach ui = UIMgr.instance.Get<UICorpsImpeach>();
        if (ui.IsOpen)
            ui.UpdatePanel();
    }
    //发起弹劾返回
    [NetHandler(MODULE_CORPS.CMD_INITIATE_IMPEACH)]
    public void InitiateImpeachHdl(ImpeachStatusRes resData)
    {
        if (resData == null)
            return;
        UIMessage.Show("发起弹劾成功");

        //更新UI
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.corpsInfo.impeach = resData.impeach;

        UICorpsImpeach ui = UIMgr.instance.Get<UICorpsImpeach>();
        if (ui.IsOpen)
            ui.UpdatePanel();
    }
    //赞成弹劾返回
    [NetHandler(MODULE_CORPS.CMD_AGREE_IMPEACH)]
    public void AgreeImpeachHdl(AgreeImpeachRes resData)
    {
        if (resData == null)
            return;
        UIMessage.Show("赞成弹劾成功");
        if (resData.result == 2)   //弹劾成功流程结束，会长已换人关闭界面
            UIMgr.instance.Get<UICorpsImpeach>().Close();
    }
    //推送弹劾成功
    [NetHandler(MODULE_CORPS.PUSH_IMPEACH_SUCCESS)]
    public void PushImpeachSuccessHdl(PushImpeachSuccessRes resData)
    {
        if (resData == null)
            return;
        UIMessage.Show(string.Format("弹劾成功，{0}成为了新会长", resData.newPreName));
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.UpdatePosById(resData.oriPreId, (int)CorpsPosEnum.Common);
        part.UpdatePosById(resData.newPreId, (int)CorpsPosEnum.President);
        part.corpsInfo.props.president = resData.newPreName;

        //更新界面
        UICorps ui = UIMgr.instance.Get<UICorps>();
        if(ui.IsOpen)
        {
            ui.UpdateOverViewPage();
            ui.UpdateMembersPage();
        }
    }
    //请求公会建设数据返回
    [NetHandler(MODULE_CORPS.CMD_CORPS_BUILD_DATA)]
    public void CorpsBuildDataResHdl(CorpsBuildDataRes resData)
    {
        if (resData == null)
            return;
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.corpsInfo.props.buildNum = resData.buildNum;
        part.ownBuildState = resData.personalState;
        part.corpsInfo.buildLogs = resData.buildLog;

        UICorpsBuild ui = UIMgr.instance.Get<UICorpsBuild>();
        if (ui.IsOpen)
            ui.UpdatePanel();
    }
    //建设公会返回
    [NetHandler(MODULE_CORPS.CMD_BUILD_CORPS)]
    public void BuildCorpsResHdl(BuildCorpsRes resData)
    {
        if (resData == null)
            return;
        UIMessage.Show("建设成功！");

        //更新part
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.ownBuildState[resData.buildId - 1] = 1;   //设置为建设过
        part.personalInfo.contribution = resData.contri;
        part.corpsInfo.props.growValue = resData.constru;
        part.corpsInfo.props.level = resData.level;
        part.corpsInfo.props.buildNum = resData.buildNum;

        //更新UI
        UICorpsBuild ui = UIMgr.instance.Get<UICorpsBuild>();
        if (ui.IsOpen)
            ui.UpdatePanel();
        UICorps ui2 = UIMgr.instance.Get<UICorps>();
        if (ui2.IsOpen)
        {
            ui2.UpdateOverViewPage();
            ui2.UpdateMembersPage();
        }

        RoleMgr.instance.Hero.Fire(MSG_ROLE.CORPS_BUILD);  //发送通知
    }
    //推送公户升级消息
    [NetHandler(MODULE_CORPS.PUSH_CORPS_LEVEL_UP)]
    public void PushCorpsLevelUpHdl(PushCorpsLevelUpRes resData)
    {
        UIMessage.ShowFlowTip("corps_level_up", resData.level);
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.corpsInfo.props.growValue = resData.constru;
        part.corpsInfo.props.level = resData.level;
    }
    //推送新的建设记录
    [NetHandler(MODULE_CORPS.PUSH_NEW_BUILD_LOG)]
    public void PushNewBuildLogHdl(PushNewBuildLogRes resData)
    {
        CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
        part.corpsInfo.buildLogs.Insert(0, resData.newLog);
        UICorpsBuild ui = UIMgr.instance.Get<UICorpsBuild>();
        if (ui.IsOpen)
            ui.UpdateLog(part);
    }

    [NetHandler(MODULE_CORPS.CMD_REQ_OTHER_CORPS)]
    public void OnReqOtherCorps(OtherCorpsRes resData)
    {
        if (resData == null)
            return;
        UIMgr.instance.Open<UICorpsInfo>(resData);
    }
}
