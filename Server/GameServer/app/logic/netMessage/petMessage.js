"use strict";

const CmdIdsPet = {
    CMD_UPGRADE_PET: 1,   //升级宠物
    CMD_ADVANCE_PET: 2,    //进阶宠物
    CMD_UPSTAR_PET: 3,     //升星宠物
    CMD_CHOOSE_PET: 4,     //选择宠物
    CMD_UPGRADE_PET_SKILL: 5, //升级宠物技能
    CMD_UPGRADE_PET_TALENT: 6, //升级宠物天赋
    CMD_UNCHOOSE_PET: 7,     //选择宠物不出战
    CMD_RECRUIT_PET: 8,     //招募宠物
    PUSH_REMOVE_PET: -1,   //删除宠物
    PUSH_ADD_PET: -2,   //添加宠物
    PUSH_ADD_OR_UPDATE_PET_SKILL: -3,   //添加更新宠物技能
    PUSH_ADD_OR_UPDATE_TALENT: -4,  //添加更新天赋
    PUSH_ADD_OR_UPDATE_PET_FORMATION: -5,  //添加更新宠物阵型
};

const ResultCodePet = {
    PET_NO_PET: 1,  //没有指定宠物
    PET_MAX_LEVEL: 2, //宠物已达到最大等级
    PET_NO_ENOUGH_ITEM: 3, //道具数量不对
    PET_WRONG_ITEM: 4, //道具类型不对
    PET_MAX_ADV_LEVEL: 5, //宠物达到最大进阶等级
    PET_MAX_STAR: 6, //宠物达到最大星数
    PET_NO_LEVEL: 7, //宠物等级未达到进阶条件
    PET_WRONG_POS: 8, //宠物位参数错误
    PET_NO_SKILL: 9, //宠物无此技能
    PET_NO_TALENT: 10, //宠物无此天赋
    PET_MAX_SKILL: 11, //宠物技能满级
    PET_MAX_TALENT: 12, //宠物天赋满级
    PET_LEVEL_OVER_OWNER: 13, //宠物等级不能超过主人等级
    PET_SKILL_NEED_STAR: 14, //宠物未达到开启技能的星级
    PET_TALENT_NEED_ADV_LV: 15, //宠物未达到开启天赋的等阶
    PET_SKILL_OVER_PET_LV: 16, //技能等级不能超过宠物等级
    PET_TALENT_OVER_PET_ADV_LV: 17, //天赋等阶不能超过宠物等阶允许等级
    PET_EXISTS: 18, //宠物已存在
    PET_POS_NEED_LEVEL: 19, //出战位需要角色等级开启
};

/////////////////////////////////请求类////////////////////////////

class UpgradePetRequestVo {
    constructor() {
        this.guid = "";           //宠物guid
        this.itemId = 0;           //使用的道具id
        this.num = 0;         //数量
    }

    static fieldsDesc() {
        return {
            guid: {type: String, notNull: true},
            itemId: {type: Number, notNull: true},
            num: {type: Number, notNull: true},
        };
    }
}

class AdvancePetRequestVo {
    constructor() {
        this.guid = "";           //宠物guid
    }

    static fieldsDesc() {
        return {
            guid: {type: String, notNull: true},
        };
    }
}

class UpstarPetRequestVo {
    constructor() {
        this.guid = "";           //宠物guid
    }

    static fieldsDesc() {
        return {
            guid: {type: String, notNull: true},
        };
    }
}

class ChoosePetRequestVo {
    constructor() {
        this.guid = "";           //宠物guid
        this.petFormation = 0;  //宠物阵型id
        this.petPos = 0;         //宠物出战位
    }

    static fieldsDesc() {
        return {
            guid: {type: String, notNull: true},
            petFormation: {type: Number, notNull: true},
            petPos: {type: Number, notNull: true},
        };
    }
}

class UnchoosePetRequestVo {
    constructor() {
        this.petFormation = 0;  //宠物阵型id
        this.petPos = 0;         //宠物出战位
    }

    static fieldsDesc() {
        return {
            petFormation: {type: Number, notNull: true},
            petPos: {type: Number, notNull: true},
        };
    }
}

class UpgradePetSkillRequestVo {
    constructor() {
        this.guid = "";           //宠物guid
        this.skillId = "";         //技能id
    }

    static fieldsDesc() {
        return {
            guid: {type: String, notNull: true},
            skillId: {type: String, notNull: true},
        };
    }
}

