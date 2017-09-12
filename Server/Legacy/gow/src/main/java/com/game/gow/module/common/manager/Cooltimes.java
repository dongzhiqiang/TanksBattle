package com.game.gow.module.common.manager;

import java.util.Date;
import java.util.HashMap;
import java.util.Map;

import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Lob;

import org.apache.commons.lang3.StringUtils;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.Persister;
import com.engine.common.ramcache.enhance.Enhance;
import com.engine.common.utils.json.JsonUtils;
import com.game.gow.module.common.CachedSizes;
import com.game.gow.module.common.Persisters;
import com.game.gow.module.common.model.CooltimeKey;

/**
 * 冷却时间集合实体
 * 
 */
@Entity
@Cached(size = CachedSizes.DEFAULT, persister = @Persister(Persisters.PRE_5_MINUTE))
public class Cooltimes implements IEntity<Long> {

	/** 主键{@link Player#getId()} */
	@Id
	private Long id;
	@Lob
	private String content;
	
	private transient volatile Map<CooltimeKey, Date> times;
	
	/**
	 * 更新指定冷却时间值
	 * @param key 冷却时间键
	 * @param time 时间值
	 */
	@Enhance
	void update(CooltimeKey key, Date time) {
		Map<CooltimeKey, Date> times = getTimes();
		times.put(key, time);
		content = JsonUtils.map2String(times);
	}
	
	/**
	 * 获取指定的冷却时间值
	 * @param key 冷却时间键
	 * @return 有可能返回null
	 */
	public Date getTime(CooltimeKey key) {
		return getTimes().get(key);
	}
	
	/**
	 * 移除指定的冷却时间值
	 * @param key 冷却时间键
	 */
	@Enhance
	void remove(CooltimeKey... keys) {
		Map<CooltimeKey, Date> times = getTimes();
		for (CooltimeKey key : keys) {
			times.remove(key);
		}
		content = JsonUtils.map2String(times);
	}

	// 内部方法
	
	/**
	 * 初始化并获取
	 * @return
	 */
	private Map<CooltimeKey, Date> getTimes() {
		if (times == null) {
			synchronized (this) {
				if (times == null) {
					if (StringUtils.isBlank(content)) {
						times = new HashMap<CooltimeKey, Date>(CooltimeKey.values().length);
					} else {
						times = JsonUtils.string2Map(content, CooltimeKey.class, Date.class);
					}
				}
			}
		}
		return times;
	}
	
	// Getter and Setter ...

	public Long getId() {
		return id;
	}

	protected void setId(Long id) {
		this.id = id;
	}

	public String getContent() {
		return content;
	}

	protected void setContent(String content) {
		this.content = content;
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
		if (!(obj instanceof Cooltimes))
			return false;
		Cooltimes other = (Cooltimes) obj;
		if (getId() == null) {
			if (other.getId() != null)
				return false;
		} else if (!getId().equals(other.getId()))
			return false;
		return true;
	}

}
