/**
 * 名称：武器信息
 * 日期：2016.4.6
 * 描述：是不是当前正在使用的武器，武器的4样技能的等级，武器的技能的铭文
 */
"use strict";
var enEquipPos = require("../equip/equip").enEquipPos;
var WeaponConfig= require("../gameConfig/weaponConfig");
var RoleSkillConfig= require("../gameConfig/roleSkillConfig");
var HeroTalentConfig = require("../gameConfig/heroTalentConfig");
var ElementConfig = require("../gameConfig/elementConfig");

var enSkillPos= {
    atk     : 0,    //普通攻击
    skill1  : 1,    //技能1
    skill2  : 2,    //技能2
    skill3  : 3,    //技能3
    max     : 4,
};

//铭文
class WeaponSkillTalent{
    /**
     *
     * @param data
     * @param {Number} idx
     * @param {WeaponSkill} parent
     */
    constructor(data,idx,parent) {
        //为了防止data为空
        data = data || {};
        //WeaponSkillTalent.CheckValid(data);

        //成员
        Object.defineProperty(this, "idx", {enumerable: false, value: idx});//第几个铭文，对应技能表铭文列的第几个
        /**
         * @type{Weapon}
         */
        Object.defineProperty(this, "parent", {enumerable: false, value: parent});
        this.lv =data.lv||0;
    }

    /*//返回一个带有默认数据的data
    static GetDefaultData(){
        return {lv:0};
    }

    //检查是不是有效，无效的话会抛出异常
    static CheckValid(data){
        if(!Object.isNumber(data.lv) )
        {
            throw new Error("武器铭文data参数不足");
        }
    }*/

    /**
     *
     * @returns {HeroTalentConfig}
     */
    getTalentCfg(){
        let skillCfg=this.parent.getSkillCfg();
        if(!skillCfg )
            return null;

        let id =skillCfg.talent[this.idx];
        if(!id)
            return null;


        return HeroTalentConfig.getTalentConfig(id);
    }
}

class WeaponSkill {
    /**
     *
     * @param data
     * @param {Number} idx
     * @param {Weapon} parent
     */
    constructor(data,idx,parent) {
        //为了防止data为空
        data = data || {};
        //WeaponSkill.CheckValid(data);


        //成员
        Object.defineProperty(this, "idx", {enumerable: false, value: idx});//第几个技能
        /**
         * @type{Weapon}
         */
        Object.defineProperty(this, "parent", {enumerable: false, value: parent});
        this.lv =data.lv || 1;
        /** @type {object.<number, WeaponSkillTalent>} 武器技能，普通攻击是0，三个主动技能分别是1、2、3*/
        this.talents = {};


        try {
            //铭文
            let talentsData = data.talents||{};
            for(let i =0,len = this.getTalentCount();i<len;++i){
                this.talents[i] =new WeaponSkillTalent(talentsData[i],i,this);
            }
        }
        catch (err) {
            err.message = "第{0}个技能,{1}".format(idx,err.message);
            throw err;
        }
    }

    //返回一个带有默认数据的data
    /*static GetDefaultData(){
        return {lv:1};
    }

    //检查是不是有效，无效的话会抛出异常
    static CheckValid(data){
        if(!Object.isNumber(data.lv) )//先设置天赋而没有设置过等级的情况下
        {
            throw new Error("武器技能data参数不足");
        }
    }*/

    /**
     *
     * @returns {RoleSkillConfig}
     */
    getSkillCfg(){
        let weaponCfg=this.parent.getWeaponCfg();
        if(!weaponCfg )
            return null;

        let skillId =weaponCfg.getSkillId(this.idx);
        if(!skillId)
            return null;

        return RoleSkillConfig.getRoleSkillConfig(this.parent.parent.getRoleCfg().id,skillId);
    }

    getTalentCount(){
        let cfg =this.getSkillCfg();
        if(!cfg || !cfg.talent )
            return 0;
        return cfg.talent.length;
    }
}

class Weapon {
    /**
     *
     * @param data
     * @param {Number} idx
     * @param {Role} parent
     */
    constructor(data,idx,parent) {
        //为了防止data为空
        data = data || {};
        //检查data是不是有效
        //Weapon.CheckValid(data);


        //成员
        Object.defineProperty(this, "idx", {enumerable: false, value: idx});//第几个武器
        /**@type {Role} 拥有者*/
        Object.defineProperty(this, "parent", {enumerable: false, value: parent});
        /** @type {object.<number, WeaponSkill>} 技能*/
        this.skills ={};
        /** @type {number[]} 元素属性*/
        this.elements =[];

        try {
            //技能的位置是普通攻击为0,三个技能是1、2、3
            let skillsData = data.skills||{};
            for(let i =0,len = enSkillPos.max;i<len;++i){
                this.skills[i] =new WeaponSkill(skillsData[i],i,this);
            }

            //元素属性
            if(data.elements == null)
            {
                for(let i =0,len = ElementConfig.getMaxElement();i<len;++i){
                    this.elements[i] =i+1;
                }
            }
            else  if(data.elements.length >=ElementConfig.getMaxElement()){
                let elementsData = data.elements||[];
                for(let i =0,len = ElementConfig.getMaxElement();i<len;++i){
                    this.elements[i] =elementsData[i]!=null?elementsData[i]:i+1;
                }
            }
            else
            {
                throw new Error("元素属性个数不对:"+data.elements.length);
            }


        }
        catch (err) {
            err.message = "第{0}个武器,{1}".format(idx,err.message);
            throw err;
        }


    }

    /*//返回一个带有默认数据的data
    static GetDefaultData(){
        return {};
    }

    //检查是不是有效，无效的话会抛出异常
    static CheckValid(data){
    }
*/


    getWeaponCfg(){
        let equipsPart =this.parent.getEquipsPart();
        let equip =equipsPart.getEquipByIndex(enEquipPos.minWeapon +this.idx);
        if(!equip)
            return null;
        let equipCfg =equip.getEquipCfg();
        if(!equipCfg)
            return null;
        return WeaponConfig.getWeaponConfig(equipCfg.weaponId);

    }
}

module.exports =Weapon;
exports.enSkillPos = enSkillPos;