package com.game.gow.module.equip.resource;

import java.util.ArrayList;

import org.apache.commons.lang.StringUtils;

import com.engine.common.resource.anno.Id;
import com.engine.common.resource.anno.Resource;

@Resource(value="equip")
public class EquipInitList {
	/** id*/
	@Id
	private String id;
	/** 装备列表*/
	private String equips;
	
	private ArrayList<Integer> equipsAry;

	public String getId() {
		return id;
	}

	public void setId(String id) {
		this.id = id;
	}
	
	public ArrayList<Integer> getEquipsAry(){
		if( equipsAry == null )
		{
			equipsAry = new ArrayList<Integer>();
			String[] temp = equips.split(",");
			for( String s : temp )
			{
				if( StringUtils.isNotBlank(s))
				{
					equipsAry.add(Integer.parseInt(s));
				}
			}
		}
		return equipsAry;
	}

	public String getEquips() {
		return equips;
	}

	public void setEquips(String equips) {
		this.equips = equips;
	}
}
