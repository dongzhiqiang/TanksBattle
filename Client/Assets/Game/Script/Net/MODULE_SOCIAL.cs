using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum HandleFriendType
{
    Agree,   //同意
    Refuse,   //拒绝
    ResuseAll,  //全部拒绝
    BeAgreed,  //被同意
    BeRefused  //被拒绝
}

public struct UnCollStamina
{
    public UnCollStamina(int heroId, long timeStamp)
    {
        this.heroId = heroId;
        this.timeStamp = timeStamp;
    }
    public int heroId;  //赠送人id
    public long timeStamp;  //赠送时间
}
public class MODULE_SOCIAL
{
    public const int CMD_REQ_FRIENDS = 1;   //请求好友列表
    public const int CMD_ADD_FRIEND = 2;  //添加好友
    public const int CMD_HANDLE_FRIEND = 3;  //处理好友请求
    public const int CMD_SEND_STAMINA = 4;  //赠送体力
    public const int CMD_GET_STAMINA = 5;  //领取体力
    public const int CMD_ONEKEY_STAMINA = 6;  //一键领取体力
    public const int CMD_DELETE_FRIEND = 7;  //删除好友
    public const int CMD_FRIEND_RECOMMEND = 8;  //好友推荐
    public const int CMD_REFRESH_RECOMMEND = 9;  //刷新推荐好友
    public const int CMD_ONEKEY_ADD_FRIEND = 10;  //一键添加好友

    public const int PUSH_ADD_FRIEND = -1;  //他人的好友请求
    public const int PUSH_NEW_STAMINA = -2;   //新的体力赠送

}

public class Friend
{
    public string name;  //名字
    public int heroId;  //id
    public int level;  //等级
    public int powerTotal; //战斗力
    public int vipLv;  //vip 
    public string roleId;
    public long lastLogout;   //登出时间
    public long upTime;   //记录更新时间

}

public class FriendInfoVo
{
    public List<Friend> friends;  //好友
    public List<Friend> addReqs;  //申请列表
    public List<int> sendStam;   //当天赠送过体力的所有heroId
    public List<int> collStam;    //当天领取过体力的所有heroId
    public List<UnCollStamina> unCollStam;  //当天未领取体力的所有heroId
}

//请求好友数据
public class ReqFriendsReq
{
}
//请求好友数据返回
public class ReqFriendsRes
{
    public List<Friend> friends;
    public List<int> collStam;
    public List<UnCollStamina> unCollStam;
    public List<int> sendStam;
    public bool reset;
}

//添加好友请求
public class AddFriendReq
{
    public string addName;   //对方名字
    public Friend adder;   //添加的人的信息
}
//添加好友返回
public class AddFriendRes
{
    public string friName;
}
//推送新的好友请求
public class PushAddFriend
{
    public List<Friend> adders;
}
//处理好友请求
public class HandleFriendReq
{
    public string name;
    public int heroId;
    public int type;
}
//处理好友请求返回
public class HandleFriendRes
{
    public int type;
    public Friend friData;
}
//赠送体力请求
public class SendStaminaReq
{
    public int heroId;
}
//赠送体力返回
public class SendStaminaRes
{
    public int heroId;
}
//领取体力请求
public class GetStaminaReq
{
    public int heroId;
}
//领取体力返回
public class GetStaminaRes
{
    public int heroId;
}
//推送别人新的赠送体力
public class PushNewStamina
{
    public string roleName; //赠送者名字
    public int heroId; //接受者id
    public long timeStamp;  //赠送时间
}
//请求一键领取体力
public class OnekeyGetStaminaReq
{
    public List<int> heroIds;
}
//请求一键领取体力返回
public class OnekeyGetStaminaRes
{
    public List<int> heroIds;
}
//请求删除好友
public class DeleteFriendReq
{
    public int heroId;
}
//请求删除好友返回
public class DeleteFriendRes
{
    public int heroId;
}
//好友推荐请求
public class FriendRecommendReq
{
    public bool isFirst;
}
//好友推荐返回
public class FriendRecommendRes
{
    public List<Friend> recFriends;
    public bool isFirst;
    public long upTime;
}
//刷新推荐好友
public class RefreshRecommendReq
{
}
//刷新推荐好友返回
public class RefreshRecommendRes
{
    public List<Friend> recFriends;
    public long upTime;
}
//一键添加好友
public class OneKeyAddFriendReq
{
    public List<string> addNames;   //添加的名字们
    public Friend adder;   //发起添加的信息
}
//一键添加好友返回
public class OneKeyAddFriendRes
{
    public List<string> addNames;   //添加的名字们
}