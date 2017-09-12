package com.game.gow.module.role.model;

import java.util.ArrayList;
import java.util.List;

import com.engine.common.protocol.annotation.Transable;
import com.game.gow.module.equip.manager.Equip;
import com.game.gow.module.equip.model.EquipVo;
import com.game.gow.module.item.model.ItemVo;
import com.game.gow.module.role.manager.Pet;
import com.game.gow.module.player.manager.Player;

/**
 * 角色VO，玩家和宠物都使用它
 * 
 */
@Transable
public class RoleVo {

	/** 主键 */
	private Long id;
	/** 玩家姓名 */
	private String name;
	/** 等級 */
	private int level;
	/** 经验 */
	private long exp;
	/** VIP等级 */
	private int vip;
	/** 用户的背包信息*/
	private List<ItemVo> items;
	/** 宠物信息*/
	private List<RoleVo> pets;
	/** 装备*/
	private List<EquipVo> equips;
	/** 角色id*/
	private Long roleId;
	/** 配置表id*/
	private String cfgId;
	/** 当前武器*/
	private int currentWeapon;
	/** 等阶*/
	private int advanceLevel;
	/** 星级*/
	private int star;

	// ---------- 构造器 ----------

	private RoleVo() {
	}

	// ---------- Getter/Setter ----------

	public Long getId() {
		return id;
	}

	public void setId(Long id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}


	public int getLevel() {
		return level;
	}

	public void setLevel(int level) {
		this.level = level;
	}

	public long getExp() {
		return exp;
	}

	public void setExp(long exp) {
		this.exp = exp;
	}


	public int getVip() {
		return vip;
	}

	public void setVip(int vip) {
		this.vip = vip;
	}

	// ---------- 静态方法 ----------


	/** 构造方法 ：玩家*/
	public static RoleVo valueOf(Player player) {
		RoleVo result = new RoleVo();
		result.id = player.getId();
		result.name = player.getName();
		result.level = player.getLevel();
		result.exp = player.getExp();
		result.vip = player.getVip();
		List<Equip> equips = player.getEquips();
		result.equips = new ArrayList<EquipVo>();
		for(Equip equip : equips)
		{
			result.equips.add(EquipVo.valueOf(equip));
		}
		result.roleId = 0l;
		result.cfgId = "kratos";
		result.currentWeapon = player.getCurrentWeapon();
		return result;
	}
	
	/** 构造方法 ：宠物*/
	public static RoleVo valueOf(Pet pet) {
		RoleVo result = new RoleVo();
		result.id = pet.getId();
		result.name = ""; // TODO
		result.level = pet.getLevel();
		result.exp = pet.getExp();
		List<Equip> equips = pet.getEquips();
		result.equips = new ArrayList<EquipVo>();
		for(Equip equip : equips)
		{
			result.equips.add(EquipVo.valueOf(equip));
		}
		result.roleId = pet.getId();
		result.cfgId = pet.getCfgId();
		result.currentWeapon = pet.getCurrentWeapon();
		result.advanceLevel = pet.getAdvanceLevel();
		result.star = pet.getStar();
		return result;
	}

	public List<ItemVo> getItems() {
		return items;
	}

	public void setItems(List<ItemVo> items) {
		this.items = items;
	}

	public List<RoleVo> getPets() {
		return pets;
	}

	public void setPets(List<RoleVo> pets) {
		this.pets = pets;
	}

	public List<EquipVo> getEquips() {
		return equips;
	}

	public void setEquips(List<EquipVo> equips) {
		this.equips = equips;
	}

	public Long getRoleId() {
		return roleId;
	}

	public void setRoleId(Long roleId) {
		this.roleId = roleId;
	}

	public String getCfgId() {
		return cfgId;
	}

	public void setCfgId(String cfgId) {
		this.cfgId = cfgId;
	}

	public int getCurrentWeapon() {
		return currentWeapon;
	}

	public void setCurrentWeapon(int currentWeapon) {
		this.currentWeapon = currentWeapon;
	}

	public int getAdvanceLevel() {
		return advanceLevel;
	}

	public void setAdvanceLevel(int advanceLevel) {
		this.advanceLevel = advanceLevel;
	}

	public int getStar() {
		return star;
	}

	public void setStar(int star) {
		this.star = star;
	}





}
