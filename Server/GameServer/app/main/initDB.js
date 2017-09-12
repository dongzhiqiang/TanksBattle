"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var dbUtil = require("../libs/dbUtil");
var logUtil = require("../libs/logUtil");

////////////模块内数据////////////
/**
 *
 * @type {{colName:string, keys:object, unique:boolean, indexName:string}[]}
 */
var indexesList = [
    {colName:"role", keys:{"props.heroId":1}, unique:true, indexName:"heroId_1"},
    {colName:"role", keys:{"props.name":1}, unique:true, indexName:"name_1"},
    {colName:"role", keys:{"props.channelId":1, "props.userId":1}, unique:false, indexName:"channelId_1_userId_1"},
    {colName:"rank", keys:{type:1}, unique:true, indexName:"type_1"},
    {colName:"robot", keys:{"props.heroId":1}, unique:true, indexName:"heroId_1"},
    {colName:"robot", keys:{"props.name":1}, unique:true, indexName:"name_1"},
    {colName:"corps", keys:{"props.corpsId":1}, unique:true, indexName:"corpsId_1"},  //公会
    {colName:"corps", keys:{"props.name":1}, unique:true, indexName:"name_1"} //公会
];

////////////导出函数////////////
var doInitCoroutine = Promise.coroutine(function * (){
    logUtil.info("数据库模块开始初始化...");
    //尝试连接数据库，连接失败这里会做关闭连接处理，抛出异常，这里不用管，让上层处理
    yield dbUtil.initDB();
    //获取db对象
    var db = dbUtil.getDB();
    //初始化各索引
    for (var i = 0; i < indexesList.length; ++i)
    {
        var item = indexesList[i];
        var colName = item.colName;
        var keys = item.keys;
        var options = {background:true};
        if (item.indexName)
            options.name = item.indexName;
        if (item.unique)
            options.unique = true;
        logUtil.debug("[数据库][游戏数据库][" + colName + "]初始化索引" + JSON.stringify(keys));
        var col = db.collection(colName);
        yield col.createIndex(keys, options);
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

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;