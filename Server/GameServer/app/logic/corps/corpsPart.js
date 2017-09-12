"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var dateUtil = require("../../libs/dateUtil");
var enProp = require("../enumType/propDefine").enProp;
var corpsMgr = require("../corps/corpsMgr");


//这里只记录一些个人有关公会操作的数据
class CorpsPart
{
    /**
     * @param {Role} role
     * @param {object} data
     */
    constructor(role, data) {
        /**
         * @type {Number[]}
         * */
        this._reqCorps = [];   //申请过的公会id
        /**
         * @type {Number}
         * */
        this._quitCorpsTime = 0;  //退出公会时间
        /**
         * @type {Number}
         * */
        Object.defineProperty(this, "_memUptime", {enumerable: false, writable:true, value: 0});    //记录自己最近一次请求公会会员和申请人信息的时间，
                                                                                                    // 每次请求时都拿这个时间和CorpsMember的uptime作对比，不同的才下发数据
        /**
         * @type {Number[]}
         * */
        Object.defineProperty(this, "_buildState", {enumerable: false, writable:true, value: [0,0,0]}); //自己参与公会建设的状态
        /**
         * 定义role
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: role});

        //登录初始化数据
        try {
            var corps = data.corps || {};
            this._reqCorps = corps.reqCorps || [];
            this._quitCorpsTime = corps.quitCorpsTime || 0;

            var corpsId = this._role.getNumber(enProp.corpsId);
            //如果有公会，做一些数据处理
            if(corpsId > 0)
            {
                var corpsData = corpsMgr.getCorpsData(corpsId);
                if (!corpsData)
                {
                    this._role.setNumber(enProp.corpsId, 0);
                    this._role.setString(enProp.corpsName, "");
                }
                else
                {
                    var hasBuild = corpsData.hasBuild || [[],[],[]];
                    for(var i = 0; i < hasBuild.length; ++i)
                    {
                        var arr = hasBuild[i];
                        //遍历数组，查找是否有自己的id
                        var state = 0;
                        for(var j = 0;j < arr.length; ++j)
                        {
                            if(arr[j] == role.getHeroId())
                            {
                                state = 1;
                                break;
                            }
                        }
                        this._buildState[i] = state;
                    }
                    //防止公会改名了没更新
                    this._role.setString(enProp.corpsName, corpsData.props.name);
                }
            }
        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    release()
    {
        this._reqCorps = [];
    }
    /**
     * 存盘数据
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj) {
        rootObj.corps = {};
        rootObj.corps.reqCorps = this._reqCorps;
    }

    /**
     * 下发客户端的数据
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj) {
        //下发公会数据
        var corpsId = this._role.getNumber(enProp.corpsId);
        rootObj.corps = corpsMgr.getCorpsData(corpsId);
    }

    getPublicNetData(rootObj){
    }

    getProtectNetData(rootObj){
    }
    //设置退出公会的cd时间
    setQuitCorpsTime(time)
    {
        this._quitCorpsTime = time;
        CorpsPart.setQuitCorpsTimeDB(this._role.getUserId(), this._role.getHeroId(), time);
    }

    //获取上次退出公户的cd时间
    getQuitCorpsTime()
    {
        return this._quitCorpsTime;
    }
    //获取自己的公会建设状态
    getOwnBuildState()
    {
        return this._buildState;
    }

    //获取自己的公会建设状态并检查次日更新 true今天已建设过
    getOwnBuildStateAndCheckUpdate()
    {
        var corpsId = this._role.getNumber(enProp.corpsId);
        if(corpsId == 0)
            return false;
        //这里检测一下次日刷新
        var isReset = corpsMgr.checkCorpsBuildReset(corpsId);
        if(isReset)
            this.resetOwnBuildState();
        var pers = this.getOwnBuildState();
        for(var i = 0; i<3; ++i)
        {
            if(pers[i] == 1)
                return true;
        }
        return false;
    }
    //更新设置建设状态
    setOwnBuildState(id, state)
    {
        this._buildState[id-1] = state;
    }
    //重置建设装态
    resetOwnBuildState()
    {
        this._buildState = [0,0,0];
    }
    //获取公会操作时间
    getMemUptime()
    {
        return this._memUptime;
    }
    //设置公会操作时间
    setMemUptime(newTime)
    {
        this._memUptime = newTime;
    }
    //获取曾申请过的所有公会id
    getHasReqCorps()
    {
        return this._reqCorps;
    }

    //静态方法 存盘操作
    static setQuitCorpsTimeDB(userId, heroId, time)
    {
        //存盘操作
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":heroId}, {"$set":{"corps.quitCorpsTime":time}});  //记录退出公会时间
    }

}

exports.CorpsPart = CorpsPart;