"use strict";

/////////////////////////////////定义////////////////////////////
const CmdIdsSystem = {
    CMD_SET_TEACH_DATA: 1,
    PUSH_ADD_OR_UPDATE_SYSTEM: -1,  //添加更新系统
    PUSH_SET_TEACH_DATA: -2,    //主要用于GM命令同步修改后的值
    PUSH_CLEAR_TEACH_DATA: -3,  //主要用于GM命令同步清空操作
    PUSH_ADD_OR_UPDATE_SYSTEMS: -4,  //添加更新系统(多个)
};

const ResultCodeSystem = {
    BAD_TEACH_DATA : 1,
    TEACH_KEYS_MAX : 2,
};

/////////////////////////////////请求类////////////////////////////
class SetTeachDataVo
{
    constructor() {
        this.key = "";
        this.val = 0;
    }

    static fieldsDesc() {
        return {
            key: {type: String, notNull: true},
            val: {type: Number, notNull: true},
        };
    }
}

/////////////////////////////////回复类////////////////////////////


/////////////////////////////////推送类////////////////////////////
class AddOrUpdateSystemVo {
    /**
     * @param {boolean} isAdd - 否则是update
     * @param {System} system
     */
    constructor(isAdd, system) {
        this.isAdd = isAdd;
        this.system = system;
    }
}

class PushSetTeachDataVo {
    constructor(key, val) {
        this.key = key;
        this.val = val;
    }
}

class AddOrUpdateSystemsVo {
    /**
     * @param {Array} system
     */
    constructor(systems) {
        this.systems = systems;
    }
}

/////////////////////////////////导出////////////////////////////
exports.CmdIdsSystem = CmdIdsSystem;
exports.ResultCodeSystem = ResultCodeSystem;

exports.SetTeachDataVo = SetTeachDataVo;
exports.AddOrUpdateSystemVo = AddOrUpdateSystemVo;
exports.PushSetTeachDataVo = PushSetTeachDataVo;
exports.AddOrUpdateSystemsVo = AddOrUpdateSystemsVo;