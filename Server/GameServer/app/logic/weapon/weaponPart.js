/**
 * 名称：武器部件
 * 日期：2016.4.6
 * 描述：武器技能升级、武器技能的铭文升级、武器属性攻击
 */
"use strict";

var enEquipPos = require("../equip/equip").enEquipPos;
var Weapon =require("./weapon");
var dbUtil = require("../../libs/dbUtil");
var propertyTable = require("../gameConfig/propertyTable");
var enProp = require("../enumType/propDefine").enProp;
var roleConfig = require("../gameConfig/roleConfig");
var roleSkillConfig = require("../gameConfig/roleSkillConfig");
var heroTalentConfig = require("../gameConfig/heroTalentConfig");
var eventNames = require("../enumType/eventDefine").eventNames;
var logUtil = require("../../libs/logUtil");

class WeaponPart {
    constructor(role, data) {
        data =data.weapons;
        let len = enEquipPos.maxWeapon -enEquipPos.minWeapon+1;

        //检查data是不是有效
        //WeaponPart.IsValid(data);

        //成员
        this.curWeapon =data.curWeapon||0;
        /**@type {Role}*/
        Object.defineProperty(this, "_role", {enumerable: false, value: role});

        /**
         * 属性计算部位中间值
         * @type {object}
         */
        Object.defineProperty(this, "_partValues", {enumerable: false, writable:true, value: {}});
        /**
         * 属性计算部位中间值
         * @type {object}
         */
        Object.defineProperty(this, "_partRates", {enumerable: false, writable:true, value: {}});

        /** @type {object.<number, Weapon>}*/
        this.weapons = {};

        try {
            let weaponsData = data.weapons|| {};
            //这里保证全部槽位都更新过
            for (let i = 0; i < len; ++i) {
                this.weapons[i] =new Weapon(weaponsData[i],i,role);
            }

            //添加事件处理
            var thisObj = this;
            role.addListener(function(eventName, context, notifier) {
                thisObj.onPartFresh();
            }, eventNames.WEAPON_CHANGE);
        }
        catch (err) {
            //清除已创建的
            this.release();
            err.message = "武器部件,{0}".format(err.message);
            throw err;
        }
    }
    /*//检查是不是有效，无效的话会抛出异常
    static IsValid(data){
        if(!Object.isNumber(data.curWeapon) )
        {
            throw new Error("当前武器没有设置");
        }
    }*/

    release() {
        this.curWeapon =0;
        this.weapons = {};
    }


    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj) {
        this.getPrivateNetData(rootObj);
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj) {
        let tem ={};
        rootObj.weapons=tem;

        tem.curWeapon =this.curWeapon;
        tem.weapons = this.weapons;
        console.log("武器:"+JSON.stringify(tem));
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        this.getPrivateNetData(rootObj);
    }

    /**
     * 技能
     * @param weaponIdx
     * @param skillIdx
     * @returns {*|WeaponSkill}
     */
    getWeaponSkill(weaponIdx,skillIdx){
        return this.weapons[weaponIdx]&&this.weapons[weaponIdx].skills[skillIdx];
    }

    /**
     *
     * @param weaponIdx
     * @param skillIdx
     * @param talentIdx
     * @returns {*|WeaponSkillTalent}
     */
    getTalent(weaponIdx,skillIdx,talentIdx){
        let skill = this.getWeaponSkill(weaponIdx,skillIdx);
        return skill&&skill.talents[talentIdx];
    }

    /**
     *
     * @param weaponIdx
     * @returns {number[]}
     */
    getWeaponElement(weaponIdx){
        return this.weapons[weaponIdx]&&this.weapons[weaponIdx].elements;
    }

    //getWeaponElement

    save(key,value){
        //存盘
        //"weapons.curWeapon":2
        //"weapons.weapons.0.elem":2
        //"weapons.weapons.0.skills.0.lv":2
        //"weapons.weapons.0.skills.0.talents.0.lv":2

        //检测条件
        var roleCur = this._role;
        var roleTop = roleCur.getOwner();
        var isCurHero = roleCur.isHero();
        var guidCur = roleCur.getGUID();

        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;
        if (isCurHero) {
            //如果是主角自己的装备
            queryObj = {"props.heroId":heroId};
            updateObj = {$set:{[key] :value}};
        }
        col.updateOneNoThrow(queryObj, updateObj);
    }


    //存盘技能升级
    saveWeaponSkillLv(weaponIdx,skillIdx){
        var skill = this.getWeaponSkill(weaponIdx,skillIdx);
        this.save("weapons.weapons." +weaponIdx+".skills."+skillIdx+".lv",skill.lv);
    }

    //存盘换武器
    saveWeaponIdx(){
        this.save("weapons.curWeapon",this.curWeapon);
    }

    //存盘天赋升级
    saveTalentLv(weaponIdx,skillIdx,talentIdx){
        let talent =this.getTalent(weaponIdx,skillIdx,talentIdx);
        this.save("weapons.weapons." +weaponIdx+".skills."+skillIdx+".talents."+talentIdx+".lv",talent.lv);
    }

    //存盘天赋升级
    saveElement(weaponIdx){
        let elements =this.getWeaponElement(weaponIdx);
        this.save("weapons.weapons." +weaponIdx+".elements",elements);
    }


    /**
     * 计算部件的属性
     */
    freshPartProp()
    {
        propertyTable.set(0,  this._partValues);
        propertyTable.set(0,  this._partRates);
        this._partValues[enProp.power] = 0;
        this._partRates[enProp.power] = 0;

        var weapon = this.weapons[this.curWeapon];
        for (var i in weapon.skills )
        {
            var weaponSkill = weapon.skills[i];
            var roleSkillCfg = weaponSkill.getSkillCfg();
            if(!roleSkillCfg)
            {
                continue;
            }
            this._partRates[enProp.power] = this._partRates[enProp.power] + roleSkillCfg.getPowerRateLvValue().getByLv(weaponSkill.lv);
            //logUtil.info("wlvl"+weaponSkill.lv);
           // logUtil.info("wLvValue"+roleSkillCfg.getPowerRateLvValue().getByLv(weaponSkill.lv));
            //铭文
            for(var j in weaponSkill.talents)
            {
                var talent = weaponSkill.talents[j];
                var talentCfg = talent.getTalentCfg();
                if(!talentCfg)
                {
                    continue;
                }
                if(talent.lv<1)continue;
                this._partRates[enProp.power] = this._partRates[enProp.power] + talentCfg.getPowerRateLvValue().getByLv(talent.lv);
                //logUtil.info("tlvl"+talent.lv);
                //logUtil.info("tLvValue"+talentCfg.getPowerRateLvValue().getByLv(talent.lv));
            }
        }


        //logUtil.info("武器技能/铭文 角色增加战斗力系数:" + this._partRates[enProp.power]);
    }

    /**
     * 累加部件的属性
     */
    onFreshBaseProp(values, rates)
    {
        propertyTable.add(values, this._partValues, values);
        propertyTable.add(rates, this._partRates, rates);
        values[enProp.power] = values[enProp.power] + (this._partValues[enProp.power] || 0);
        rates[enProp.power] = rates[enProp.power] + (this._partRates[enProp.power] || 0);
    }

    onPartFresh()
    {
       // logUtil.info("Fresh part:weaponPart");
        this.freshPartProp();
        this._role.getPropsPart().onFreshBasePropUpdate();
    }
}

module.exports = WeaponPart;