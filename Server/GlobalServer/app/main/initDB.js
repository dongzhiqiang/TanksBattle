"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appUtil = require("../libs/appUtil");
var dbUtil = require("../libs/dbUtil");
var logUtil = require("../libs/logUtil");

////////////模块内数据////////////
/**
 * 用于等待某一个模块初始化DB完成再继续执行
 * @type {boolean}
 */
var initDBOKFlag = false;

/**
 *
 * @type {{colName:string, keys:object, unique:boolean, indexName:string}[]}
 */
var indexesList = [
    {colName:"accountList", keys:{userId:1}, unique:true, indexName:"userId_1"},
    {colName:"accountList", keys:{username:1}, unique:true, indexName:"username_1"},
    {colName:"roleList", keys:{channelId:1, userId:1}, unique:false, indexName:"channelId_1_userId_1"},     //一个用户可以有多个角色，所以不唯一
    {colName:"roleList", keys:{name:1}, unique:true, indexName:"name_1"},
    {colName:"roleList", keys:{heroId:1}, unique:true, indexName:"heroId_1"},
    {colName:"serverList", keys:{serverId:1}, unique:true, indexName:"serverId_1"},
    {colName:"corpsList", keys:{corpsId:1}, unique:true, indexName:"corpsId_1"},  //公会
    {colName:"corpsList", keys:{name:1}, unique:true, indexName:"name_1"} //公会
];

////////////导出函数////////////
var doInitCoroutine = Promise.coroutine(function * (){
    logUtil.info("数据库模块开始初始化...");

    //尝试连接数据库，连接失败这里会做关闭连接处理，抛出异常，这里不用管，让上层处理
    yield dbUtil.initDB();

    //主进程或1号子进程做这个，避免其它进程重复执行
    if (appUtil.getWorkerID() <= 1) {
        //获取db对象
        var db = dbUtil.getDB();
        //初始化numberSeed
        logUtil.debug("[数据库][全局数据库][numberSeed]初始化");
        var col = db.collection("numberSeed");
        //这里用了$max，用于保证如果有现成的字段，就用现成的，还有upsert，用于能不存在数据行时插入数据行
        yield col.updateOne({_id: 0}, {$max: {userId: 0, corpsId: 10000, heroId: 0}}, {upsert: true});
        //初始化各索引
        for (var i = 0; i < indexesList.length; ++i) {
            var item = indexesList[i];
            var colName = item.colName;
            var keys = item.keys;
            var options = {background: true};
            if (item.indexName)
                options.name = item.indexName;
            if (item.unique)
                options.unique = true;
            logUtil.debug("[数据库][全局数据库][" + colName + "]初始化索引" + JSON.stringify(keys));
            col = db.collection(colName);
            yield col.createIndex(keys, options);
        }

        //通知其它子进程，数据库初始化完成了
        appUtil.fireProcessEvent("initDBOK");
    }
    else {
        //等某个模块初始化DB完成
        while (!initDBOKFlag)
            yield Promise.delay(1);
    }

    logUtil.info("数据库模块完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("数据库模块开始销毁...");
    yield dbUtil.closeDB();
    logUtil.info("数据库模块完成销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}

////////////全局执行////////////
appUtil.addProcessListener("initDBOK", function() {
    initDBOKFlag = true;
    logUtil.info("检测到DB初始化完成");
});

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;