class UpgradePetTalentRequestVo {
    constructor() {
        this.guid = "";           //宠物guid
        this.talentId = "";         //天赋id
    }

    static fieldsDesc() {
        return {
            guid: {type: String, notNull: true},
            talentId: {type: String, notNull: true},
        };
    }
}

class RecruitPetRequestVo {
    constructor() {
        this.roleId = "";           //宠物roleId
    }

    static fieldsDesc() {
        return {
            roleId: {type: String, notNull: true},
        };
    }
}

/////////////////////////////////回复类////////////////////////////

class UpdatePetResultVo {
    constructor() {
        this.addLv = 0;           //增加的等级
        this.addExp = 0;           //增加的经验值
    }

    static fieldsDesc() {
        return {
            addLv: {type: Number, notNull: true},
            addExp: {type: Number, notNull: true},
        };
    }
}

class UpstarPetResultVo {
    constructor() {
        this.newStar = 0;           //新的星级
    }

    static fieldsDesc() {
        return {
            newStar: {type: Number, notNull: true},
        };
    }
}

class ChoosePetResultVo {
    constructor() {
        this.needUpdatePets = [];
    }

    static fieldsDesc() {
        return {
            needUpdatePets: {type: Array, itemType: String, notNull: true}
        };
    }
}

class UpgradePetSkillResultVo {
    constructor() {
        this.skillId = "";         //技能id
    }

    static fieldsDesc() {
        return {
            skillId: {type: String, notNull: true},
        };
    }
}

class UpgradePetTalentResultVo {
    constructor() {
        this.talentId = "";         //天赋id
    }

    static fieldsDesc() {
        return {
            talentId: {type: String, notNull: true},
        };
    }
}

class RecruitPetResultVo {
    constructor() {
        this.guid = "";           //宠物guid
    }

    static fieldsDesc() {
        return {
            guid: {type: String, notNull: true},
        };
    }
}

/////////////////////////////////推送类////////////////////////////

class RemovePetRoleVo {
    /**
     *
     * @param {string} guid
     */
    constructor(guid) {
        this.guid = guid;
    }
}

class AddOrUpdatePetSkillVo {
    /**
     * @param {string} guid
     * @param {boolean} isAdd - 否则是update
     * @param {PetSkill} petSkill
     */
    constructor(guid, isAdd, petSkill) {
        this.guid = guid
        this.isAdd = isAdd;
        this.petSkill = petSkill;
    }
}

class AddOrUpdateTalentVo {
    /**
     * @param {string} guid
     * @param {boolean} isAdd - 否则是update
     * @param {Talent} talent
     */
    constructor(guid, isAdd, talent) {
        this.guid = guid
        this.isAdd = isAdd;
        this.talent = talent;
    }
}

class AddOrUpdatePetFormationVo {
    /**
     * @param {boolean} isAdd - 否则是update
     * @param {PetFormation} petFormation
     */
    constructor(isAdd, petFormation) {
        this.isAdd = isAdd;
        this.petFormation = petFormation;
    }
}

exports.CmdIdsPet = CmdIdsPet;
exports.ResultCodePet = ResultCodePet;

exports.RemovePetRoleVo = RemovePetRoleVo;
exports.UpgradePetRequestVo = UpgradePetRequestVo;
exports.UpdatePetResultVo = UpdatePetResultVo;
exports.AdvancePetRequestVo = AdvancePetRequestVo;
exports.UpstarPetRequestVo = UpstarPetRequestVo;
exports.UpstarPetResultVo = UpstarPetResultVo;
exports.UpgradePetSkillResultVo = UpgradePetSkillResultVo;
exports.UpgradePetTalentResultVo = UpgradePetTalentResultVo;
exports.ChoosePetResultVo = ChoosePetResultVo;
exports.ChoosePetRequestVo = ChoosePetRequestVo;
exports.UpgradePetSkillRequestVo = UpgradePetSkillRequestVo;
exports.AddOrUpdatePetSkillVo = AddOrUpdatePetSkillVo;
exports.UpgradePetTalentRequestVo = UpgradePetTalentRequestVo;
exports.AddOrUpdateTalentVo = AddOrUpdateTalentVo;
exports.UnchoosePetRequestVo = UnchoosePetRequestVo;
exports.RecruitPetRequestVo = RecruitPetRequestVo;
exports.RecruitPetResultVo = RecruitPetResultVo;
exports.AddOrUpdatePetFormationVo = AddOrUpdatePetFormationVo;