"use strict";

const CmdIdsRole = {
    CMD_REQ_HERO_INFO: 1,   //请求主角信息（包括宠物）
    CMD_REQ_PET_INFO: 2,   //请求宠物信息
    PUSH_SYNC_PROP: -1,     //同步属性
};

const ResultCodeRole = {
    ROLE_NOT_EXISTS : 1, //角色不存在
    PET_NOT_EXISTS : 2, //宠物不存在
};

class RoleSyncPropVo {
    /**
     *
     * @param {string} guid
     * @param {object.<string, *>} props
     */
    constructor(guid, props) {
        this.guid = guid;
        this.props = props;
    }
}

class FullRoleInfoVo {
    constructor() {
        this.props = {};
        this.equips = {};
        this.pets = {};
        this.items = {};
        this.levelInfo = {};
        this.petSkills = {};
        this.talents = {};
        this.actProps = {};
        this.weapons={};
        this.systems={};
        this.teaches={};
        this.mails={};
        this.opActProps={};
        this.flames={};
        this.taskProps={};
		this.social={};
        this.corps={};
        this.shops = {};
        this.eliteLevels = {};
        this.petFormations = {};
        this.treasures = {};
    }
}

class RequestHeroInfoVo
{
    constructor() {
        this.heroId = 0;
    }

    static fieldsDesc() {
        return {
            heroId: {type: Number, notNull:true},
        };
    }
}

class RequestPetInfoVo
{
    constructor() {
        this.heroId = 0;    //主人的ID
        this.guid = "";     //神侍的唯一ID
    }

    static fieldsDesc() {
        return {
            heroId: {type: Number, notNull:true},
            guid: {type: String, notNull:true},
        };
    }
}

exports.CmdIdsRole = CmdIdsRole;
exports.ResultCodeRole = ResultCodeRole;

exports.RoleSyncPropVo = RoleSyncPropVo;
exports.FullRoleInfoVo = FullRoleInfoVo;
exports.RequestHeroInfoVo = RequestHeroInfoVo;
exports.RequestPetInfoVo = RequestPetInfoVo;