"use strict";

var gameConfig = require("./gameConfig");
var mapByName = {};

class PropTypeConfig
{
    constructor() {
        this.id = 0;
        this.name = "";
        this.format = 0;
        this.key = "";
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            name: {type: String},
            format: {type: Number},
            key: {type: String}
        };
    }

    static afterDefReadRow(row)
    {
        mapByName[row.name] = row;
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {PropTypeConfig}
 */
function getPropTypeConfig(key)
{
    return gameConfig.getCsvConfig("propType", key);
}

/**
 *
 * @param {string} name
 * @returns {PropTypeConfig}
 */
function getByName(name)
{
    return mapByName[name];
}

exports.PropTypeConfig = PropTypeConfig;
exports.getPropTypeConfig = getPropTypeConfig;
exports.getByName = getByName;