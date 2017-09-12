"use strict";

const CmdIdsRank = {
    CMD_REQUEST_RANK : 1,
    CMD_REQ_MY_RANK_VAL : 2,
};

const ResultCodeRank = {
    RANK_TYPE_WRONG : 1,    //排行类型不对
};

//////////////////////////////
class RequestRankDataVo
{
    constructor() {
        this.type = "";     //排行类型
        this.time = 0;      //客户端数据时间
        this.start= 0;      //从第几行开始（基于0）
        this.len  = 0;      //取数据行数
        this.myRank = true;  //获取我的排行信息（取首页时有效）
    }

    static fieldsDesc() {
        return {
            type: {type: String, notNull: true},
            time: {type: Number, notNull: true},
            start: {type: Number, notNull: true},
            len: {type: Number, notNull: true},
            myRank: {type: Number, notNull: true}
        };
    }
}

class ReqMyRankValueVo
{
    constructor() {
        this.type = "";     //排行类型
    }

    static fieldsDesc() {
        return {
            type: {type: String, notNull: true}
        };
    }
}

//////////////////////////////
class RankDataVo
{
    constructor() {
        this.type = "";     //排行类型
        this.clientNew = false;//客户端数据是否本来就是最新的
        this.upTime = 0;    //数据更新时间
        this.data = "";     //排行数据的json化字符串
        this.myRank = -1;    //我的排名，如果不在排名内，则是-1
        this.myData = "";   //我在排行里的数据
        this.extra = "";    //额外数据的的json化字符串
        this.start = 0;     //数据从第几行开始
        this.reqLen = 0;    //请求数据数
        this.total = 0;     //总数据行数
    }
}

class MyRankValueVo
{
    constructor() {
        this.type = "";     //排行类型
        this.rankVal = -1;   //排名
    }
}


//////////////////////////////
exports.CmdIdsRank = CmdIdsRank;
exports.ResultCodeRank = ResultCodeRank;

exports.RequestRankDataVo = RequestRankDataVo;
exports.ReqMyRankValueVo = ReqMyRankValueVo;

exports.RankDataVo = RankDataVo;
exports.MyRankValueVo = MyRankValueVo;