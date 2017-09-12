#region Header
/**
 * 名称：AccountInfo
 * 作者：XiaoLizhi
 * 日期：2015.12.23
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;

public class AccountInfo
{
    public string channelId;
    public string userId;
    public string token;
}

public class RoleInfo : IComparable<RoleInfo>
{
    public string name;
    public int level;
    public string roleId;
    public int serverId;
    public int heroId;
    public long lastLogin;

    public ServerInfo serverInfo;

    public int CompareTo(RoleInfo other)
    {
        //取反，为了从大到小排序
        int ret = -this.lastLogin.CompareTo(other.lastLogin);
        return ret;
    }
}

public class ServerInfo : IComparable<ServerInfo>
{
    public enum enShowState
    {
        normal,     //普通
        newSvr,     //新服
        recommend,  //推荐
        hotSvr,     //热服
    }

    public enum enLoadState
    {
        down,   //维护中
        idle,   //流畅
        busy,   //繁忙
        crowd,  //拥挤       
    }

    public string area;

    public string name;

    public int index;

    public string host;

    public int port;

    public int serverId;

    public string loadState;

    public string showState;

    public int CompareTo(ServerInfo other)
    {
        int ret = this.index.CompareTo(other.index);
        if (ret == 0)
            ret = this.serverId.CompareTo(other.serverId);
        return ret;
    }

    public enShowState ShowState
    {
        get
        {
            if (showState == "newSvr")
                return enShowState.newSvr;
            else if (showState == "recommend")
                return enShowState.recommend;
            else if (showState == "hotSvr")
                return enShowState.hotSvr;
            else
                return enShowState.normal;
        }
    }

    public enLoadState LoadState
    {
        get
        {
            if (loadState == "down")
                return enLoadState.down;
            else if (loadState == "busy")
                return enLoadState.busy;
            else if (loadState == "crowd")
                return enLoadState.crowd;
            else
                return enLoadState.idle;
        }
    }
}

public class LoginInfo
{
    public AccountInfo accountInfo = new AccountInfo();

    public List<ServerInfo> serverList = new List<ServerInfo>();

    public List<RoleInfo>   roleList = new List<RoleInfo>();

    //服务器ID和服务器信息的映射
    public Dictionary<int, ServerInfo> serversById = new Dictionary<int, ServerInfo>();

    //区名和服务器列表映射
    public SortedDictionary<string, List<ServerInfo>> serversByArea = new SortedDictionary<string, List<ServerInfo>>();

    //推荐服列表
    public List<ServerInfo> serversByRecommend = new List<ServerInfo>();

    public void Init(JsonData data)
    {
        LoginInfo loginInfo = LitJson.JsonMapper.ToObject<LoginInfo>(data.ToJson());
        serverList = loginInfo.serverList;
        roleList = loginInfo.roleList;

        //给服务器列表建立索引
        serversById.Clear();
        serversByArea.Clear();
        serversByRecommend.Clear();
        foreach (ServerInfo s in serverList)
        {
            serversById[s.serverId] = s;
            serversByArea.GetNewIfNo(s.area).Add(s);
            if (s.ShowState == ServerInfo.enShowState.recommend)
                serversByRecommend.Add(s);
        }

        //给区名对应的服务器列表、推荐服安索引属性排序
        serversByRecommend.Sort();
        foreach (var svrList in serversByArea)
        {
            svrList.Value.Sort();
        }

        //按登录时间排序，第一个就是最后登录的角色
        roleList.Sort();

        //这里可能会删除元素，所以从后往前遍历
        for (int i = roleList.Count - 1; i >= 0; --i)
        {
            RoleInfo info = roleList[i];
            ServerInfo svrInfo = serversById.Get(info.serverId);
            if (svrInfo == null)
                roleList.RemoveAt(i);
            else
                info.serverInfo = svrInfo;
        }
    }
}