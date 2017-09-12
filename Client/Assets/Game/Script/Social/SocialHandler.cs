using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[NetModule(MODULE.MODULE_SOCIAL)]
public class SocialHandler
{
    //添加好友
    public void AddFriend(string name)
    {
        SocialPart socialPart = RoleMgr.instance.Hero.SocialPart;
        if (socialPart.IsFriendByName(name))  //已经是好友
            UIMessage.ShowFlowTip("friend_already");
        else if (socialPart.friends.Count >= FriendMaxCfg.Get(RoleMgr.instance.Hero.GetInt(enProp.level)).maxFriend)  //好友已满
            UIMessage.ShowFlowTip("friend_list_full");
        else//向服务端请求添加好友
        {
            Role role = RoleMgr.instance.Hero;
            Friend ownInfo = socialPart.MakeFriendDataByRole(role);
            SendAddFriendReq(name, ownInfo);
        }
    }

    /******************************************************* 请求消息 ***************************************************************/
    //请求有变动的好友数据
    public void SendReqFriendData()
    {
        ReqFriendsReq request = new ReqFriendsReq();
        NetMgr.instance.Send(MODULE.MODULE_SOCIAL, MODULE_SOCIAL.CMD_REQ_FRIENDS, request);
    }
    //请求添加好友
    public void SendAddFriendReq(string name, Friend adder)
    {
        AddFriendReq request = new AddFriendReq();
        request.addName = name;
        request.adder = adder;
        NetMgr.instance.Send(MODULE.MODULE_SOCIAL, MODULE_SOCIAL.CMD_ADD_FRIEND, request);
    }

