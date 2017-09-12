"use strict";

//无效模块
const INVALID_MODULE = 0;
//无效命令
const INVALID_COMMAND = 0;

//值不可以为0
const ModuleIds = {
    MODULE_ACCOUNT: 1,
    MODULE_ROLE: 2,
    MODULE_ITEM: 3,
    MODULE_EQUIP: 4,
    MODULE_GM: 5,
    MODULE_PET: 6,
    MODULE_LEVEL: 7,
    MODULE_ACTIVITY: 8,
    MODULE_RANK:9,
    MODULE_WEAPON:10,
    MODULE_OPACTIVITY:11,
    MODULE_MAIL:12,
    MODULE_SYSTEM:13,
    MODULE_FLAME:14,
    MODULE_TASK:15,
    MODULE_SOCIAL:16,
    MODULE_CORPS:17,
    MODULE_CHAT:18,
    MODULE_SHOP : 19,
    MODULE_ELITE_LEVEL : 20,
    MODULE_TREASURE : 21,
};

//错误码，这里主要是出错
const ResultCode = {
    SUCCESS: 0,
    SERVER_ERROR: -1,   //服务器错误，一般是保持当前界面，或者跳到服务器选择界面
    DB_ERROR: -2,   //数据库错误，一般是保持当前界面，或者跳到服务器选择界面
    BAD_PARAMETER: -3,   //参数错误，一般是保持当前界面，或者跳到服务器选择界面
    BAD_REQUEST: -4,    //请求不被处理
    PARSE_ERROR: -5,    //解析数据出错，一般用于客户端，但还是写入统一错误码
    CONFIG_ERROR: -6,   //配置有错
    NOT_EXIST_ERROR:-7, //找不到操作对象
    NO_ENOUGH_GOLD:-8,  //金币不足
    DIAMOND_INSUFFICIENT:-9,   //钻石不足
    NO_ENOUGH_LEVEL:-10,    //等级不足
};

//当值为INVALID_COMMAND时表示这个模块的消息全不重发
//格式：{模块1ID:{命令1ID:true,命令2ID:true},模块2ID:0}
//如果外层的value为0时，表示整个模块下的消息码都不重发
//示例：{"1":{"1":true,"2":true},"2":0}
var SvrNoResend = {};

/**
 *
 * @param {number} module
 * @param {number[]|number|null} cmdIds - 如果是数组，就把旧配置覆盖，如果是数字，就是添加到现有，如果现有的是一个0，那就改为object，也就是全禁止改为部分禁止，如果是null，那就这个模块下的消息全部不重发
 */
function addServerNoResend(module, cmdIds) {
    if (cmdIds == null)
    {
        SvrNoResend[module] = 0;
    }
    else if (Object.isNumber(cmdIds))
    {
        let cfgs = SvrNoResend[module];
        if (!Object.isObject(cfgs))
            SvrNoResend[module] = {[cmdIds]:true};
        else
            cfgs[cmdIds] = true;
    }
    else if (Object.isArray(cmdIds))
    {
        let cfgs = SvrNoResend[module];
        if (!Object.isObject(cfgs))
            SvrNoResend[module] = cfgs = {};
        for (var i = 0; i < cmdIds.length; ++i)
            cfgs[cmdIds[i]] = true;
    }
}

/**
 *
 * @param module
 * @param cmd
 * @returns {boolean}
 */
function isServerNoResend(module, cmd) {
    var cfgs = SvrNoResend[module];
    //注意一定要全等
    return cfgs === INVALID_COMMAND || !!(cfgs && cfgs[cmd]);
}

exports.ModuleIds = ModuleIds;
exports.ResultCode = ResultCode;
exports.addServerNoResend = addServerNoResend;
exports.isServerNoResend = isServerNoResend;