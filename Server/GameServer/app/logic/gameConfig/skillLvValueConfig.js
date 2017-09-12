"use strict";

var gameConfig = require("./gameConfig");

class SkillLvValueConfig
{
    constructor() {
        this.id = "";
        this.value=1;
        this.rate=1;
        this.dot=0;
        this.prefix="";
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            value: {type: Number},
            rate: {type: Number},
            doc: {type: Number},
        };
    }

    /**
     * 使用默认读取方式读完一行数据后可以执行对行对象的再次处理
     * 如果使用自定义读取方式，直接在那个自定义读取方式里处理就行了，不用这个函数了
     * 这个函数可选，没有就不执行
     * @param {object} row - 行数据对象
     */
    static afterDefReadRow(row)
    {
        var idx = row.id.indexOf(":");
        row.prefix = row.id.substr(0, idx==-1?row.id.length:idx);
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {SkillLvValueConfig}
 */
function getSkillLvValueConfig(key)
{
    return gameConfig.getCsvConfig("skillLvValue", key);
}

exports.SkillLvValueConfig = SkillLvValueConfig;
exports.getSkillLvValueConfig = getSkillLvValueConfig;