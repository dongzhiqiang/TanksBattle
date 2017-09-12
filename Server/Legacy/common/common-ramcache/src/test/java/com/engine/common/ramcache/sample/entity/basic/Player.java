package com.engine.common.ramcache.sample.entity.basic;

import javax.persistence.Entity;
import javax.persistence.Id;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.enhance.Enhance;

/**
 * 用户对象实体
 * 
 */
@Entity
@Cached(size = "default")
public class Player implements IEntity<Integer> {
	
	public static Player valueOf(int id, String name, int gold) {
		Player result = new Player();
		result.id = id;
		result.name = name;
		result.gold = gold;
		return result;
	}

	/** 用户标识 */
	@Id
	private Integer id;
	/** 用户名 */
	private String name;
	/** 金币 */
	private int gold;
	
	/**
	 * 添加金币方法
	 * @param value 增量值
	 * @return true:添加成功;false:添加失败
	 */
	@Enhance("true")
	public boolean increaseGold(int value) {
		if (value <= 0) {
			return false;
		}
		gold += value;
		return true;
	}
	
	/**
	 * 扣费方法
	 * @param value 扣费值
	 * @throws ChargeFailException 扣费失败时抛出
	 */
	@Enhance(ignore = ChargeFailException.class)
	public void charge(int value) throws ChargeFailException {
		if (value <= 0) {
			throw new ChargeFailException();
		}
		if (value > gold) {
			throw new ChargeFailException();
		}
		gold -= value;
	}

	// Getter and Setter ...

	public Integer getId() {
		return id;
	}

	protected void setId(Integer id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	@Enhance
	public void setName(String name) {
		this.name = name;
	}

	public int getGold() {
		return gold;
	}

	protected void setGold(int gold) {
		this.gold = gold;
	}

}
