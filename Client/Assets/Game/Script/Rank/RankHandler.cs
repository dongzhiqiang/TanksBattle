using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[NetModule(MODULE.MODULE_RANK)]
public class RankHandler
{
    public void SendRequestRankData(string type, int start, int len, bool fetchMyRankInfo = false)
    {
        RequestRankDataVo req = new RequestRankDataVo();
        req.type = type;
        //这里只判断start对应的数据在客户端存在就行了，因为如果客户端的这页不满页（其实就是末页），服务端如果数量变化了（尾部增加了），这个upTime也会变化，也就会下发
        req.time = RankMgr.instance.GetRankDataItemCount(type) > start ? RankMgr.instance.GetRankDataUpTime(type) : 0;
        req.start = start;
        req.len = len;
        req.myRank = fetchMyRankInfo ? 1 : 0;

        NetMgr.instance.Send(MODULE.MODULE_RANK, MODULE_RANK.CMD_REQUEST_RANK, req);
    }

    public void SendReqRankValData(string type)
    {
        ReqMyRankValueVo req = new ReqMyRankValueVo();
        req.type = type;
        NetMgr.instance.Send(MODULE.MODULE_RANK, MODULE_RANK.CMD_REQ_MY_RANK_VAL, req);
    }

    public void SendReqDoLikeRankItem(string type, string key)
    {
        var req = new DoLikeRankItemReq();
        req.type = type;
        req.key = key;
        NetMgr.instance.Send(MODULE.MODULE_RANK, MODULE_RANK.CMD_DO_LIKE, req);
    }

    [NetHandler(MODULE_RANK.CMD_REQUEST_RANK)]
    public void OnRetRankData(RankDataVo data)
    {
        RankMgr.instance.OnRetRankData(data);
    }

    [NetHandler(MODULE_RANK.CMD_REQ_MY_RANK_VAL)]
    public void OnRetRankValue(MyRankValueVo data)
    {
        RankMgr.instance.OnRetRankValue(data);
    }

    [NetHandler(MODULE_RANK.CMD_DO_LIKE)]
    public void OnRetDoLikeRankItem(DoLikeRankItemRes data)
    {
        RankMgr.instance.OnRetDoLikeRankItem(data);
    }
}