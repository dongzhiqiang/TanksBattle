"use strict";

var gameConfig = require("./gameConfig");
var LvValue = require("./lvValue").LvValue;
var AddPropCxt = require("./lvValue").AddPropCxt;

class PetBondConfig
{
    constructor() {
        this.id = "";
        this.name="";
        this.pet1="";
        this.pet2="";
        this.pet3="";
        this.pet4="";
        this.param="";
        this.desc="";
        this.powerRate="";
        this.pets = [];
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            name: {type: String},
            pet1: {type: String},
            pet2: {type: String},
            pet3: {type: String},
            pet4: {type: String},
            param: {type: String},
            desc: {type: String},
            powerRate: {type: String},



        };
    }

    /** 因为有配置依赖关系只能在使用的时候再构造
     * returns {LvValue}
     */
    getPowerRateLvValue()
    {
        if(!this._powerRateLvValue)
        {
            this._powerRateLvValue = new LvValue(this.powerRate);
        }
        return this._powerRateLvValue;
    }

    /** 因为有配置依赖关系只能在使用的时候再构造
     * returns {Array}
     */
    getRateCxts()
    {
        if(!this._rateCxts)
        {
            this._rateCxts = [];
            var params = this.param.split(",");
            for(var i=0; i<params.length; i++)
            {
                if(params[i])
                {
                    this._rateCxts.push(new AddPropCxt(params[i]));
                }
            }
        }
        return this._rateCxts;
    }

    /**
     * 使用默认读取方式读完一行数据后可以执行对行对象的再次处理
     * 如果使用自定义读取方式，直接在那个自定义读取方式里处理就行了，不用这个函数了
     * 这个函数可选，没有就不执行
     * @param {object} row - 行数据对象
     */
    static afterDefReadRow(row)
    {
        if(row.pet1)row.pets.push(row.pet1);
        if(row.pet2)row.pets.push(row.pet2);
        if(row.pet3)row.pets.push(row.pet3);
        if(row.pet4)row.pets.push(row.pet4);
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {PetBondConfig}
 */
function getPetBondConfig(key)
{
    return gameConfig.getCsvConfig("petBond", key);
}

exports.PetBondConfig = PetBondConfig;
exports.getPetBondConfig = getPetBondConfig;