package com.engine.common.ramcache.sample.actor;

import java.util.Date;
import java.util.concurrent.atomic.AtomicInteger;

import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Transient;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.enhance.Enhance;
import com.engine.common.utils.json.JsonUtils;
@Entity
@Cached(size = "default")
public class Actor implements IEntity<Long> {

	public static Actor valueOf(Long id) {
		Actor result = new Actor();
		result.id = id;
		return result;
	}

	/** 用户标识 */
	@Id
	private Long id;
	/** 用户名 */
	private String name;
	/** 金币 */
	private int gold;
	
	private Date date;
	
	private int make;
    private String idorContent;
	@Transient
	private AtomicInteger idor;

	// Getter and Setter ...

	public Long getId() {
		return id;
	}
	
	public AtomicInteger getIdor() {
		if (idor == null) {
			synchronized (this) {
				if (idor == null) {
					if (idorContent == null) {
						idor = new AtomicInteger();
					} else {
						idor = JsonUtils.string2Object(idorContent,
								AtomicInteger.class);
					}
				}
			}
		}
		return idor;
	}

	protected void setId(Long id) {
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

	public int getMake() {
		return make;
	}
	
	
	
    public Date getDate() {
		return date;
	}

	@Enhance
	public void setMake(int make) {
		this.make = make;
		this.idorContent=JsonUtils.object2String(getIdor());
	}
	
}
