package com.game.gow.module.role.manager;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Lob;
import javax.persistence.NamedQueries;
import javax.persistence.NamedQuery;
import javax.persistence.Transient;

import org.apache.commons.lang.StringUtils;
import org.hibernate.annotations.Table;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.CacheUnit;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.Idx;
import com.engine.common.ramcache.enhance.Enhance;
import com.engine.common.utils.json.JsonUtils;
import com.game.gow.module.common.CachedSizes;
import com.game.gow.module.equip.manager.Equip;


/**
 * 宠物实体
 */
@Entity
@NamedQueries({
	@NamedQuery(name = Pet.MAX_ID, query = "select max(t.id) from Pet as t where t.id between ? and ?"),
	@NamedQuery(name = "Pet.owner", query = "from Pet t where t.owner = ?")})
@Cached(size = CachedSizes.DEFAULT, unit = CacheUnit.REGION)
@Table(appliesTo = "Pet")
public class Pet implements IEntity<Long>, IRole {
	/* 命名查询名 */
	public static final String MAX_ID = "Pet_maxId";

	/** 唯一编号 */
	@Id
	private Long id;
	
	@Idx(query = "Pet.owner")
	private Long owner;
	
	/** 等級 */
	private int level = 1;
	
	/** 经验 */
	private int exp;
	
	/** 等阶*/
	private int advanceLevel = 1;
	
	/** 星级*/
	private int star = 1;
	
	/** 装备*/
	@Lob
	private String equipsContent;
	
	/** 配置表id*/
	private String cfgId;
	
	/** 当前武器*/
	private int currentWeapon = 0;
	
	public int getCurrentWeapon() {
		return currentWeapon;
	}
	public void setCurrentWeapon(int currentWeapon) {
		this.currentWeapon = currentWeapon;
	}
	public Long getId() {
		return id;
	}
	public void setId(Long id) {
		this.id = id;
	}
	public Long getOwner() {
		return owner;
	}
	public void setOwner(Long owner) {
		this.owner = owner;
	}
	public int getLevel() {
		return level;
	}
	public void setLevel(int level) {
		this.level = level;
	}
	public int getExp() {
		return exp;
	}
	public void setExp(int exp) {
		this.exp = exp;
	}
	@SuppressWarnings("unchecked")
	@Transient
	public List<Equip> getEquips() {
		List<Equip> result;
		if(StringUtils.isBlank(getEquipsContent()))
		{
			result = new ArrayList<Equip>();
		}
		else
		{
			try {
				result = JsonUtils.string2Collection(getEquipsContent(), List.class, Equip.class);
			} catch(Exception e)
			{
				result = new ArrayList<Equip>();
			}
		}
		return result;
	}

	@Transient
	public void setEquips(List<Equip> equips) {
		setEquipsContent(JsonUtils.object2String(equips));
	}


	public String getEquipsContent() {
		return equipsContent;
	}

	@Enhance
	public void setEquipsContent(String equipsContent) {
		this.equipsContent = equipsContent;
	
	}
	
	public String getCfgId() {
		return cfgId;
	}
	public void setCfgId(String cfgId) {
		this.cfgId = cfgId;
	}
	public int getAdvanceLevel() {
		return advanceLevel;
	}
	@Enhance
	public void setAdvanceLevel(int advanceLevel) {
		this.advanceLevel = advanceLevel;
	}
	public int getStar() {
		return star;
	}
	@Enhance
	public void setStar(int star) {
		this.star = star;
	}

}
