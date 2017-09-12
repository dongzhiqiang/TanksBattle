"use strict";

////////////一般要修改的配置////////////
var debug               = true; //正式运营这个值为false
var consoleLog          = true; //正式运营这个值为false
var dbUrl               = "mongodb://127.0.0.1:27017/globaldb?maxPoolSize=1&autoReconnect=true&w=1&wtimeoutMS=5000"; //这里的maxPoolSize=1很重要，不然同一个用户的数据写到不同的连接里，导致数据乱序

////////////一般不修改的配置////////////
/////首要/////
var appName             = "全局服务器";             //本应用名字
var maxWorkerCnt        = 4;                        //最大允许的工作进程数，<=0就用核心数
/////端口/////
var bindIP              = "";                       //一般留空
var port                = 80;                       //HTTP的监听端口
var port4Https          = 443;                      //HTTPS的监听端口
/////日志/////
var logDir              = "logs";                   //必须用"/"作分隔符，不要用"\"
var logFileInv          = 60 * 60 * 24;             //单位秒，每多长时间重新创建日志文件
var logFileTime         = 60 * 60 * 24 * 7;         //单位秒，每个日志文件多长时间清理
/////文件/////
var resDirName          = "public";                 //资源文件目录
var fileCacheTime       = 30;                       //单位秒，文件缓存时间
/////数据库/////
var dbPoolSize          = 5;                        //数据库连接池，要保证同一个用户的数据在同一个连接里操作
var minReconnInv        = 2000;                     //单位毫秒，数据库重连最小等待时间
var maxReconnInv        = 10000;                    //单位毫秒，数据库重连最大等待时间
var incReconnInv        = 500;                      //单位毫秒，数据库重连等待时间的增量，也就是等待时间会慢慢增大
var maxRetryCntRunning  = 50;                       //程序运行中时，连接最大重试次数
var maxRetryCntExiting  = 5;                        //程序退出中时，连接最大重试次数
/////安全/////
var maxSubmitBodyLen    = 1024 * 1024;              //单位字符数或字节，对于String是字符数，对于Buffer是字节
var adminKey            = "gow_admin";              //想对这个服务器执行管理操作要用的Key
/////逻辑/////
var svrListCacheTime    = 100;                      //单位秒，服务器列表缓存时间

////////////导出元素////////////
exports.debug               = debug;
exports.consoleLog          = consoleLog;
exports.dbUrl               = dbUrl;
exports.appName             = appName;
exports.maxWorkerCnt        = maxWorkerCnt;
exports.bindIP              = bindIP;
exports.port                = port;
exports.port4Https          = port4Https;
exports.logDir              = logDir;
exports.logFileInv          = logFileInv;
exports.logFileTime         = logFileTime;
exports.resDirName          = resDirName;
exports.fileCacheTime       = fileCacheTime;
exports.dbPoolSize          = dbPoolSize;
exports.minReconnInv        = minReconnInv;
exports.maxReconnInv        = maxReconnInv;
exports.incReconnInv        = incReconnInv;
exports.maxRetryCntRunning  = maxRetryCntRunning;
exports.maxRetryCntExiting  = maxRetryCntExiting;
exports.maxSubmitBodyLen    = maxSubmitBodyLen;
exports.adminKey            = adminKey;
exports.svrListCacheTime    = svrListCacheTime;

////////////说明////////////
/*
 数据库连接字符串示例
 如果有副本集：
  mongodb://账号:密码@主机1:端口,主机2:端口,主机3:端口/数据库名?replicaSet=副本集名&maxPoolSize=1&autoReconnect=true&w=1&wtimeoutMS=5000&readPreference=nearest
 如果是单机或分片集群：
 mongodb://账号:密码@主机:端口/数据库名?maxPoolSize=1&autoReconnect=true&w=1&wtimeoutMS=5000
 这里的maxPoolSize=1很重要，不然同一个用户的数据写到不同的连接里，导致数据乱序
 */