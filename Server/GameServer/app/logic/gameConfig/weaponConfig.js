"use strict";

var gameConfig = require("./gameConfig");
class WeaponConfig
{
    constructor() {
        this.id = 0;
        this.name ="";
        this.behitRate =1;
        this.atkUpSkill ="";
        this.skills =null;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            name: {type: String},
            behitRate: {type: Number},
            atkUpSkill: {type: String},
            skills: {type: Array, elemType:String},
        };
    }



    static getWeaponConfig(key)
    {
        return gameConfig.getCsvConfig("weapon", key);
    }

    getSkillId(idx){
        if(idx==0)
            return this.atkUpSkill;
        else
            return this.skills[idx-1];

    }

}

module.exports = WeaponConfig;