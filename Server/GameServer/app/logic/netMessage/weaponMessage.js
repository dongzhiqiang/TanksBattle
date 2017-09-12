/**
 * 名称：武器消息
 * 日期：2016.4.7
 * 描述：
 */
"use strict";

const CmdIdsWeapon= {
    CMD_CHANGE_WEAPON: 1,    // 更换武器
    CMD_SKILL_LEVEL_UP: 2,   //升级武器的技能
    CMD_TALENT_LEVEL_UP: 3,   //升级武器的技能的铭文
    CMD_CHANGE_ELEMENT: 4,   //切换武器元素属性
};

const ResultCodeWeapon= {
    LEVEL_ERROR:1,//要升的等级不是下一级，可能重复操作
    ROLE_LEVEL_LIMIT:2,//要升的级数不能超过角色等级
    NO_ENOUGH_GOLD:3,//金币不足
    NO_ENOUGH_ITEM:4,//材料不足
    LEVEL_MAX: 5, // 已经满级
    WEAPON_POS_ERROR: 6, // 装备位错误
    ELEMENT_POS_ERROR: 7, // 属性位置错误
};

class WeaponChangeReq
{
    constructor(weapon) {
        this.weapon = weapon;          //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
    }

    static fieldsDesc() {
        return {
            weapon: {type: Number},
        };
    }
}

class WeaponChangeRes
{
    constructor(weapon) {
        this.weapon = weapon;          //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
    }
}

class WeaponSkillLevelUpReq {
    constructor(weapon,skill,lv) {
        this.weapon = weapon;          //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
        this.skill = skill;           //武器的第几个技能
        this.lv =lv;               //要升到的等级
    }

    static fieldsDesc() {
        return {
            weapon : {type: Number},
            skill: {type: Number},
            lv: {type: Number},
        };
    }
}

class WeaponSkillLevelUpRes{
    constructor(weapon,skill,lv) {
        this.weapon = weapon;          //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
        this.skill = skill;           //武器的第几个技能
        this.lv =lv;               //要升到的等级
    }
}

class WeaponSkillTalentUpReq {
    constructor(weapon,skill,talent,lv) {
        this.weapon = weapon;      //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
        this.skill = skill;        //武器的第几个技能
        this.talent = talent;      //技能的第几个天赋
        this.lv =lv;               //要升到的等级
    }

    static fieldsDesc() {
        return {
            weapon : {type: Number},
            skill: {type: Number},
            talent: {type: Number},
            lv: {type: Number},
        };
    }
}

class WeaponSkillTalentUpRes{
    constructor(weapon,skill,talent,lv) {
        this.weapon = weapon;      //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
        this.skill = skill;        //武器的第几个技能
        this.talent = talent;      //技能的第几个天赋
        this.lv =lv;               //要升到的等级
    }
}

class WeaponElementChangeReq {
    constructor(weapon,idx) {
        this.weapon = weapon;      //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
        this.idx = idx;        //要把哪个位置和第一个位置交换
    }

    static fieldsDesc() {
        return {
            weapon : {type: Number},
            idx: {type: Number},
        };
    }
}

class WeaponElementChangeRes{
    /**
     * @param {number} weapon
     * @param {number[]} elements
     */
    constructor(weapon,elements) {
        this.weapon = weapon;      //第几件武器从0开始，等于装备位减enEquipPos.minWeapon
        this.elements= elements;
    }
}

/////////////////////////////////导出元素////////////////////////////
exports.CmdIdsWeapon = CmdIdsWeapon;
exports.ResultCodeWeapon = ResultCodeWeapon;

exports.WeaponChangeReq = WeaponChangeReq;
exports.WeaponChangeRes = WeaponChangeRes;
exports.WeaponSkillLevelUpReq = WeaponSkillLevelUpReq;
exports.WeaponSkillLevelUpRes = WeaponSkillLevelUpRes;
exports.WeaponSkillTalentUpReq = WeaponSkillTalentUpReq;
exports.WeaponSkillTalentUpRes = WeaponSkillTalentUpRes;
exports.WeaponElementChangeReq = WeaponElementChangeReq;
exports.WeaponElementChangeRes = WeaponElementChangeRes;