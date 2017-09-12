using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MODULE_CORPS
{
    public const int CMD_CREATE_CORPS = 1;     //创建公会
    public const int CMD_REQ_CORPS = 2;   //请求公会信息
    public const int CMD_MODIFYDECLARE = 3;  //修改公会宣言
    public const int CMD_APPLY_JOIN = 4;  //申请加入公会
    public const int CMD_HANDLE_MEMBER = 5;  //处理会员操作
    public const int CMD_CORPS_SET = 6;          //公会设置
    public const int CMD_EXIT_CORPS= 7;           //退出公会
    public const int CMD_REQ_MEMBERS = 8;          //请求成员和申请信息
    public const int CMD_REQ_ALL_CORPS = 9;         //请求所有公户
    public const int CMD_REQ_IMPEACH_STATUS = 10;    //请求弹劾信息
    public const int CMD_INITIATE_IMPEACH = 11;     //发起弹劾
    public const int CMD_AGREE_IMPEACH = 12;    //同意弹劾
    public const int CMD_CORPS_BUILD_DATA = 13;  //公会建设数据
    public const int CMD_BUILD_CORPS = 14;   //建设公会
    public const int CMD_REQ_OTHER_CORPS = 15;    //请求别人的公会信息

    public const int PUSH_CORPS_POS_CHANGE = -1;   //自己职位的变动
    public const int PUSH_CORPS_MEMBER_REQ = -2;    //通知新成员信息和新申请信息
    public const int PUSH_NEW_LOG = -3;                  //通知新的内存
    public const int PUSH_IMPEACH_SUCCESS = -4;      //弹劾成功会长换人,推送信息
    public const int PUSH_CORPS_LEVEL_UP = -5;         //推送公会升级
    public const int PUSH_NEW_BUILD_LOG = -6;        //推送新的建设记录

}

//操作类型
public enum CorpsPosFunc
{
    Appoint  = 1,    //任命职位
    Modify  = 2,    //修改公告
    KickedOut  =  3,   //踢除会员
    HandleReq =   4,   //处理入会请求
    CorpsSet = 5,  //入会设置
    CorpsBossSet = 6,  //公会boss设置
    Quit = 7,  //退出公会
    Impeach = 8,  //弹劾
}
//公会日志类型
public enum CorpsLogType
{
    NewElder = 1,  //xx成为了长老
    JoinCorps = 2,  //xx加入了公会
    ExitCorps = 3,  //xx离开了公会
    KillBoss = 4,  //xx击杀了boss
    Impeach =5,  //xx发起了弹劾
    NewPresident = 6,  //xx成为了新会长
    CorpsLevelUp = 7,  //公会升级
    KickedOut = 8,  //xx被踢出了公会
    CorpsCreate = 9,  //xx创建了公会
}
//职位类型
public enum CorpsPosEnum
{
    Req = 0,   //申请中
    President = 1,    //会长
    Elder = 2,      //长老
    Common = 3    //普通会员
}
//1：同意入会 2：拒绝入会 3：踢出公会 4：任命职位
public enum HandlerMemberType
{
    agree,
    refuse,
    kickout,
    appoint
}
/******************************************************* 数据类 ***************************************************************/
/// <summary>
/// 公会所有数据
/// </summary>
public class CorpsInfo
{
    public CorpsProps props;  //公会基础属性
    public List<CorpsMember> members;  //成员
    public List<CorpsMember> reqs;   //入会申请
    public List<CorpsLogInfo> logs;    //日志
    public ImpeachInfo impeach;    //弹劾信息
    public List<BuildLogData> buildLogs;  //建设日志

    public CorpsInfo()
    {
        //初始化数据
        props = new CorpsProps();
        members = new List<CorpsMember>();
        reqs = new List<CorpsMember>();
        logs = new List<CorpsLogInfo>();
        impeach = new ImpeachInfo();
        buildLogs = new List<BuildLogData>();
    }
}

/// <summary>
/// 公会属性数据
/// </summary>
public class CorpsProps
{
    public int corpsId;
    public int serverId;
    public long createTime;
    public string name;
    public string president;
    public int level;
    public int growValue;  //建设值
    public string declare;  //宣言
    public int rank;
    public int joinSet;  //入会设置 0则为无限制，1需要申请
    public int joinSetLevel;   //joinSetLevel 入会等级设置
    public int memsNum;  //公会人数
    public int buildNum;  //今日建设人数   这个字段服务端没有，对应服务端的buildIds长度
}

//会员数据类型
public class CorpsMember
{
    public int heroId;
    public string name;
    public int level;
    public string roleId;
    public int powerTotal;
    public long lastLogout;
    public long upTime;
    public int contribution;
    public int pos;
}
//日志数据结构
public struct CorpsLogInfo
{
    public int id;  //日志id
    public string opt;   //附加参数：角色名字、等级
    public long time;  //写入日志时间
}
//包含时间的日志数据结构
public struct CorpsLogTimeInfo
{
    public CorpsLogTimeInfo(string time, List<CorpsLogInfo> list)
    {
        this.time = time;
        this.list = list;
    }
    public string time;
    public List<CorpsLogInfo> list;
}
//弹劾信息
public struct ImpeachInfo
{ 
    public int initiateId;   //发起弹劾者id
    public string initiateName;   //发起弹劾者名字
    public long time;   //发起弹劾时间
    public List<int> agree;   //赞成的id
}
//建设记录结构
public struct BuildLogData
{
    public BuildLogData(string name, int buildId)
    {
        this.name = name;
        this.buildId = buildId;
    }
    public string name;
    public int buildId;
}


