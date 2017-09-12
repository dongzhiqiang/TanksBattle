"use strict";

const CmdIdsRole = {
    CMD_REQ_HERO_INFO: 1,   //请求主角信息（包括宠物）
    PUSH_SYNC_PROP: -1,     //同步属性
};

const ResultCodeRole = {
    ROLE_NOT_EXISTS : 1, //角色不存在
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
    }
}

class RequestHeroInfoVo
{
    constructor() {
        this.heroId = 0;
    }
}

exports.CmdIdsRole = CmdIdsRole;
exports.ResultCodeRole = ResultCodeRole;

exports.RoleSyncPropVo = RoleSyncPropVo;
exports.FullRoleInfoVo = FullRoleInfoVo;
exports.RequestHeroInfoVo = RequestHeroInfoVo;