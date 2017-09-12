"use strict";

////////////一般要修改的配置////////////
var globalURL           = "http://127.0.0.1/";      //全局服URL
var gameSvrIdToLogin    = 1;                        //要自动登录的服务器ID
var createPlayerNum     = 1;                        //要创建的虚拟玩家数
var usernameStartNum    = 1;                        //为了方便多个测试客户端同时登录一个服，这里设置账号编号起始值，相互错开

////////////一般不修改的配置////////////
/////首要/////
var appName             = "测试客户端";             //本应用名字
var channelId           = "default";                //使用渠道ID
/////日志/////
var debug               = true;                     //如果debug的话，日志更多
var consoleLog          = true;                     //是否打印日志到console，一般要的
var logDir              = "logs";                   //必须用"/"作分隔符，不要用"\"
var logFileInv          = 60 * 60 * 24;             //单位秒，每多长时间重新创建日志文件
var logFileTime         = 60 * 60 * 24 * 7;         //单位秒，每个日志文件多长时间清理

////////////导出元素////////////
exports.globalURL           = globalURL;
exports.gameSvrIdToLogin    = gameSvrIdToLogin;
exports.createPlayerNum     = createPlayerNum;
exports.usernameStartNum    = usernameStartNum;
exports.appName             = appName;
exports.channelId           = channelId;
exports.debug               = debug;
exports.consoleLog          = consoleLog;
exports.logDir              = logDir;
exports.logFileInv          = logFileInv;
exports.logFileTime         = logFileTime;