"use strict";

////////////一般要修改的配置////////////
//对外
var areaName            = "开发区";       //服务器区名
var serverName          = "TestServer";      //服务器名
var serverId            = 1;              //服务器ID
var publicHostName      = "127.0.0.1";      //客户端连接用的主机名（不一定是IP，可以是域名，对外运营一般是公网IP）
//对内
var debug               = true;             //正式运营这个值为false
var consoleLog          = true;             //正式运营这个值为false
var dbUrl               = "mongodb://192.168.1.106:27017/gamedb?maxPoolSize=1&autoReconnect=true&w=1&wtimeoutMS=5000"; //这里的maxPoolSize=1很重要，不然同一个用户的数据写到不同的连接里，导致数据乱序
var globalServerUrl     = "http://192.168.1.106/";      //全局服地址
var adminServerUrl      = "";  //管理后台地址
var lanHostName         = "127.0.0.1";          //其它服连接用的主机名（一般是局域网IP）

////////////一般不修改的配置////////////
/////首要/////
var appName             = "游戏服务器";             //本应用名字
var showIndex           = 0;                        //显示靠前优先级，这个值相同就使用serverId来排序
var showState           = "normal";                 //展示状态，值分别有：normal（默认）、newSvr（新服）、recommend（推荐）、hotSvr（热服）
/////端口/////
var bindIP              = "";                       //游戏服务绑定的IP，一般留空
var httpBindIP          = "";                       //HTTP服务绑定的IP，一般留空，这个HTTP服务主要用于管理
var port                = 20168;                    //游戏服务绑定的端口
var httpPort            = 8080;                     //HTTP服务绑定的端口，这个HTTP服务主要用于管理
/////日志/////
var logDir              = "logs";                   //必须用"/"作分隔符，不要用"\"
var logFileInv          = 60 * 60 * 24;             //单位秒，每多长时间重新创建日志文件
var logFileTime         = 60 * 60 * 24 * 7;         //单位秒，每个日志文件多长时间清理
/////数据库/////
var dbPoolSize          = 5;                        //数据库连接池，要保证同一个用户的数据在同一个连接里操作
var minReconnInv        = 2000;                     //单位毫秒，数据库重连最小等待时间
var maxReconnInv        = 10000;                    //单位毫秒，数据库重连最大等待时间
var incReconnInv        = 500;                      //单位毫秒，数据库重连等待时间的增量，也就是等待时间会慢慢增大
var maxRetryCntRunning  = 50;                       //程序运行中时，连接最大重试次数
var maxRetryCntExiting  = 5;                        //程序退出中时，连接最大重试次数
/////安全/////
var maxSubmitBodyLen    = 1024 * 1024;              //单位字符数或字节，HTTP提交的数据大小限制，对于String是字符数，对于Buffer是字节
var adminKey            = "gow_admin";              //想对这个服务器执行管理操作要用的Key
var maxMessageLen       = 1024 * 1024;              //单位字节，Socket发送的包限制，客户端上发的一个消息体最大长度

////////////全局执行////////////
//给数据库名加上服务器ID后缀
dbUrl = dbUrl.replace("/gamedb?", "/gamedb" + serverId + "?");

////////////导出元素////////////
exports.areaName            = areaName;
exports.serverName          = serverName;
exports.serverId            = serverId;
exports.publicHostName      = publicHostName;
exports.debug               = debug;
exports.consoleLog          = consoleLog;
exports.dbUrl               = dbUrl;
exports.globalServerUrl     = globalServerUrl;
exports.adminServerUrl      = adminServerUrl;
exports.lanHostName         = lanHostName;
exports.appName             = appName;
exports.showIndex           = showIndex;
exports.showState           = showState;
exports.bindIP              = bindIP;
exports.httpBindIP          = httpBindIP;
exports.port                = port;
exports.httpPort            = httpPort;
exports.logDir              = logDir;
exports.logFileInv          = logFileInv;
exports.logFileTime         = logFileTime;
exports.dbPoolSize          = dbPoolSize;
exports.minReconnInv        = minReconnInv;
exports.maxReconnInv        = maxReconnInv;
exports.incReconnInv        = incReconnInv;
exports.maxRetryCntRunning  = maxRetryCntRunning;
exports.maxRetryCntExiting  = maxRetryCntExiting;
exports.maxSubmitBodyLen    = maxSubmitBodyLen;
exports.adminKey            = adminKey;
exports.maxMessageLen       = maxMessageLen;

////////////说明////////////
/*
 数据库连接字符串示例
 如果有副本集：
  mongodb://账号:密码@主机1:端口,主机2:端口,主机3:端口/数据库名?replicaSet=副本集名&maxPoolSize=1&autoReconnect=true&w=1&wtimeoutMS=5000&readPreference=nearest
 如果是单机或分片集群：
 mongodb://账号:密码@主机:端口/数据库名?maxPoolSize=1&autoReconnect=true&w=1&wtimeoutMS=5000
 这里的maxPoolSize=1很重要，不然同一个用户的数据写到不同的连接里，导致数据乱序
 */