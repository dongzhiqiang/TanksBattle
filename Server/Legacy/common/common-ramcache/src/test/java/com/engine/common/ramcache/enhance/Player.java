package com.engine.common.ramcache.enhance;

import javax.persistence.Embedded;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.ChkUnique;
import com.engine.common.ramcache.anno.Unique;
import com.engine.common.ramcache.enhance.Enhance;
import com.engine.common.ramcache.enhance.Item.Content;

@Cached(size = "default")
public class Player implements IEntity<Integer> {

	private Integer id;
	@Unique(query = "Player.name")
	private String name;
	private int gold;

	@Embedded
	private Content content;

	@Enhance("true")
	public boolean increaseGold(int value) {
		if (value <= 0) {
			return false;
		}
		gold += value;
		return true;
	}

	@Enhance(ignore = IllegalStateException.class)
	public void charge(int value) throws IllegalStateException {
		if (value <= 0) {
			throw new IllegalArgumentException();
		}
		gold -= value;
		if (gold < 0) {
			throw new IllegalStateException();
		}
	}

	@Enhance
	void setName(@ChkUnique("name") String name) {
		this.name = name;
	}

	@Enhance
	public int[] getPrimitiveArray() {
		return new int[] {0, 1, 2};
	}
	
	@Enhance
	public Player[] getObjectArray() {
		return new Player[] {this};
	}

	// Getter And Setter

	public Integer getId() {
		return id;
	}

	protected void setId(Integer id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	public int getGold() {
		return gold;
	}

	protected void setGold(int gold) {
		this.gold = gold;
	}

	/* @Override public int hashCode() { final int prime = 31; int result = 1; result = prime * result + ((id == null) ?
	 * 0 : id.hashCode()); return result; }
	 * 
	 * @Override public boolean equals(Object obj) { if (this == obj) return true; if (obj == null) return false; if
	 * (getClass() != obj.getClass()) return false; Player other = (Player) obj; if (id == null) { if (other.id != null)
	 * return false; } else if (!id.equals(other.id)) return false; return true; } */

	public Content getContent() {
		return content;
	}

	public void setContent(Content content) {
		this.content = content;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((this.getId() == null) ? 0 : this.getId().hashCode());
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
		if (this.getId() == null) {
			if (other.getId() != null)
				return false;
		} else if (!this.getId().equals(other.getId()))
			return false;
		return true;
	}

}
