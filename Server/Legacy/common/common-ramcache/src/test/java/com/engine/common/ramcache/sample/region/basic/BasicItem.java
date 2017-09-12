package com.engine.common.ramcache.sample.region.basic;

import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.NamedQueries;
import javax.persistence.NamedQuery;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.CacheUnit;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.Idx;
import com.engine.common.ramcache.anno.InitialConfig;
import com.engine.common.ramcache.enhance.Enhance;

/**
 * 测试道具实体
 * 
 */
@Entity
@NamedQueries({ @NamedQuery(name = "BasicItem.owner", query = "from BasicItem t where t.owner = ?") })
@Cached(size = "default", unit = CacheUnit.REGION)
@InitialConfig
public class BasicItem implements IEntity<Integer> {
	
	public static BasicItem valueOf(int id, int owner, int amount) {
		BasicItem result = new BasicItem();
		result.id = id;
		result.owner = owner;
		result.amount = amount;
		return result;
	}

	@Id
	private Integer id;
	@Idx(query = "BasicItem.owner")
	private int owner;
	private int amount;

	/**
	 * 添加数量
	 * @param value 增量值
	 * @return true:添加成功;false:添加失败
	 */
	@Enhance("true")
	public boolean increaseAmount(int value) {
		if (value <= 0) {
			return false;
		}
		amount += value;
		return true;
	}
	
	/**
	 * 扣减数量
	 * @param value 扣减值
	 * @throws AmountNotEnoughException 数量不足时抛出
	 */
	@Enhance(ignore = AmountNotEnoughException.class)
	public void decreaseAmount(int value) throws AmountNotEnoughException {
		if (value <= 0) {
			throw new IllegalArgumentException();
		}
		
		amount -= value;
		if (amount < 0) {
			AmountNotEnoughException ex = new AmountNotEnoughException(Math.abs(amount));
			amount = 0;
			throw ex;
		}
	}

	@Enhance
	public void setAmount(int amount) {
		this.amount = amount;
	}

	// Getter and Setter ...
	
	public Integer getId() {
		return id;
	}

	protected void setId(Integer id) {
		this.id = id;
	}

	public int getOwner() {
		return owner;
	}

	protected void setOwner(int owner) {
		this.owner = owner;
	}

	public int getAmount() {
		return amount;
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
		if (!(obj instanceof BasicItem))
			return false;
		BasicItem other = (BasicItem) obj;
		if (this.getId() == null) {
			if (other.getId() != null)
				return false;
		} else if (!this.getId().equals(other.getId()))
			return false;
		return true;
	}

}
