package com.game.gow.module.role.resource;

import com.engine.common.resource.anno.Id;
import com.engine.common.resource.anno.Resource;

@Resource(value="role")
public class RoleList {
	/** 标识*/
	@Id
	private String id;
	
	/** 名称*/
	private String name;
	
	/** 模型id*/
	private String mod;
	
	/** 属性id*/
	private String prop;
	
	/** 属性分配比例*/
	private String propRate;
	
	/** 僵直系数*/
	private float behitRate;
	
	/** 被击特效*/
	private String behitFxs;
	
	/** 死亡特效*/
	private String deadFx;
	
	/** 怒气技能*/
	private String uniqueSkill;
	
	/** 普通攻击*/
	private String atkUpSkill;
	
	/** 技能*/
	private String skills;
	
	/** 技能文件*/
	private String skillFile;
	
	/** 最高星级*/
	private int maxStar;
	
	/** 最高等阶*/
	private int maxAdvanceLevel;
	
	/** 初始装备*/
	private String initEquips;
	
	/** 死后魂值*/
	private int soulNum;
	
	/** 最大等级*/
	private int maxLevel;
	
	/** 属性成长比例*/
	private String attributeAllocateId;
	
	/** 升级消耗id*/
	private String upgradeCostId;
	
	/** 进阶消耗id*/
	private String advanceCostId;
	
	/** 升星消耗id*/
	private String upstarCostId;
	
	/** 定位描述*/
	private String positioning;
	
	/** 头像图标*/
	private String icon;
	
	/** ui缩放*/
	private float uiModScale;


	public String getId() {
		return id;
	}

	public void setId(String id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}



	public String getUniqueSkill() {
		return uniqueSkill;
	}

	public void setUniqueSkill(String uniqueSkill) {
		this.uniqueSkill = uniqueSkill;
	}

	public String getAtkUpSkill() {
		return atkUpSkill;
	}

	public void setAtkUpSkill(String atkUpSkill) {
		this.atkUpSkill = atkUpSkill;
	}

	public String getSkills() {
		return skills;
	}

	public void setSkills(String skills) {
		this.skills = skills;
	}

	public String getSkillFile() {
		return skillFile;
	}

	public void setSkillFile(String skillFile) {
		this.skillFile = skillFile;
	}

	public int getMaxStar() {
		return maxStar;
	}

	public void setMaxStar(int maxStar) {
		this.maxStar = maxStar;
	}

	public int getMaxAdvanceLevel() {
		return maxAdvanceLevel;
	}

	public void setMaxAdvanceLevel(int maxAdvanceLevel) {
		this.maxAdvanceLevel = maxAdvanceLevel;
	}

	public String getInitEquips() {
		return initEquips;
	}

	public void setInitEquips(String initEquips) {
		this.initEquips = initEquips;
	}

	public String getMod() {
		return mod;
	}

	public void setMod(String mod) {
		this.mod = mod;
	}

	public String getProp() {
		return prop;
	}

	public void setProp(String prop) {
		this.prop = prop;
	}

	public String getPropRate() {
		return propRate;
	}

	public void setPropRate(String propRate) {
		this.propRate = propRate;
	}

	public float getBehitRate() {
		return behitRate;
	}

	public void setBehitRate(float behitRate) {
		this.behitRate = behitRate;
	}

	public String getBehitFxs() {
		return behitFxs;
	}

	public void setBehitFxs(String behitFxs) {
		this.behitFxs = behitFxs;
	}

	public String getDeadFx() {
		return deadFx;
	}

	public void setDeadFx(String deadFx) {
		this.deadFx = deadFx;
	}

	public int getSoulNum() {
		return soulNum;
	}

	public void setSoulNum(int soulNum) {
		this.soulNum = soulNum;
	}

	public int getMaxLevel() {
		return maxLevel;
	}

	public void setMaxLevel(int maxLevel) {
		this.maxLevel = maxLevel;
	}

	public String getAttributeAllocateId() {
		return attributeAllocateId;
	}

	public void setAttributeAllocateId(String attributeAllocateId) {
		this.attributeAllocateId = attributeAllocateId;
	}

	public String getAdvanceCostId() {
		return advanceCostId;
	}

	public void setAdvanceCostId(String advanceCostId) {
		this.advanceCostId = advanceCostId;
	}

	public String getUpstarCostId() {
		return upstarCostId;
	}

	public void setUpstarCostId(String upstarCostId) {
		this.upstarCostId = upstarCostId;
	}

	public String getUpgradeCostId() {
		return upgradeCostId;
	}

	public void setUpgradeCostId(String upgradeCostId) {
		this.upgradeCostId = upgradeCostId;
	}

	public String getPositioning() {
		return positioning;
	}

	public void setPositioning(String positioning) {
		this.positioning = positioning;
	}

	public String getIcon() {
		return icon;
	}

	public void setIcon(String icon) {
		this.icon = icon;
	}

	public float getUiModScale() {
		return uiModScale;
	}

	public void setUiModScale(float uiModScale) {
		this.uiModScale = uiModScale;
	}
	


}
