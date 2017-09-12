package com.game.gow.module.item.manager;

import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Lob;
import javax.persistence.NamedQueries;
import javax.persistence.NamedQuery;

import org.hibernate.annotations.Table;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.CacheUnit;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.Idx;
import com.engine.common.ramcache.enhance.Enhance;
import com.game.gow.module.common.CachedSizes;

/**
 * 道具实体
 */
@Entity
@NamedQueries({
	@NamedQuery(name = Item.MAX_ID, query = "select max(t.id) from Item as t where t.id between ? and ?"),
	@NamedQuery(name = "Item.owner", query = "from Item t where t.owner = ?")})
@Cached(size = CachedSizes.DEFAULT, unit = CacheUnit.REGION)
@Table(appliesTo = "Item")
public class Item implements IEntity<Long> {

	
	/* 命名查询名 */
	public static final String MAX_ID = "Item_maxId";

	/** 唯一编号 */
	@Id
	private Long id;
	
	@Idx(query = "Item.owner")
	private Long owner;
	
	/** 配置表ID */
	private int baseId;
	
	/** 道具数量 */
	private int num = 1;
	
	/** 附加属性 */
	@Lob
	private String content;
	
	// Getter and Setter ...
	public Long getId() {
		return id;
	}

	protected void setId(Long id) {
		this.id = id;
	}
	
	public Long getOwner() {
		return owner;
	}

	public void setOwner(Long owner) {
		this.owner = owner;
	}

	public int getBaseId() {
		return baseId;
	}

	@Enhance
	public void setBaseId(int baseId) {
		this.baseId = baseId;
	}
	
	/** 构造方法 */
	public static Item valueOf(long id, long owner, int baseId, int num, String content)
	{
		Item result = new Item();
		result.id = id;
		result.owner = owner;
		result.baseId = baseId;
		result.num = num;
		result.content = content;
		return result;
	}

	public String getContent() {
		return content;
	}

	@Enhance
	public void setContent(String content) {
		this.content = content;
	}

	public int getNum() {
		return num;
	}

	@Enhance
	public void setNum(int num) {
		this.num = num;
	}
}