/******************************************************* 请求消息类 ***************************************************************/
//请求创建公会
public class CreateCorpsReq
{
    public string name;
}
//请求公会信息
public class CorpsDataReq
{
    public int corpsId; //公会id
    public bool isFirst;   //是否初次请求公会数据，第一次才返回日志等数据，否则只返回公会基础数据
}
//修改公会宣言
public class ModifyCorpsDeclareReq
{
    public int corpsId;
    public string declare;
    public int heroId; 
}
//加入公会申请
public class ApplyJoinCorpsReq
{
    public int corpsId;
    public int heroId;
}
//处理会员操作
//1：同意入会 2：拒绝入会 3：踢出公会 4：任命职位
public class HandleMemberReq
{
    public int corpsId;   //公会id
    public int handler;  //处理者id
    public int beHandler;  //被处理者id
    public string beHandlerName;  //被处理者名字
    public int type;  //处理方式
    public int option;  //附加参数 如果type=4则option代表职位
}
//公会设置
public class CorpsSetReq
{
    public int corpsId;
    public int handler;   //处理者heroId
    public int type;  //设置类型 1：入会设置
    public int opt1;
    public int opt2;
}

//退出公会/会长解散公会
public class ExitCorpsReq
{
    public int corpsId;
    public int heroId;
}
//请求会员数据和申请数据
public class CorpsMembersReq
{
    public bool isInit;
    public int corpsId;
}
//请求获取所有公会
public class GetAllCorpsReq
{
    public bool isFirst;   //第一次为true服务端会把请求过的id发过来
}
//请求弹劾情况
public class ImpeachStatusReq
{
    public int corpsId;
}
//发起弹劾
public class InitiateImpeachReq
{
    public int corpsId;
    public int heroId;
}
//赞成弹劾
public class AgreeImpeachReq
{
    public int corpsId;
    public int heroId;
}
//请求公会建设数据
public class CorpsBuildDataReq
{
    public int corpsId;
}
//建设公会请求
public class BuildCorpsReq
{
    public int corpsId;   //公会iid
    public int heroId;    //建设者id
    public int buildId;  //建设id
}
//请求他人的公会信息
public class OtherCorpsReq
{
    public int corpsId;
}


/******************************************************* 返回消息类 ***************************************************************/

//公会信息返回
public class CorpsDataRes
{
    public CorpsProps corpsProps;  //基础数据
    public List<CorpsLogInfo> logs;  //日志数据
    public bool isFirst;
}
//修改公会宣言返回
public class ModifyCorpsDeclareRes
{
    public string newDeclare;
}
//公会进入/被踢 通知
public class CorpsJoinExitRes
{
    public CorpsProps corpsProps;
    public int status;  //状态 0：申请成功 1：加入成功 2:被踢出公会 3:已在申请列表中
    public int option;
} 
//处理会员操作返回
public class HandleMemberRes
{
    public int type;  //操作类型  1：同意入会 2：拒绝入会 3：踢出公会  4:任命职位
    public int beHandelId;
    public string beHandelName;
    public int opt;  //附加参数 如果type=4则option代表新职位 type=1则option=-1代表该玩家已加入其他公会，通知客户端刷新界面
}
//公会设置返回
public class CorpsSetRes
{
    public int type;
    public int opt1;
    public int opt2;
}
//公会职位变动通知
public class CorpsPosChangeRes
{
    public int pos;
}
//退出公会/解散公会返回
public class ExitCorpsRes
{
    public int status;
}
//请求会员数据和申请数据返回 只返回有更新的数据
public class CorpsMembersRes
{
    public List<CorpsMember> members;
    public List<CorpsMember> reqs;
    public bool isInit;
}
//获取所有公会返回
public class GetAllCorpsRes
{
    public List<CorpsProps> corpsList;    //所有公户列表
    public List<int> hasReqs;     //曾申请过的公会id  只有第一次请求才会下发，否则返回空数组
}

//推送新的会员信息和新的申请人信息
public class PushNewMemsReqDataRes
{
    public CorpsMember newMems;    //更新的会员信息或申请信息  ——pos为0代表是新的申请信息
}
//推送新的日志
public class PushNewLogRes
{
    public CorpsLogInfo newLog;
}
//发起弹劾返回
public class InitiateImpeachRes
{
}
//赞成弹劾返回
public class AgreeImpeachRes
{
    public int result;
}
//请求弹劾信息返回
public class ImpeachStatusRes
{
    public ImpeachInfo impeach;
}
//弹劾成功，推送消息
public class PushImpeachSuccessRes
{
   public int oriPreId;  //原会长id
    public int newPreId;  //新会长id
    public string newPreName;  //新会长名字
}
//请求公会建设数据返回
public class CorpsBuildDataRes
{
    public int buildNum;   //公会今日建设人数
    public List<BuildLogData> buildLog;   //建设记录
    public List<int> personalState;  //自己的建设状态
}
//建设公会返回
public class BuildCorpsRes
{
    public int buildId;   //建设id
    public int contri;   //更新自己的贡献值
    public int constru;  //更新公会的建设值
    public int level;   //公会等级
    public int buildNum;   //更新今日建设人数
}
//推送公会升级
public class PushCorpsLevelUpRes
{
    public int constru;   //更新公会的建设值
    public int level;   //公会等级
}
//推送新的建设记录
public class PushNewBuildLogRes
{
    public BuildLogData newLog;
}
//请求查看其他公会信息返回
public class OtherCorpsRes
{
    public CorpsProps props;
    public List<CorpsMember> mems;
    public int corpsPower;
}