    //处理好友
    public void SendHandleFriend(string name, int heroId, int type)
    {
        HandleFriendReq req = new HandleFriendReq();
        req.name = name;
        req.heroId = heroId;
        req.type = type;
        NetMgr.instance.Send(MODULE.MODULE_SOCIAL, MODULE_SOCIAL.CMD_HANDLE_FRIEND, req);
    }
    //赠送好友体力
    public void SendFriendStamina(int heroId)
    {
        SendStaminaReq req = new SendStaminaReq();
        req.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_SOCIAL, MODULE_SOCIAL.CMD_SEND_STAMINA, req);
    }
    //领取好友送的体力
    public void GetFriendStamina(int heroId)
    {
        GetStaminaReq req = new GetStaminaReq();
        req.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_SOCIAL, MODULE_SOCIAL.CMD_GET_STAMINA, req);
    }
    //一键领取体力
    public void OnekeyStamina(List<int> heroIds)
    {
        OnekeyGetStaminaReq req = new OnekeyGetStaminaReq();
        req.heroIds = heroIds;
        NetMgr.instance.Send(MODULE.MODULE_SOCIAL, MODULE_SOCIAL.CMD_ONEKEY_STAMINA, req);
    }
    //删除好友
    public void DeleteFriend(int heroId)
    {
        DeleteFriendReq req = new DeleteFriendReq();
        req.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_SOCIAL, MODULE_SOCIAL.CMD_DELETE_FRIEND, req);
    }
    //请求推荐好友
    public void ReqRecommendFriend(bool isFirst)
    {
        FriendRecommendReq req = new FriendRecommendReq();
        req.isFirst = isFirst;
        NetMgr.instance.Send(MODULE.MODULE_SOCIAL, MODULE_SOCIAL.CMD_FRIEND_RECOMMEND, req);
    }
    //请求刷新推荐好友
    public void ReqRefreshRecommend()
    {
        RefreshRecommendReq req = new RefreshRecommendReq();
        NetMgr.instance.Send(MODULE.MODULE_SOCIAL, MODULE_SOCIAL.CMD_REFRESH_RECOMMEND, req);
    }
    //请求一键添加好友
    public void OneKeyAddFriend(List<string> addNames, Friend adder)
    {
        OneKeyAddFriendReq req = new OneKeyAddFriendReq();
        req.adder = adder;
        req.addNames = addNames;
        NetMgr.instance.Send(MODULE.MODULE_SOCIAL, MODULE_SOCIAL.CMD_ONEKEY_ADD_FRIEND, req);
    } 

    
    /******************************************************* 数据返回 ***************************************************************/
    //请求有变动的好友数据返回
    [NetHandler(MODULE_SOCIAL.CMD_REQ_FRIENDS)]
    public void UpdateFriends(ReqFriendsRes info)
    {
        if (info == null)
            return;
        Role role = RoleMgr.instance.Hero;
        SocialPart part = role.SocialPart;
        if (info.friends.Count > 0)
            part.UpdateFriends(info.friends);
        //如果有重置的或者第一次获取数据时才会更新这三个列表
        if (info.reset || info.collStam.Count > 0)
            part.colStms = info.collStam;
        if (info.reset || info.unCollStam.Count > 0)
            part.unColStms = info.unCollStam;
        if (info.reset || info.sendStam.Count > 0)
            part.sendStms = info.sendStam;

        part.CheckTip();   //检测一下叹号提示
        //刷新界面
        UIFriend ui = UIMgr.instance.Get<UIFriend>();
        if (ui.IsOpen)
            ui.UpdateFriendPage();

    }

    //请求添加好友返回
    [NetHandler(MODULE_SOCIAL.CMD_ADD_FRIEND)]
    public void SendAddFriendRes(AddFriendRes info)
    {
        if (info == null)
            return;
        UIMessage.ShowFlowTip("friend_add_success");
        RoleMgr.instance.Hero.SocialPart.addReqs.Add(info.friName);   //记录
        UIInputBox inputBox = UIMgr.instance.Get<UIInputBox>();
        if (inputBox.IsOpen)
            inputBox.Close();
    }

    //推送别人的好友请求
    [NetHandler(MODULE_SOCIAL.PUSH_ADD_FRIEND)]
    public void PushAddFriendHdl(PushAddFriend info)
    {
        Role role = RoleMgr.instance.Hero;
        SocialPart part = role.SocialPart;
        for(int i=0;i<info.adders.Count;i++)
            part.AddReqList(info.adders[i]);
        //刷新界面
        UIFriend ui = UIMgr.instance.Get<UIFriend>();
        if (ui.IsOpen)
        {
            ui.UpdateReqPage();
            ui.SetReqTip(true);
        }
        
    }

    //处理好友返回
    [NetHandler(MODULE_SOCIAL.CMD_HANDLE_FRIEND)]
    public void HandleFriendResHdl(HandleFriendRes info)
    {
        if (info == null)
            return;

        Role role = RoleMgr.instance.Hero;
        SocialPart part = role.SocialPart;
        UIFriend ui = UIMgr.instance.Get<UIFriend>();
        switch (info.type)
        {
            case (int)HandleFriendType.Agree:    //自己同意
                part.AddFriend(info.friData);
                part.RemoveReq(info.friData);
                //刷新界面
                if (ui.IsOpen)
                {
                    ui.UpdateReqPage();
                    ui.UpdateFriendPage();
                }
                UIMessage.Show("同意添加对方为好友");
                role.Fire(MSG_ROLE.ADD_FRIEND);  //发送通知
                break;
            case (int)HandleFriendType.Refuse:    //自己拒绝
                part.RemoveReq(info.friData);
                //刷新界面
                if (ui.IsOpen)
                    ui.UpdateReqPage();
                UIMessage.Show("拒绝了对方的好友请求");
                break;

            case (int)HandleFriendType.BeAgreed:     //别人同意
                if(LevelMgr.instance.IsMainCity())
                    UIMessage.ShowFlowTip("friend_agreed", info.friData.name);

                part.AddFriend(info.friData);
                part.RemoveReq(info.friData);   //如果刚好申请表里有对方，把对方去掉
                //刷新界面
                if (ui.IsOpen)
                {
                    ui.UpdateReqPage();
                    ui.UpdateFriendPage();
                }
                role.Fire(MSG_ROLE.ADD_FRIEND);  //发送通知
                break;

            case (int)HandleFriendType.BeRefused:   //别人拒绝
                if (LevelMgr.instance.IsMainCity())
                    UIMessage.ShowFlowTip("friend_refused", info.friData.name);
                break;

            case (int)HandleFriendType.ResuseAll:    //全部拒绝
                part.RemoveAllReq();
                if (ui.IsOpen)
                    ui.UpdateReqPage();
                UIMessage.Show("一键拒绝了所有好友请求");
                break;
        }
    }
    //赠送体力返回
    [NetHandler(MODULE_SOCIAL.CMD_SEND_STAMINA)]
    public void SendStaminaResHdl(SendStaminaRes info)
    {
        UIMessage.ShowFlowTip("send_friend_stam_success");
        //更新客户端
        Role role = RoleMgr.instance.Hero;
        SocialPart part = role.SocialPart;
        part.AddSendStm(info.heroId);

        //刷新界面
        UIFriend ui = UIMgr.instance.Get<UIFriend>();
        if (ui.IsOpen)
        {
            ui.UpdateFriendPage();
        }
    }

    //领取体力返回
    [NetHandler(MODULE_SOCIAL.CMD_GET_STAMINA)]
    public void GetStaminaResHdl(GetStaminaRes info)
    {
        UIMessage.ShowFlowTip("get_friend_stam_success");
        //更新客户端
        Role role = RoleMgr.instance.Hero;
        SocialPart part = role.SocialPart;
        part.AddColStm(info.heroId);
        //刷新界面
        UIFriend ui = UIMgr.instance.Get<UIFriend>();
        if (ui.IsOpen)
            ui.UpdateFriendPage();

    }
    //新的体力赠送推送消息
    [NetHandler(MODULE_SOCIAL.PUSH_NEW_STAMINA)]
    public void PushNewStaminaHdl(PushNewStamina info)
    {
        if (Room.instance && LevelMgr.instance.IsMainCity())  //在主城直接弹框  Room.instance为空可能是转场景中，所以为空就直接回主城再打开
        {
            //UIMessageBox.Open(string.Format(LanguageCfg.Get("some_send_stamina"), info.roleName), () =>
            //{
            //    UIFriend u = UIMgr.instance.Get<UIFriend>();
            //    if (!u.IsOpen)
            //        UIMgr.instance.Open<UIFriend>();
            //}, null, "前往");
            UIMessage.ShowFlowTip("some_send_stamina", info.roleName);
        }
        else  //不在主城 先记着，等回到主城再打开
            RoleMgr.instance.Hero.SocialPart.sendStamName = info.roleName;

        //更新客户端
        Role role = RoleMgr.instance.Hero;
        SocialPart part = role.SocialPart;
        part.AddUnColStm(info.heroId, info.timeStamp);

        //刷新界面
        UIFriend ui = UIMgr.instance.Get<UIFriend>();
        if (ui.IsOpen)
            ui.UpdateFriendPage();
        
    }
    //一键领取体力返回
    [NetHandler(MODULE_SOCIAL.CMD_ONEKEY_STAMINA)]
    public void OnekeyStaminaHdl(OnekeyGetStaminaRes info)
    {
        if (info.heroIds.Count > 0)
        {
            UIMessage.ShowFlowTip("onekey_get_stam_success");

            Role role = RoleMgr.instance.Hero;
            SocialPart part = role.SocialPart;
            for(int i=0; i<info.heroIds.Count; ++i)
                part.AddColStm(info.heroIds[i]);
            //刷新界面
            UIFriend ui = UIMgr.instance.Get<UIFriend>();
            if (ui.IsOpen)
                ui.UpdateFriendPage();
        }
    }
    //删除好友返回
    [NetHandler(MODULE_SOCIAL.CMD_DELETE_FRIEND)]
    public void DeleteFriendHdl(DeleteFriendRes info)
    {
        SocialPart part = RoleMgr.instance.Hero.SocialPart;
        UIMessage.Show("删除成功");

        part.RemoveFriend(info.heroId);
        part.RemoveUnColStm(info.heroId);
        //刷新界面
        UIFriend ui = UIMgr.instance.Get<UIFriend>();
        if (ui.IsOpen)
            ui.UpdateFriendPage();
    }
    //推荐好友返回
    [NetHandler(MODULE_SOCIAL.CMD_FRIEND_RECOMMEND)]
    public void FriendRecommendResHdl(FriendRecommendRes resData)
    {
        if (resData == null)
            return;
        SocialPart part = RoleMgr.instance.Hero.SocialPart;
        part.recommendUptime = resData.upTime;  
        if (resData.isFirst || resData.recFriends.Count > 0)
        {
            part.isRecFirst = false;
            part.SetRecommends(resData.recFriends);
        }
        //更新UI
        UIFriendRecommend ui = UIMgr.instance.Get<UIFriendRecommend>();   //更新UI
        if (ui.IsOpen)
            ui.UpdatePanel();
    }
    //刷新推荐好友返回
    [NetHandler(MODULE_SOCIAL.CMD_REFRESH_RECOMMEND)]
    public void RefreshRecommend(RefreshRecommendRes resData)
    {
        if (resData == null)
            return;
        SocialPart part = RoleMgr.instance.Hero.SocialPart;
        part.SetRecommends(resData.recFriends);
        part.recommendUptime = resData.upTime;
        //重置申请过的列表
        part.addReqs = new HashSet<string>();

        UIFriendRecommend ui = UIMgr.instance.Get<UIFriendRecommend>();
        if (ui.IsOpen)
            ui.UpdatePanel();
        UIMessage.Show("刷新成功");

    }
    //一键添加好友返回
    [NetHandler(MODULE_SOCIAL.CMD_ONEKEY_ADD_FRIEND)]
    public void OneKeyAddFriendResHdl(OneKeyAddFriendRes resData)
    {
        if (resData == null)
            return;
        UIMessage.Show("一键添加成功，请等待审核");

        SocialPart part = RoleMgr.instance.Hero.SocialPart;
        for (int i = 0, len = resData.addNames.Count; i < len; ++i)
            part.addReqs.Add(resData.addNames[i]);
        UIFriendRecommend ui = UIMgr.instance.Get<UIFriendRecommend>();
        if (ui.IsOpen)
            ui.UpdatePanel();
    }
}
