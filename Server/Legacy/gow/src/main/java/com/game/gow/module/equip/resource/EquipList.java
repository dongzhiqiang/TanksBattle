package com.game.gow.module.equip.resource;

import com.engine.common.resource.anno.Id;
import com.engine.common.resource.anno.Resource;

@Resource(value="equip")
public class EquipList {
	/** 标识*/
	@Id
	private int id;
	
	/** 名称*/
	private String name;
	
	/** 装备位*/
	private int posIndex;
	
	/** 品质*/
	private int quality;
	
	/** 状态ID*/
	private String stateId;
	
	/** 属性ID*/
	private String attributeId;
	
	/** 属性分配*/
	private String attributeAllocateId;
	
	/** 允许出售*/
	private boolean isSell;
	
	/** 价格*/
	private String priceId;
	
	/** 星级*/
	private int star;
	
	/** 基础属性比例*/
	private double attributeRate;
	
	/** 升级属性比例*/
	private double upgradeAttributeRate;
	
	/** 最大等级*/
	private int maxLevel;
	
	/** 进阶后装备ID*/
	private int advanceEquipId;
	
	/** 进阶消耗ID*/
	private String advanceCostId;
	
	/** 觉醒后装备ID*/
	private int rouseEquipId;
	
	/** 觉醒消耗ID*/
	private String rouseCostId;
	
	/** 武器ID*/
	private String weaponId;
	
	/** 图标*/
	private String icon;
	
	/** 品质等级*/
	private int qualityLevel;
	
	/** 觉醒描述*/
	private String rouseDescription;
	
	public int getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public int getQuality() {
		return quality;
	}

	public void setQuality(int quality) {
		this.quality = quality;
	}

	public String getStateId() {
		return stateId;
	}

	public void setStateId(String stateId) {
		this.stateId = stateId;
	}

	public String getAttributeId() {
		return attributeId;
	}

	public void setAttributeId(String attributeId) {
		this.attributeId = attributeId;
	}

	public String getAttributeAllocateId() {
		return attributeAllocateId;
	}

	public void setAttributeAllocateId(String attributeAllocateId) {
		this.attributeAllocateId = attributeAllocateId;
	}

	public boolean isSell() {
		return isSell;
	}

	public void setSell(boolean isSell) {
		this.isSell = isSell;
	}

	public String getPriceId() {
		return priceId;
	}

	public void setPriceId(String priceId) {
		this.priceId = priceId;
	}



	public int getStar() {
		return star;
	}

	public void setStar(int star) {
		this.star = star;
	}

	public double getAttributeRate() {
		return attributeRate;
	}

	public void setAttributeRate(double attributeRate) {
		this.attributeRate = attributeRate;
	}

	public double getUpgradeAttributeRate() {
		return upgradeAttributeRate;
	}

	public void setUpgradeAttributeRate(double upgradeAttributeRate) {
		this.upgradeAttributeRate = upgradeAttributeRate;
	}

	public int getMaxLevel() {
		return maxLevel;
	}

	public void setMaxLevel(int maxLevel) {
		this.maxLevel = maxLevel;
	}

	public int getAdvanceEquipId() {
		return advanceEquipId;
	}

	public void setAdvanceEquipId(int advanceEquipId) {
		this.advanceEquipId = advanceEquipId;
	}

	public String getAdvanceCostId() {
		return advanceCostId;
	}

	public void setAdvanceCostId(String advanceCostId) {
		this.advanceCostId = advanceCostId;
	}

	public int getRouseEquipId() {
		return rouseEquipId;
	}

	public void setRouseEquipId(int rouseEquipId) {
		this.rouseEquipId = rouseEquipId;
	}

	public String getRouseCostId() {
		return rouseCostId;
	}

	public void setRouseCostId(String rouseCostId) {
		this.rouseCostId = rouseCostId;
	}

	public int getPosIndex() {
		return posIndex;
	}

	public void setPosIndex(int posIndex) {
		this.posIndex = posIndex;
	}

	public String getWeaponId() {
		return weaponId;
	}

	public void setWeaponId(String weaponId) {
		this.weaponId = weaponId;
	}

	public String getIcon() {
		return icon;
	}

	public void setIcon(String icon) {
		this.icon = icon;
	}

	public int getQualityLevel() {
		return qualityLevel;
	}

	public void setQualityLevel(int qualityLevel) {
		this.qualityLevel = qualityLevel;
	}

	public String getRouseDescription() {
		return rouseDescription;
	}

	public void setRouseDescription(String rouseDescription) {
		this.rouseDescription = rouseDescription;
	}


}
