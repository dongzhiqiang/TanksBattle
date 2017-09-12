package com.game.gow.module.common.manager;

import javax.persistence.Entity;
import javax.persistence.EnumType;
import javax.persistence.Enumerated;
import javax.persistence.Id;
import javax.persistence.Lob;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.CacheType;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.InitialConfig;
import com.engine.common.ramcache.enhance.Enhance;
import com.engine.common.utils.json.JsonUtils;
import com.game.gow.module.common.CachedSizes;

/**
 * 全局信息实体<br/>
 * 该实体用于记录整台物理服务器上唯一的信息，不同信息需要自行扩展{@link GlobalKey}的成员
 * 
 */
@Entity
@Cached(size = CachedSizes.DEFAULT, type = CacheType.MANUAL)
@InitialConfig
public class GlobalInfo implements IEntity<GlobalKey> {

	@Id
	@Enumerated(EnumType.STRING)
	private GlobalKey id;
	/** 内容(JSON字符串) */
	@Lob
	private String content;

	// 增强方法

	/** 更新内容 */
	@Enhance
	void update(Object value) {
		if (value == null) {
			content = null;
		} else {
			content = JsonUtils.object2String(value);
		}
	}

	// 逻辑方法

	/**
	 * 获取全局信息值
	 * @param clz 全局信息值类型
	 * @return 信息不存在会返回null
	 */
	public <T> T getValue(Class<T> clz) {
		if (content == null) {
			return null;
		}
		return JsonUtils.string2Object(content, clz);
	}

	/**
	 * 获取全局信息值
	 * @param clz 全局信息值类型
	 * @return
	 */
	public <T> T[] getArrayValue(Class<T> clz) {
		if (content == null) {
			return null;
		}
		return JsonUtils.string2Array(content, clz);
	}

	// Getter and Setter ...

	public GlobalKey getId() {
		return id;
	}

	protected void setId(GlobalKey id) {
		this.id = id;
	}

	public String getContent() {
		return content;
	}

	public void setContent(String content) {
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
		if (!(obj instanceof GlobalInfo))
			return false;
		GlobalInfo other = (GlobalInfo) obj;
		if (getId() == null) {
			if (other.getId() != null)
				return false;
		} else if (!getId().equals(other.getId()))
			return false;
		return true;
	}

	// Static Method

	/** 构造方法 */
	public static GlobalInfo valueOf(GlobalKey id, Object defaultValue) {
		GlobalInfo result = new GlobalInfo();
		result.id = id;
		if (defaultValue != null) {
			result.content = JsonUtils.object2String(defaultValue);
		}
		return result;
	}

}
