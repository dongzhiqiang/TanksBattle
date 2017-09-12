
"use strict";
var gameConfig = require("./gameConfig");

class LanguageConfig
{
    constructor() {
        this.key = 0;
        this.desc = "";
    }

    static fieldsDesc() {
        return {
            key: {type: Number},
            desc: {type: String},
        };
    }
}

function getLanguage(key)
{
    return gameConfig.getCsvConfig("languageCfg", key).desc;
}

exports.LanguageConfig = LanguageConfig;
exports.getLanguage = getLanguage;



