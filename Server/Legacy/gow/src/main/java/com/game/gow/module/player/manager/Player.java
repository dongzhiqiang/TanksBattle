package com.game.gow.module.player.manager;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Lob;
import javax.persistence.Transient;

import org.apache.commons.lang.StringUtils;
import org.hibernate.annotations.Index;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.enhance.Enhance;
import com.engine.common.utils.json.JsonUtils;
import com.game.gow.module.common.CachedSizes;
import com.game.gow.module.equip.manager.Equip;
import com.game.gow.module.role.manager.IRole;

/**
 * 玩家实体
 * 
 * @author wenkin
 */
@Entity
@Cached(size=CachedSizes.MAXIMUM)
public class Player implements IEntity<Long>, IRole {
	
	/* 索引与命名查询 */
	static final String ARENA = "Player_arena";
	static final String LEVEL = "Player_level";
	static final String PLAYER_NAME = "Player_name";
	static final String NAME2ID = "Player_name2Id";
	
	
	/**主键*/
	@Id
	private Long id;
	/**角色名称*/
	@Index(name = PLAYER_NAME)
	@Column(unique = true, nullable = false)
	private String name;	
	/** 等級 */
	@Index(name = LEVEL)
	private int level = 1;
	/** 经验 */
	private int exp;
	/** VIP等级 */
	private int vip;
	/** 临时VIP */
	private int expVip;
	/** 玩家状态 */
	private int state;
	/** 是否禁言true：禁言，false：正常 */
	private boolean block = false;
	/** 是否需要改名(次状态用于强制玩家改名) */
	private Boolean renamed = false;
	/** 装备*/
	@Lob
	private String equipsContent;
    
	// Getter and Setter ...
	
	@Override
	public Long getId() {
		// TODO Auto-generated method stub
		return this.id;
	}
    
	protected void setId(Long id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

    @Enhance
	protected void setName(String name) {
		this.name = name;
	}


	public int getLevel() {
		return level;
	}

	@Enhance
	public void setLevel(int level) {
		this.level = level;
	}


	public int getExp() {
		return exp;
	}


	protected void setExp(int exp) {
		this.exp = exp;
	}


	public int getVip() {
		return vip;
	}


	protected void setVip(int vip) {
		this.vip = vip;
	}


	public int getExpVip() {
		return expVip;
	}


	protected void setExpVip(int expVip) {
		this.expVip = expVip;
	}


	public int getState() {
		return state;
	}


	protected void setState(int state) {
		this.state = state;
	}


	public boolean isBlock() {
		return block;
	}


	protected void setBlock(boolean block) {
		this.block = block;
	}


	public Boolean getRenamed() {
		return renamed;
	}


	protected void setRenamed(Boolean renamed) {
		this.renamed = renamed;
	}
	
	/** 当前武器*/
	private int currentWeapon = 0;
	
	public int getCurrentWeapon() {
		return currentWeapon;
	}
	@Enhance
	public void setCurrentWeapon(int currentWeapon) {
		this.currentWeapon = currentWeapon;
	}
	
	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((getId() == null) ? 0 : getId().hashCode());
		return result;
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (!(obj instanceof Player))
			return false;
		Player other = (Player) obj;
		if (getId() == null) {
			if (other.getId() != null)
				return false;
		} else if (!getId().equals(other.getId()))
			return false;
		return true;
	}

	// Static Method's ...
	
	/**构造方法*/
	public static Player valueOf(long id,String name){
		Player result=new Player();
		result.id=id;
		result.name=name;
		return result;
	}

	@SuppressWarnings({ "unchecked"})
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
}
