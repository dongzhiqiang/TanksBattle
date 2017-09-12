package com.game.gow.module.item.resource;

import com.engine.common.resource.anno.Id;
import com.engine.common.resource.anno.Resource;

@Resource(value="item")
public class ItemList {
	/** 标识*/
	@Id
	private int id;
	
	/** 名称*/
	private String name;
	
	/** 子类型*/
	private int subType;
	
	/** 品质*/
	private int quality;
	
	/** 叠加数量*/
	private int packNum;
	
	/** 状态ID*/
	private String stateId;
	
	/** 允许出售*/
	private boolean isSell;
	
	/** 价格*/
	private String priceId;
	
	/** 使用获得值*/
	private String useValue1;
	
	/** 图标*/
	private String icon;
	
	/** 描述*/
	private String description;
	
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

	public int getSubType() {
		return subType;
	}

	public void setSubType(int subType) {
		this.subType = subType;
	}

	public int getPackNum() {
		return packNum;
	}

	public void setPackNum(int packNum) {
		this.packNum = packNum;
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

	public String getUseValue1() {
		return useValue1;
	}

	public void setUseValue1(String useValue1) {
		this.useValue1 = useValue1;
	}

	public String getIcon() {
		return icon;
	}

	public void setIcon(String icon) {
		this.icon = icon;
	}

	public String getDescription() {
		return description;
	}

	public void setDescription(String description) {
		this.description = description;
	}



}
