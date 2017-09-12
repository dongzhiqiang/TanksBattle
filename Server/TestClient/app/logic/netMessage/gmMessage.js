"use strict";

const CmdIdsGm = {
    CMD_PROCESS_GM_CMD: 1,     // 执行gm命令
};

const ResultCodeGm = {
    GM_WRONG_FORMAT: 1, // 指令格式错误
    GM_EXECUTE_ERROR: 2, // 指令执行错误
};

/////////////////////////////////请求类////////////////////////////

class ProcessGmCmdVo {
    constructor() {
        this.msg = "";           //指令
    }

    static fieldsDesc() {
        return {
            msg: {type: String, notNull: true},
        };
    }
}

/////////////////////////////////导出元素////////////////////////////
exports.CmdIdsGm = CmdIdsGm;
exports.ResultCodeGm = ResultCodeGm;

exports.ProcessGmCmdVo = ProcessGmCmdVo;